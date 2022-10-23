using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

namespace JiME.Views
{
	/// <summary>
	/// Interaction logic for ReplaceTokenInteractionWindow.xaml
	/// </summary>
	public partial class ReplaceTokenInteractionWindow : Window, INotifyPropertyChanged
	{
		string oldName;

		public Scenario scenario { get; set; }
		public ReplaceTokenInteraction interaction { get; set; }
		bool closing = false;

		public event PropertyChangedEventHandler PropertyChanged;
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
		public ObservableCollection<IInteraction> eventToReplace { get; set; }
		public ObservableCollection<IInteraction> replaceWith { get; set; }

		public ReplaceTokenInteractionWindow( Scenario s, ReplaceTokenInteraction inter = null )
		{
			scenario = s;
			interaction = inter ?? new ReplaceTokenInteraction("New Replace Token Event");

			InitializeComponent();

			DataContext = this;

			cancelButton.Visibility = inter == null ? Visibility.Visible : Visibility.Collapsed;

			isThreatTriggered = scenario.threatObserver.Any( x => x.triggerName == interaction.dataName );
			if ( isThreatTriggered )
			{
				addMainTriggerButton.IsEnabled = false;
				triggeredByCB.IsEnabled = false;
				//isTokenCB.IsEnabled = false;
				interaction.isTokenInteraction = false;
			}

			oldName = interaction.dataName;

			eventToReplace = new ObservableCollection<IInteraction>( scenario.interactionObserver.Where( x =>
			( x.isTokenInteraction || x.dataName == "None" )
			&& x.dataName != interaction.dataName
			&& !x.dataName.Contains( "GRP" ) ) );

			replaceWith = new ObservableCollection<IInteraction>( scenario.interactionObserver.Where( x =>
			( x.isTokenInteraction || x.dataName == "None" )
			&& x.dataName != interaction.dataName
			&& !x.dataName.Contains( "GRP" ) ) );
		}

		private void EditFlavorButton_Click( object sender, RoutedEventArgs e )
		{
			TextEditorWindow tw = new TextEditorWindow( scenario, EditMode.Flavor, interaction.textBookData );
			if ( tw.ShowDialog() == true )
			{
				interaction.textBookData.pages = tw.textBookController.pages;
			}
		}

		private void EditEventButton_Click( object sender, RoutedEventArgs e )
		{
			TextEditorWindow tw = new TextEditorWindow( scenario, EditMode.Event, interaction.eventBookData );
			if ( tw.ShowDialog() == true )
			{
				interaction.eventBookData.pages = tw.textBookController.pages;
			}
		}

		bool TryClosing()
		{
			//check for dupe name
			if ( interaction.dataName == "New Replace Token Event" || scenario.interactionObserver.Count( x => x.dataName == interaction.dataName ) > 1 )
			{
				MessageBox.Show( "Give this Event a unique name.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
				return false;
			}

			if ( interaction.eventToReplace == interaction.dataName
				|| interaction.replaceWithEvent == interaction.dataName )
			{
				MessageBox.Show( "This Event can't replace or be replaced with itself.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
				return false;
			}

			if ( interaction.eventToReplace == "None" || interaction.replaceWithEvent == "None" )
			{
				MessageBox.Show( "The target and source Events can't be set to None.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
				return false;
			}

			if ( interaction.eventToReplace == interaction.replaceWithEvent )
			{
				MessageBox.Show( "The target and source replacement Events can't be the same.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
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

			tokenTypeSelector.AssignValuesFromSelections();

			scenario.UpdateEventReferences( oldName, interaction );

			interaction.eventToReplace = ( (IInteraction)eventToReplaceList.SelectedItem ).dataName;

			interaction.replaceWithEvent = ( (IInteraction)replaceWithList.SelectedItem ).dataName;

			interaction.replaceWithGUID = scenario.interactionObserver.Where( x => x.dataName == interaction.replaceWithEvent ).Select( x => x.GUID ).First();

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
	}
}
