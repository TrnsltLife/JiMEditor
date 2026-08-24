// Ported from: JiMEditor/Views/CampaignWindow.xaml.cs (2026-04-05) — Avalonia UI.
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using JiMEditor.Avalonia;
using JiME;
using Newtonsoft.Json;

namespace JiMEditor.Avalonia.Views;

public partial class CampaignWindow : Window
{
	const string UnsavedCampaignMessage =
		"The Campaign has changes that haven't been saved. Are you sure you want to exit without saving?";

	public Campaign campaign { get; private set; } = null!;

	string _campaignFolder = "";
	bool _controlDown;
	bool _skipDirtyPrompt;
	CampaignDirtyTracker? _dirtyTracker;

	public CampaignWindow() : this( null )
	{
	}

	public CampaignWindow( Campaign? c )
	{
		InitializeComponent();
		campaign = c ?? new Campaign();
		DataContext = this;
		_campaignFolder = Path.Combine( UserJourneyPaths.GetBasePath(), campaign.campaignGUID.ToString() );

		if ( c == null )
		{
			CancelFooterButton.IsVisible = true;
			DeleteCampaignButton.IsVisible = false;
		}
		else
		{
			CancelFooterButton.IsVisible = false;
			DeleteCampaignButton.IsVisible = true;
		}

		SaveCampaign();
		campaign.ClearDirty();
		campaign.PropertyChanged += OnCampaignPropertyChanged;
		_dirtyTracker = CampaignDirty.Attach( campaign );
		CampaignDirty.WatchInputs( this, campaign );
		SyncTitle();

		DescriptionTextBox.TextChanged += DescriptionTextBox_OnTextChanged;
		TriggersCombo.SelectionChanged += ( _, _ ) => UpdateDelTriggerButtonEnabled();
		UpdateDelTriggerButtonEnabled();
	}

	void OnCampaignPropertyChanged( object? sender, PropertyChangedEventArgs e )
	{
		if ( e.PropertyName is nameof( Campaign.isDirty ) or nameof( Campaign.campaignName ) )
			SyncTitle();
	}

	void SyncTitle()
	{
		string baseTitle;
		if ( CancelFooterButton.IsVisible )
			baseTitle = "Campaign Manager - New Campaign";
		else if ( string.IsNullOrWhiteSpace( campaign.campaignName ) )
			baseTitle = "Campaign Manager - Edit Campaign";
		else
			baseTitle = "Campaign Manager - Edit Campaign - " + campaign.campaignName;
		Title = campaign.isDirty ? baseTitle + "*" : baseTitle;
	}

	void UpdateDelTriggerButtonEnabled() =>
		DelTriggerButton.IsEnabled = TriggersCombo.SelectedItem is Trigger;

	/// <summary>Campaign description is a single line in the Companion App; collapse line breaks from paste or legacy data.</summary>
	bool _descriptionTextBoxNormalizing;

	void DescriptionTextBox_OnTextChanged( object? sender, TextChangedEventArgs e )
	{
		if ( _descriptionTextBoxNormalizing )
			return;
		if ( sender is not TextBox box )
			return;
		var t = box.Text ?? "";
		if ( t.IndexOfAny( new[] { '\r', '\n' } ) < 0 )
			return;
		_descriptionTextBoxNormalizing = true;
		using var pause = campaign.PauseDirtyTracking();
		try
		{
			var caret = box.CaretIndex;
			var normalized = t.Replace( "\r\n", " " ).Replace( '\r', ' ' ).Replace( '\n', ' ' );
			box.Text = normalized;
			box.CaretIndex = Math.Min( Math.Max( 0, caret ), normalized.Length );
		}
		finally
		{
			_descriptionTextBoxNormalizing = false;
		}
	}

	async void StoryButton_Click( object? sender, RoutedEventArgs e )
	{
		var dlg = new StoryTextEditWindow( campaign.storyText ?? "", windowTitle: "Edit Campaign Story" );
		await dlg.ShowDialog( this );
		if ( !dlg.Accepted )
			return;
		var next = dlg.StoryText ?? "";
		if ( next == ( campaign.storyText ?? "" ) )
			return;
		campaign.storyText = next;
		StoryStatusTextBlock.Text = string.IsNullOrWhiteSpace( campaign.storyText )
			? "The Story Text is not set."
			: "The Story Text is set.";
		campaign.MarkDirty();
	}

	async void ImageButton_Click( object? sender, RoutedEventArgs e )
	{
		var dlg = new CoverImageEditorWindow( campaign.coverImage );
		await dlg.ShowDialog( this );
		if ( !dlg.Accepted )
			return;
		var next = string.IsNullOrWhiteSpace( dlg.CoverImage ) ? null : dlg.CoverImage;
		var previous = string.IsNullOrWhiteSpace( campaign.coverImage ) ? null : campaign.coverImage;
		if ( next == previous )
			return;
		campaign.coverImage = next;
		campaign.MarkDirty();
	}

	async void TriggerHelpButton_Click( object? sender, RoutedEventArgs e )
	{
		// Tab 3 = "Campaign Triggers". (WPF passed 2 = "Last Stand Triggers"; index 3 matches this screen.)
		await HelpWindow.ShowDialogAsync( this, HelpType.Triggers, 3 );
	}

	async void AddTriggerButton_Click( object? sender, RoutedEventArgs e )
	{
		var dlg = new CampaignTriggerEditorWindow( campaign );
		await dlg.ShowDialog( this );
		if ( !dlg.Accepted )
			return;
		var t = new Trigger( dlg.ResultTriggerName );
		t.isCampaignTrigger = true;
		t.isMultiTrigger = true;
		campaign.triggerCollection.Add( t );
		TriggersCombo.SelectedIndex = campaign.triggerCollection.Count - 1;
		UpdateDelTriggerButtonEnabled();
	}

	void DelTriggerButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( TriggersCombo.SelectedItem is not Trigger selected )
			return;
		campaign.triggerCollection.Remove( selected );
		TriggersCombo.SelectedIndex = campaign.triggerCollection.Count > 0 ? 0 : -1;
		UpdateDelTriggerButtonEnabled();
	}

	void CampaignWindow_Opened( object? sender, EventArgs e )
	{
		CampaignGuidTextBlock.Text = "Campaign GUID:  " + campaign.campaignGUID;
		StoryStatusTextBlock.Text = string.IsNullOrWhiteSpace( campaign.storyText )
			? "The Story Text is not set."
			: "The Story Text is set.";
		using ( campaign.PauseDirtyTracking() )
		{
			if ( TriggersCombo.Items.Count > 0 )
				TriggersCombo.SelectedIndex = 0;
			else
				TriggersCombo.SelectedIndex = -1;
		}
		UpdateDelTriggerButtonEnabled();
		CampaignNameTextBox?.SelectAll();
	}

	void CampaignWindow_KeyDown( object? sender, KeyEventArgs e ) =>
		_controlDown = EditorHotKeys.HasPrimaryModifier( e );

	void CampaignWindow_KeyUp( object? sender, KeyEventArgs e )
	{
		if ( e.Key is Key.LeftCtrl or Key.RightCtrl or Key.LWin or Key.RWin )
			_controlDown = false;
		else
			_controlDown = EditorHotKeys.HasPrimaryModifier( e );
	}

	static CampaignItem? CampaignItemFromSource( object? source )
	{
		for ( var el = source as Control; el != null; el = el.Parent as Control )
		{
			if ( el.DataContext is CampaignItem ci )
				return ci;
		}

		return null;
	}

	void SaveCampaign()
	{
		if ( !Directory.Exists( _campaignFolder ) )
		{
			var di = Directory.CreateDirectory( _campaignFolder );
			if ( di == null )
			{
				MessageBox.Show(
					"Could not create the Campaign project folder.\r\nTried to create: " + _campaignFolder,
					"App Exception",
					MessageBoxButton.OK,
					MessageBoxImage.Error );
				return;
			}
		}

		campaign.fileVersion = Utils.formatVersion;
		var output = JsonConvert.SerializeObject( campaign, Formatting.Indented );
		var outpath = Path.Combine( _campaignFolder, campaign.campaignGUID + ".json" );
		try
		{
			File.WriteAllText( outpath, output );
			campaign.ClearDirty();
		}
		catch ( Exception ex )
		{
			MessageBox.Show(
				"Could not save the Campaign file.\r\n\r\nException:\r\n" + ex.Message,
				"App Exception",
				MessageBoxButton.OK,
				MessageBoxImage.Error );
		}
	}

	void OpenExplorer_Click( object? sender, RoutedEventArgs e )
	{
		try
		{
			if ( !Directory.Exists( _campaignFolder ) )
				Directory.CreateDirectory( _campaignFolder );
			Process.Start( new ProcessStartInfo
			{
				FileName = _campaignFolder,
				UseShellExecute = true
			} );
		}
		catch ( Exception ex )
		{
			MessageBox.Show( "Could not open folder:\r\n" + ex.Message, "App Exception", MessageBoxButton.OK, MessageBoxImage.Error );
		}
	}

	void CreateNewButton_Click( object? sender, RoutedEventArgs e )
	{
		var scenario = new Scenario();
		scenario.campaignGUID = campaign.campaignGUID;
		scenario.projectType = ProjectType.Campaign;
		var doEdit = _controlDown;

		var fm = new FileManager( scenario );
		if ( !fm.SaveAs( _campaignFolder ) )
			return;

		var ci = new CampaignItem
		{
			scenarioName = scenario.scenarioName,
			fileName = fm.fileName
		};
		campaign.scenarioCollection.Add( ci );
		scenario.fileName = fm.fileName;
		SaveCampaign();

		if ( !doEdit )
			return;

		foreach ( var t in campaign.triggerCollection )
			scenario.triggersObserver.Add( t );

		scenario.TriggerTitleChange( false );
		_skipDirtyPrompt = true;
		var main = new MainWindow( scenario );
		if ( global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
			desktop.MainWindow = main;
		main.Show();
		Close();
	}

	void AddExistingButton_Click( object? sender, RoutedEventArgs e )
	{
		var basePath = UserJourneyPaths.GetBasePath();
		if ( !Directory.Exists( basePath ) )
		{
			var di = Directory.CreateDirectory( basePath );
			if ( di == null )
			{
				MessageBox.Show(
					"Could not create the scenario project folder.\r\nTried to create: " + basePath,
					"App Exception",
					MessageBoxButton.OK,
					MessageBoxImage.Error );
				return;
			}
		}

		var picked = FileDialogs.PickOpenFile(
			title: "Add Scenario to Campaign",
			defaultExtension: ".jime",
			filterName: "Journey File (*.jime)",
			filterPattern: "*.jime",
			initialDirectory: basePath );
		if ( string.IsNullOrEmpty( picked ) )
			return;

		var safeName = Path.GetFileName( picked );
		var s = FileManager.LoadProject( safeName );
		if ( s == null )
			return;

		var ci = new CampaignItem
		{
			scenarioName = s.scenarioName,
			fileName = safeName
		};
		campaign.scenarioCollection.Add( ci );
		SaveCampaign();

		var fi = new FileInfo( picked );
		var moveto = Path.Combine( _campaignFolder, safeName );
		fi.MoveTo( moveto );

		var scenario = FileManager.LoadProjectFromPath( _campaignFolder, ci.fileName );
		if ( scenario == null )
		{
			MessageBox.Show(
				"Could not modify the Scenario to use a Campaign.\r\nTried to modify: " + Path.Combine( _campaignFolder, ci.fileName ),
				"App Exception",
				MessageBoxButton.OK,
				MessageBoxImage.Error );
			return;
		}

		scenario.campaignGUID = campaign.campaignGUID;
		scenario.projectType = ProjectType.Campaign;
		var fm = new FileManager( scenario );
		fm.Save();
	}

	async void AddGeneratedButton_Click( object? sender, RoutedEventArgs e )
	{
		var procWindow = new ProceduralGeneratorWindow( allowDirectSaving: false );
		await procWindow.ShowDialog( this );
		if ( !procWindow.Accepted || procWindow.Scenario == null )
			return;

		var scenario = procWindow.Scenario;
		scenario.campaignGUID = campaign.campaignGUID;
		scenario.projectType = ProjectType.Campaign;

		var fm = new FileManager( scenario );
		if ( !fm.SaveAs( _campaignFolder ) )
			return;

		var ci = new CampaignItem
		{
			scenarioName = scenario.scenarioName,
			fileName = fm.fileName
		};
		campaign.scenarioCollection.Add( ci );
		scenario.fileName = fm.fileName;
		SaveCampaign();
	}

	void PackageCampaignButton_Click( object? sender, RoutedEventArgs e )
	{
		SaveCampaign();

		if ( string.IsNullOrWhiteSpace( campaign.campaignName ) )
		{
			MessageBox.Show(
				"The Campaign must have a name before it can be packaged.",
				"Package Error",
				MessageBoxButton.OK,
				MessageBoxImage.Error );
			return;
		}

		if ( campaign.scenarioCollection.Count == 0 )
		{
			MessageBox.Show(
				"This Campaign doesn't contain any Scenarios for packaging.",
				"Package Error",
				MessageBoxButton.OK,
				MessageBoxImage.Error );
			return;
		}

		var zipFileName = BuildCampaignPackageFileName( campaign.campaignName, campaign.campaignVersion );
		var zipPath = Path.Combine( _campaignFolder, zipFileName );
		try
		{
			var fi = new FileInfo( zipPath );
			if ( fi.Exists )
				fi.Delete();

			using ( var zipStream = new FileStream( zipPath, FileMode.OpenOrCreate ) )
			using ( var archive = new ZipArchive( zipStream, ZipArchiveMode.Update ) )
			{
				AddFileToPackage( archive, Path.Combine( _campaignFolder, campaign.campaignGUID + ".json" ), campaign.campaignGUID + ".json" );
				foreach ( var item in campaign.scenarioCollection )
					AddFileToPackage( archive, Path.Combine( _campaignFolder, item.fileName ), item.fileName );
			}

			MessageBox.Show(
				"Campaign Package successfully created.\r\n\r\n",
				"Campaign Packaging Successful",
				MessageBoxButton.OK,
				MessageBoxImage.Information );
		}
		catch ( Exception ex )
		{
			MessageBox.Show(
				"Could not create the Package ZIP.\r\n\r\nException:\r\n" + ex.Message,
				"App Exception",
				MessageBoxButton.OK,
				MessageBoxImage.Error );
		}
	}

	static void AddFileToPackage( ZipArchive archive, string sourcePath, string entryName )
	{
		var entry = archive.CreateEntry( entryName, CompressionLevel.Optimal );
		using var entryStream = entry.Open();
		using var fileStream = new FileStream( sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read );
		fileStream.CopyTo( entryStream );
	}

	/// <summary>{CampaignName}{campaignVersion}.zip — version suffix omitted when blank so prior packages are kept.</summary>
	static string BuildCampaignPackageFileName( string campaignName, string? campaignVersion )
	{
		var namePart = SanitizeFileNameSegment( campaignName );
		var versionPart = campaignVersion?.Trim() ?? "";
		if ( versionPart.Length > 0 )
			versionPart = SanitizeFileNameSegment( versionPart );
		return namePart + versionPart + ".zip";
	}

	static string SanitizeFileNameSegment( string name )
	{
		var invalid = Path.GetInvalidFileNameChars();
		var chars = name.Select( c => invalid.Contains( c ) ? '_' : c ).ToArray();
		return new string( chars ).Trim();
	}

	void MoveUp_Click( object? sender, RoutedEventArgs e )
	{
		var ci = CampaignItemFromSource( e.Source );
		if ( ci == null )
			return;
		var idx = campaign.scenarioCollection.IndexOf( ci );
		if ( idx <= 0 )
			return;
		var prev = campaign.scenarioCollection[idx - 1];
		campaign.scenarioCollection[idx - 1] = ci;
		campaign.scenarioCollection[idx] = prev;
	}

	void MoveDown_Click( object? sender, RoutedEventArgs e )
	{
		var ci = CampaignItemFromSource( e.Source );
		if ( ci == null )
			return;
		var idx = campaign.scenarioCollection.IndexOf( ci );
		if ( idx < 0 || idx >= campaign.scenarioCollection.Count - 1 )
			return;
		var next = campaign.scenarioCollection[idx + 1];
		campaign.scenarioCollection[idx + 1] = ci;
		campaign.scenarioCollection[idx] = next;
	}

	void EditScenario_Click( object? sender, RoutedEventArgs e )
	{
		var ci = CampaignItemFromSource( e.Source );
		if ( ci == null )
			return;

		SaveCampaign();

		var scenario = FileManager.LoadProjectFromPath( _campaignFolder, ci.fileName );
		if ( scenario == null )
			return;

		scenario.campaignGUID = campaign.campaignGUID;
		var fm = new FileManager( scenario );
		fm.Save();

		foreach ( var t in campaign.triggerCollection )
			scenario.triggersObserver.Add( t );

		scenario.TriggerTitleChange( false );
		_skipDirtyPrompt = true;
		var main = new MainWindow( scenario );
		if ( global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
			desktop.MainWindow = main;
		main.Show();
		Close();
	}

	void RemoveScenario_Click( object? sender, RoutedEventArgs e )
	{
		var ci = CampaignItemFromSource( e.Source );
		if ( ci == null )
			return;

		var basePath = UserJourneyPaths.GetBasePath();
		var doDelete = _controlDown;

		if ( doDelete )
		{
			var res = MessageBox.Show(
				"Are you sure you want to PERMANENTLY DELETE the Scenario?\r\n\r\n" + ci.scenarioName + "\r\n\r\n" + Path.Combine( _campaignFolder, ci.fileName ),
				"Confirm Scenario Deletion",
				MessageBoxButton.YesNo,
				MessageBoxImage.Question );
			if ( res != MessageBoxResult.Yes )
				return;
			campaign.scenarioCollection.Remove( ci );
			var fi = new FileInfo( Path.Combine( _campaignFolder, ci.fileName ) );
			if ( fi.Exists )
				fi.Delete();
			SaveCampaign();
		}
		else
		{
			campaign.scenarioCollection.Remove( ci );
			SaveCampaign();
			var fi = new FileInfo( Path.Combine( _campaignFolder, ci.fileName ) );
			var moveto = Path.Combine( basePath, ci.fileName );
			if ( fi.Exists )
				fi.MoveTo( moveto );
			var scenario = FileManager.LoadProject( ci.fileName );
			if ( scenario != null )
			{
				scenario.campaignGUID = Guid.Empty;
				scenario.projectType = ProjectType.Standalone;
				var fm = new FileManager( scenario );
				fm.Save();
			}
			else
			{
				MessageBox.Show(
					"Could not open the Scenario to reset its Campaign setting to Empty.\r\nTried to modify: " + Path.Combine( _campaignFolder, ci.fileName ),
					"App Exception",
					MessageBoxButton.OK,
					MessageBoxImage.Error );
			}
		}
	}

	void SaveAndClose_Click( object? sender, RoutedEventArgs e )
	{
		SaveCampaign();
		ReturnToProjectWindow();
	}

	void DeleteCampaignButton_Click( object? sender, RoutedEventArgs e )
	{
		var res = MessageBox.Show(
			"Are you sure you want to permanently delete this Campaign and its shareable Package, if it exists?\r\n\r\nTHIS ACTION CANNOT BE UNDONE.\r\n\r\nAny individual Scenarios added to this Campaign WILL NOT BE DELETED - they will be moved back to the base folder.",
			"Permanently Delete This Campaign And Its Data?",
			MessageBoxButton.YesNo,
			MessageBoxImage.Question );
		if ( res != MessageBoxResult.Yes )
			return;
		MoveScenariosToBaseFolderAndDeleteCampaign();
		ReturnToProjectWindow();
	}

	void Cancel_Click( object? sender, RoutedEventArgs e )
	{
		MoveScenariosToBaseFolderAndDeleteCampaign();
		ReturnToProjectWindow();
	}

	/// <summary>WPF <c>cancelButton_Click</c> / <c>deleteCampaign_Click</c>: relocate .jime files, then remove the campaign folder.</summary>
	void MoveScenariosToBaseFolderAndDeleteCampaign()
	{
		var di = new DirectoryInfo( _campaignFolder );
		if ( !di.Exists )
			return;

		foreach ( var fi in di.GetFiles().Where( f => f.Extension == ".jime" ) )
		{
			var s = FileManager.LoadProjectFromPath( _campaignFolder, fi.Name );
			if ( s == null )
				continue;
			s.campaignGUID = Guid.Empty;
			s.projectType = ProjectType.Standalone;
			var fm = new FileManager( s );
			fm.Save();
		}

		if ( Directory.Exists( _campaignFolder ) )
			Directory.Delete( _campaignFolder, true );
	}

	void ReturnToProjectWindow()
	{
		_skipDirtyPrompt = true;
		var pw = new ProjectWindow();
		if ( global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
			desktop.MainWindow = pw;
		pw.Show();
		Close();
	}

	protected override void OnClosing( WindowClosingEventArgs e )
	{
		if ( !_skipDirtyPrompt && campaign is { isDirty: true } )
		{
			if ( MessageBox.Show(
				     UnsavedCampaignMessage,
				     "Campaign Changes Not Saved",
				     MessageBoxButton.YesNo,
				     MessageBoxImage.Question )
			     != MessageBoxResult.Yes )
			{
				e.Cancel = true;
				return;
			}
		}

		_dirtyTracker?.Dispose();
		_dirtyTracker = null;
		campaign.PropertyChanged -= OnCampaignPropertyChanged;
		base.OnClosing( e );
	}
}
