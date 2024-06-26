﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace JiME.Views
{
	/// <summary>
	/// Interaction logic for TileEditorWindow.xaml
	/// </summary>
	public partial class TileEditorWindow : Window, INotifyPropertyChanged
	{
		BaseTile _selected;
		public BaseTile selected
		{
			get => _selected;
			set
			{
				_selected = value;
				PropChanged( "selected" );
			}
		}
		public Chapter chapter { get; set; }
		public Scenario scenario { get; set; }

		bool dragging;

		public event PropertyChangedEventHandler PropertyChanged;

		public TileEditorWindow( Scenario s, Chapter c = null )
		{
			InitializeComponent();
			DataContext = this;

			scenario = s;
			chapter = c ?? new Chapter( "New Block" );
			selected = null;

			if(scenario.scenarioTypeJourney)
            {
				canvas.Style = (Style)FindResource("hexGrid"); ;
			}
			else
            {
				canvas.Style = (Style)FindResource("squareGrid"); ;
			}
			scenario.RefilterGlobalTilePool();

			//Uncomment these for visual debugging
			//HexTile.printPivot = true;
			//HexTile.printClick = true;
			//HexTile.printRect = true;
			//SquareTile.printPivot = true;

			//rehydrate existing tiles in this chapter
			for ( int i = 0; i < chapter.tileObserver.Count; i++ )
			{
				BaseTile bt = ((BaseTile)chapter.tileObserver[i]);
				if (bt.tileType == TileType.Hex)
				{
					HexTile hex = ((HexTile)chapter.tileObserver[i]);
					hex.useGraphic = scenario.useTileGraphics;
					hex.Rehydrate(canvas);
					hex.ChangeColor(i);
					hex.ToggleGraphic(canvas);
				}
				else if (bt.tileType == TileType.Square)
                {
					SquareTile sqr = ((SquareTile)chapter.tileObserver[i]);
					sqr.useGraphic = scenario.useTileGraphics;
					sqr.Rehydrate(canvas);
					sqr.ChangeColor(i);
					sqr.ToggleGraphic(canvas);
				}
			}

			//editTokenButton.IsEnabled = !chapter.usesRandomGroups;
			//disabledMessage.Visibility = chapter.usesRandomGroups ? Visibility.Visible : Visibility.Collapsed;
			editTokenButton.IsEnabled = true;
			disabledMessage.Visibility = Visibility.Collapsed;
			//SourceInitialized += ( x, y ) =>
			//{
			//	this.HideMinimizeAndMaximizeButtons();
			//};
		}

		int GetUnusedColor()
		{
			int[] used = chapter.tileObserver.Select( x => ( (BaseTile)x ).color ).ToArray();
			for ( int i = 0; i < chapter.tileObserver.Count; i++ )
			{
				if ( i < used.Length && !used.Contains( i ) )
					return i;
			}
			return chapter.tileObserver.Count;
		}

		void PropChanged( string name )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}

		private void OkButton_Click( object sender, RoutedEventArgs e )
		{
			DialogResult = true;
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

		public Rect GetBounds( FrameworkElement of, FrameworkElement from )
		{
			// Might throw an exception if of and from are not in the same visual tree
			GeneralTransform transform = of.TransformToVisual( from );

			return transform.TransformBounds( new Rect( 0, 0, of.ActualWidth, of.ActualHeight ) );
		}

		private void Window_PreviewKeyDown( object sender, KeyEventArgs e )
		{
			if ( selected != null )
			{
				if ( e.Key == Key.PageUp )
					selected.Rotate(-1, canvas );
				else if ( e.Key == Key.PageDown )
					selected.Rotate(1, canvas );
				else if ( e.Key == Key.Delete )
				{
					DeleteTileAction();
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

		private void DeleteTileAction()
		{
			var ret = MessageBox.Show("Are you sure you want to delete this Tile?\n\nALL ITS TOKENS WILL BE DELETED.", "Delete Tile", MessageBoxButton.YesNo, MessageBoxImage.Question);
			if (ret == MessageBoxResult.Yes)
			{
				RemoveTile(selected, true);
				selected = null;
				//sort list
				TileSorter sorter = new TileSorter();
				List<int> foo = scenario.globalTilePool.ToList();
				foo.Sort(sorter);
				scenario.globalTilePool.Clear();
				foreach (int s in foo)
					scenario.globalTilePool.Add(s);
				tilePool.SelectedIndex = 0;
			}
		}

		private void AddTileButton_Click( object sender, RoutedEventArgs e )
		{
			if ( tilePool.SelectedIndex == -1 || chapter.tileObserver.Count == 5 )
				return;

			foreach ( BaseTile tt in chapter.tileObserver )
			{
				tt.Unselect();
			}

			int color = GetUnusedColor();
			BaseTile tile;
			if (scenario.scenarioTypeJourney)
			{
				tile = new HexTile((int)tilePool.SelectedItem);
			}
			else
            {
				tile = new SquareTile((int)tilePool.SelectedItem);
            }
			tile.useGraphic = scenario.useTileGraphics;
			tile.ChangeColor( color );
			//ChangeTileSide() also rehydrates and adds tile image
			//This is why no need to call canvas.Children.Add(hex.hexPathShape)
			tile.ChangeTileSide("A", canvas);
			chapter.AddTile( tile );
			selected = tile;
			selected.Select();
			radioA.IsChecked = selected.tileSide == "A";
			radioB.IsChecked = selected.tileSide == "B";
			inChapterCB.SelectedIndex = chapter.tileObserver.Count - 1;
			scenario.globalTilePool.Remove( (int)tilePool.SelectedItem );
		}

		private void removeTileButton_Click( object sender, RoutedEventArgs e )
		{
			if ( selected != null )
			{
				DeleteTileAction();
			}
		}

		private void RemoveTile(BaseTile tile, bool affectScenarioAndChapter)
        {
			tile.canvas.Children.Clear();
			//canvas.Children.Remove(tile.pathShape);
			//tile.canvas.Children.Remove(tile.tileImage);
			canvas.Children.Remove(tile.canvas);
			if (affectScenarioAndChapter)
			{
				scenario.globalTilePool.Add(tile.idNumber);
				chapter.RemoveTile(tile);
			}
		}

		//private void AddEventButton_Click( object sender, RoutedEventArgs e )
		//{
		//	//EventEditorWindow ew = new EventEditorWindow( scenario );
		//	TriggerEditorWindow ew = new TriggerEditorWindow( scenario );
		//	if ( ew.ShowDialog() == true )
		//	{
		//		scenario.AddTrigger( ew.triggerName );
		//		int idx = int.Parse( (string)( (Button)sender ).DataContext );
		//		selected.triggerNames[idx] = ew.triggerName;
		//		selected = selected;
		//	}
		//}

		/// <summary>
		/// tiles in chapter CB
		/// </summary>
		private void ComboBox_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			foreach ( BaseTile tt in chapter.tileObserver )
			{
				tt.Unselect();
			}

			selected = null;

			var s = ( (ComboBox)sender ).SelectedItem as BaseTile;
			if ( s != null )
			{
				var t = ( from BaseTile tile in chapter.tileObserver where s.GUID == tile.GUID select tile ).FirstOr( null );
				if ( t != null )
				{
					selected = t;
					selected.Select();
					radioA.IsChecked = selected.tileSide == "A";
					radioB.IsChecked = selected.tileSide == "B";
					tokenCount.Text = "Tokens in Tile: " + t.tokenList.Count;
				}
			}
		}

		private void Window_Closing( object sender, CancelEventArgs e )
		{
			canvas.Children.Clear();
		}

		private void editTokenButton_Click( object sender, RoutedEventArgs e )
		{
			selected.Unselect();
			TokenEditorWindow tw = new TokenEditorWindow( selected, scenario );
			if(tw.ShowDialog() == true)
            {
				selected.Rehydrate(canvas);
            }
			selected = null;
		}

		private void canvas_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
		{
			if ( e.ClickCount == 1 )
			{
				foreach (BaseTile tt in chapter.tileObserver)
				{
					tt.Unselect();
				}

				selected = null;

				if ( e.Source is Path )
				{
					Path path = e.Source as Path;
					selected = path.DataContext as BaseTile;
					selected.Select();
					dragging = true;
					//selected.SetClickV( e, canvas );
					selected.SetClickV(e, selected.canvas);
					inChapterCB.SelectedItem = selected;
					radioA.IsChecked = selected.tileSide == "A";
					radioB.IsChecked = selected.tileSide == "B";
					tokenCount.Text = "Tokens in Tile: " + selected.tokenList.Count;

					//Info printed on status bar
					infoTileID.Text = selected.idNumberAndCollection;
					infoTilePosition.Text = selected.position.ToString();
					infoTileSize.Text = selected.tileImage.Source.Width + "," + selected.tileImage.Source.Height;
					infoTileRotation.Text = selected.angle.ToString();
				}
			}

			//double click
			else if ( e.ChangedButton == MouseButton.Left && e.ClickCount == 2 )
			{
				dragging = false;
				if ( selected == null )
					return;

				TokenEditorWindow tw = new TokenEditorWindow( selected, scenario );
				selected.Unselect();
				if(tw.ShowDialog() == true)
                {
					selected.Rehydrate(canvas);
                }
				selected = null;
			}
		}

		private void canvas_MouseWheel( object sender, MouseWheelEventArgs e )
        {
			if ( selected != null )
            {
				if (e.Delta > 0)
					selected.Rotate(-1, canvas);
				else if (e.Delta < 0)
					selected.Rotate(1, canvas);
            }
        }

		private void radioA_Click( object sender, RoutedEventArgs e )
		{
			if ( selected != null )
			{
				if ( selected.tileSide == "A" )
					return;
				RemoveTile(selected, false);
				selected.ChangeTileSide( "A", canvas );
			}
		}

		private void radioB_Click( object sender, RoutedEventArgs e )
		{
			if ( selected != null )
			{
				if ( selected.tileSide == "B" )
					return;
				RemoveTile(selected, false);
				selected.ChangeTileSide( "B", canvas );
			}
		}

		private void tileGalleryButton_Click( object sender, RoutedEventArgs e )
		{
			GalleryWindow gw = new GalleryWindow( scenario, chapter.tileObserver.Count );
			if ( gw.ShowDialog() == true && gw.selectedData.Length > 0 )
			{
				foreach ( BaseTile tt in chapter.tileObserver )
				{
					tt.Unselect();
				}

				foreach ( var t in gw.selectedData )
				{
					int color = GetUnusedColor();
					BaseTile tile = BaseTile.CreateTile( t.Item1 );
					tile.useGraphic = scenario.useTileGraphics;
					tile.ChangeColor( color );
					//ChangeTileSide() also rehydrates and adds tile image
					//This is why no need to call canvas.Children.Add(hex.hexPathShape)
					tile.ChangeTileSide( t.Item2, canvas );
					chapter.AddTile(tile);
					selected = tile;
					selected.Select();
					radioA.IsChecked = selected.tileSide == "A";
					radioB.IsChecked = selected.tileSide == "B";
					inChapterCB.SelectedIndex = chapter.tileObserver.Count - 1;
					scenario.globalTilePool.Remove( t.Item1 );
				}
			}
		}

		private void addExploredTriggerButton_Click( object sender, RoutedEventArgs e )
		{
			TriggerEditorWindow tw = new TriggerEditorWindow( scenario );
			if ( tw.ShowDialog() == true )
			{
				selected.triggerName = tw.triggerName;
			}
		}

		private void toggleUseGraphics_Click( object sender, RoutedEventArgs e )
		{
			for ( int i = 0; i < chapter.tileObserver.Count; i++ )
			{
				( (BaseTile)chapter.tileObserver[i] ).useGraphic = scenario.useTileGraphics;
				( (BaseTile)chapter.tileObserver[i] ).ToggleGraphic( canvas );
			}
		}

		private void StartingTile_Checked(object sender, RoutedEventArgs e)
        {
			if (selected != null)
			{
				//Clear Starting Position token and isStart for other tiles
				foreach (BaseTile otherTile in chapter.tileObserver)
                {
					if (otherTile.idNumber != selected.idNumber)
					{
						otherTile.isStartTile = false;
						Token t = otherTile.tokenList.Where(it => it.tokenType == TokenType.Start).FirstOrDefault();
						if (t != null)
						{
							otherTile.tokenList.Remove(t);
							otherTile.Rehydrate(canvas);
						}
					}
				}
			}
		}

		private void StartingTile_Unchecked(object sender, RoutedEventArgs e)
		{
			if (selected != null)
			{
				//Clear Starting Position token if it exists in the tile
				Token t = selected.tokenList.Where(it => it.tokenType == TokenType.Start).FirstOrDefault();
				if (t != null)
				{
					selected.tokenList.Remove(t);
					selected.Rehydrate(canvas);
				}
			}
		}
	}
}