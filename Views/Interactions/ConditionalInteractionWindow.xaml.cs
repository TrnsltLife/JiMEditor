using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace JiME.Views
{
	/// <summary>
	/// Interaction logic for ConditionalInteractionWindow.xaml
	/// </summary>
	public partial class ConditionalInteractionWindow : Window, INotifyPropertyChanged
	{
		string oldName;

		public Scenario scenario { get; set; }
		public ConditionalInteraction interaction { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;
		bool closing = false;
		bool _isThreatTriggered;
		public bool isThreatTriggered
		{
			get => _isThreatTriggered;
			set
			{
				_isThreatTriggered = value;
				PropChanged( "isThreatTriggered" );
			}
		}

		public ConditionalInteractionWindow( Scenario s, ConditionalInteraction inter = null, bool showCancelButton = false )
		{
			InitializeComponent();
			DataContext = this;

			scenario = s;
			cancelButton.Visibility = (inter == null || showCancelButton) ? Visibility.Visible : Visibility.Collapsed;
			interaction = inter ?? new ConditionalInteraction( "New Conditional Event" );


			isThreatTriggered = scenario.threatObserver.Any( x => x.triggerName == interaction.dataName );
			if ( isThreatTriggered )
			{
				interaction.isTokenInteraction = false;
			}

			oldName = interaction.dataName;
		}

		private void ComboBox_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			//this was commented out??
			//if ( !interaction.isFromThreatThreshold )
			//{
			//	if ( interaction.triggerName == "None" )
			//		eventbox.Visibility = Visibility.Visible;
			//	else
			//		eventbox.Visibility = Visibility.Collapsed;
			//}
			//else
			//	flavorbox.Visibility = Visibility.Collapsed;
		}

		private void EditFlavorButton_Click( object sender, RoutedEventArgs e )
		{
			TextEditorWindow tw = new TextEditorWindow( scenario, EditMode.Flavor, interaction.textBookData );
			if ( tw.ShowDialog() == true )
			{
				interaction.textBookData.pages = tw.textBookController.pages;
				//flavorTB.Text = tw.textBookController.pages[0];
			}
		}

		private void EditEventButton_Click( object sender, RoutedEventArgs e )
		{
			TextEditorWindow tw = new TextEditorWindow( scenario, EditMode.Event, interaction.eventBookData );
			if ( tw.ShowDialog() == true )
			{
				interaction.eventBookData.pages = tw.textBookController.pages;
				//eventTB.Text = tw.textBookController.pages[0];
			}
		}

		bool TryClosing()
		{
			//check for dupe name
			if ( interaction.dataName == "New Conditional Event" || scenario.interactionObserver.Count(x => x.dataName == interaction.dataName && x.GUID != interaction.GUID) > 0)
			{
				MessageBox.Show("An Event with this name already exists. Give this Event a unique name.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
				return false;
			}

			return true;
		}

		private void Window_Closing( object sender, CancelEventArgs e )
		{
			if ( !closing )
				e.Cancel = true;
		}

		private void OkButton_Click( object sender, RoutedEventArgs e )
		{
			if ( !TryClosing() )
				return;

			scenario.UpdateEventReferences( oldName, interaction );

			closing = true;
			DialogResult = true;
		}

		private void CancelButton_Click( object sender, RoutedEventArgs e )
		{
			closing = true;
			DialogResult = false;
		}

		private void Window_ContentRendered( object sender, System.EventArgs e )
		{
			nameTB.Focus();
			nameTB.SelectAll();
			triggerCB.SelectedIndex = 0;
		}

		private void addMainTriggerAfterButton_Click( object sender, RoutedEventArgs e )
		{
			TriggerEditorWindow tw = new TriggerEditorWindow( scenario );
			if ( tw.ShowDialog() == true )
			{
				interaction.triggerAfterName = tw.triggerName;
			}
		}

		private void addMainTriggerButton_Click( object sender, RoutedEventArgs e )
		{
			TriggerEditorWindow tw = new TriggerEditorWindow( scenario );
			if ( tw.ShowDialog() == true )
			{
				interaction.triggerName = tw.triggerName;
			}
		}

		void PropChanged( string name )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}

		private void groupHelp_Click( object sender, RoutedEventArgs e )
		{
			HelpWindow hw = new HelpWindow( HelpType.Grouping );
			hw.ShowDialog();
		}

		private void nameTB_TextChanged( object sender, TextChangedEventArgs e )
		{
			interaction.dataName = ( (TextBox)sender ).Text;
			Regex rx = new Regex( @"\sGRP\d+$" );
			MatchCollection matches = rx.Matches( interaction.dataName );
			if ( matches.Count > 0 )
				groupInfo.Text = "This Event is in the following group: " + matches[0].Value.Trim();
			else
				groupInfo.Text = "This Event is in the following group: None";
		}

		private void triggerCB_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			//addSelectedTriggerButton.IsEnabled = triggerCB.SelectedIndex != 0;
			addSelectedTriggerButton.IsEnabled = triggerCB.SelectedValue as string != "None";
		}

		private void addSelectedTriggerButton_Click( object sender, RoutedEventArgs e )
		{
			string t = triggerCB.SelectedValue as string;
			if ( !interaction.triggerList.Contains( t ) && t != "None")
			{
				interaction.triggerList.Add( t );
			}
		}

		private void addTriggerButton_Click( object sender, RoutedEventArgs e )
		{
			TriggerEditorWindow tw = new TriggerEditorWindow( scenario );
			if ( tw.ShowDialog() == true )
			{
				if ( !interaction.triggerList.Contains( tw.triggerName ) )
					interaction.triggerList.Add( tw.triggerName );
			}
		}

		private void removeTriggerButton_Click( object sender, RoutedEventArgs e )
		{
			string sel = ( (Button)sender ).DataContext as string;
			if ( interaction.triggerList.Contains( sel ) )
				interaction.triggerList.Remove( sel );
		}

		private void addFinishedTriggerButton_Click( object sender, RoutedEventArgs e )
		{
			TriggerEditorWindow tw = new TriggerEditorWindow( scenario );
			if ( tw.ShowDialog() == true )
			{
				interaction.finishedTrigger = tw.triggerName;
			}
		}
	}
}
