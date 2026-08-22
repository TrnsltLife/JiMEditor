// Ported from: JiMEditor/Common/HexTileData.cs — Point → Vector2 for Mac MVP (2026-08-09).
using System;

namespace JiME
{
	public class HexTileData
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

		public static Vector2[] ConvertCoords( string coords ) => BaseTileData.ConvertHexCoords( coords );

		public static Vector2 ConvertCoord( string coord ) => BaseTileData.ConvertHexCoord( coord );

		public static Vector2[] ExtractCoords( string coords ) => BaseTileData.ExtractCoords( coords );

		public static Vector2[] Mirror( Vector2[] sideA ) => BaseTileData.Mirror( sideA );
	}
}
