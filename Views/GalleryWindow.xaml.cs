using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace JiME.Views
{
	/// <summary>
	/// Interaction logic for GalleryWindow.xaml
	/// </summary>
	public partial class GalleryWindow : Window, INotifyPropertyChanged
	{
		int _page, _maxSelect;
		string _selectedTiles, _max;
		int indexOffset = 0;
		int indexLimit = 0;

		public ObservableCollection<GalleryTile> tileObserver { get; set; }
		public Scenario scenario { get; set; }
		public int maxSelect
		{
			get { return _maxSelect; }
			set { _maxSelect = value; max = ""; }
		}
		public int page
		{
			get { return _page; }
			set
			{
				_page = value;
				PropChanged( "page" );
			}
		}
		public string selectedTiles
		{
			get { return _selectedTiles; }
			set
			{
				_selectedTiles = value;
				PropChanged( "selectedTiles" );
			}
		}
		public string max
		{
			get { return _max; }
			set
			{
				_max = "Select up to " + maxSelect;
				PropChanged( "max" );
			}
		}//max # tiles can be chosen
		public Tuple<int, string>[] selectedData;

		bool side = true;//true=A, false=B
		int tileCount = 0;
		GalleryTile[] galleryTiles;
		public event PropertyChangedEventHandler PropertyChanged;


		void PropChanged( string name )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}

		public GalleryWindow( Scenario s, int used, bool allowSideToggle = true )
		{
			InitializeComponent();
			scenario = s;
			sideB.IsEnabled = allowSideToggle;

			tileObserver = new ObservableCollection<GalleryTile>();

			int[] ids = Utils.LoadTiles().ToArray();
			if (scenario.scenarioTypeJourney)
			{
				//Load the hex tiles
				indexOffset = 0;
				indexLimit = 2;
				tileCount = Utils.tileSourceA.Length - indexLimit;
				galleryTiles = new GalleryTile[tileCount];
			}
			else
            {
				//Just the battle tiles
				indexOffset = 42;
				indexLimit = 0;
				tileCount = 2;
				galleryTiles = new GalleryTile[2];
				ids = new int[] { 998, 999 };
            }

			for ( int i = 0; i < tileCount; i++ )
			{
				galleryTiles[i] = new GalleryTile();
				galleryTiles[i].id = ids[i];
				Debug.Log("Utils.tileSourceA.Count = " + Utils.tileSourceA.Length + " vs attempted index of " + (i + indexOffset));
				galleryTiles[i].source = Utils.tileSourceA[i + indexOffset];
				galleryTiles[i].enabled = galleryTiles[i].enabled && scenario.IsCollectionEnabled(galleryTiles[i].collection);
			}

			for (int i = 0; i < 4 && i < tileCount; i++)
			{
				tileObserver.Add(galleryTiles[i]);
			}

			page = 1;
			left.IsEnabled = false;
			selectedTiles = "None";
			maxSelect = 5 - used;

			DataContext = this;

			UpdateGallery();
		}

		private void left_Click( object sender, RoutedEventArgs e )
		{
			page = Math.Max( page - 1, 1 );
			if ( page == 1 )
				left.IsEnabled = false;
			right.IsEnabled = true;
			UpdateGallery();
		}

		private void right_Click( object sender, RoutedEventArgs e )
		{
			var maxPage = (tileCount + 4) / 4;
			page = Math.Min( page + 1, maxPage );
			if ( page == maxPage )
				right.IsEnabled = false;
			left.IsEnabled = true;
			UpdateGallery();
		}

		void UpdateGallery()
		{
			selectedData = new Tuple<int, string>[selectedTiles.Split( ',' ).Length];

			for ( int i = 0; i < tileCount; i++ )
			{
				if ( !galleryTiles[i].selected && side )
				{
					galleryTiles[i].source = Utils.tileSourceA[i + indexOffset];
					galleryTiles[i].side = "A";
				}
				else if ( !galleryTiles[i].selected && !side )
				{
					galleryTiles[i].source = Utils.tileSourceB[i + indexOffset];
					galleryTiles[i].side = "B";
				}
			}

			tileObserver.Clear();
			int start = ( page - 1 ) * 4;
			for ( int i = start; i < Math.Min(start + 4, tileCount); i++ )
			{
				tileObserver.Add( galleryTiles[i] );
			}

			UpdateEnabled();
		}

		private void okButton_Click( object sender, RoutedEventArgs e )
		{
			DialogResult = false;
		}

		private void tileButton_Click( object sender, RoutedEventArgs e )
		{
			GalleryTile gt = ( (Button)sender ).DataContext as GalleryTile;
			if ( !gt.selected && selectedTiles.Split( ',' ).Length > 4 )
				return;

			gt.selected = !gt.selected;
			gt.side = side ? "A" : "B";

			string[] selected = ( from t in galleryTiles where t.selected select t.id + t.side + "," ).ToArray();
			if ( selected.Length > 0 )
			{
				selectedTiles = selected.Aggregate( ( acc, x ) => { return acc + x; } );
				selectedTiles = selectedTiles.Substring( 0, selectedTiles.Length - 1 );
			}
			else
				selectedTiles = "None";

			UpdateGallery();
		}

		void UpdateEnabled()
		{
			int selected = selected = ( from t in galleryTiles where t.selected select t.id + t.side + "," ).Count();

			if ( maxSelect == selected )
				for ( int i = 0; i < galleryTiles.Length; i++ )
				{
					if ( !galleryTiles[i].selected )
						galleryTiles[i].enabled = false;
				}
			else
			{
				for ( int i = 0; i < galleryTiles.Length; i++ )
					if ( !galleryTiles[i].selected )
						galleryTiles[i].enabled = true;
			}

			for ( int i = 0; i < galleryTiles.Length; i++ )
			{
				if ( !galleryTiles[i].selected && !scenario.globalTilePool.Contains( galleryTiles[i].id ) )
					galleryTiles[i].enabled = false;

				if ( !scenario.IsCollectionEnabled(galleryTiles[i].collection) )
					galleryTiles[i].enabled = false;

			}
		}

		private void sideB_Click( object sender, RoutedEventArgs e )
		{
			side = false;
			UpdateGallery();
		}

		private void sideA_Click( object sender, RoutedEventArgs e )
		{
			side = true;
			UpdateGallery();
		}

		private void addTilesButton_Click( object sender, RoutedEventArgs e )
		{
			selectedData = ( from gt in galleryTiles where gt.selected select new Tuple<int, string>( gt.id, gt.side ) ).ToArray();

			DialogResult = true;
		}
	}
}
