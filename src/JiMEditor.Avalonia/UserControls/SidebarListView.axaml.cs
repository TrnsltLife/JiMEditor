// Ported from: JiMEditor/UserControls/SidebarListView.xaml.cs (2026-04-05)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiMEditor.Avalonia.Converters;
using JiME;

namespace JiMEditor.Avalonia.UserControls;

public partial class SidebarListView : UserControl, INotifyPropertyChanged
{
	IEnumerable? _rawSource;
	INotifyCollectionChanged? _notifiable;
	string _title = "";

	public string Title
	{
		get => _title;
		set
		{
			if ( _title == value )
				return;
			_title = value;
			Notify( nameof( Title ) );
		}
	}

	public ObservableCollection<object> SortedItems { get; } = new();

	public EventHandler? onAddEvent;
	public EventHandler? onRemoveEvent;
	public EventHandler? onSettingsEvent;
	public EventHandler? onDuplicateEvent;
	public EventHandler? onImportEvent;
	public EventHandler? onExportEvent;

	public event Action<string>? NewItemSelectedEvent;

	bool _showDuplicateButton;
	bool _showImportButton;
	bool _showExportButton;

	public bool ShowDuplicateButton
	{
		get => _showDuplicateButton;
		set
		{
			if ( _showDuplicateButton == value )
				return;
			_showDuplicateButton = value;
			Notify( nameof( ShowDuplicateButton ) );
		}
	}

	public bool ShowImportButton
	{
		get => _showImportButton;
		set
		{
			if ( _showImportButton == value )
				return;
			_showImportButton = value;
			Notify( nameof( ShowImportButton ) );
		}
	}

	public bool ShowExportButton
	{
		get => _showExportButton;
		set
		{
			if ( _showExportButton == value )
				return;
			_showExportButton = value;
			Notify( nameof( ShowExportButton ) );
		}
	}

	/// <summary>WPF parity name for the scenario list combo (future handlers use SelectedItem / Items).</summary>
	public ComboBox DataListView => dataListView;

	/// <summary>Green add button (for attaching a flyout / context menu from the parent window).</summary>
	public Button AddInteractionButton => addInteraction;

	public SidebarListView()
	{
		InitializeComponent();
		// MainWindow sets DataContext = scenario on the window; that inherits here and replaces any ctor assignment.
		// Re-assert this control as its own binding root after attach so Title, SortedItems, and toolbar bindings work.
		Loaded += OnSidebarLoaded;
		dataListView.ItemsSource = SortedItems;
	}

	void OnSidebarLoaded( object? sender, RoutedEventArgs e )
	{
		DataContext = this;
	}

	public void SetItemsSource( IEnumerable? source )
	{
		DetachItemsSource();
		_rawSource = source;
		_notifiable = source as INotifyCollectionChanged;
		RebuildSorted();
		if ( _notifiable != null )
			_notifiable.CollectionChanged += OnSourceCollectionChanged;
	}

	public void DetachItemsSource()
	{
		if ( _notifiable != null )
			_notifiable.CollectionChanged -= OnSourceCollectionChanged;
		_notifiable = null;
		_rawSource = null;
		SortedItems.Clear();
		dataListView.SelectedItem = null;
		UpdateToolbarEnabled();
	}

	/// <summary>
	/// Rebuilds sorted rows so in-place edits to display fields (e.g. <see cref="ICommonData.dataName"/>) show in the combo without collection churn.
	/// </summary>
	public void RefreshSortedItems() => RebuildSorted();

	void OnSourceCollectionChanged( object? sender, NotifyCollectionChangedEventArgs e ) => RebuildSorted();

	void RebuildSorted()
	{
		var prev = dataListView.SelectedItem;
		SortedItems.Clear();
		if ( _rawSource != null )
		{
			foreach ( var o in SortSidebarItems( _rawSource ) )
				SortedItems.Add( o );
		}

		if ( prev != null && SortedItems.Contains( prev ) )
			dataListView.SelectedItem = prev;
		else if ( SortedItems.Count > 0 )
			dataListView.SelectedIndex = 0;
		else
			dataListView.SelectedItem = null;

		UpdateToolbarEnabled();
	}

	static IEnumerable<object> SortSidebarItems( IEnumerable source )
	{
		return source.Cast<object>()
			.OrderByDescending( o => o is ICommonData cd && cd.isEmpty )
			.ThenBy( o => GetSortName( o ), StringComparer.OrdinalIgnoreCase );
	}

	static string GetSortName( object o ) => o switch
	{
		ICommonData c => c.dataName ?? "",
		Translation t => t.dataName ?? "",
		IInteraction i => i.dataName ?? "",
		_ => ""
	};

	void DataListView_OnSelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		UpdateToolbarEnabled();
		if ( e.AddedItems.Count != 1 )
			return;
		var sel = e.AddedItems[0];
		if ( sel == null )
			return;
		switch ( sel )
		{
			case IInteraction i:
				NewItemSelectedEvent?.Invoke( i.dataName );
				break;
			case ICommonData c:
				NewItemSelectedEvent?.Invoke( c.dataName );
				break;
			case Translation t:
				NewItemSelectedEvent?.Invoke( t.dataName );
				break;
		}
	}

	void UpdateToolbarEnabled()
	{
		var ok = SidebarSettingsConverter.CanUseSettingsOnItem( dataListView.SelectedItem );
		removeInteraction.IsEnabled = ok;
		duplicate.IsEnabled = ok;
		export.IsEnabled = ok;
		settings.IsEnabled = ok;
	}

	void Add_Click( object? sender, RoutedEventArgs e ) => onAddEvent?.Invoke( sender, e );

	void Settings_Click( object? sender, RoutedEventArgs e ) => onSettingsEvent?.Invoke( sender, e );

	void Remove_Click( object? sender, RoutedEventArgs e ) => onRemoveEvent?.Invoke( sender, e );

	void Duplicate_Click( object? sender, RoutedEventArgs e ) => onDuplicateEvent?.Invoke( sender, e );

	void Import_Click( object? sender, RoutedEventArgs e ) => onImportEvent?.Invoke( sender, e );

	void Export_Click( object? sender, RoutedEventArgs e ) => onExportEvent?.Invoke( sender, e );

	public event PropertyChangedEventHandler? PropertyChanged;

	void Notify( string name ) => PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
}
