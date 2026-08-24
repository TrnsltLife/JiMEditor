using Avalonia.Controls;
using Avalonia.Interactivity;
using JiMEditor.Avalonia;
using JiME.TextFormat;

namespace JiMEditor.Avalonia.Views;

public partial class StoryTextEditWindow : Window
{
	public bool Accepted { get; private set; }

	public string StoryText { get; private set; } = "";

	public StoryTextEditWindow()
	{
		InitializeComponent();
		Loaded += ( _, _ ) => ScenarioDirty.WatchInputs( this );
	}

	public StoryTextEditWindow( string? initialStoryText, string? watermark = null, string? windowTitle = null )
		: this()
	{
		Title = string.IsNullOrWhiteSpace( windowTitle ) ? "Edit Text" : windowTitle;
		Editor.Text = initialStoryText ?? "";
		Editor.Watermark = watermark ?? "";
	}

	void Ok_Click( object? sender, RoutedEventArgs e )
	{
		Accepted = true;
		StoryText = JiMePseudoHtmlFormat.NormalizePseudoHtml( Editor.Text.Trim() );
		Close();
	}

	void Cancel_Click( object? sender, RoutedEventArgs e )
	{
		Accepted = false;
		Close();
	}
}
