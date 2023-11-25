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
	/// Interaction logic for ItemInteractionWindow.xaml
	/// </summary>
	public partial class ItemInteractionWindow : Window, INotifyPropertyChanged
	{
		string oldName;

		public Scenario scenario { get; set; }
		public ItemInteraction interaction { get; set; }
		bool closing = false;

		private static List<Item> _itemList = new List<Item>(Items.list.Where(it =>
			(it.slotId == Slot.TRINKET && it.tier == 1) || (it.slotId == Slot.MOUNT && it.tier == 0)));
		public static List<Item> itemList
		{
			get => _itemList;
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

		public ItemInteractionWindow( Scenario s, ItemInteraction inter = null, bool showCancelButton = false )
		{
			scenario = s;
			interaction = inter ?? new ItemInteraction("New Item Event");

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

			Debug.Log("ItemInteractionWindow: " + String.Join(", ", itemList.ConvertAll(it => it.dataName)));
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
			if ( interaction.dataName == "New Item Event" || scenario.interactionObserver.Count( x => x.dataName == interaction.dataName && x.GUID != interaction.GUID ) > 0 )
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

		private void itemCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			addSelectedItemButton.IsEnabled = true;
			//addSelectedItemButton.IsEnabled = itemCB.SelectedIndex != 0;
			//addSelectedItemButton.IsEnabled = itemCB.SelectedValue as string != "None";
			Debug.Log("ItemCB SelectedValue: " + itemCB.SelectedValue);
		}

		private void addSelectedItemButton_Click(object sender, RoutedEventArgs e)
		{
			int id = (int)itemCB.SelectedValue;
			if (!interaction.itemList.Contains(id))
			{
				interaction.itemList.Add(id);
			}
		}

		private void removeItemButton_Click(object sender, RoutedEventArgs e)
		{
			string sel = ((Button)sender).DataContext as string;
			int id = (int)itemList.FirstOrDefault(it => it.dataName == sel)?.id;
			if (interaction.itemList.Contains(id))
				interaction.itemList.Remove(id);
		}

		private void addFinishedTriggerButton_Click(object sender, RoutedEventArgs e)
		{
			TriggerEditorWindow tw = new TriggerEditorWindow(scenario);
			if (tw.ShowDialog() == true)
			{
				interaction.finishedTrigger = tw.triggerName;
			}
		}
	}
}
