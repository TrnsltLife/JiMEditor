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
	public partial class MonsterActivationItemEditorWindow : Window
	{
		public Scenario scenario { get; set; }
		public MonsterActivations activations { get; set; }
		public MonsterActivationItem activationItem { get; set; }
		public string shortName { get; set; }
		private string originalText;
		private string originalEffect;

		public MonsterActivationItemEditorWindow( Scenario s, MonsterActivations act, MonsterActivationItem actItem, bool isNew = true )
		{
			InitializeComponent();
			DataContext = this;

			scenario = s;
			activations = act;
			activationItem = actItem;

			cancelButton.Visibility = isNew ? Visibility.Visible : Visibility.Collapsed;
			originalText = actItem.text; //use this to keep track if text has changed during editing
			originalEffect = actItem.effect;
		}

		private void OkButton_Click( object sender, RoutedEventArgs e )
		{
			activationItem.text = textRTB.Text;
			activationItem.effect = effectRTB.Text;

			//check empty string
			if ( string.IsNullOrEmpty( activationItem.text ) )
			{
				MessageBox.Show( "The Attack Text cannot be empty.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
				return;
			}

			if (activationItem.text != originalText)
			{
				// Notify that the text property has changed
				activationItem.NotifyPropertyChanged(nameof(activationItem.text));
				activationItem.NotifyPropertyChanged(nameof(activationItem.TextFlowDocument));
			}

			if (activationItem.effect != originalEffect)
			{
				// Notify that the text property has changed
				activationItem.NotifyPropertyChanged(nameof(activationItem.effect));
				activationItem.NotifyPropertyChanged(nameof(activationItem.EffectFlowDocument));
			}


			//check if trigger isn't set
			//if ( ( (Trigger)triggerCB.SelectedItem ).dataName == "None"
			//	|| ( (Trigger)triggerCB.SelectedItem ).dataName.Contains( "Random" ) )
			//{
			//	MessageBox.Show( "You must select a Trigger.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
			//	return;
			//}

			//objective.triggerName = ( (Trigger)triggerCB.SelectedItem ).dataName;
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
