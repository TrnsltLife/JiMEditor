// Ported from: JiMEditor/Views/ProceduralGeneratorWindow.xaml.cs (2026-06-13)
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using JiME;
using JiME.Procedural;
using JiME.Procedural.StoryElements;
using Newtonsoft.Json;

namespace JiMEditor.Avalonia.Views;

public partial class ProceduralGeneratorWindow : Window
{
	const string RandomText = "RANDOM";

	static string SettingsStorageFile => Path.Combine( UserJourneyPaths.GetBasePath(), "ProcParameters.json" );

	public bool Accepted { get; private set; }
	public bool AllowDirectSaving { get; }
	public Scenario? Scenario { get; private set; }
	public ProceduralGenerator Generator { get; }
	public ProceduralGeneratorParameters GeneratorParameters { get; private set; } = null!;
	bool _suppressComboSelectionSync;

	public ProceduralGeneratorWindow() : this( allowDirectSaving: true )
	{
	}

	public ProceduralGeneratorWindow( bool allowDirectSaving )
	{
		AllowDirectSaving = allowDirectSaving;
		InitializeComponent();
		Generator = new ProceduralGenerator();
		Scenario = null;

		VisualizeButton.IsEnabled = false;
		SaveButton.IsEnabled = false;
		OkButton.IsEnabled = false;
		SaveButton.IsVisible = allowDirectSaving;

		LogList.ItemsSource = new List<ProceduralGeneratorContext.LogItem>
		{
			new() { Type = ProceduralGeneratorContext.LogType.Info, Message = "Click the Die to generate..." }
		};

		LoadParameterValues();
	}

	void LoadParameterValues()
	{
		if ( File.Exists( SettingsStorageFile ) )
		{
			try
			{
				var json = File.ReadAllText( SettingsStorageFile );
				GeneratorParameters = JsonConvert.DeserializeAnonymousType( json, Generator.GetDefaultParameters() )
				                    ?? Generator.GetDefaultParameters();
			}
			catch
			{
				GeneratorParameters = Generator.GetDefaultParameters();
			}
		}
		else
		{
			GeneratorParameters = Generator.GetDefaultParameters();
		}

		DataContext = GeneratorParameters;

		var allArchetypes = Enum.GetValues( typeof( StoryArchetype.Type ) ).Cast<StoryArchetype.Type?>()
			.Select( x => x.ToString() )
			.OrderBy( x => x )
			.ToList();
		allArchetypes.Insert( 0, RandomText );
		_suppressComboSelectionSync = true;
		try
		{
			ArchetypeCombo.ItemsSource = allArchetypes;
			ArchetypeCombo.SelectedIndex = allArchetypes.IndexOf(
				GeneratorParameters.StoryArchetype == null ? RandomText : GeneratorParameters.StoryArchetype.Value.ToString() );
		}
		finally
		{
			_suppressComboSelectionSync = false;
		}
		UpdateArchetypeTooltip();

		UpdateTemplateSelections( GeneratorParameters.StoryTemplate );

		DebugDontFillStoriesCheckBox.IsChecked = GeneratorParameters.DebugSkipStoryPointsFillIn;
		DebugVerboseStoryElementCheck.IsChecked = GeneratorParameters.DebugVerboseStoryElementCheck;
	}

	void UpdateTemplateSelections( string? overrideTemplateSelection )
	{
		var selectedArchetype = ArchetypeCombo.SelectedItem as string;
		StoryArchetype.Type? fixedArchetype = string.IsNullOrEmpty( selectedArchetype ) || selectedArchetype == RandomText
			? null
			: (StoryArchetype.Type?)Enum.Parse( typeof( StoryArchetype.Type ), selectedArchetype );
		var validTemplates = StoryTemplate.GetAllKnownTemplates()
			.OrderBy( x => x )
			.Where( x => fixedArchetype == null || StoryTemplate.GetTemplate( x ).SupportedArchetypes.ContainsKey( fixedArchetype.Value ) )
			.ToList();
		validTemplates.Insert( 0, RandomText );

		var oldSelection = TemplateCombo.SelectedItem as string;
		_suppressComboSelectionSync = true;
		try
		{
			TemplateCombo.ItemsSource = validTemplates;
			if ( overrideTemplateSelection?.Length > 0 )
			{
				TemplateCombo.SelectedIndex = validTemplates.IndexOf(
					GeneratorParameters.StoryTemplate?.Length > 0 ? GeneratorParameters.StoryTemplate : RandomText );
			}
			else if ( oldSelection != null && validTemplates.Contains( oldSelection ) )
			{
				TemplateCombo.SelectedIndex = validTemplates.IndexOf( oldSelection );
			}
			else
			{
				TemplateCombo.SelectedIndex = 0;
			}
		}
		finally
		{
			_suppressComboSelectionSync = false;
		}

		UpdateTemplateTooltip();
	}

	void ArchetypeCombo_SelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		if ( _suppressComboSelectionSync )
			return;
		UpdateTemplateSelections( null );
		UpdateArchetypeTooltip();
	}

	void TemplateCombo_SelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		if ( _suppressComboSelectionSync )
			return;
		UpdateTemplateTooltip();
	}

	void UpdateArchetypeTooltip()
	{
		var selected = ArchetypeCombo.SelectedItem as string;
		if ( string.IsNullOrEmpty( selected ) || selected == RandomText )
		{
			ToolTip.SetTip( ArchetypeCombo, "Story Archetype will be randomly selected" );
			return;
		}

		var a = StoryArchetype.GetArchetype( (StoryArchetype.Type)Enum.Parse( typeof( StoryArchetype.Type ), selected! ) );
		var sb = new StringBuilder();
		sb.AppendLine( selected );
		sb.AppendLine( a.Description );
		sb.AppendLine( "START: " + a.Start.Comment );
		sb.AppendLine( "MIDDLE: " + a.Middle.Comment );
		sb.Append( "END: " + a.End.Comment );
		ToolTip.SetTip( ArchetypeCombo, sb.ToString() );
	}

	void UpdateTemplateTooltip()
	{
		var selected = TemplateCombo.SelectedItem as string;
		if ( string.IsNullOrEmpty( selected ) || selected == RandomText )
		{
			ToolTip.SetTip( TemplateCombo, "Story Template will be randomly selected" );
			return;
		}

		var t = StoryTemplate.GetTemplate( selected! );
		var sb = new StringBuilder();
		sb.AppendLine( t.Name );
		sb.Append( t.Description );
		ToolTip.SetTip( TemplateCombo, sb.ToString() );
	}

	void SaveParameterValues()
	{
		Directory.CreateDirectory( UserJourneyPaths.GetBasePath() );
		var json = JsonConvert.SerializeObject( GeneratorParameters );
		File.WriteAllText( SettingsStorageFile, json );
	}

	void VisualizeButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( Scenario == null )
			return;
		var main = new MainWindow( Scenario );
		if ( global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
			desktop.MainWindow = main;
		main.WindowState = WindowState.Maximized;
		main.Show();
	}

	void SaveButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( Scenario == null )
			return;
		var fileManager = new FileManager( Scenario );
		fileManager.SaveAs();
	}

	void OkButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( Scenario == null )
			return;
		Accepted = true;
		Close();
	}

	void CancelButton_Click( object? sender, RoutedEventArgs e )
	{
		Accepted = false;
		Close();
	}

	void GenerateButton_Click( object? sender, RoutedEventArgs e )
	{
		var archetypeSelection = ArchetypeCombo.SelectedItem as string;
		var templateSelection = TemplateCombo.SelectedItem as string;
		GeneratorParameters.StoryArchetype = archetypeSelection == RandomText
			? null
			: (StoryArchetype.Type?)Enum.Parse( typeof( StoryArchetype.Type ), archetypeSelection! );
		GeneratorParameters.StoryTemplate = templateSelection == RandomText ? null : templateSelection;
		GeneratorParameters.DebugSkipStoryPointsFillIn = DebugDontFillStoriesCheckBox.IsChecked == true;
		GeneratorParameters.DebugVerboseStoryElementCheck = DebugVerboseStoryElementCheck.IsChecked == true;

		SaveParameterValues();

		var generatorContext = Generator.GenerateScenario( GeneratorParameters );
		Scenario = generatorContext.Scenario;

		if ( generatorContext.Scenario != null )
		{
			generatorContext.LogInfo( AllowDirectSaving
				? "Use buttons below to Visualize, Save or Accept the Scenario..."
				: "Use buttons below to Visualize or Accept the Scenario..." );
		}

		LogList.ItemsSource = generatorContext.GeneratorLogs;
		LogScrollViewer.ScrollToEnd();

		var hasScenario = Scenario != null;
		VisualizeButton.IsEnabled = hasScenario;
		SaveButton.IsEnabled = hasScenario;
		OkButton.IsEnabled = hasScenario;
	}
}
