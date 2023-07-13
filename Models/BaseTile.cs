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
using System.Collections.Generic;

namespace JiME
{
	public class BaseTile : Translatable, INotifyPropertyChanged, ITile
	{
		override public string TranslationKeyName() { return idNumber.ToString(); }
		override public string TranslationKeyPrefix() { return idNumber.ToString(); }

		override protected void DefineTranslationAccessors()
		{
			List<TranslationAccessor> list = new List<TranslationAccessor>();
			translationAccessors = list;
		}

		[JsonIgnore]
		public Canvas canvas = new Canvas();
        [JsonIgnore]
		protected Vector tileDims;

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
		public double rotationAngle = 15;
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

        [JsonIgnore]
		public static bool printRect = false;
		[JsonIgnore]
		public Path rectPathShape;
		[JsonIgnore]
		public static bool printPivot = false;
		[JsonIgnore]
		public Path pivotPathShape;
		[JsonIgnore]
		public static bool printClick = false;
		[JsonIgnore]
		public Path clickPathShape;

		public event PropertyChangedEventHandler PropertyChanged;

		protected Point clickV;

		public static BaseTile CreateTile(int n, bool skipBuild = false)
        {
			if(n == 998 || n == 999)
            {
				return new SquareTile(n, skipBuild);
            }
			else
            {
				return new HexTile(n, skipBuild);
            }
        }

		public BaseTile() : base() {}

		public BaseTile(int n, bool skipBuild = false) : base()
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

			//get size dimensions of PATH object
			if (tileSide == "A")
				tileDims = new Vector(Utils.tileDictionary[idNumber].width, Utils.tileDictionary[idNumber].height);
			else
				tileDims = new Vector(Utils.tileDictionaryB[idNumber].width, Utils.tileDictionary[idNumber].height);
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
		virtual protected void Update() { }

		virtual public void Select()
		{
			pathShape.Stroke = new SolidColorBrush( Colors.Red );
			Canvas.SetZIndex(canvas, 100);
			Canvas.SetZIndex( pathShape, 101 );
			Canvas.SetZIndex( tileImage, 102 );
			if (printRect)
				Canvas.SetZIndex(rectPathShape, 100);
			if(printPivot)
				Canvas.SetZIndex(pivotPathShape, 103);
			if (printClick)
				Canvas.SetZIndex(clickPathShape, 104);
		}

		virtual public void Unselect()
		{
			pathShape.Stroke = new SolidColorBrush( Colors.White );
			Canvas.SetZIndex(canvas, 0);
			Canvas.SetZIndex( pathShape, 1 );
			Canvas.SetZIndex( tileImage, 2 );
			if (printRect)
				Canvas.SetZIndex(rectPathShape, 0);
			if (printPivot)
				Canvas.SetZIndex(pivotPathShape, 3);
			if (printClick)
				Canvas.SetZIndex(clickPathShape, 4);
		}

		virtual public void Rehydrate( Canvas parentCanvas )
		{
			BuildShape();
			BuildImage();
			//canvas size comes from the tileImage size; the size will be shared with Token.Rehydrate via canvas being passed in
			//double scale = Math.Max(tileDims.X, tileDims.Y) / 512d;
			canvas.Width = tileDims.X;
			canvas.Height = tileDims.Y;

			//Remove canvas children before re-adding them
			canvas.Children.Clear();
			parentCanvas.Children.Remove(canvas);

			//The tile shape/outline
			pathShape.DataContext = this;
			canvas.Children.Add(pathShape);
			Canvas.SetLeft(pathShape, -30);
			Canvas.SetTop(pathShape, -30);

			//The tile image
			if (useGraphic)
			{
				canvas.Children.Add(tileImage);
				Canvas.SetLeft(tileImage, 0);
				Canvas.SetTop(tileImage, 0);
			}

			//Add tokens
			for (int t = 0; t < tokenList.Count; t++)
			{
				tokenList[t].parentTile = this;
				tokenList[t].parentCanvas = canvas;
				tokenList[t].Rehydrate(canvas);
			}

			//Rectangle for debugging
			if (printRect)
			{
				canvas.Children.Add(rectPathShape);
				Canvas.SetLeft(rectPathShape, 0);
				Canvas.SetTop(rectPathShape, 0);
			}

			//Pivot point for debugging
			if (printPivot)
			{
				canvas.Children.Add(pivotPathShape);
				Canvas.SetLeft(pivotPathShape, 0);
				Canvas.SetTop(pivotPathShape, 0);
			}

			//Pivot point for debugging
			if (printClick)
			{
				canvas.Children.Add(clickPathShape);
				Canvas.SetLeft(clickPathShape, 0);
				Canvas.SetTop(clickPathShape, 0);
			}

			parentCanvas.Children.Add(canvas);
			Update();
		}

		virtual public void ToggleGraphic( Canvas parentCanvas )
		{
			if ( useGraphic )
			{
				if (!canvas.Children.Contains(tileImage))
				{
					canvas.Children.Add(tileImage);
				}
			}
			else
			{
				canvas.Children.Remove(tileImage);
			}
			ChangeColor( color );
		}

		virtual public void ChangeTileSide( string side, Canvas parentCanvas )
		{
			tileSide = side;
			position = new Vector( Utils.dragSnapX[5], Utils.dragSnapY[5] );
			angle = 0;
			Rehydrate( parentCanvas );
			ChangeColor( color );
			Select();
		}

		virtual public void Rotate( double direction, Canvas parentCanvas )
		{
			canvas.Children.Clear();
			parentCanvas.Children.Remove(canvas);
			angle += direction * rotationAngle;
			angle %= 360;
			Rehydrate( parentCanvas );
			ChangeColor( color );
			Select();
		}

		virtual public void SetClickV( MouseButtonEventArgs e, Canvas parentCanvas )
		{
			clickV = new Point();
			//TransformGroup grp = pathShape.RenderTransform as TransformGroup;
			TransformGroup grp = canvas.RenderTransform as TransformGroup;

			Debug.Log("SetClickV transform children: " + grp?.Children.Count);
			if ( grp?.Children.Count == 2 ) //HexTile
			{
				Vector gv = new Vector( ( (TranslateTransform)grp.Children[0] ).X, ( (TranslateTransform)grp.Children[0] ).Y );

				clickV = e.GetPosition( parentCanvas );

				Debug.Log("canvas X:" + gv.X + " Y:" + gv.Y);
				Debug.Log("click   X: " + clickV.X + " Y:" + clickV.Y);
				//clickV.X -= gv.X;
				//clickV.Y -= gv.Y;
				Debug.Log("click/\\ X: " + clickV.X + " Y:" + clickV.Y);

			}
			else if( grp?.Children.Count == 3) //SquareTile
            {
				//Order of the transforms as set in SquareTile was ScaleTransform=0, TranslateTransform=1, RotateTransform=2
				Vector gv = new Vector(((TranslateTransform)grp.Children[1]).X, ((TranslateTransform)grp.Children[1]).Y);
				clickV = e.GetPosition(parentCanvas);
				clickV.X -= gv.X;
				clickV.Y -= gv.Y;
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

			if (tileType == TileType.Hex)
			{
				p.X = (from snapx in Utils.hexSnapX where p.X.WithinTolerance(snapx, 5) select snapx).FirstOr(-1);
				p.Y = (from snapy in Utils.hexSnapY where p.Y.WithinTolerance(snapy, 5) select snapy).FirstOr(-1);
			}
			else if(tileType == TileType.Square)
            {
				p.X = (from snapx in Utils.sqrSnapX where p.X.WithinTolerance(snapx, 5) select snapx).FirstOr(-1);
				p.Y = (from snapy in Utils.sqrSnapY where p.Y.WithinTolerance(snapy, 5) select snapy).FirstOr(-1);
			}
			return p;
		}

		virtual public void Move(double x, double y)
		{
			position = new Vector(position.X + (x * 48), position.Y + (y * 55.4256256d / 2));

			Update();
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
