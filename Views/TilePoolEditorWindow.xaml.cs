﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace JiME.Views
{
	/// <summary>
	/// Interaction logic for TilePoolEditorWindow.xaml
	/// </summary>
	public partial class TilePoolEditorWindow : Window, INotifyPropertyChanged
	{
		//int _selectedTileNumber;
		HexTile _selected;
		public HexTile selected
		{
			get => _selected;
			set
			{
				_selected = value;
				PropChanged( "selected" );
			}
		}

		public Scenario scenario { get; set; }
		public Chapter chapter { get; set; }
		bool closing = false;

		public event PropertyChangedEventHandler PropertyChanged;

		void PropChanged( string name )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}

		public TilePoolEditorWindow( Scenario s, Chapter c )
		{
			scenario = s;
			scenario.RefilterGlobalTilePool();

			InitializeComponent();
			DataContext = this;

			chapter = c;
			selected = null;
		}

		bool TryClose()
		{
			return true;
			//if ( chapter.tileObserver.Count >= 1 )
			//	return true;
			//else
			//{
			//	MessageBox.Show( "There must be at least 1 Tile in the Random Tile Pool.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
			//	return false;
			//}
		}

		private void OkButton_Click( object sender, RoutedEventArgs e )
		{
			if ( TryClose() )
			{
				closing = true;
				DialogResult = true;
			}
		}

		private void Window_Closing( object sender, CancelEventArgs e )
		{
			//chapter.randomInteractionGroup = randInter.Text;
			if ( !closing && !TryClose() )
				e.Cancel = true;
		}

		private void ToRandomButton_Click( object sender, RoutedEventArgs e )
		{
			int item = (int)global.SelectedItem;
			AddTile( item );
		}

		private void Global_MouseDoubleClick( object sender, System.Windows.Input.MouseButtonEventArgs e )
		{
			if(global == null || global.SelectedItem == null) { return; }
			int item = (int)global.SelectedItem;
			AddTile( item );
		}

		/// <summary>
		/// if tile count < 5, removes tile from global pool, adds it to Chapter's tileObserver, selected last in list
		/// </summary>
		void AddTile( int id, string side = "A" )
		{
			if ( chapter.tileObserver.Count < 5 )
			{
				scenario.globalTilePool.Remove( id );
				global.UnselectAll();
				HexTile hex = new HexTile( id, true );
				hex.tileSide = side;
				chapter.AddTile( hex );
				selected = hex;
				random.SelectedIndex = random.Items.Count - 1;
				ShowExploreStatus();
			}
		}

		/// <summary>
		/// removes "selected" from tileObserver, adds idNumber back to global
		/// </summary>
		void RemoveTile()
		{
			if ( selected == null )
				return;

			scenario.globalTilePool.Add( selected.idNumber );
			chapter.RemoveTile( selected );

			List<int> sorted = scenario.globalTilePool.OrderBy( key => key ).ToList();
			for ( int i = 0; i < sorted.Count; i++ )
				scenario.globalTilePool[i] = sorted[i];

			selected = null;
			int ret = random.SelectedIndex = 0;
			if ( ret != -1 )
			{
				selected = random.SelectedItem as HexTile;
				tokenEditButton.IsEnabled = true;
				addFlavor.IsEnabled = true;
			}
			if ( random.Items.Count == 0 )
			{
				selected = null;
				tokenEditButton.IsEnabled = false;
				addFlavor.IsEnabled = false;
				sideA.IsEnabled = sideB.IsEnabled = sideRandom.IsEnabled = false;
				exploreStatus.Visibility = Visibility.Collapsed;
			}
		}

		private void ToPoolButton_Click( object sender, RoutedEventArgs e )
		{
			selected = (HexTile)random.SelectedItem;
			RemoveTile();
		}

		private void Random_MouseDoubleClick( object sender, System.Windows.Input.MouseButtonEventArgs e )
		{
			//int item = (int)random.SelectedItem;
			selected = null;
			ListBox cb = e.Source as ListBox;
			selected = cb.SelectedItem as HexTile;
			//if ( s != null )
			//{
			//	var t = ( from HexTile tile in chapter.tileObserver where s.GUID == tile.GUID select tile ).FirstOr( null );
			//	if ( t != null )
			//		selected = t;
			//}

			RemoveTile();
			//if ( item > 0 )
			//{
			//	TileSorter sorter = new TileSorter();
			//	chapter.randomTilePool.Remove( item );
			//	scenario.globalTilePool.Add( item );
			//	List<int> foo = scenario.globalTilePool.ToList();
			//	foo.Sort( sorter );
			//	scenario.globalTilePool.Clear();
			//	foreach ( int s in foo )
			//		scenario.globalTilePool.Add( s );
			//	random.UnselectAll();
			//}

			//UpdateTexts();
		}

		private void groupHelp_Click( object sender, RoutedEventArgs e )
		{
			HelpWindow hw = new HelpWindow( HelpType.Grouping, 1 );
			hw.ShowDialog();
		}

		private void tileGalleryButton_Click( object sender, RoutedEventArgs e )
		{
			GalleryWindow gw = new GalleryWindow( scenario, chapter.tileObserver.Count, true );
			if ( gw.ShowDialog() == true && gw.selectedData.Length > 0 )
			{
				foreach ( var t in gw.selectedData )
				{
					AddTile( t.Item1, t.Item2 );
				}
			}
		}

		private void random_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			selected = null;

			ListBox cb = e.Source as ListBox;
			selected = cb.SelectedItem as HexTile;
			if ( selected != null )
			{
				sideA.IsEnabled = sideB.IsEnabled = sideRandom.IsEnabled = true;
				sideA.IsChecked = selected.tileSide == "A";
				sideB.IsChecked = selected.tileSide == "B";
				sideRandom.IsChecked = selected.tileSide == "Random";
				//if ( !chapter.usesRandomGroups )
					tokenEditButton.IsEnabled = !sideRandom.IsChecked.Value;
				ShowExploreStatus();
			}
		}

		private void Window_Loaded( object sender, RoutedEventArgs e )
		{
		}

		private void tokenEditButton_Click( object sender, RoutedEventArgs e )
		{
			if ( selected == null )
				return;

			TokenEditorWindow tw = new TokenEditorWindow( selected, scenario, true );
			tw.ShowDialog();
		}

		private void sideA_Click( object sender, RoutedEventArgs e )
		{
			if ( selected != null )
				selected.tileSide = "A";
			//if ( !chapter.usesRandomGroups )
			tokenEditButton.IsEnabled = true;
		}

		private void sideB_Click( object sender, RoutedEventArgs e )
		{
			if ( selected != null )
				selected.tileSide = "B";
			//if ( !chapter.usesRandomGroups )
			tokenEditButton.IsEnabled = true;
		}

		private void sideRandom_Click( object sender, RoutedEventArgs e )
		{
			if ( selected != null )
				selected.tileSide = "Random";
			//if ( !chapter.usesRandomGroups )
			tokenEditButton.IsEnabled = false;
		}

		void ShowExploreStatus()
		{
			addFlavor.IsEnabled = true;
			exploreStatus.Visibility = Visibility.Visible;
			if ( string.IsNullOrEmpty( selected.flavorBookData.pages[0] ) )
				exploreStatus.Text = "Exploration Text is Empty";
			else
				exploreStatus.Text = "Exploration Text is Set";
		}

		private void addFlavor_Click( object sender, RoutedEventArgs e )
		{
			if ( selected != null )
			{
				TextEditorWindow tw = new TextEditorWindow( scenario, EditMode.Flavor, selected.flavorBookData );
				if ( tw.ShowDialog() == true )
					selected.flavorBookData.pages = tw.textBookController.pages;

				if ( string.IsNullOrEmpty( selected.flavorBookData.pages[0] ) )
					exploreStatus.Text = "Exploration Text is Empty";
				else
					exploreStatus.Text = "Exploration Text is Set";
			}
		}
	}
}
