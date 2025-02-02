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

namespace JiME.Views
{
	/// <summary>
	/// Interaction logic for ObjectiveWindow.xaml
	/// </summary>
	public partial class TranslationItemEditorWindow : Window
	{
		public TranslationItem translationItem { get; set; }
		public TranslationItem defaultTranslation { get; set; }

		private string originalText;

		public TranslationItemEditorWindow(TranslationItem translationItem, TranslationItem defaultTranslation)
		{
			InitializeComponent();
			DataContext = this;

			this.translationItem = translationItem;
			this.defaultTranslation = defaultTranslation;
			originalText = translationItem.text; //use this to keep track if text has changed during editing
		}

		private void OkButton_Click( object sender, RoutedEventArgs e )
		{
			translationItem.text = translationRTB.Text;

			//check empty string
			if ( string.IsNullOrEmpty(translationItem.text ) )
			{
				//MessageBox.Show( "The translation text cannot be empty.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
				//return;
			}
			//Console.WriteLine("OKButton_Click " + translationItem.text);

			//If the text has changed during editing, set the flag to true.
			if (translationItem.text != originalText)
            {
				    // Notify that the text property has changed
			    translationItem.NotifyPropertyChanged(nameof(translationItem.text));
				translationItem.NotifyPropertyChanged(nameof(translationItem.TextFlowDocument));
				translationItem.NotifyPropertyChanged(nameof(translationItem.Background));
				translationItem.updatedWhileEditing = true;
            }
			DialogResult = true;
		}

		private void CancelButton_Click( object sender, RoutedEventArgs e )
		{
			DialogResult = false;
		}

		private void Window_ContentRendered( object sender, System.EventArgs e )
		{
			textRTB.Focus();
		}
	}
}
