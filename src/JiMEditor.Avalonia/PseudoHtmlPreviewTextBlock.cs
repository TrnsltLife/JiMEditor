using Avalonia;
using Avalonia.Controls;
using JiME.TextFormat;

namespace JiMEditor.Avalonia;

/// <summary>
/// Renders JiME pseudo-HTML (<b>, <i>, <font="Icon">) into Avalonia TextBlock inlines.
/// </summary>
public class PseudoHtmlPreviewTextBlock : TextBlock
{
	public static readonly StyledProperty<string?> PseudoHtmlProperty =
		AvaloniaProperty.Register<PseudoHtmlPreviewTextBlock, string?>( nameof( PseudoHtml ) );

	public string? PseudoHtml
	{
		get => GetValue( PseudoHtmlProperty );
		set => SetValue( PseudoHtmlProperty, value );
	}

	protected override void OnPropertyChanged( AvaloniaPropertyChangedEventArgs change )
	{
		base.OnPropertyChanged( change );
		if ( change.Property == PseudoHtmlProperty || change.Property == ForegroundProperty || change.Property == FontFamilyProperty )
			RenderPreview();
	}

	void RenderPreview()
	{
		var doc = JiMePseudoHtmlFormat.Parse( PseudoHtml );
		JiMeAvaloniaRichPreview.Apply( this, doc, FontFamily );
	}
}
