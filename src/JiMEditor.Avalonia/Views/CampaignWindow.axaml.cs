// Ported from: JiMEditor/Views/CampaignWindow.xaml.cs (2026-04-05) — Avalonia UI; file dialogs / editors TODO-Avalonia-Reenable.
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using JiMEditor.Avalonia;
using JiME;
using Newtonsoft.Json;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace JiMEditor.Avalonia.Views;

public partial class CampaignWindow : Window
{
	public Campaign campaign { get; private set; } = null!;

	string _campaignFolder = "";
	bool _controlDown;

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
			Title = "Campaign Manager - New Campaign";
			CancelFooterButton.IsVisible = true;
			DeleteCampaignButton.IsVisible = false;
		}
		else
		{
			Title = string.IsNullOrWhiteSpace( c.campaignName )
				? "Campaign Manager - Edit Campaign"
				: "Campaign Manager - Edit Campaign - " + c.campaignName;
			CancelFooterButton.IsVisible = false;
			DeleteCampaignButton.IsVisible = true;
		}

		SaveCampaign();
	}

	void CampaignWindow_Opened( object? sender, EventArgs e )
	{
		CampaignGuidTextBlock.Text = "Campaign GUID:  " + campaign.campaignGUID;
		StoryStatusTextBlock.Text = string.IsNullOrWhiteSpace( campaign.storyText )
			? "The Story Text is not set."
			: "The Story Text is set.";
		if ( TriggersCombo.Items.Count > 0 )
			TriggersCombo.SelectedIndex = 0;
		CampaignNameTextBox?.SelectAll();
	}

	void CampaignWindow_KeyDown( object? sender, KeyEventArgs e )
	{
		if ( e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl )
			_controlDown = true;
	}

	void CampaignWindow_KeyUp( object? sender, KeyEventArgs e )
	{
		if ( e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl )
			_controlDown = false;
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
		var pw = new ProjectWindow();
		if ( global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
			desktop.MainWindow = pw;
		pw.Show();
		Close();
	}

	void Cancel_Click( object? sender, RoutedEventArgs e )
	{
		var di = new DirectoryInfo( _campaignFolder );
		if ( di.Exists )
		{
			foreach ( var fi in di.GetFiles().Where( f => f.Extension == ".jime" ) )
			{
				var s = FileManager.LoadProjectFromPath( _campaignFolder, fi.Name );
				if ( s == null )
					continue;
				s.campaignGUID = Guid.Empty;
				var fm = new FileManager( s );
				fm.Save();
			}

			if ( Directory.Exists( _campaignFolder ) )
				Directory.Delete( _campaignFolder, true );
		}

		var pw = new ProjectWindow();
		if ( global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
			desktop.MainWindow = pw;
		pw.Show();
		Close();
	}
}
