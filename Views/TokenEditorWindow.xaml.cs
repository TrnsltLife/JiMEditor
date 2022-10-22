﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace JiME.Views
{
	/// <summary>
	/// Interaction logic for TokenEditorWindow.xaml
	/// </summary>
	public partial class TokenEditorWindow : Window, INotifyPropertyChanged
	{
		Token _selected;
		public Token selected
		{
			get { return _selected; }
			set
			{
				_selected = value;
				PropChanged( "selected" );
			}
		}
		public Scenario scenario { get; set; }
		public BaseTile tile { get; set; }
		public event PropertyChangedEventHandler PropertyChanged;
		public ObservableCollection<IInteraction> tokenInteractions { get; set; }

		//token drawing
		bool dragging;

		public TokenEditorWindow( BaseTile bt, Scenario s, bool fromRandom = false )
		{
			InitializeComponent();
			DataContext = this;

			scenario = s;
			tile = bt;
			selected = null;

			tokenInteractions = new ObservableCollection<IInteraction>( scenario.interactionObserver.Where( x => ( x.isTokenInteraction && !Regex.IsMatch( x.dataName, @"\sGRP\d+$" ) ) || x.dataName == "None" ) );

			if ( tile.tokenList.Count > 0 )
				tokenCombo.SelectedIndex = 0;

			//rehydrate existing tokens in this tile
			for ( int i = 0; i < tile.tokenList.Count; i++ )
				tile.tokenList[i].Rehydrate( canvas );

			try
			{
				tileImage.Source = new BitmapImage( new Uri( $"pack://application:,,,/JiME;component/Assets/Tiles{tile.tileSide}/{tile.idNumber}.png" ) );
			}
			catch ( Exception e ) { Debug.Log( e.Message ); }
			UpdateButtonsEnabled();

			if (tile.tileType == TileType.Hex)
			{
				if (string.IsNullOrEmpty(((HexTile)tile).flavorBookData.pages[0]))
					exploreStatus.Text = "Exploration Text is Empty";
				else
					exploreStatus.Text = "Exploration Text is Set";
			}
			else
            {
				exploreStatus.Text = "Exploration Text: N/A";
            }

			if ( fromRandom )
				explorationBox.Visibility = Visibility.Collapsed;
		}

		private void OkButton_Click( object sender, RoutedEventArgs e )
		{
			DialogResult = true;
		}

		//private void addSearch_Click( object sender, RoutedEventArgs e )
		//{
		//	foreach ( Token tt in tile.tokenList )
		//		tt.Unselect();
		//	Token t = new Token( TokenType.Search );
		//	tile.tokenList.Add( t );
		//	selected = t;
		//	canvas.Children.Add( t.tokenPathShape );
		//	t.Select();
		//	UpdateButtonsEnabled();
		//}

		//private void addPerson_Click( object sender, RoutedEventArgs e )
		//{
		//	foreach ( Token tt in tile.tokenList )
		//		tt.Unselect();
		//	Token t = new Token( TokenType.Person );
		//	tile.tokenList.Add( t );
		//	selected = t;
		//	canvas.Children.Add( t.tokenPathShape );
		//	t.Select();
		//	UpdateButtonsEnabled();
		//}

		//private void addThreat_Click( object sender, RoutedEventArgs e )
		//{
		//	foreach ( Token tt in tile.tokenList )
		//		tt.Unselect();
		//	Token t = new Token( TokenType.Threat );
		//	tile.tokenList.Add( t );
		//	selected = t;
		//	canvas.Children.Add( t.tokenPathShape );
		//	t.Select();
		//	UpdateButtonsEnabled();
		//}

		//private void addDarkness_Click( object sender, RoutedEventArgs e )
		//{
		//	foreach ( Token tt in tile.tokenList )
		//		tt.Unselect();
		//	Token t = new Token( TokenType.Darkness );
		//	tile.tokenList.Add( t );
		//	selected = t;
		//	canvas.Children.Add( t.tokenPathShape );
		//	t.Select();
		//	UpdateButtonsEnabled();
		//}

		void UpdateButtonsEnabled()
		{
			addToken.IsEnabled = tile.tokenList.Count < tile.tokenCount;
		}

		private void Canvas_MouseDown( object sender, MouseButtonEventArgs e )
		{
			foreach ( Token tt in tile.tokenList )
				tt.Unselect();
			selected = null;
			if ( e.Source is Ellipse )
			{
				Ellipse path = e.Source as Ellipse;
				selected = path.DataContext as Token;
				selected.Select();
				dragging = true;
				selected.SetClickV( e, canvas );
				tokenCombo.SelectedItem = selected;
			}
		}

		private void Canvas_MouseUp( object sender, MouseButtonEventArgs e )
		{
			dragging = false;
		}

		private void Canvas_MouseMove( object sender, MouseEventArgs e )
		{
			if ( dragging && selected != null )
			{
				selected.Drag( e, canvas );
			}
		}

		void PropChanged( string name )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}

		private void token_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			var s = ( (ComboBox)sender ).SelectedItem as Token;
			if ( s != null )
			{
				foreach ( Token tt in tile.tokenList )
					tt.Unselect();
				s.Select();
				selected = s;
			}
		}

		private void removeToken_Click( object sender, RoutedEventArgs e )
		{
			if ( selected == null )
				return;

			var s = tokenCombo.SelectedItem as Token;
			if ( s != null )
				tile.tokenList.Remove( s );
			UpdateButtonsEnabled();
			canvas.Children.Remove( s.tokenPathShape );
			selected = null;
		}

		private void addFlavor_Click( object sender, RoutedEventArgs e )
		{
			if (tile.tileType == TileType.Hex)
			{
				HexTile hexTile = (HexTile)tile;
				TextEditorWindow tw = new TextEditorWindow(scenario, EditMode.Flavor, hexTile.flavorBookData);
				if (tw.ShowDialog() == true)
					hexTile.flavorBookData.pages = tw.textBookController.pages;

				if (string.IsNullOrEmpty(hexTile.flavorBookData.pages[0]))
					exploreStatus.Text = "Exploration Text is Empty";
				else
					exploreStatus.Text = "Exploration Text is Set";
			}
			else
            {
				exploreStatus.Text = "Exploration Text: N/A";
            }
		}

		private void addTrigger_Click( object sender, RoutedEventArgs e )
		{
			if ( selected == null )
				return;

			TriggerEditorWindow tw = new TriggerEditorWindow( scenario );
			if ( tw.ShowDialog() == true )
			{
				selected.triggeredByName = tw.triggerName;
			}
		}

		private void addToken_Click( object sender, RoutedEventArgs e )
		{
			foreach ( Token tt in tile.tokenList )
				tt.Unselect();
			Token t = new Token( TokenType.None );
			tile.tokenList.Add( t );
			selected = t;
			canvas.Children.Add( t.tokenPathShape );
			t.Select();
			UpdateButtonsEnabled();
		}

		private void interactionCB_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			if ( selected != null && ( (ComboBox)sender ).SelectedItem != null )
			{
				selected.tokenType = ( (IInteraction)( (ComboBox)sender )?.SelectedItem ).tokenType;
				selected.personType = ( (IInteraction)( (ComboBox)sender )?.SelectedItem ).personType;
				selected.dataName = selected.tokenType.ToString();
				selected.ReColor();
			}

			//	Debug.Log( selected.tokenType );
			//var foo = ( (ComboBox)sender )?.SelectedItem;
			//if ( foo != null )
			//	Debug.Log( ( (ComboBox)sender ).SelectedItem.ToString() );
		}

		private void Window_PreviewKeyDown( object sender, KeyEventArgs e )
		{
			if ( selected != null )
			{
				if ( e.Key == Key.Delete )
				{
					var s = tokenCombo.SelectedItem as Token;
					if ( s != null )
						tile.tokenList.Remove( s );
					UpdateButtonsEnabled();
					canvas.Children.Remove( s.tokenPathShape );
					selected = null;
				}
			}
		}
	}
}
