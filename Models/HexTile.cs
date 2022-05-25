﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace JiME
{
	public class HexTile : INotifyPropertyChanged, ITile
	{
		string _tileSide, _triggerName;
		bool _isStartTile;

		public double angle { get; set; }
		public int idNumber { get; set; }
		public int tokenCount { get; set; }
		public Guid GUID { get; set; }
		public string tileSide
		{
			get => _tileSide;
			set
			{
				_tileSide = value;
				PropChanged( "tileSide" );
			}
		}
		public Vector position { get; set; }
		public TileType tileType { get; set; }
		public TextBookData flavorBookData { get; set; }
		public ObservableCollection<Token> tokenList { get; set; }
		public bool isStartTile
		{
			get => _isStartTile;
			set
			{
				_isStartTile = value;
				PropChanged( "isStartTile" );
			}
		}
		public int color;
		public Vector hexRoot;
		public string triggerName
		{
			get { return _triggerName; }
			set
			{
				_triggerName = value;
				PropChanged( "triggerName" );
			}
		}

		[JsonIgnore]
		public Path hexPathShape;
		[JsonIgnore]
		public Image tileImage;
		[JsonIgnore]
		public bool useGraphic;

		public static bool printRect = false;
		public Path rectPathShape;
		public static bool printPivot = false;
		public Path pivotPathShape;

		public event PropertyChangedEventHandler PropertyChanged;

		Point clickV;

		public HexTile() { }

		//public HexTile( int n, Vector position, float angle )
		//{
		//	tileType = TileType.Hex;
		//	idNumber = n;
		//	tokenCount = ( n / 100 ) % 10;
		//	GUID = Guid.NewGuid();
		//	tileSide = "A";
		//	this.position = position;
		//	this.angle = angle;
		//}

		public HexTile( int n, bool skipBuild = false )
		{
			tileType = TileType.Hex;
			idNumber = n;
			tokenCount = ( n / 100 ) % 10;
			GUID = Guid.NewGuid();
			tileSide = "A";
			position = new Vector( Utils.dragSnapX[5], Utils.dragSnapY[5] );
			tokenList = new ObservableCollection<Token>();
			flavorBookData = new TextBookData( "" );
			flavorBookData.pages.Add( "" );
			isStartTile = false;
			triggerName = "None";

			if ( !skipBuild )
			{
				BuildShape();
				BuildImage();
				Update();
			}
		}

		void BuildShape()
		{
			//where is this from?????
			//dims: 68.8660278320313,86.4256286621094 of ONE hexagon

			//canvas grid 32x28
			//hexagon ratio 1 : 1.1547005
			//unit dims: 1 x 0.8660254 = hex ratio

			//dims 64 x 55.4256256
			// distance between hextiles = 55.425626

			hexPathShape = new Path();
			hexPathShape.Stroke = Brushes.White;
			hexPathShape.StrokeThickness = 2;
			hexPathShape.Fill = new SolidColorBrush( Color.FromRgb( 70, 70, 74 ) );

			HexTileData hexdata;
			if ( tileSide == "A" )
				hexdata = Utils.hexDictionary[idNumber];
			else
				hexdata = Utils.hexDictionaryB[idNumber];

			//store the FIRST hexagon position (hexRoot) that makes up the tile (searching top to bottom from left to right)
			//hexRoot is needed by companion app to calculate offsets
			Point[] hexPoints = HexTileData.ExtractCoords(hexdata.coords);
			hexRoot = new Vector(hexPoints[0].X, hexPoints[0].Y);

			//local coords, example: (0,1) translated to canvas coords
			Point[] hexPositions = HexTileData.ConvertCoords( String.Join(" ", hexPoints )); //Use hexPoints because it is sorted from top to bottom, left to right
			Point center = hexPositions[0];
			PathFigure[] hexfigures = new PathFigure[hexdata.tileCount];
			for ( int i = 0; i < hexfigures.Length; i++ )
			{
				hexPositions[i] = RotatePoint( hexPositions[i],
					//new Point( 0, 55.4256256d / 2 ), 
					center,
					angle );
				hexfigures[i] = BuildRegularPolygon( hexPositions[i], 32, 6, 0 );
			}
			hexPathShape.Data = new PathGeometry( hexfigures );
			hexPathShape.DataContext = this;


			if (printRect)
			{
				//Rectangle for verifying proper size of HexTileData width and height
				rectPathShape = new Path();
				rectPathShape.Stroke = Brushes.White;
				rectPathShape.StrokeThickness = 2;
				rectPathShape.Fill = new SolidColorBrush(Color.FromRgb(70, 70, 74));
				PathFigure[] rectfigures = new PathFigure[1];
				int rectOffsetY = -28; // -28 * (int)hexPoints[0].Y;
				rectfigures[0] = BuildRectangle(new Point(0, 0), Utils.hexDictionary[idNumber].width, Utils.hexDictionary[idNumber].height);
				rectPathShape.Data = new PathGeometry(rectfigures);
				rectPathShape.DataContext = this;
			}

			if (printPivot)
			{
				//Pivot center for rotation - debug display
				pivotPathShape = new Path();
				pivotPathShape.Stroke = Brushes.Magenta;
				pivotPathShape.StrokeThickness = 2;
				pivotPathShape.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
				PathFigure[] pivotfigures = new PathFigure[1];
				pivotfigures[0] = BuildRectangle(new Point(-1, -1), 3, 3);
				pivotPathShape.Data = new PathGeometry(pivotfigures);
				pivotPathShape.DataContext = this;
			}


			//Debug.Log( "POS:" + position );
			//Debug.Log( "hexpos:" + hexPositions[0] );
			//string[] array = hexdata.coords.Split( ' ' );
			//string[] xy = array[0].Split( ',' );
			//Debug.Log( "xy: " + xy[0] + ", " + xy[1] );
			//Point p = new Point( double.Parse( xy[0] ), double.Parse( xy[1] ) );
			////p = new Point( p.X / 2, p.Y / 2 );
			//Point c = new Point( 0, 1 );
			//Debug.Log( "POINT: " + c );
			//Vector diff = p - c;
			//diff /= 2;
			//Debug.Log( diff );
			//Point newpos = new Point( diff.X, diff.Y * 55.4256256 );
			//position += new Vector( newpos.X, newpos.Y );
			//Debug.Log( "NEW:" + position );
		}

		void BuildImage()
		{
			tileImage = new Image();
			int idx = Utils.LoadTiles().IndexOf( idNumber );
			if ( tileSide == "A" )
				tileImage.Source = Utils.tileSourceA[idx];
			else
				tileImage.Source = Utils.tileSourceB[idx];
			tileImage.Width = Math.Ceiling( tileImage.Source.Width );
			tileImage.Height = Math.Ceiling( tileImage.Source.Height );
			tileImage.IsHitTestVisible = false;
		}

		PathFigure BuildRectangle( Point c, double width, double height )
        {
			LineSegment[] segments = new LineSegment[5];
			Point cur = c;
			Point startPoint = cur;
			cur.X += width;
			segments[0] = new LineSegment(new Point(cur.X, cur.Y), true);
			cur.Y += height;
            segments[1] = new LineSegment(new Point(cur.X, cur.Y), true);
			cur.X -= width;
			segments[2] = new LineSegment(new Point(cur.X, cur.Y), true);
			cur.Y -= height;
			segments[3] = new LineSegment(new Point(cur.X, cur.Y), true);
			segments[4] = new LineSegment(startPoint, true);
			PathFigure figure = new PathFigure(startPoint, segments, false);
			return figure;
		}

		PathFigure BuildRegularPolygon( Point c, double r, int numSides, double offsetDegree )
		{
			// c is the center, r is the radius,
			// numSides the number of sides, offsetDegree the offset in Degrees.
			// Do not add the last point.
			double a = Math.PI * offsetDegree / 180.0d;
			double step = 2d * Math.PI / Math.Max( numSides, 3 );

			LineSegment[] segments = new LineSegment[numSides + 1];

			Point cur = c;
			cur.X = c.X + r * Math.Cos( a );
			cur.Y = c.Y + r * Math.Sin( a );
			Point startPoint = cur;
			for ( int i = 0; i < numSides; i++, a += step )
			{
				cur.X = c.X + r * Math.Cos( a );
				cur.Y = c.Y + r * Math.Sin( a );
				//Debug.Log( cur );
				segments[i] = new LineSegment( new Point( cur.X, cur.Y ), true );
			}
			segments[numSides] = new LineSegment( startPoint, true );
			PathFigure figure = new PathFigure( startPoint, segments, false );

			//Debug.Log( "end poly" );
			return figure;

			//StreamGeometry geometry = new StreamGeometry();

			//using ( StreamGeometryContext ctx = geometry.Open() )
			//{
			//	double step = 2d * Math.PI / Math.Max( numSides, 3 );
			//	Point cur = c;

			//	cur.X = c.X + r * Math.Cos( a );
			//	cur.Y = c.Y + r * Math.Sin( a );
			//	ctx.BeginFigure( cur, true, true );

			//	for ( int i = 0; i < numSides; i++, a += step )
			//	{
			//		cur.X = c.X + r * Math.Cos( a );
			//		cur.Y = c.Y + r * Math.Sin( a );
			//		ctx.LineTo( cur, true, false );
			//	}
			//}

			//return geometry;
		}

		public void ChangeColor( int idx )
		{
			color = idx;
			if ( !useGraphic )
				hexPathShape.Fill = Utils.hexColors[Math.Max( idx, 0 )];
			else
				hexPathShape.Fill = new SolidColorBrush( Colors.White );
		}

		/// <summary>
		/// updates shape position on canvas
		/// </summary>
		void Update()
		{
			//Apply translation to the hexPathShape
			hexPathShape.RenderTransformOrigin = new Point( 0, 0 );
			TranslateTransform tf = new TranslateTransform(position.X, position.Y);

			TransformGroup hexgrp = new TransformGroup();
			hexgrp.Children.Add( tf );
			hexPathShape.RenderTransform = hexgrp;


			//Apply scale, translation, and rotation to the tile image
			//get size dimensions of PATH object
			Vector dims;
			if ( tileSide == "A" )
				dims = new Vector( Utils.hexDictionary[idNumber].width, Utils.hexDictionary[idNumber].height );
			else
				dims = new Vector( Utils.hexDictionaryB[idNumber].width, Utils.hexDictionary[idNumber].height );

			//calculate the SCALE of largest side (width or height)
			double scale;
			if ( tileImage.Source.Width > tileImage.Source.Height )
				scale = dims.X / 512;
			else
				scale = dims.Y / 512;

			TransformGroup imggrp = new TransformGroup();

			ScaleTransform scaleTransform = new ScaleTransform( scale, scale );

			float imgTranslateX = (float)position.X - 32f;
			float imgTranslateY = (float)position.Y - 27.7128128f;
			TranslateTransform translateTransform = new TranslateTransform(imgTranslateX, imgTranslateY);
			//Console.WriteLine("imgTranslate: " + imgTranslateX + "," + imgTranslateY);

			RotateTransform rotateTransform = new RotateTransform( angle );
			rotateTransform.CenterX = position.X + (hexRoot.X * 32f);
			rotateTransform.CenterY = position.Y + (hexRoot.Y * 27.7128128f);
			//Console.WriteLine("rotateCenter: " + rotateTransform.CenterX + "," + rotateTransform.CenterY);

			imggrp.Children.Add( scaleTransform );
			imggrp.Children.Add( translateTransform );
			imggrp.Children.Add(rotateTransform);
			tileImage.RenderTransform = imggrp;

			if (printRect)
			{
				TransformGroup rectgrp = new TransformGroup();
				rectgrp.Children.Add(translateTransform);
				rectgrp.Children.Add(rotateTransform);
				rectPathShape.RenderTransform = rectgrp;
			}

			if (printPivot)
			{
				TransformGroup pivotgrp = new TransformGroup();
				TranslateTransform pivottf = new TranslateTransform(rotateTransform.CenterX, rotateTransform.CenterY);
				pivotgrp.Children.Add(pivottf);
				pivotPathShape.RenderTransform = pivotgrp;
			}

			//dims 64 x 55.4256256
			//27.7128128
		}

		public void Select()
		{
			hexPathShape.Stroke = new SolidColorBrush( Colors.Red );
			Canvas.SetZIndex( hexPathShape, 100 );
			Canvas.SetZIndex( tileImage, 101 );
			if (printRect)
				Canvas.SetZIndex(rectPathShape, 100);
			if(printPivot)
				Canvas.SetZIndex(pivotPathShape, 102);
		}

		public void Unselect()
		{
			hexPathShape.Stroke = new SolidColorBrush( Colors.White );
			Canvas.SetZIndex( hexPathShape, 0 );
			Canvas.SetZIndex( tileImage, 1 );
		}

		public void Rehydrate( Canvas canvas )
		{
			BuildShape();
			BuildImage();
			hexPathShape.DataContext = this;
			canvas.Children.Add( hexPathShape );
			if ( useGraphic )
				canvas.Children.Add( tileImage );
			Update();
		}

		public void ToggleGraphic( Canvas canvas )
		{
			if ( useGraphic )
			{
				if ( !canvas.Children.Contains( tileImage ) )
					canvas.Children.Add( tileImage );
			}
			else
			{
				canvas.Children.Remove( tileImage );
			}
			ChangeColor( color );
		}

		public void ChangeTileSide( string side, Canvas canvas )
		{
			tileSide = side;
			position = new Vector( Utils.dragSnapX[5], Utils.dragSnapY[5] );
			angle = 0;
			Rehydrate( canvas );
			ChangeColor( color );
			Select();
		}

		public void Rotate( double amount, Canvas canvas )
		{
			//4,57.7128128
			canvas.Children.Remove( hexPathShape );
			canvas.Children.Remove( tileImage );
			if(printRect)
				canvas.Children.Remove(rectPathShape);
			angle += amount;
			angle %= 360;
			Rehydrate( canvas );
			ChangeColor( color );
			Select();
		}

		public void SetClickV( MouseButtonEventArgs e, Canvas canvas )
		{
			clickV = new Point();
			TransformGroup grp = hexPathShape.RenderTransform as TransformGroup;

			if ( grp?.Children.Count == 1 )
			{
				Vector gv = new Vector( ( (TranslateTransform)grp.Children[0] ).X, ( (TranslateTransform)grp.Children[0] ).Y );
				//Debug.Log( "shape position(center):" + gv.X + "," + gv.Y );
				//Debug.Log( $"dims: {hexPathShape.ActualWidth},{hexPathShape.ActualHeight}" );
				clickV = e.GetPosition( canvas );
				clickV.X -= gv.X;
				clickV.Y -= gv.Y;
				//Debug.Log( "clickV:" + clickV );
			}
		}

		public void Drag( MouseEventArgs e, Canvas canvas )
		{
			Vector clickPoint = new Vector( e.GetPosition( canvas ).X - clickV.X, e.GetPosition( canvas ).Y - clickV.Y );

			Vector snapped = new Vector();
			snapped.X = ( from snapx in Utils.dragSnapX where clickPoint.X.WithinTolerance( snapx, Utils.tolerance ) select snapx ).FirstOr( -1 );
			snapped.Y = ( from snapy in Utils.dragSnapY where clickPoint.Y.WithinTolerance( snapy, Utils.tolerance ) select snapy ).FirstOr( -1 );

			position = snapped;

			if ( snapped != new Vector( -1, -1 ) )
				Update();
		}

		Point RotatePoint( Point pointToRotate, Point centerPoint, double angleInDegrees )
		{
			double angleInRadians = angleInDegrees * ( Math.PI / 180 );
			double cosTheta = Math.Cos( angleInRadians );
			double sinTheta = Math.Sin( angleInRadians );
			Point p = new Point
			{
				X =
							(int)
							( cosTheta * ( pointToRotate.X - centerPoint.X ) -
							sinTheta * ( pointToRotate.Y - centerPoint.Y ) + centerPoint.X ),
				Y =
							(int)
							( sinTheta * ( pointToRotate.X - centerPoint.X ) +
							cosTheta * ( pointToRotate.Y - centerPoint.Y ) + centerPoint.Y )
			};

			p.X = ( from snapx in Utils.hexSnapX where p.X.WithinTolerance( snapx, 5 ) select snapx ).FirstOr( -1 );
			p.Y = ( from snapy in Utils.hexSnapY where p.Y.WithinTolerance( snapy, 5 ) select snapy ).FirstOr( -1 );
			return p;
		}

		public void RenameTrigger( string oldName, string newName )
		{
			foreach ( Token t in tokenList )
			{
				if ( t.triggerName == oldName )
					t.triggerName = newName;
			}

			if ( triggerName == oldName )
				triggerName = newName;
		}

		void PropChanged( string name )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}
	}
}
