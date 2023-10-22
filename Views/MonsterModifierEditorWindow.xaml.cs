using System;
using System.Linq;
using System.Windows;

namespace JiME.Views
{
	/// <summary>
	/// Interaction logic for MonsterModifierEditorWindow.xaml
	/// </summary>
	public partial class MonsterModifierEditorWindow : Window
	{
		public Scenario scenario { get; set; }
		public MonsterModifier modifier { get; set; }
		public string shortName { get; set; }

		public MonsterModifierEditorWindow( Scenario s, MonsterModifier mod = null, bool isNew = true )
		{
			InitializeComponent();
			DataContext = this;

			scenario = s;

			//Get next id starting at 1000 to create the new item
			int maxId = scenario.monsterModifierObserver.Max(a => a.id);
			int newId = Math.Max(maxId + 1, MonsterModifier.START_OF_CUSTOM_MODIFIERS); //Get the next id over 1000

			modifier = mod ?? new MonsterModifier(newId);

			shortName = modifier.name;

			if ( !isNew )
				cancelButton.Visibility = Visibility.Collapsed;
		}

		private void OkButton_Click( object sender, RoutedEventArgs e )
		{
			modifier.name = shortName.Trim();

			//check empty string
			if ( string.IsNullOrEmpty(modifier.name) )
			{
				MessageBox.Show( "The Monster Bonus Name.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
				return;
			}

			if (string.IsNullOrEmpty(modifier.dataName))
            {
				modifier.dataName = modifier.name;
            }

			//check if dataName is duplicated
			if ( scenario.IsDuplicate( modifier ) )//ret != null )
			{
				MessageBox.Show( $"A Monster Bonus with In-Editor Name [{modifier.dataName}] already exists.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
				return;
			}

			DialogResult = true;
		}

		private void CancelButton_Click( object sender, RoutedEventArgs e )
		{
			DialogResult = false;
		}

		private void Window_ContentRendered( object sender, System.EventArgs e )
		{
			nameTB.Focus();
			nameTB.SelectAll();
		}
	}
}
