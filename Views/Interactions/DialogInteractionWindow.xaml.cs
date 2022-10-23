﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace JiME.Views
{
	/// <summary>
	/// Interaction logic for DialogInteractionWindow.xaml
	/// </summary>
	public partial class DialogInteractionWindow : Window, INotifyPropertyChanged
	{
		string oldName;
		public Scenario scenario { get; set; }
		public DialogInteraction interaction { get; set; }
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

		public DialogInteractionWindow( Scenario s, DialogInteraction inter = null )
		{
			scenario = s;
			interaction = inter ?? new DialogInteraction("New Dialog Event");

			InitializeComponent();
			DataContext = this;

			cancelButton.Visibility = inter == null ? Visibility.Visible : Visibility.Collapsed;

			isThreatTriggered = scenario.threatObserver.Any( x => x.triggerName == interaction.dataName );
			if ( isThreatTriggered )
			{
				addMainTriggerButton.IsEnabled = false;
				triggeredByCB.IsEnabled = false;
				//tokenTypeSelector.isTokenCB.IsEnabled = false;
				interaction.isTokenInteraction = false;
			}

			oldName = interaction.dataName;
		}

		void PropChanged( string name )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}

		private void okButton_Click( object sender, RoutedEventArgs e )
		{
			if ( !TryClosing() )
				return;

			tokenTypeSelector.AssignValuesFromSelections();

			scenario.UpdateEventReferences( oldName, interaction );

			closing = true;
			DialogResult = true;
		}

		private void Window_Closing( object sender, CancelEventArgs e )
		{
			if ( !closing )
				e.Cancel = true;
		}

		bool TryClosing()
		{
			//check for dupe name
			if ( interaction.dataName == "New Dialog Event" || scenario.interactionObserver.Count( x => x.dataName == interaction.dataName ) > 1 )
			{
				MessageBox.Show( "Give this Event a unique name.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
				return false;
			}

			return true;
		}

		private void Window_ContentRendered( object sender, EventArgs e )
		{
			nameTB.Focus();
			nameTB.SelectAll();
		}

		private void story1_Click( object sender, RoutedEventArgs e )
		{
			TextBookData tbd = new TextBookData( "Dialog Text" );
			tbd.pages.Add( interaction.c1Text );

			TextEditorWindow te = new TextEditorWindow( scenario, EditMode.Dialog, tbd );
			if ( te.ShowDialog() == true )
			{
				interaction.c1Text = te.textBookController.pages[0];
			}
		}

		private void story2_Click( object sender, RoutedEventArgs e )
		{
			TextBookData tbd = new TextBookData( "Dialog Text" );
			tbd.pages.Add( interaction.c2Text );

			TextEditorWindow te = new TextEditorWindow( scenario, EditMode.Dialog, tbd );
			if ( te.ShowDialog() == true )
			{
				interaction.c2Text = te.textBookController.pages[0];
			}
		}

		private void story3_Click( object sender, RoutedEventArgs e )
		{
			TextBookData tbd = new TextBookData( "Dialog Text" );
			tbd.pages.Add( interaction.c3Text );

			TextEditorWindow te = new TextEditorWindow( scenario, EditMode.Dialog, tbd );
			if ( te.ShowDialog() == true )
			{
				interaction.c3Text = te.textBookController.pages[0];
			}
		}

		private void addTrigger1Button_Click( object sender, RoutedEventArgs e )
		{
			TriggerEditorWindow tw = new TriggerEditorWindow( scenario );
			if ( tw.ShowDialog() == true )
			{
				interaction.c1Trigger = tw.triggerName;
			}
		}

		private void addTrigger2Button_Click( object sender, RoutedEventArgs e )
		{
			TriggerEditorWindow tw = new TriggerEditorWindow( scenario );
			if ( tw.ShowDialog() == true )
			{
				interaction.c2Trigger = tw.triggerName;
			}
		}

		private void addTrigger3Button_Click( object sender, RoutedEventArgs e )
		{
			TriggerEditorWindow tw = new TriggerEditorWindow( scenario );
			if ( tw.ShowDialog() == true )
			{
				interaction.c3Trigger = tw.triggerName;
			}
		}

		private void cancelButton_Click( object sender, RoutedEventArgs e )
		{
			closing = true;
			DialogResult = false;
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

		private void addMainTriggerButton_Click( object sender, RoutedEventArgs e )
		{
			TriggerEditorWindow tw = new TriggerEditorWindow( scenario );
			if ( tw.ShowDialog() == true )
			{
				interaction.triggerName = tw.triggerName;
			}
		}

		private void addMainTriggerAfterButton_Click( object sender, RoutedEventArgs e )
		{
			TriggerEditorWindow tw = new TriggerEditorWindow( scenario );
			if ( tw.ShowDialog() == true )
			{
				interaction.triggerAfterName = tw.triggerName;
			}
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

		private void editPersText_Click( object sender, RoutedEventArgs e )
		{
			TextBookData tbd = new TextBookData( "Dialog Text" );
			tbd.pages.Add( interaction.persistentText );

			TextEditorWindow te = new TextEditorWindow( scenario, EditMode.Persistent, tbd );
			if ( te.ShowDialog() == true )
			{
				interaction.persistentText = te.textBookController.pages[0];
			}
		}
	}
}
