// Ported from: JiMEditor TextEditorWindow (EditMode.Resolution) + ScenarioWindow resolution handlers (2026-04-21).
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiMEditor.Avalonia;
using JiME;
using JiME.TextFormat;

namespace JiMEditor.Avalonia.Views;

public partial class ResolutionEditorWindow : Window
{
	readonly Scenario _scenario;
	readonly TextBookData _resolution;
	readonly bool _isNew;
	string _originalDataName = "";

	public bool Accepted { get; private set; }

	/// <summary>End state stored in <see cref="Scenario.scenarioEndStatus"/> (success vs failure resolution).</summary>
	public bool EndsInSuccess { get; private set; } = true;

	public ResolutionEditorWindow()
	{
		InitializeComponent();
		_scenario = null!;
		_resolution = null!;
	}

	public ResolutionEditorWindow( Scenario scenario, TextBookData resolution, bool isNew )
		: this()
	{
		_scenario = scenario;
		_resolution = resolution;
		_isNew = isNew;
		TriggerComboBox.ItemsSource = scenario.triggersObserver;
		Loaded += OnWindowLoaded;
	}

	/// <summary>Shows the editor and runs the translation tail (including on Cancel), matching <see cref="Translatable.HandleWindow"/> parity.</summary>
	public static async Task<(bool Accepted, bool EndsInSuccess)> ShowEditAsync( Window owner, Scenario scenario, TextBookData data, bool isNew )
	{
		var originals = data.CaptureStartingValues();
		var originalPrefix = data.TranslationKeyPrefix();
		var originalKeyName = data.TranslationKeyName();
		var dlg = new ResolutionEditorWindow( scenario, data, isNew );
		await dlg.ShowDialog( owner );
		data.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originals, originalPrefix, originalKeyName );
		return ( dlg.Accepted, dlg.EndsInSuccess );
	}

	void OnWindowLoaded( object? sender, RoutedEventArgs e )
	{
		Loaded -= OnWindowLoaded;
		_originalDataName = _resolution.dataName;
		ShortNameTextBox.Text = _resolution.dataName;
		if ( _resolution.pages == null || _resolution.pages.Count == 0 )
		{
			_resolution.pages = new System.Collections.Generic.List<string>();
			_resolution.pages.Add( "" );
		}

		BodyEditor.Text = _resolution.pages[0] ?? "";

		TriggerComboBox.SelectedItem = _scenario.triggersObserver.FirstOrDefault( t => t.dataName == _resolution.triggerName )
		                              ?? _scenario.triggersObserver.FirstOrDefault();

		var success = true;
		if ( !_isNew && _scenario.scenarioEndStatus.TryGetValue( _originalDataName, out var endVal ) )
			success = endVal;
		SuccessEndRadio.IsChecked = success;
		FailureEndRadio.IsChecked = !success;
		ScenarioDirty.WatchInputs( this, _scenario );
	}

	async void AddTriggerButton_Click( object? sender, RoutedEventArgs e )
	{
		var w = new TriggerEditorWindow( _scenario );
		await w.ShowDialog( this );
		if ( !w.Accepted || _scenario.triggersObserver.Count == 0 )
			return;
		var last = _scenario.triggersObserver[^1];
		TriggerComboBox.ItemsSource = null;
		TriggerComboBox.ItemsSource = _scenario.triggersObserver;
		TriggerComboBox.SelectedItem = last;
	}

	void CancelButton_Click( object? sender, RoutedEventArgs e )
	{
		Accepted = false;
		Close();
	}

	void OkButton_Click( object? sender, RoutedEventArgs e )
	{
		var name = ( ShortNameTextBox.Text ?? "" ).Trim();
		if ( string.IsNullOrEmpty( name ) )
		{
			MessageBox.Show( "The Resolution name cannot be empty.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
			return;
		}

		if ( name == "None" )
		{
			MessageBox.Show( "The name [None] is reserved.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
			return;
		}

		if ( _scenario.resolutionObserver.Any( r => !ReferenceEquals( r, _resolution ) && r.dataName == name ) )
		{
			MessageBox.Show( $"A Resolution named [{name}] already exists.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
			return;
		}

		var trigger = TriggerComboBox.SelectedItem as Trigger;
		var trigName = trigger?.dataName ?? "None";

		var endsSuccess = SuccessEndRadio.IsChecked == true;

		if ( !_isNew && _originalDataName != name && _scenario.scenarioEndStatus.ContainsKey( _originalDataName ) )
			_scenario.scenarioEndStatus.Remove( _originalDataName );

		_resolution.dataName = name;
		_resolution.triggerName = trigName;
		while ( _resolution.pages.Count < 1 )
			_resolution.pages.Add( "" );
		_resolution.pages[0] = JiMePseudoHtmlFormat.NormalizePseudoHtml( BodyEditor.Text.Trim() );

		if ( !_isNew )
			_scenario.scenarioEndStatus[name] = endsSuccess;

		EndsInSuccess = endsSuccess;
		Accepted = true;
		Close();
	}
}
