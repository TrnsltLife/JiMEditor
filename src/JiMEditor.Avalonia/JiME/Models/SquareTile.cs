// Ported from: JiMEditor/Models/SquareTile.cs — WPF visuals stripped for Mac MVP (2026-08-09).
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace JiME
{
	public class SquareTile : BaseTile, INotifyPropertyChanged
	{
		public SquareTile()
		{
			tileType = TileType.Square;
			rotationAngle = 90;
		}

		public SquareTile( int n, bool skipBuild = true )
		{
			tileType = TileType.Square;
			rotationAngle = 90;
			InitCommon( n );
			// Ridiculous but covers walls + tokens per large space (WPF parity).
			tokenCount = 33;
			position = new Vector2( Utils.sqrSnapX[5], Utils.sqrSnapY[5] );
			_ = skipBuild;
		}
	}
}
