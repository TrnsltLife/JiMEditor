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
	public class HexTile : BaseTile, INotifyPropertyChanged
	{
		public TextBookData flavorBookData { get; set; }

		public HexTile() 
		{
			tileType = TileType.Hex;
			rotationAngle = 60;
		}

		public HexTile( int n, bool skipBuild = false )
		{
			tileType = TileType.Hex;
			rotationAngle = 60;
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
			collection = Collection.FromTileNumber(idNumber);

			if ( !skipBuild )
			{
				BuildShape();
				BuildImage();
				Update();
			}
		}

		override protected void BuildShape()
		{
			//where is this from?????
			//dims: 68.8660278320313,86.4256286621094 of ONE hexagon

			//canvas grid 32x28
			//hexagon ratio 1 : 1.1547005
			//unit dims: 1 x 0.8660254 = hex ratio

			//dims 64 x 55.4256256
			// distance between hextiles = 55.425626

			pathShape = new Path();
			pathShape.Stroke = Brushes.White;
			pathShape.StrokeThickness = 2;
			pathShape.Fill = new SolidColorBrush( Color.FromRgb( 70, 70, 74 ) );

			BaseTileData hexdata;
			if ( tileSide == "A" )
				hexdata = Utils.tileDictionary[idNumber];
			else
				hexdata = Utils.tileDictionaryB[idNumber];

			//store the FIRST hexagon position (pathRoot) that makes up the tile (searching top to bottom from left to right)
			//pathRoot is needed by companion app to calculate offsets
			Point[] hexPoints = BaseTileData.ExtractCoords(hexdata.coords);
			pathRoot = new Vector(hexPoints[0].X, hexPoints[0].Y);

			//local coords, example: (0,1) translated to canvas coords
			Point[] hexPositions = BaseTileData.ConvertHexCoords( String.Join(" ", hexPoints )); //Use hexPoints because it is sorted from top to bottom, left to right
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
			pathShape.Data = new PathGeometry( hexfigures );
			pathShape.DataContext = this;


			if (printRect)
			{
				//Rectangle for verifying proper size of BaseTileData width and height
				rectPathShape = new Path();
				rectPathShape.Stroke = Brushes.White;
				rectPathShape.StrokeThickness = 2;
				rectPathShape.Fill = new SolidColorBrush(Color.FromRgb(70, 70, 74));
				PathFigure[] rectfigures = new PathFigure[1];
				rectfigures[0] = BuildRectangle(new Point(0, 0), Utils.tileDictionary[idNumber].width, Utils.tileDictionary[idNumber].height);
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
		}

		/// <summary>
		/// updates shape position on canvas
		/// </summary>
		override protected void Update()
		{
			Debug.Log("Update-Hex");
			//Apply translation to the pathShape
			pathShape.RenderTransformOrigin = new Point( 0, 0 );
			TranslateTransform tf = new TranslateTransform(position.X, position.Y);

			TransformGroup hexgrp = new TransformGroup();
			hexgrp.Children.Add( tf );
			pathShape.RenderTransform = hexgrp;


			//Apply scale, translation, and rotation to the tile image
			//get size dimensions of PATH object
			Vector dims;
			if ( tileSide == "A" )
				dims = new Vector( Utils.tileDictionary[idNumber].width, Utils.tileDictionary[idNumber].height );
			else
				dims = new Vector( Utils.tileDictionaryB[idNumber].width, Utils.tileDictionary[idNumber].height );

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
			rotateTransform.CenterX = position.X + (pathRoot.X * 32f);
			rotateTransform.CenterY = position.Y + (pathRoot.Y * 27.7128128f);
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

		override public void Drag( MouseEventArgs e, Canvas canvas )
		{
			Vector clickPoint = new Vector( e.GetPosition( canvas ).X - clickV.X, e.GetPosition( canvas ).Y - clickV.Y );

			Vector snapped = new Vector();
			snapped.X = ( from snapx in Utils.dragSnapX where clickPoint.X.WithinTolerance( snapx, Utils.tolerance ) select snapx ).FirstOr( -1 );
			snapped.Y = ( from snapy in Utils.dragSnapY where clickPoint.Y.WithinTolerance( snapy, Utils.tolerance ) select snapy ).FirstOr( -1 );

			position = snapped;

			if ( snapped != new Vector( -1, -1 ) )
				Update();
		}
	}
}
