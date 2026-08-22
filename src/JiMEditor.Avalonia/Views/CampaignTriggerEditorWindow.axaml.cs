// Ported from: JiMEditor/Views/CampaignTriggerEditor.xaml.cs (2026-04-06) — Avalonia UI.
using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiME;

namespace JiMEditor.Avalonia.Views;

public partial class CampaignTriggerEditorWindow : Window
{
	Campaign _campaign = null!;

	public bool Accepted { get; private set; }

	/// <summary>Full trigger name including <c> [CMPN]</c> suffix, set when <see cref="Accepted"/> is true.</summary>
	public string ResultTriggerName { get; private set; } = "";

	public bool IsMultiTrigger { get; private set; }

	public CampaignTriggerEditorWindow()
	{
		InitializeComponent();
	}

	public CampaignTriggerEditorWindow( Campaign campaign ) : this()
	{
		_campaign = campaign;
		NameTextBox.Text = "";
	}

	void Window_Opened( object? sender, EventArgs e )
	{
		NameTextBox.Focus();
		NameTextBox.SelectAll();
	}

	void OkButton_Click( object? sender, RoutedEventArgs e )
	{
		var raw = NameTextBox.Text?.Trim() ?? "";
		if ( raw.Length == 0 )
		{
			MessageBox.Show( "The name can't be an empty string.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
			return;
		}

		var withSuffix = raw + " [CMPN]";
		if ( _campaign.triggerCollection.Any( x => x.dataName == withSuffix ) )
		{
			MessageBox.Show( "A Campaign Trigger with this name already exists.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
			return;
		}

		ResultTriggerName = withSuffix;
		IsMultiTrigger = MultiCheckBox.IsChecked == true;
		Accepted = true;
		Close();
	}

	void CancelButton_Click( object? sender, RoutedEventArgs e )
	{
		Accepted = false;
		Close();
	}

	async void Help_Click( object? sender, RoutedEventArgs e )
	{
		await HelpWindow.ShowDialogAsync( this, HelpType.Triggers, 3 );
	}
}
