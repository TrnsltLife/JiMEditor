using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace JiME.Views
{
	/// <summary>
	/// Interaction logic for TitleInteractionWindow.xaml
	/// </summary>
	public partial class TitleInteractionWindow : Window, INotifyPropertyChanged
	{
		string oldName;

		public Scenario scenario { get; set; }
		public TitleInteraction interaction { get; set; }
		bool closing = false;

		private List<Title> _titleList = new List<Title>();

		public List<Title> titleList
		{
			get => _titleList;
			set
            {
				_titleList = value;
				PropChanged("titleList");
            }
		}

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

		public TitleInteractionWindow( Scenario s, TitleInteraction inter = null, bool showCancelButton = false )
		{
			scenario = s;
			interaction = inter ?? new TitleInteraction("New Title Event");

			_titleList = Titles.list.Where(it => it.id != 0 && scenario.collectionObserver.Contains(Collection.FromID(it.collection))).ToList();

			InitializeComponent();
			DataContext = this;

			cancelButton.Visibility = (inter == null || showCancelButton) ? Visibility.Visible : Visibility.Collapsed;
			updateUIForEventGroup();

			isThreatTriggered = scenario.threatObserver.Any( x => x.triggerName == interaction.dataName );
			if ( isThreatTriggered )
			{
				addMainTriggerButton.IsEnabled = false;
				triggeredByCB.IsEnabled = false;
				//tokenTypeSelector.isTokenCB.IsEnabled = false;
				interaction.isTokenInteraction = false;
			}

			oldName = interaction.dataName;

			Debug.Log("TitleInteractionWindow: " + String.Join(", ", titleList.ConvertAll(it => it.dataName)));
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
			if ( interaction.dataName == "New Title Event" || scenario.interactionObserver.Count( x => x.dataName == interaction.dataName && x.GUID != interaction.GUID ) > 0 )
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

			tokenTypeSelector.AssignValuesFromSelections();

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

		private void titleCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			addSelectedTitleButton.IsEnabled = true;
			//addSelectedTitleButton.IsEnabled = titleCB.SelectedIndex != 0;
			//addSelectedTitleButton.IsEnabled = titleCB.SelectedValue as string != "None";
			Debug.Log("TitleCB SelectedValue: " + titleCB.SelectedValue);
		}

		private void addSelectedTitleButton_Click(object sender, RoutedEventArgs e)
		{
			int id = (int)titleCB.SelectedValue;
			Title title = titleList.FirstOrDefault(it => it.id == id);
			if (!interaction.titleList.Contains(title))
			{
				interaction.titleList.Add(title);
			}
		}

		private void removeTitleButton_Click(object sender, RoutedEventArgs e)
		{
			Title title = (Title)((Button)sender).DataContext;
			//int id = (int)((Button)sender).DataContext;
			//Title title = titleList.FirstOrDefault(it => it.id == id);
			if (interaction.titleList.Contains(title))
				interaction.titleList.Remove(title);
		}

		private void addFallbackTriggerButton_Click(object sender, RoutedEventArgs e)
		{
			TriggerEditorWindow tw = new TriggerEditorWindow(scenario);
			if (tw.ShowDialog() == true)
			{
				interaction.fallbackTrigger = tw.triggerName;
			}
		}
	}
}
