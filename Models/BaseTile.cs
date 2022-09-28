using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using JiME.Models;

namespace JiME
{
	public class BaseTile : INotifyPropertyChanged, ITile
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
		public Vector pathRoot;
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
		public Path pathShape;
		[JsonIgnore]
		public Image tileImage;
		[JsonIgnore]
		public bool useGraphic;
		[JsonIgnore]
		private Collection _collection;
		[JsonIgnore]
		public Collection collection { get { if (_collection == null) { _collection = Collection.FromTileNumber(idNumber); } return _collection; } 
									   set { _collection = value; } }
		[JsonIgnore]
		public string idNumberAndCollection { get { return "" + idNumber + collection.FontCharacter; } }

		public static bool printRect = false;
		public Path rectPathShape;
		public static bool printPivot = false;
		public Path pivotPathShape;

		public event PropertyChangedEventHandler PropertyChanged;

		protected Point clickV;

		public BaseTile() {}

		public BaseTile( int n, bool skipBuild = false )
		{
			idNumber = n;
			tokenCount = ( n / 100 ) % 10;
			GUID = Guid.NewGuid();
			tileSide = "A";
			position = new Vector( Utils.dragSnapX[5], Utils.dragSnapY[5] );
			tokenList = new ObservableCollection<Token>();
			isStartTile = false;
			triggerName = "None";
			collection = Collection.FromTileNumber(idNumber);

			if ( !skipBuild )
			{
				BuildShape();
				BuildImage();
				Update();
			}
		}

		virtual protected void BuildShape()
		{
			pathShape = new Path();
			pathShape.Stroke = Brushes.White;
			pathShape.StrokeThickness = 2;
			pathShape.Fill = new SolidColorBrush( Color.FromRgb( 70, 70, 74 ) );
		}

		virtual protected void BuildImage()
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

		virtual protected PathFigure BuildRectangle( Point c, double width, double height )
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

		virtual protected PathFigure BuildRegularPolygon( Point c, double r, int numSides, double offsetDegree )
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
		}

		virtual public void ChangeColor( int idx )
		{
			color = idx;
			if ( !useGraphic )
				pathShape.Fill = Utils.hexColors[Math.Max( idx, 0 )];
			else
				pathShape.Fill = new SolidColorBrush( Colors.White );
		}

		/// <summary>
		/// updates shape position on canvas
		/// </summary>
		virtual protected void Update()
		{
			Debug.Log("Update-Base");
			//Apply translation to the pathShape
			pathShape.RenderTransformOrigin = new Point( 0, 0 );
			TranslateTransform tf = new TranslateTransform(position.X, position.Y);

			TransformGroup trnsgrp = new TransformGroup();
			trnsgrp.Children.Add( tf );
			pathShape.RenderTransform = trnsgrp;


			//Apply scale, translation, and rotation to the tile image
			//get size dimensions of PATH object
			Vector dims;
			if ( tileSide == "A" )
				dims = new Vector( 512, 512 );
			else
				dims = new Vector( 512, 512 );

			//calculate the SCALE of largest side (width or height)
			double scale;
			if ( tileImage.Source.Width > tileImage.Source.Height )
				scale = dims.X / 512;
			else
				scale = dims.Y / 512;

			TransformGroup imggrp = new TransformGroup();

			ScaleTransform scaleTransform = new ScaleTransform( scale, scale );

			float imgTranslateX = (float)position.X; // - 32f;
			float imgTranslateY = (float)position.Y; // - 27.7128128f;
			TranslateTransform translateTransform = new TranslateTransform(imgTranslateX, imgTranslateY);
			//Console.WriteLine("imgTranslate: " + imgTranslateX + "," + imgTranslateY);

			RotateTransform rotateTransform = new RotateTransform( angle );
			rotateTransform.CenterX = position.X; // + (pathRoot.X * 32f);
			rotateTransform.CenterY = position.Y; // + (pathRoot.Y * 27.7128128f);
			//Console.WriteLine("rotateCenter: " + rotateTransform.CenterX + "," + rotateTransform.CenterY);

			imggrp.Children.Add( scaleTransform );
			imggrp.Children.Add( translateTransform );
			imggrp.Children.Add( rotateTransform );
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
		}

		virtual public void Select()
		{
			pathShape.Stroke = new SolidColorBrush( Colors.Red );
			Canvas.SetZIndex( pathShape, 100 );
			Canvas.SetZIndex( tileImage, 101 );
			if (printRect)
				Canvas.SetZIndex(rectPathShape, 100);
			if(printPivot)
				Canvas.SetZIndex(pivotPathShape, 102);
		}

		virtual public void Unselect()
		{
			pathShape.Stroke = new SolidColorBrush( Colors.White );
			Canvas.SetZIndex( pathShape, 0 );
			Canvas.SetZIndex( tileImage, 1 );
		}

		virtual public void Rehydrate( Canvas canvas )
		{
			BuildShape();
			BuildImage();
			pathShape.DataContext = this;
			canvas.Children.Add( pathShape );
			if ( useGraphic )
				canvas.Children.Add( tileImage );
			Update();
		}

		virtual public void ToggleGraphic( Canvas canvas )
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

		virtual public void ChangeTileSide( string side, Canvas canvas )
		{
			tileSide = side;
			position = new Vector( Utils.dragSnapX[5], Utils.dragSnapY[5] );
			angle = 0;
			Rehydrate( canvas );
			ChangeColor( color );
			Select();
		}

		virtual public void Rotate( double amount, Canvas canvas )
		{
			//4,57.7128128
			canvas.Children.Remove( pathShape );
			canvas.Children.Remove( tileImage );
			if(printRect)
				canvas.Children.Remove(rectPathShape);
			angle += amount;
			angle %= 360;
			Rehydrate( canvas );
			ChangeColor( color );
			Select();
		}

		virtual public void SetClickV( MouseButtonEventArgs e, Canvas canvas )
		{
			clickV = new Point();
			TransformGroup grp = pathShape.RenderTransform as TransformGroup;

			if ( grp?.Children.Count == 1 )
			{
				Vector gv = new Vector( ( (TranslateTransform)grp.Children[0] ).X, ( (TranslateTransform)grp.Children[0] ).Y );
				//Debug.Log( "shape position(center):" + gv.X + "," + gv.Y );
				//Debug.Log( $"dims: {pathShape.ActualWidth},{pathShape.ActualHeight}" );
				clickV = e.GetPosition( canvas );
				clickV.X -= gv.X;
				clickV.Y -= gv.Y;
				Debug.Log( "clickV:" + clickV );
			}
		}

		virtual public void Drag( MouseEventArgs e, Canvas canvas )
		{
			Vector clickPoint = new Vector( e.GetPosition( canvas ).X - clickV.X, e.GetPosition( canvas ).Y - clickV.Y );

			Vector snapped = new Vector();
			snapped.X = ( from snapx in Utils.dragSnapX where clickPoint.X.WithinTolerance( snapx, Utils.tolerance ) select snapx ).FirstOr( -1 );
			snapped.Y = ( from snapy in Utils.dragSnapY where clickPoint.Y.WithinTolerance( snapy, Utils.tolerance ) select snapy ).FirstOr( -1 );

			position = snapped;

			if ( snapped != new Vector( -1, -1 ) )
				Update();
		}

		virtual protected Point RotatePoint( Point pointToRotate, Point centerPoint, double angleInDegrees )
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

		virtual public void RenameTrigger( string oldName, string newName )
		{
			foreach ( Token t in tokenList )
			{
				if ( t.triggerName == oldName )
					t.triggerName = newName;
			}

			if ( triggerName == oldName )
				triggerName = newName;
		}

		virtual protected void PropChanged( string name )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}
	}
}
