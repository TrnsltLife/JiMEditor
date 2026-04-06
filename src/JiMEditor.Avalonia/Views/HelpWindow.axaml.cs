// Ported from: JiMEditor/Views/HelpWindow.xaml.cs (2026-04-05) — Avalonia; same HelpType + tab index API as WPF for callers.
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiME;

namespace JiMEditor.Avalonia.Views;

public partial class HelpWindow : Window
{
	/// <summary>Same <paramref name="helpType"/> / tab order as WPF <c>HelpWindow</c>.</summary>
	public static async Task ShowDialogAsync( Window owner, HelpType helpType, int tabIndex = 0 )
	{
		var w = new HelpWindow( helpType, tabIndex );
		await w.ShowDialog( owner );
	}

	public HelpWindow()
	{
		InitializeComponent();
	}

	public HelpWindow( HelpType helpType, int tab = 0 ) : this()
	{
		if ( helpType == HelpType.Token )
		{
			Title = "Help On Tokens and Events";
			tokenHelp.IsVisible = true;
			tokenHelp.SelectedIndex = ClampTabIndex( tokenHelp, tab );
		}
		else if ( helpType == HelpType.Grouping )
		{
			Title = "Help On Token Interaction Groups";
			groupHelp.IsVisible = true;
			groupHelp.SelectedIndex = ClampTabIndex( groupHelp, tab );
		}
		else if ( helpType == HelpType.Enemies )
		{
			Title = "Help On Enemy Damage and Difficulty";
			threatHelp.IsVisible = true;
			threatHelp.SelectedIndex = ClampTabIndex( threatHelp, tab );
		}
		else if ( helpType == HelpType.Triggers )
		{
			Title = "Help On Triggers";
			triggerHelp.IsVisible = true;
			triggerHelp.SelectedIndex = ClampTabIndex( triggerHelp, tab );
		}
	}

	static int ClampTabIndex( TabControl tc, int tab )
	{
		var n = tc.Items?.Count ?? 0;
		if ( n <= 0 )
			return 0;
		if ( tab < 0 )
			return 0;
		if ( tab >= n )
			return n - 1;
		return tab;
	}

	void okButton_Click( object? sender, RoutedEventArgs e ) => Close();
}
