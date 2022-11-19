using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Documents;
using System.Text;
using System;
using System.Xml;
using System.Windows.Markup;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Windows.Input;

namespace JiME.UserControls
{
	/// <summary>
	/// Interaction logic for SidebarListView.xaml
	/// </summary>
	public partial class RichTextBoxIconEditor : UserControl, INotifyPropertyChanged
	{
		public static readonly DependencyProperty DocumentProperty =
			DependencyProperty.Register("Text", typeof(string),
			typeof(RichTextBoxIconEditor), new FrameworkPropertyMetadata
			(null, new PropertyChangedCallback(OnTextChanged)));

		public RichTextBoxIconEditor()
		{
			InitializeComponent();
			//DataContext = this;
		}

		public string Text
		{
			get => GetRichTextBoxSimpleHtml();
			set
			{
				SetRichTextBoxFromSimpleHtml(value);
				PropChanged("Text");
			}
		}

		public double MinTextHeight
		{
			get => iconRTB.MinHeight;
			set
			{
				iconRTB.MinHeight = value;
			}
		}

		public double TextHeight
        {
			get => iconRTB.Height;
			set
            {
				iconRTB.Height = value;
            }
        }

		public double MaxTextHeight
		{
			get => iconRTB.MaxHeight;
			set
			{
				iconRTB.MaxHeight = value;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		void PropChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		public static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			RichTextBoxIconEditor editor = (RichTextBoxIconEditor)obj;
			editor.Text = (string)args.NewValue;
		}


		private void InsertButton_Click(object sender, RoutedEventArgs e)
		{
			Button button = sender as Button;

			iconRTB.BeginChange();

			var caretPos = iconRTB.CaretPosition;

			var selection = iconRTB.Selection;
			selection.Text = " ";

			Run myRun = new Run(button.Content as string, caretPos);
			myRun.FontFamily = new FontFamily("LoTR JiME Icons");
			myRun.Background = Brushes.LightGray;

			//iconRTB.CaretPosition = iconRTB.CaretPosition.GetNextInsertionPosition(LogicalDirection.Forward); //move caret forward one position?
			iconRTB.Focus();
			var nextPos = caretPos.GetNextInsertionPosition(LogicalDirection.Forward);
			selection.Select(nextPos, nextPos);

			iconRTB.EndChange();
		}

		private void ClearFormattingButton_Click(object sender, RoutedEventArgs e)
		{
			iconRTB.BeginChange();
			var selection = iconRTB.Selection;
			selection.ClearAllProperties();
			selection.ClearAllProperties();
			selection.ClearAllProperties();
			iconRTB.Focus();
			iconRTB.EndChange();
		}

		private void OutputButton_Click(object sender, RoutedEventArgs e)
		{
			Console.WriteLine(GetRichTextBoxSimpleHtml());
		}

		private string GetRichTextBoxSimpleHtml()
		{
			FlowDocument document = iconRTB.Document;
			if (document == null) return String.Empty;
			else
			{
				string s = "";

				try
				{
					StringBuilder sb = new StringBuilder();
					using (XmlWriter xw = XmlWriter.Create(sb))
					{
						XamlDesignerSerializationManager sm = new XamlDesignerSerializationManager(xw);
						sm.XamlWriterMode = XamlWriterMode.Expression;

						XamlWriter.Save(document, sm);
					}
					//sb.Replace("{}", "");
					//Console.WriteLine(sb.ToString());
					//sb.Replace("</Run>", "</font>");
					sb.Replace("</Paragraph>", "\r\n\r\n");
					sb.Replace("</FlowDocument>", "");
					s = sb.ToString();
					s = Regex.Replace(s, @"<\?xml[^>]*>", "");
					s = Regex.Replace(s, @"<FlowDocument[^>]*>", "");
					s = Regex.Replace(s, "<Paragraph[^>]*>", "");

					//icon font, bold, and italic
					s = Regex.Replace(s, "<Run FontFamily=\"LoTR JiME Icons\"[^>]*FontStyle=\"Italic\"[^>]*FontWeight=\"Bold\"[^>]*>(.*?)</Run>", "<font=\"Icon\"><b><i>$1</i></b></font>");

					//icon font and italic
					s = Regex.Replace(s, "<Run FontFamily=\"LoTR JiME Icons\"[^>]*FontStyle=\"Italic\"[^>]*>(.*?)</Run>", "<font=\"Icon\"><i>$1</i></font>");

					//icon font and bold
					s = Regex.Replace(s, "<Run FontFamily=\"LoTR JiME Icons\"[^>]*FontWeight=\"Bold\"[^>]*>(.*?)</Run>", "<font=\"Icon\"><b>$1</b></font>");

					//icon font
					s = Regex.Replace(s, "<Run FontFamily=\"LoTR JiME Icons\"[^>]*>(.*?)</Run>", "<font=\"Icon\">$1</font>");

					//bold and italic
					s = Regex.Replace(s, "<Run[^>]*FontStyle=\"Italic\"[^>]*FontWeight=\"Bold\"[^>]*>(.*?)</Run>", "<b><i>$1</i></b>");

					//bold
					s = Regex.Replace(s, "<Run[^>]*FontWeight=\"Bold\"[^>]*>(.*?)</Run>", "<b>$1</b>");

					//italic
					s = Regex.Replace(s, "<Run[^>]*FontStyle=\"Italic\"[^>]*>(.*?)</Run>", "<i>$1</i>");

					//any other Run, Span, List, TextDecoration
					s = Regex.Replace(s, @"<Run[^>]*>", "");
					s = Regex.Replace(s, @"<Span[^>]*>", "");
					s = Regex.Replace(s, @"</Span[^>]*>", "");
					s = Regex.Replace(s, @"<List[^>]*>", "");
					s = Regex.Replace(s, @"</List[^>]*>", "");
					s = Regex.Replace(s, @"<(Run\.)?TextDecoration[^>]*?>", "");
					s = Regex.Replace(s, @"</(Run\.)?TextDecoration>", "");
				}
				catch
                {
					s = document.ToString();
                    System.Windows.Forms.MessageBox.Show("An error occurred while trying to save your text. This is what we were able to salvage. Copy it with Ctrl+C or it will be lost.\r\n\r\n" + s,
						"Error Saving Text",
						System.Windows.Forms.MessageBoxButtons.OK,
						System.Windows.Forms.MessageBoxIcon.Error);
				}
				return s;
			}
		}

		private void SetRichTextBoxFromSimpleHtml(string html)
        {
			string originalHtml = html;
			FlowDocument document = iconRTB.Document;
			document.Blocks.Clear();
			iconRTB.Document = RichTextBoxIconEditor.CreateFlowDocumentFromSimpleHtml(html, "</Paragraph><Paragraph xml:space=\"preserve\">");
		}

		public static FlowDocument CreateFlowDocumentFromSimpleHtml(string html, string paragraphReplacement=" ", string paragraphAttributes="")
		{
			string originalHtml = html;
			if (html == null) { return null; }
			//Create a FlowDocument from the simple HTML in order to show the icons in the detail view. However, just transform internal paragraphs into spaces.
			//html = Regex.Replace(html, "\r\n", "</Paragraph><Paragraph>");
			html = Regex.Replace(html, "\r\n\r\n", paragraphReplacement);

			//icon font, bold and italic
			html = Regex.Replace(html, "<font=\"Icon\"><b><i>(.*?)</i></b></font>", "<Run FontFamily=\"LoTR JiME Icons\" FontStyle=\"Italic\" FontWeight=\"Bold\" Background=\"#FFD3D3D3\" xml:space=\"preserve\">$1</Run>");

			//icon font and bold
			html = Regex.Replace(html, "<font=\"Icon\"><b>(.*?)</b></font>", "<Run FontFamily=\"LoTR JiME Icons\" FontWeight=\"Bold\" Background=\"#FFD3D3D3\" xml:space=\"preserve\">$1</Run>");

			//icon font and italic
			html = Regex.Replace(html, "<font=\"Icon\"><i>(.*?)</i></font>", "<Run FontFamily=\"LoTR JiME Icons\" FontStyle=\"Italic\" Background=\"#FFD3D3D3\" xml:space=\"preserve\">$1</Run>");

			//icon font
			html = Regex.Replace(html, "<font=\"Icon\">(.*?)</font>", "<Run FontFamily=\"LoTR JiME Icons\" Background=\"#FFD3D3D3\" xml:space=\"preserve\">$1</Run>");

			//bold and italic
			html = Regex.Replace(html, "<b><i>(.*?)</i></b>", "<Run FontStyle=\"Italic\" FontWeight=\"Bold\" xml:space=\"preserve\">$1</Run>");

			//bold
			html = Regex.Replace(html, "<b>(.*?)</b>", "<Run FontWeight=\"Bold\" xml:space=\"preserve\">$1</Run>");

			//italic
			html = Regex.Replace(html, "<i>(.*?)</i>", "<Run FontStyle=\"Italic\" xml:space=\"preserve\">$1</Run>");

			html = "<Paragraph " + paragraphAttributes + " xml:space=\"preserve\">" + html + "</Paragraph>";
			html = "<?xml version=\"1.0\" encoding=\"utf-16\"?><FlowDocument PagePadding=\"5,0,5,0\" AllowDrop=\"True\" NumberSubstitution.CultureSource=\"User\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" + html + "</FlowDocument>";

			try
			{
				return (FlowDocument)XamlReader.Parse(html);
			}
			catch
			{
				//Debug.Log("Problem converting simple html");
				//Debug.Log("original: " + originalHtml);
				//Debug.Log("transform " + html);
				System.Windows.Forms.MessageBox.Show("An error occurred while trying to load your text. The underlying HTML may have been corrupted and you may need to fix it in the .jime file. Copy it with Ctrl+C. Here's what it looks like:\r\n\r\n" + originalHtml,
					"Error Loading Text",
					System.Windows.Forms.MessageBoxButtons.OK,
					System.Windows.Forms.MessageBoxIcon.Error);
			}
			return null;
		}


		private void CommandBinding_CannotExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = false;
			e.Handled = true;
		}
		private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
			e.Handled = true;
		}
	}
}
