using System;
using System.Windows;

namespace JiME
{
	public class HexTileData
	{
		public int id { get; set; }
		public string coords { get; set; }
		public string pivot { get; set; }
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
		public static Point[] ConvertCoords( string coords )
		{
			Point[] vectors;
			string[] array = coords.Split( ' ' );
			vectors = new Point[array.Length];
			for ( int i = 0; i < array.Length; i++ )
			{
				string[] xy = array[i].Split( ',' );
				double x = Utils.hexSnapX[int.Parse( xy[0] ) + 10];
				double y = Utils.hexSnapY[int.Parse( xy[1] ) + 10];
				vectors[i] = new Point( x, y );
			}
			return vectors;
		}

		public static Point ConvertCoord( string coord )
        {
			Point vector;
			string[] xy = coord.Split(',');
			double x = Utils.hexSnapX[int.Parse(xy[0]) + 10];
			double y = Utils.hexSnapY[int.Parse(xy[1]) + 10];
			vector = new Point(x, y);
			return vector;
		}

		/// <summary>
		/// parses string of coords into an array of Points that represent the hex sections that are present in this tile
		/// </summary>
		public static Point[] ExtractCoords( string coords )
        {
			//Console.WriteLine("ExtractCoords: " + coords);
			Point[] vectors;
			string[] array = coords.Split(' ');
			vectors = new Point[array.Length];
			for(int i=0; i < array.Length; i++)
            {
				string[] xy = array[i].Split(',');
				vectors[i] = new Point(int.Parse(xy[0]), int.Parse(xy[1]));
            }
			//Sort Points based on X value first then on Y value. So all hexes in the first column come before all hexes in the second column.
			Array.Sort(vectors, delegate(Point point1, Point point2) {
				var xComparison = point1.X.CompareTo(point2.X);
				if (xComparison != 0) { return xComparison; }
				else return point1.Y.CompareTo(point2.Y);
            });
			//Console.WriteLine("Results:       " + String.Join(" ", vectors));
			return vectors;
        }

		/// <summary>
		/// Mirror takes a list of hex points and flips or mirrors them horizontally. Useful for generating Side B of a tile once Side A's points have been entered.
		/// </summary>
		public static Point[] Mirror(Point[] sideA)
		{
			Point[] sideB = new Point[sideA.Length];
			double maxX = 0;
			foreach (Point point in sideA)
			{
				if(point.X > maxX) { maxX = point.X; }
			}
			int index = 0;
			foreach(Point point in sideA)
            {
				sideB[index] = new Point(maxX - point.X, point.Y);
				index++;
            }

			return sideB;
		}
	}
}
