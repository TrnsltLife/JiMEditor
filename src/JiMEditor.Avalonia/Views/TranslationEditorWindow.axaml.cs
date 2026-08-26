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
	const string ActivationKeyPrefix = "activation.";

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
		// Filters always reset for each open; window instance is created per edit.
		FilterOkToggle.IsChecked = true;
		FilterSuperfluousToggle.IsChecked = true;
		FilterNeedsToggle.IsChecked = true;
		HideActivationsCheckBox.IsChecked = false;
		KeyFilterTextBox.Text = "";
		OriginalFilterTextBox.Text = "";
		TranslationFilterTextBox.Text = "";
		TextFilterModeToggle.IsChecked = true;
		UpdateTextFilterModeToggleLabel();
		ApplyTranslationListFilter();
		LangCodeTextBox.Focus();
		ScenarioDirty.WatchInputs( this, scenario );
	}

	void FilterToggle_IsCheckedChanged( object? sender, RoutedEventArgs e )
	{
		if ( !_hydrated )
			return;
		ApplyTranslationListFilter();
	}

	void TextFilter_Changed( object? sender, RoutedEventArgs e )
	{
		if ( !_hydrated )
			return;
		ApplyTranslationListFilter();
	}

	void TextFilterModeToggle_IsCheckedChanged( object? sender, RoutedEventArgs e )
	{
		UpdateTextFilterModeToggleLabel();
		if ( !_hydrated )
			return;
		ApplyTranslationListFilter();
	}

	void UpdateTextFilterModeToggleLabel() =>
		TextFilterModeToggle.Content = TextFilterModeToggle.IsChecked == true ? "Show" : "Hide";

	/// <summary>
	/// Filters the list view only. Full list stays in <see cref="translation"/>; OK still saves everything.
	/// Order: status color toggles → hide activations → text Show/Hide OR filters (empty boxes ignored).
	/// </summary>
	void ApplyTranslationListFilter()
	{
		if ( translation?.translationItems == null || scenario == null )
			return;

		var showOk = FilterOkToggle.IsChecked == true;
		var showSuperfluous = FilterSuperfluousToggle.IsChecked == true;
		var showNeeds = FilterNeedsToggle.IsChecked == true;
		var hideActivations = HideActivationsCheckBox.IsChecked == true;

		var keyFilter = ( KeyFilterTextBox.Text ?? "" ).Trim();
		var originalFilter = ( OriginalFilterTextBox.Text ?? "" ).Trim();
		var translationFilter = ( TranslationFilterTextBox.Text ?? "" ).Trim();
		var hasTextFilter = keyFilter.Length > 0 || originalFilter.Length > 0 || translationFilter.Length > 0;
		var showMatchingText = TextFilterModeToggle.IsChecked == true;

		IEnumerable<TranslationItem> filtered = translation.translationItems.Where( item =>
		{
			if ( item.superfluous )
			{
				if ( !showSuperfluous )
					return false;
			}
			else if ( item.translationOK )
			{
				if ( !showOk )
					return false;
			}
			else if ( !showNeeds )
			{
				return false;
			}

			if ( hideActivations && item.key.StartsWith( ActivationKeyPrefix, StringComparison.OrdinalIgnoreCase ) )
				return false;

			if ( !hasTextFilter )
				return true;

			var matchesText = false;
			if ( keyFilter.Length > 0 && item.key.Contains( keyFilter, StringComparison.OrdinalIgnoreCase ) )
				matchesText = true;
			if ( !matchesText && originalFilter.Length > 0 )
			{
				var original = defaultTranslation.TryGetValue( item.key, out var def ) ? ( def.text ?? "" ) : "";
				if ( original.Contains( originalFilter, StringComparison.OrdinalIgnoreCase ) )
					matchesText = true;
			}
			if ( !matchesText && translationFilter.Length > 0
			     && ( item.text ?? "" ).Contains( translationFilter, StringComparison.OrdinalIgnoreCase ) )
				matchesText = true;

			return showMatchingText ? matchesText : !matchesText;
		} );

		using ( scenario.PauseDirtyTracking() )
		{
			TranslationItemsControl.ItemsSource = null;
			TranslationItemsControl.ItemsSource = filtered.ToList();
		}
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
			ApplyTranslationListFilter();
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
