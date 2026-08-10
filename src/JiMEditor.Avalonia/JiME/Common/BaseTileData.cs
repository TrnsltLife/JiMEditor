// Ported from: JiMEditor/Common/BaseTileData.cs — Point → Vector2 for Mac MVP (2026-08-09).
using System;

namespace JiME
{
	public class BaseTileData
	{
		public int id { get; set; }
		public string coords { get; set; } = "";
		public string pivot { get; set; } = "";
		public float rotation { get; set; }
		public int tileCount;
		public double width, height;

		public void Init()
		{
			tileCount = coords.Split( ' ' ).Length;
		}

		/// <summary>
		/// parses string of coords into absolute vector positions on the canvas
		/// </summary>
		public static Vector2[] ConvertHexCoords( string coords )
		{
			string[] array = coords.Split( ' ' );
			var vectors = new Vector2[array.Length];
			for ( int i = 0; i < array.Length; i++ )
			{
				string[] xy = array[i].Split( ',' );
				double x = Utils.hexSnapX[int.Parse( xy[0] ) + 10];
				double y = Utils.hexSnapY[int.Parse( xy[1] ) + 10];
				vectors[i] = new Vector2( x, y );
			}
			return vectors;
		}

		public static Vector2 ConvertHexCoord( string coord )
		{
			string[] xy = coord.Split( ',' );
			double x = Utils.hexSnapX[int.Parse( xy[0] ) + 10];
			double y = Utils.hexSnapY[int.Parse( xy[1] ) + 10];
			return new Vector2( x, y );
		}

		/// <summary>
		/// parses string of coords into absolute vector positions on the canvas
		/// </summary>
		public static Vector2[] ConvertSquareCoords( string coords, double width, double height )
		{
			double subsquareWidth = width / 3;
			double subsquareHeight = height / 3;
			string[] array = coords.Split( ' ' );
			var vectors = new Vector2[array.Length];
			for ( int i = 0; i < array.Length; i++ )
			{
				string[] xy = array[i].Split( ',' );
				double x = int.Parse( xy[0] ) * subsquareWidth + subsquareWidth / 2;
				double y = int.Parse( xy[1] ) * subsquareHeight + subsquareHeight / 2;
				vectors[i] = new Vector2( x, y );
			}
			return vectors;
		}

		public static Vector2 ConvertSquareCoord( string coord, double width, double height )
		{
			double subsquareWidth = width / 3;
			double subsquareHeight = height / 3;
			string[] xy = coord.Split( ',' );
			double x = int.Parse( xy[0] ) * subsquareWidth + subsquareWidth / 2;
			double y = int.Parse( xy[1] ) * subsquareHeight + subsquareHeight / 2;
			return new Vector2( x, y );
		}

		/// <summary>
		/// parses string of coords into an array of Points that represent the hex sections that are present in this tile
		/// </summary>
		public static Vector2[] ExtractCoords( string coords )
		{
			string[] array = coords.Split( ' ' );
			var vectors = new Vector2[array.Length];
			for ( int i = 0; i < array.Length; i++ )
			{
				string[] xy = array[i].Split( ',' );
				vectors[i] = new Vector2( int.Parse( xy[0] ), int.Parse( xy[1] ) );
			}
			Array.Sort( vectors, ( point1, point2 ) =>
			{
				var xComparison = point1.X.CompareTo( point2.X );
				if ( xComparison != 0 )
					return xComparison;
				return point1.Y.CompareTo( point2.Y );
			} );
			return vectors;
		}

		/// <summary>
		/// Mirror takes a list of hex points and flips or mirrors them horizontally.
		/// </summary>
		public static Vector2[] Mirror( Vector2[] sideA )
		{
			var sideB = new Vector2[sideA.Length];
			double maxX = 0;
			foreach ( Vector2 point in sideA )
			{
				if ( point.X > maxX )
					maxX = point.X;
			}
			int index = 0;
			foreach ( Vector2 point in sideA )
			{
				sideB[index] = new Vector2( maxX - point.X, point.Y );
				index++;
			}

			return sideB;
		}
	}
}
