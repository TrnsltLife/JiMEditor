// Ported from: JiMEditor/Views/TriggerEditorWindow.xaml.cs (2026-04-06) — Avalonia UI.
using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiME;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace JiMEditor.Avalonia.Views;

public partial class TriggerEditorWindow : Window
{
	Scenario _scenario = null!;
	string _oldName = "";
	bool _isNew;

	/// <summary>True when OK applied a new trigger or rename (WPF <c>DialogResult == true</c>); false for cancel or multi-only tweak.</summary>
	public bool Accepted { get; private set; }

	public TriggerEditorWindow()
	{
		InitializeComponent();
	}

	/// <summary>Edit or rename an existing trigger (<paramref name="editName"/> is current <see cref="Trigger.dataName"/>).</summary>
	public TriggerEditorWindow( Scenario s, string editName ) : this()
	{
		_scenario = s;
		NameTextBox.Text = editName;
		_oldName = editName;
		if ( editName.Length > 0 )
		{
			Title = "Trigger Editor - Rename Trigger";
			NameLabel.Text = "Trigger Name (Rename):";
		}

		MultiCheckBox.IsChecked = false;
		CampaignCheckBox.IsChecked = false;

		if ( s.projectType == ProjectType.Standalone )
			CampaignCheckBox.IsVisible = false;

		var existing = s.triggersObserver.FirstOrDefault( x => x.dataName == editName );
		if ( existing != null )
		{
			MultiCheckBox.IsChecked = existing.isMultiTrigger;
			CampaignCheckBox.IsChecked = existing.isCampaignTrigger;
			if ( CampaignCheckBox.IsChecked == true )
			{
				NameTextBox.IsEnabled = false;
				MultiCheckBox.IsEnabled = false;
			}
		}
	}

	/// <summary>Create a new trigger.</summary>
	public TriggerEditorWindow( Scenario s ) : this( s, "" )
	{
		_isNew = true;
	}

	/// <summary>Duplicate: pre-filled from <paramref name="trigger"/> (user may rename); OK runs <see cref="Scenario.AddTrigger"/>.</summary>
	public TriggerEditorWindow( Scenario s, JiME.Trigger trigger, bool showCancelButtonIgnored = false ) : this()
	{
		_scenario = s;
		_isNew = true;
		_oldName = "";
		NameTextBox.Text = trigger.dataName;
		MultiCheckBox.IsChecked = trigger.isMultiTrigger;
		CampaignCheckBox.IsChecked = trigger.isCampaignTrigger;

		if ( s.projectType == ProjectType.Standalone )
			CampaignCheckBox.IsVisible = false;

		if ( CampaignCheckBox.IsChecked == true )
		{
			NameTextBox.IsEnabled = false;
			MultiCheckBox.IsEnabled = false;
		}
	}

	void Window_Opened( object? sender, EventArgs e )
	{
		NameTextBox.Focus();
		NameTextBox.SelectAll();
	}

	void OkButton_Click( object? sender, RoutedEventArgs e )
	{
		var triggerName = NameTextBox.Text?.Trim() ?? "";

		// Same name: only update Multi-Trigger (existing row). Skip when _oldName is empty (new trigger, empty name handled below).
		if ( _oldName == triggerName && !string.IsNullOrEmpty( _oldName ) )
		{
			var t = _scenario.triggersObserver.FirstOrDefault( x => x.dataName == _oldName );
			if ( t != null )
				t.isMultiTrigger = MultiCheckBox.IsChecked == true;
			_scenario.TriggerTitleChange( true );
			Accepted = false;
			Close();
			return;
		}

		if ( triggerName.Length == 0 )
		{
			MessageBox.Show( "The Trigger Name cannot be empty.", "Invalid Trigger Name", MessageBoxButton.OK, MessageBoxImage.Error );
			NameTextBox.Focus();
			return;
		}

		if ( !_isNew )
		{
			if ( _scenario.RenameTrigger( _oldName, triggerName, MultiCheckBox.IsChecked == true ) )
			{
				_scenario.TriggerTitleChange( true );
				Accepted = true;
				Close();
			}

			return;
		}

		if ( _scenario.AddTrigger( triggerName, MultiCheckBox.IsChecked == true ) )
		{
			_scenario.TriggerTitleChange( true );
			Accepted = true;
			Close();
		}
		else
		{
			MessageBox.Show( "A Trigger with this name already exists.", "Invalid Trigger Name", MessageBoxButton.OK, MessageBoxImage.Error );
			NameTextBox.Focus();
		}
	}

	void CancelButton_Click( object? sender, RoutedEventArgs e )
	{
		Accepted = false;
		Close();
	}

	async void Help_Click( object? sender, RoutedEventArgs e ) =>
		await HelpWindow.ShowDialogAsync( this, HelpType.Triggers, 0 );
}
