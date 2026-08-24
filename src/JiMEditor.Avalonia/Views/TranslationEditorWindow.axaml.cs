// Ported from: JiMEditor/Views/TranslationEditorWindow.xaml.cs (2026-04-06) — Avalonia; TryClosing fixed when lang name empty.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using JiMEditor.Avalonia;
using JiME;

namespace JiMEditor.Avalonia.Views;

public partial class TranslationEditorWindow : Window, INotifyPropertyChanged
{
	bool _allowClose;
	bool _hydrated;

	public Scenario scenario { get; private set; } = null!;
	public Translation translation { get; private set; } = null!;
	public Translation translationInitialState { get; private set; } = null!;
	public Dictionary<string, TranslationItem> defaultTranslation = null!;
	public bool IsNew { get; private set; }

	Dictionary<string, TranslationItem> _translationDict = null!;

	public bool Accepted { get; private set; }

	public TranslationEditorWindow()
	{
		InitializeComponent();
	}

	public TranslationEditorWindow( Scenario s, Dictionary<string, TranslationItem> defaultTranslation, Translation? translationParam = null, bool isNew = true )
		: this()
	{
		DataContext = this;
		scenario = s;
		this.defaultTranslation = defaultTranslation;
		translationInitialState = translationParam ?? new Translation();
		IsNew = isNew;
		TranslationUnion();
		CancelButton.IsVisible = isNew;
		HookTranslationItemChangeTracking();
		Loaded += OnTranslationEditorLoaded;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	void OnTranslationEditorLoaded( object? sender, RoutedEventArgs e )
	{
		if ( _hydrated )
			return;
		_hydrated = true;
		LangCodeTextBox.Text = translation.dataName ?? "";
		LangNameTextBox.Text = translation.langName ?? "";
		// Avoid nested path binding (translation.translationItems); assign once the merged list exists.
		TranslationItemsControl.ItemsSource = translation.translationItems;
		LangCodeTextBox.Focus();
		ScenarioDirty.WatchInputs( this, scenario );
	}

	void HookTranslationItemChangeTracking()
	{
		translation.translationItems.CollectionChanged += TranslationItems_CollectionChanged;
		foreach ( var item in translation.translationItems )
			item.PropertyChanged += TranslationItem_PropertyChanged;
	}

	void TranslationItems_CollectionChanged( object? sender, NotifyCollectionChangedEventArgs e )
	{
		if ( e.NewItems != null )
		{
			foreach ( var item in e.NewItems.OfType<TranslationItem>() )
				item.PropertyChanged += TranslationItem_PropertyChanged;
		}

		if ( e.OldItems != null )
		{
			foreach ( var item in e.OldItems.OfType<TranslationItem>() )
				item.PropertyChanged -= TranslationItem_PropertyChanged;
		}

		RefreshTranslationListVisuals();
	}

	void TranslationItem_PropertyChanged( object? sender, PropertyChangedEventArgs e )
	{
		// Row tint depends on translationOK / superfluous. text is included so edits that clear content also repaint.
		if ( e.PropertyName is nameof( TranslationItem.translationOK ) or nameof( TranslationItem.superfluous ) or nameof( TranslationItem.text ) )
			RefreshTranslationListVisuals();
	}

	void RefreshTranslationListVisuals()
	{
		Dispatcher.UIThread.Post( () =>
		{
			// Rebinding row CheckBoxes fires IsCheckedChanged; that is not a user edit.
			using ( scenario.PauseDirtyTracking() )
			{
				var source = translation.translationItems;
				TranslationItemsControl.ItemsSource = null;
				TranslationItemsControl.ItemsSource = source;
			}
		} );
	}

	public void TranslationUnion()
	{
		_translationDict = new Dictionary<string, TranslationItem>();
		translation = new Translation( translationInitialState.dataName );
		translation.langName = translationInitialState.langName;

		foreach ( var item in translationInitialState.translationItems )
		{
			var clonedItem = item.Clone();
			clonedItem.superfluous = !defaultTranslation.ContainsKey( item.key );
			translation.translationItems.Add( clonedItem );
			_translationDict.Add( item.key, clonedItem );
		}

		foreach ( var entity in defaultTranslation )
		{
			if ( !_translationDict.ContainsKey( entity.Key ) )
			{
				var clonedItem = entity.Value.Clone();
				clonedItem.translationOK = false;
				clonedItem.text = "";
				translation.translationItems.Add( clonedItem );
				_translationDict.Add( entity.Key, clonedItem );
			}
		}

		var sortedList = translation.translationItems.ToList();
		sortedList.Sort();
		translation.translationItems.Clear();
		foreach ( var item in sortedList )
			translation.translationItems.Add( item );
	}

	bool TryClosing()
	{
		translation.dataName = ( LangCodeTextBox.Text ?? "" ).Trim();
		translation.langName = ( LangNameTextBox.Text ?? "" ).Trim();

		var dupOther = scenario.translationObserver.Any( it =>
			it.dataName == translation.dataName && !ReferenceEquals( it, translationInitialState ) );
		if ( string.IsNullOrEmpty( translation.dataName ) || dupOther )
		{
			MessageBox.Show( "Give this Translation a unique language code.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
			return false;
		}

		if ( string.IsNullOrEmpty( translation.langName ) )
		{
			MessageBox.Show( "Give this Translation a language name so the players can tell what language it is.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
			return false;
		}

		return true;
	}

	protected override void OnClosing( WindowClosingEventArgs e )
	{
		if ( !_allowClose )
			e.Cancel = true;
		if ( e.Cancel )
		{
			base.OnClosing( e );
			return;
		}
		translation.translationItems.CollectionChanged -= TranslationItems_CollectionChanged;
		foreach ( var item in translation.translationItems )
			item.PropertyChanged -= TranslationItem_PropertyChanged;
		base.OnClosing( e );
	}

	void OkButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( !TryClosing() )
			return;

		translationInitialState.dataName = translation.dataName;
		translationInitialState.langName = translation.langName;
		translationInitialState.translationItems.Clear();
		foreach ( var item in translation.translationItems )
		{
			if ( defaultTranslation.ContainsKey( item.key ) && item.text == defaultTranslation[item.key].text && !item.translationOK )
				continue;
			if ( item.superfluous && item.text.Trim() == "" )
				continue;
			if ( item.added && item.text.Trim() == "" )
				continue;
			if ( item.text.Trim() == "" )
				continue;
			if ( item.deleted )
				continue;
			translationInitialState.translationItems.Add( item );
		}

		Accepted = true;
		_allowClose = true;
		Close();
	}

	void CancelButton_Click( object? sender, RoutedEventArgs e )
	{
		Accepted = false;
		_allowClose = true;
		Close();
	}

	async void EditButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( sender is not Button btn || btn.DataContext is not TranslationItem ti )
			return;
		var def = defaultTranslation.ContainsKey( ti.key ) ? defaultTranslation[ti.key] : new TranslationItem( ti.key, "" );
		var tew = new TranslationItemEditorWindow( ti, def );
		await tew.ShowDialog( this );
		NotifyItemChanged( ti );
	}

	void DeleteButton_Click( object? sender, RoutedEventArgs e )
	{
		var ret = MessageBox.Show(
			"Are you sure you want to delete this translation string?\n\nTHIS CANNOT BE UNDONE.",
			"Delete Translation String", MessageBoxButton.YesNo, MessageBoxImage.Question );
		if ( ret != MessageBoxResult.Yes )
			return;
		if ( sender is not Button btn || btn.DataContext is not TranslationItem ti )
			return;
		_translationDict.Remove( ti.key );
		translation.translationItems.Remove( ti );
	}

	void NotifyItemChanged( TranslationItem ti )
	{
		ti.NotifyPropertyChanged( "text" );
		ti.NotifyPropertyChanged( "translationOK" );
	}

	public void NotifyPropertyChanged( string propName ) =>
		PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propName ) );
}
