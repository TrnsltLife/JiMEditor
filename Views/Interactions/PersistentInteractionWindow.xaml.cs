using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using JiME.UserControls;
using System.Collections.Specialized;

namespace JiME.Views
{
	/// <summary>
	/// Interaction logic for PersistentInteractionWindow.xaml
	/// </summary>
	public partial class PersistentInteractionWindow : Window, INotifyPropertyChanged
	{
		string oldName;

		public Scenario scenario { get; set; }
		public ObservableCollection<string> interactionObserver { get; set; }
		public PersistentTokenInteraction interaction { get; set; }
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

		public PersistentInteractionWindow( Scenario s, PersistentTokenInteraction inter = null , bool showCancelButton = false)
		{
			scenario = s;
			interaction = inter ?? new PersistentTokenInteraction("New Persistent Event");
			interaction.isTokenInteraction = true;

			InitializeComponent();
			DataContext = this;

			cancelButton.Visibility = (inter == null || showCancelButton) ? Visibility.Visible : Visibility.Collapsed;
			updateUIForEventGroup();

			//interactions that are NOT Token Interactions
			interactionObserver = new ObservableCollection<string>( scenario.interactionObserver.Where( x => x.isTokenInteraction ).Select( x => x.dataName ) );
			scenario.interactionObserver.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(InteractionsCollectionChangedMethod); //used to update after Add Interaction dialogs

			isThreatTriggered = scenario.threatObserver.Any( x => x.triggerName == interaction.dataName );
			if ( isThreatTriggered )
			{
				//addMainTriggerButton.IsEnabled = false;
				//triggeredByCB.IsEnabled = false;
				//isTokenCB.IsEnabled = false;
				interaction.isTokenInteraction = false;
			}

			oldName = interaction.dataName;

			altTB.Document = RichTextBoxIconEditor.CreateFlowDocumentFromSimpleHtml(interaction.alternativeBookData.pages[0], "", "FontFamily=\"Segoe UI\" FontSize=\"12\"");
		}

		private void InteractionsCollectionChangedMethod(object sender, NotifyCollectionChangedEventArgs e)
		{
			//different kind of changes that may have occurred in collection when returning from the various Add Interaction dialogs
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				RefreshTokenInteractions();
			}
			if (e.Action == NotifyCollectionChangedAction.Replace)
			{
				RefreshTokenInteractions();
			}
			if (e.Action == NotifyCollectionChangedAction.Remove)
			{
				RefreshTokenInteractions();
			}
			if (e.Action == NotifyCollectionChangedAction.Move)
			{
				RefreshTokenInteractions();
			}
		}

		private void RefreshTokenInteractions()
		{
			ObservableCollection<IInteraction> newEvents = new ObservableCollection<IInteraction>(scenario.interactionObserver.Where(x => (x.isTokenInteraction && !Regex.IsMatch(x.dataName, @"\sGRP\d+$")) || x.dataName == "None"));
			foreach (var interaction in newEvents)
			{
				if (!interactionObserver.Contains(interaction.dataName))
				{
					interactionObserver.Add(interaction.dataName);
				}
			}
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

		private void EditEventButton_Click( object sender, RoutedEventArgs e )
		{
			TextEditorWindow tw = new TextEditorWindow( scenario, EditMode.Event, interaction.eventBookData );
			if ( tw.ShowDialog() == true )
			{
				interaction.eventBookData.pages = tw.textBookController.pages;
				eventTB.Text = tw.textBookController.pages[0];
			}
		}

		public void CreatedNewEvent(InteractionBase ib)
		{
			interaction.eventToActivate = ib.dataName;
		}

		bool TryClosing()
		{
			//check for dupe name
			if ( interaction.dataName == "New Persistent Event" || scenario.interactionObserver.Count(x => x.dataName == interaction.dataName && x.GUID != interaction.GUID) > 0)
			{
				MessageBox.Show( "Give this Event a unique name.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
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

			scenario.interactionObserver.CollectionChanged -= InteractionsCollectionChangedMethod;

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
			updateUIForEventGroup();
		}

		private void updateUIForEventGroup()
		{
			Regex rx = new Regex(@"\sGRP\d+$");
			MatchCollection matches = rx.Matches(interaction.dataName);
			if (matches.Count > 0)
			{
				groupInfo.Text = "This Event is in the following group: " + matches[0].Value.Trim();
				isReusableCB.Visibility = Visibility.Visible;
			}
			else
			{
				groupInfo.Text = "This Event is in the following group: None";
				isReusableCB.Visibility = Visibility.Collapsed;
			}
		}

		private void editAltButton_Click( object sender, RoutedEventArgs e )
		{
			TextEditorWindow tw = new TextEditorWindow( scenario, EditMode.Flavor, interaction.alternativeBookData );
			if ( tw.ShowDialog() == true )
			{
				interaction.alternativeBookData.pages = tw.textBookController.pages;
				//altTB.Text = tw.textBookController.pages[0];
				altTB.Document = RichTextBoxIconEditor.CreateFlowDocumentFromSimpleHtml(interaction.alternativeBookData.pages[0], "", "FontFamily=\"Segoe UI\" FontSize=\"12\"");
			}
		}

		private void addAltTrigger_Click( object sender, RoutedEventArgs e )
		{
			TriggerEditorWindow tw = new TriggerEditorWindow( scenario );
			if ( tw.ShowDialog() == true )
			{
				interaction.alternativeTextTrigger = tw.triggerName;
			}
		}

		private void eventCB_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			string s = ( (ComboBox)sender ).SelectedValue as string;
			if ( s == interaction.dataName )
				( (ComboBox)sender ).SelectedIndex = 0;
		}
	}
}
