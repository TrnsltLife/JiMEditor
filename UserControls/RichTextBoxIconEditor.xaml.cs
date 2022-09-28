using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Documents;
using System.IO;
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
			var caretPos = iconRTB.CaretPosition;
			//caretPos.InsertTextInRun(button.Content as string);

			var selection = iconRTB.Selection;
			selection.Text = " ";
			//caretPos.DeleteTextInRun(selection.Start.GetOffsetToPosition(selection.End));

			//Span mySpan = new Span(selection.Start, selection.End);
			/*
			Span mySpan = new Span(new Run(button.Content as string), caretPos);
			mySpan.FontFamily = new FontFamily("LoTR JiME Icons");
			mySpan.Background = Brushes.LightGray;
			*/
			//Span mySpan = new Span(new Run(button.Content as string), caretPos);
			Run myRun = new Run(button.Content as string, caretPos);
			myRun.FontFamily = new FontFamily("LoTR JiME Icons");
			myRun.Background = Brushes.LightGray;

			//iconRTB.CaretPosition = iconRTB.CaretPosition.GetNextInsertionPosition(LogicalDirection.Forward); //move caret forward one position?
			iconRTB.Focus();
			//selection.Select(caretPos, caretPos.GetNextInsertionPosition(LogicalDirection.Forward));
			//EditingCommands.Delete
			//EditingCommands.ToggleBold.Execute(null, iconRTB);

			//Bold myBold = new Bold(new Run(button.Content as string), caretPos);
			//myBold.FontFamily = new FontFamily("LoTR JiME Icons");

			//Span mySpan = new Span(new Run(button.Content as string), caretPos);
			//mySpan.FontFamily = new FontFamily("LoTR JiME Icons");
		}

		private void ClearFormattingButton_Click(object sender, RoutedEventArgs e)
		{
			var selection = iconRTB.Selection;
			selection.ClearAllProperties();
			selection.ClearAllProperties();
			selection.ClearAllProperties();
			iconRTB.Focus();
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
				string s = sb.ToString();
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
				return s;
			}
		}

		private void SetRichTextBoxFromSimpleHtml(string html)
        {
			FlowDocument document = iconRTB.Document;
			document.Blocks.Clear();
			html = Regex.Replace(html, "\r\n", "</Paragraph><Paragraph>");
			html = Regex.Replace(html, "<b>", "<Run FontFamily=\"LoTR JiME Icons\" Background=\"#FFD3D3D3\">");
			html = Regex.Replace(html, "</b>", "</Run>");
			html = "<Paragraph>" + html + "</Paragraph>";
			html = "<?xml version=\"1.0\" encoding=\"utf-16\"?><FlowDocument PagePadding=\"5,0,5,0\" AllowDrop=\"True\" NumberSubstitution.CultureSource=\"User\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" + html + "</FlowDocument>";
			Console.WriteLine("html:\r\n" + html);
			iconRTB.Document = (FlowDocument)XamlReader.Parse(html);
        }


		private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = false;
			e.Handled = true;
		}
	}
}
