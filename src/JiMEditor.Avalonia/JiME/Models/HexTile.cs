// Ported from: JiMEditor/Models/HexTile.cs — WPF visuals stripped for Mac MVP (2026-08-09).
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace JiME
{
	public class HexTile : BaseTile, INotifyPropertyChanged
	{
		override protected void DefineTranslationAccessors()
		{
			translationAccessors = new List<TranslationAccessor>()
			{
				new TranslationAccessor( "tile.{0}.exploredText", () => this.flavorBookData.pages[0] )
			};
		}

		public TextBookData flavorBookData { get; set; }

		public HexTile()
		{
			tileType = TileType.Hex;
			rotationAngle = 60;
			flavorBookData = new TextBookData( "" );
			flavorBookData.pages.Add( "" );
		}

		public HexTile( int n, bool skipBuild = true )
		{
			tileType = TileType.Hex;
			rotationAngle = 60;
			InitCommon( n );
			flavorBookData = new TextBookData( "" );
			flavorBookData.pages.Add( "" );
			_ = skipBuild;
		}
	}
}
