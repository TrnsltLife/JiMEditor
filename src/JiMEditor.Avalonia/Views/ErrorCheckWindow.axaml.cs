// Advisory report dialog for ErrorChecker (2026-08-23). Does not block save.
using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Media;
using JiMEditor.Avalonia.ScenarioGraph;
using JiME;

namespace JiMEditor.Avalonia.Views;

public partial class ErrorCheckWindow : Window
{
	ErrorChecker _checker = null!;
	Scenario _scenario = null!;
	Func<Window, ErrorCheckNav, Task> _openNav = null!;
	bool _busy;

	public ErrorCheckWindow()
	{
		InitializeComponent();
	}

	public ErrorCheckWindow(
		ErrorChecker checker,
		Scenario scenario,
		Func<Window, ErrorCheckNav, Task> openNav ) : this()
	{
		_checker = checker;
		_scenario = scenario;
		_openNav = openNav;
		RenderReport();
	}

	void RenderReport()
	{
		CountText.Text = _checker.errorCount;
		ReportPanel.Children.Clear();

		var general = _checker.WarningItems
			.Where( w => w.Section != ErrorCheckSection.DisabledCollections )
			.ToList();
		var collections = _checker.WarningItems
			.Where( w => w.Section == ErrorCheckSection.DisabledCollections )
			.ToList();

		if ( general.Count == 0 && collections.Count == 0 )
		{
			ReportPanel.Children.Add( new TextBlock
			{
				Text = "None Found",
				Foreground = Brushes.White,
				FontSize = 13
			} );
			return;
		}

		foreach ( var warning in general )
			ReportPanel.Children.Add( BuildWarningLine( warning ) );

		if ( collections.Count == 0 )
			return;

		ReportPanel.Children.Add( new TextBlock
		{
			Text = ErrorChecker.DisabledCollectionsHeading,
			Classes = { "heading" },
			TextWrapping = TextWrapping.Wrap
		} );

		var settingsLink = new Button
		{
			Classes = { "errorCheckLink" },
			Content = new TextBlock { Text = ErrorChecker.DisabledCollectionsSettingsLink }
		};
		ToolTip.SetTip( settingsLink, "Open Settings" );
		settingsLink.Click += async ( _, _ ) =>
			await OpenSpanAsync( new ErrorCheckNav( ErrorCheckTargetKind.Settings ) );
		ReportPanel.Children.Add( settingsLink );

		foreach ( var warning in collections )
			ReportPanel.Children.Add( BuildWarningLine( warning ) );
	}

	Control BuildWarningLine( ErrorCheckWarning warning )
	{
		var tb = new SelectableTextBlock
		{
			TextWrapping = TextWrapping.Wrap,
			Foreground = Brushes.White,
			FontSize = 13
		};
		foreach ( var span in warning.Spans )
		{
			if ( span.Nav == null )
			{
				tb.Inlines.Add( new Run( span.Text ) );
				continue;
			}

			var nav = span.Nav;
			var button = new Button
			{
				Classes = { "errorCheckLink" },
				Content = new TextBlock { Text = span.Text }
			};
			ToolTip.SetTip( button, "Open editor" );
			button.Click += async ( _, _ ) => await OpenSpanAsync( nav );
			tb.Inlines.Add( new InlineUIContainer { Child = button } );
		}

		return tb;
	}

	async Task OpenSpanAsync( ErrorCheckNav nav )
	{
		if ( _busy )
			return;
		_busy = true;
		try
		{
			await _openNav( this, nav );
			_checker.CheckErrors( _scenario );
			RenderReport();
		}
		finally
		{
			_busy = false;
		}
	}

	public static ScenarioGraphNode? ToGraphNode( ErrorCheckNav nav ) =>
		nav.Kind switch
		{
			ErrorCheckTargetKind.Event when nav.Name == StartInteraction.DEFAULT_NAME => null,
			ErrorCheckTargetKind.Event when nav.Name != null =>
				new ScenarioGraphNode( ScenarioGraphFullScenarioDocumentBuilder.InteractionId( nav.Name ), ScenarioGraphNodeKind.Interaction, nav.Name ),
			ErrorCheckTargetKind.Trigger when nav.Name != null =>
				new ScenarioGraphNode( ScenarioGraphFullScenarioDocumentBuilder.TriggerId( nav.Name ), ScenarioGraphNodeKind.Trigger, nav.Name ),
			ErrorCheckTargetKind.Objective when nav.Name != null =>
				new ScenarioGraphNode( ScenarioGraphFullScenarioDocumentBuilder.ObjectiveId( nav.Name ), ScenarioGraphNodeKind.Objective, nav.Name ),
			ErrorCheckTargetKind.Resolution when nav.Name != null =>
				new ScenarioGraphNode( ScenarioGraphFullScenarioDocumentBuilder.ResolutionId( nav.Name ), ScenarioGraphNodeKind.Resolution, nav.Name ),
			ErrorCheckTargetKind.TileBlock when nav.Name != null =>
				new ScenarioGraphNode( ScenarioGraphFullScenarioDocumentBuilder.ChapterId( nav.Name ), ScenarioGraphNodeKind.Chapter, nav.Name ),
			ErrorCheckTargetKind.Tile when nav.Id is { } tileId =>
				new ScenarioGraphNode( ScenarioGraphFullScenarioDocumentBuilder.TileId( tileId ), ScenarioGraphNodeKind.Tile, "" ),
			ErrorCheckTargetKind.Token when nav.Id is { } tokenId =>
				new ScenarioGraphNode( ScenarioGraphFullScenarioDocumentBuilder.TokenId( tokenId ), ScenarioGraphNodeKind.Token, "" ),
			ErrorCheckTargetKind.Threat =>
				new ScenarioGraphNode( "Threat_errorcheck", ScenarioGraphNodeKind.ThreatLevel, "Threat" ),
			_ => null
		};

	async void CopyAllButton_Click( object? sender, RoutedEventArgs e )
	{
		var text = _checker.Errors ?? "";
		if ( Clipboard != null )
			await Clipboard.SetTextAsync( text );
	}

	void OkButton_Click( object? sender, RoutedEventArgs e ) => Close();
}
