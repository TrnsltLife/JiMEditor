using System.Windows;

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

		public MonsterActivationItemEditorWindow( Scenario s, MonsterActivations act, MonsterActivationItem actItem, bool isNew = true )
		{
			InitializeComponent();
			DataContext = this;

			scenario = s;
			activations = act;
			activationItem = actItem;

			cancelButton.Visibility = isNew ? Visibility.Visible : Visibility.Collapsed;
		}

		private void OkButton_Click( object sender, RoutedEventArgs e )
		{
			//check empty string
			if ( string.IsNullOrEmpty( activationItem.text ) )
			{
				MessageBox.Show( "The Attack Text cannot be empty.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
				return;
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
			textTB.Focus();
		}
	}
}
