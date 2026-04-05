// Ported from: JiMEditor/UserControls/RichTextBoxIconEditor.xaml.cs — static HTML→FlowDocument only (2026-04-04). Full WPF control not ported.

using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;

namespace JiME.UserControls;

/// <summary>Subset of WPF RichTextBoxIconEditor for model-layer properties that expose FlowDocument.</summary>
public static class RichTextBoxIconEditor
{
	public static FlowDocument? CreateFlowDocumentFromSimpleHtml(string html, string paragraphReplacement = " ", string paragraphAttributes = "")
	{
		string? originalHtml = html;
		if (html == null)
			return null;

		html = Regex.Replace(html, "\r\n\r\n", paragraphReplacement);

		html = Regex.Replace(html, "<font=\"Icon\"><b><i>(.*?)</i></b></font>", "<Run FontFamily=\"LoTR JiME Icons\" FontStyle=\"Italic\" FontWeight=\"Bold\" Background=\"#FFD3D3D3\" xml:space=\"preserve\">$1</Run>");
		html = Regex.Replace(html, "<font=\"Icon\"><b>(.*?)</b></font>", "<Run FontFamily=\"LoTR JiME Icons\" FontWeight=\"Bold\" Background=\"#FFD3D3D3\" xml:space=\"preserve\">$1</Run>");
		html = Regex.Replace(html, "<font=\"Icon\"><i>(.*?)</i></font>", "<Run FontFamily=\"LoTR JiME Icons\" FontStyle=\"Italic\" Background=\"#FFD3D3D3\" xml:space=\"preserve\">$1</Run>");
		html = Regex.Replace(html, "<font=\"Icon\">(.*?)</font>", "<Run FontFamily=\"LoTR JiME Icons\" Background=\"#FFD3D3D3\" xml:space=\"preserve\">$1</Run>");
		html = Regex.Replace(html, "<b><i>(.*?)</i></b>", "<Run FontStyle=\"Italic\" FontWeight=\"Bold\" xml:space=\"preserve\">$1</Run>");
		html = Regex.Replace(html, "<b>(.*?)</b>", "<Run FontWeight=\"Bold\" xml:space=\"preserve\">$1</Run>");
		html = Regex.Replace(html, "<i>(.*?)</i>", "<Run FontStyle=\"Italic\" xml:space=\"preserve\">$1</Run>");

		html = "<Paragraph " + paragraphAttributes + " xml:space=\"preserve\">" + html + "</Paragraph>";
		html = "<?xml version=\"1.0\" encoding=\"utf-16\"?><FlowDocument PagePadding=\"5,0,5,0\" AllowDrop=\"True\" NumberSubstitution.CultureSource=\"User\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" + html + "</FlowDocument>";

		try
		{
			return (FlowDocument)XamlReader.Parse(html);
		}
		catch (Exception ex)
		{
			Debug.Log("Problem converting simple html: " + ex.Message);
			Debug.Log("original: " + originalHtml);
			Debug.Log("transform " + html);
		}

		return null;
	}
}
