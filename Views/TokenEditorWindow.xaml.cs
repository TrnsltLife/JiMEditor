using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Collections.Generic;

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

		public TokenEditorWindow( BaseTile bt, Scenario s, bool fromRandom = false, Token highlightToken = null )
		{
			InitializeComponent();
			DataContext = this;

			scenario = s;
			tile = bt;
			selected = null;

			tokenInteractions = new ObservableCollection<IInteraction>( scenario.interactionObserver.Where( x => ( x.isTokenInteraction && !Regex.IsMatch( x.dataName, @"\sGRP\d+$" ) ) || x.dataName == "None" ) );
			scenario.interactionObserver.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(InteractionsCollectionChangedMethod); //used to update after Add Interaction dialogs

            // Adjust token selection
            if (tile.tokenList.Count > 0)
            {
                int selectionIndex = 0;
                if (highlightToken != null)
                {
                    var highlightIndex = tile.tokenList.ToList().FindIndex(t => t.dataName == highlightToken.dataName);
                    selectionIndex = highlightIndex != -1 ? highlightIndex : 0;
                }
                // Set the selected token and update it's graphics (with small delay so things have had change to render)
                selected = tile.tokenList[selectionIndex];
                WpfUtils.DeferExecution("MarkPreselectedToken", 100, () => selected?.Select());
            }

			//rehydrate existing tokens in this tile
			for (int i = 0; i < tile.tokenList.Count; i++)
			{
				tile.tokenList[i].parentTile = null;
				tile.tokenList[i].Rehydrate(canvas);
			}

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

			if ( fromRandom || tile.tileType == TileType.Square )
				explorationBox.Visibility = Visibility.Collapsed;
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
			foreach(var interaction in newEvents)
            {
				if(!tokenInteractions.Contains(interaction))
                {
					tokenInteractions.Add(interaction);
                }
            }
		}


		private void OkButton_Click( object sender, RoutedEventArgs e )
		{
			scenario.interactionObserver.CollectionChanged -= InteractionsCollectionChangedMethod;
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
			if ( e.Source is Shape )
			{
				Shape path = e.Source as Shape;
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

		private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (selected != null)
			{
				if (e.Delta > 0)
					selected.Rotate(-1, canvas);
				else if (e.Delta < 0)
					selected.Rotate(1, canvas);
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
				Dictionary<string, string> originals = hexTile.CaptureStartingValues();
				string originalKeyName = hexTile.TranslationKeyName();
				string originalPrefix = hexTile.TranslationKeyPrefix();
				TextEditorWindow tw = new TextEditorWindow(scenario, EditMode.Flavor, hexTile.flavorBookData);
				if (tw.ShowDialog() == true)
					hexTile.flavorBookData.pages = tw.textBookController.pages;

				if (string.IsNullOrEmpty(hexTile.flavorBookData.pages[0]))
					exploreStatus.Text = "Exploration Text is Empty";
				else
					exploreStatus.Text = "Exploration Text is Set";

				hexTile.UpdateKeysStartingWith(scenario.translationObserver, originalKeyName);
				hexTile.DecertifyChangedValues(scenario.translationObserver, originals, originalKeyName);
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
			//canvas.Children.Add( t.tokenPathShape );
			t.Rehydrate(canvas);
			t.Select();
			UpdateButtonsEnabled();
		}

		public void CreatedNewEvent(InteractionBase ib)
		{
			interactionCB.SelectedValue = ib.dataName;
		}

		private void interactionCB_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			if ( selected != null && ( (ComboBox)sender ).SelectedItem != null )
			{
				selected.tokenType = ( (IInteraction)( (ComboBox)sender )?.SelectedItem ).tokenType;
				selected.personType = ( (IInteraction)( (ComboBox)sender )?.SelectedItem ).personType;
				selected.terrainType = ((IInteraction)((ComboBox)sender)?.SelectedItem).terrainType;
				//selected.dataName = selected.tokenType.ToString();
				selected.Rehydrate(canvas);
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
				if (e.Key == Key.PageUp)
					selected.Rotate(-1, canvas);
				else if (e.Key == Key.PageDown)
					selected.Rotate(1, canvas);
				else if (e.Key == Key.Delete)
				{
					var s = tokenCombo.SelectedItem as Token;
					if (s != null)
						tile.tokenList.Remove(s);
					UpdateButtonsEnabled();
					canvas.Children.Remove(s.tokenPathShape);
					selected = null;
				}
				else if (e.Key == Key.Up)
					selected.Move(0, -1);
				else if (e.Key == Key.Down)
					selected.Move(0, 1);
				else if (e.Key == Key.Left)
					selected.Move(-1, 0);
				else if (e.Key == Key.Right)
					selected.Move(1, 0);
			}
		}
	}
}
