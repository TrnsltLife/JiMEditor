using Avalonia.Controls;
using Avalonia.Interactivity;
using JiME.TextFormat;

namespace JiMEditor.Avalonia.Views;

public partial class ObjectiveSummaryEditWindow : Window
{
	public bool Accepted { get; private set; }

	public string SummaryText { get; private set; } = "";

	public ObjectiveSummaryEditWindow()
	{
		InitializeComponent();
	}

	public ObjectiveSummaryEditWindow( string? initialPageText )
		: this()
	{
		Editor.Text = initialPageText ?? "";
	}

	void Ok_Click( object? sender, RoutedEventArgs e )
	{
		Accepted = true;
		SummaryText = JiMePseudoHtmlFormat.NormalizePseudoHtml( Editor.Text.Trim() );
		Close();
	}

	void Cancel_Click( object? sender, RoutedEventArgs e )
	{
		Accepted = false;
		Close();
	}
}
