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
					Console.WriteLine(sb.ToString());
					sb.Replace("</Run>", "</b>");
					sb.Replace("</Paragraph>", "\r\n");
					sb.Replace("</FlowDocument>", "");
					s = sb.ToString();
					s = Regex.Replace(s, @"<\?xml[^>]*>", "");
					s = Regex.Replace(s, @"<FlowDocument[^>]*>", "");
					s = Regex.Replace(s, "<Paragraph[^>]*>", "");
					s = Regex.Replace(s, "<Run FontFamily=\"LoTR JiME Icons\"[^>]*>", "<b>");
					s = Regex.Replace(s, @"<Run[^>]*>", "");
					s = Regex.Replace(s, @"<Span[^>]*>", "");
					s = Regex.Replace(s, @"</Span[^>]*>", "");
					s = Regex.Replace(s, @"<List[^>]*>", "");
					s = Regex.Replace(s, @"</List[^>]*>", "");
					s = Regex.Replace(s, @"<TextDecoration[^>]*>", "");
					s = Regex.Replace(s, @"</TextDecoration>", "");
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
			html = Regex.Replace(html, "\r\n", "</Paragraph><Paragraph>");
			html = Regex.Replace(html, "<b>", "<Run FontFamily=\"LoTR JiME Icons\" Background=\"#FFD3D3D3\">");
			html = Regex.Replace(html, "</b>", "</Run>");
			html = "<Paragraph>" + html + "</Paragraph>";
			html = "<?xml version=\"1.0\" encoding=\"utf-16\"?><FlowDocument PagePadding=\"5,0,5,0\" AllowDrop=\"True\" NumberSubstitution.CultureSource=\"User\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" + html + "</FlowDocument>";
			Console.WriteLine("html:\r\n" + html);
			try
			{
				iconRTB.Document = (FlowDocument)XamlReader.Parse(html);
			}
			catch
            {
				System.Windows.Forms.MessageBox.Show("An error occurred while trying to load your text. The underlying HTML may have been corrupted and you may need to fix it in the .jime file. Copy it with Ctrl+C. Here's what it looks like:\r\n\r\n" + originalHtml,
					"Error Loading Text",
					System.Windows.Forms.MessageBoxButtons.OK,
					System.Windows.Forms.MessageBoxIcon.Error);
			}
		}

		public static FlowDocument CreateFlowDocumentFromSimpleHtml(string html, string paragraphReplacement="\r\n", string paragraphAttributes="")
		{
			string originalHtml = html;
			if (html == null) { return null; }
			//Create a FlowDocument from the simple HTML in order to show the icons in the detail view. However, just transform internal paragraphs into spaces.
			//html = Regex.Replace(html, "\r\n", "</Paragraph><Paragraph>");
			html = Regex.Replace(html, "\r\n", paragraphReplacement);
			html = Regex.Replace(html, "<b>", "<Run FontFamily=\"LoTR JiME Icons\">");
			html = Regex.Replace(html, "</b>", "</Run>");
			html = "<Paragraph " + paragraphAttributes + ">" + html + "</Paragraph>";
			html = "<?xml version=\"1.0\" encoding=\"utf-16\"?><FlowDocument PagePadding=\"5,0,5,0\" AllowDrop=\"True\" NumberSubstitution.CultureSource=\"User\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" + html + "</FlowDocument>";

			try
			{
				return (FlowDocument)XamlReader.Parse(html);
			}
			catch
			{
				System.Windows.Forms.MessageBox.Show("An error occurred while trying to load your text. The underlying HTML may have been corrupted and you may need to fix it in the .jime file. Copy it with Ctrl+C. Here's what it looks like:\r\n\r\n" + originalHtml,
					"Error Loading Text",
					System.Windows.Forms.MessageBoxButtons.OK,
					System.Windows.Forms.MessageBoxIcon.Error);
			}
			return null;
		}


		private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = false;
			e.Handled = true;
		}
	}
}
