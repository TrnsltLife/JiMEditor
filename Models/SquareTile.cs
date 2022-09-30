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
	public class SquareTile : BaseTile, INotifyPropertyChanged
	{
		public SquareTile()
		{
			tileType = TileType.Hex;
			rotationAngle = 90;
		}

		public SquareTile( int n, bool skipBuild = false )
		{
			tileType = TileType.Square;
			rotationAngle = 90;
			idNumber = n;
			tokenCount = ( n / 100 ) % 10;
			GUID = Guid.NewGuid();
			tileSide = "A";
			position = new Vector( Utils.sqrSnapX[5], Utils.sqrSnapY[5] );
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

        protected override void BuildShape()
        {
			double img2PathScale = 4d / 3d;
			double shrinkScale = 0.8d;

			base.BuildShape();
			pathShape = new Path();
			pathShape.Stroke = Brushes.White;
			pathShape.StrokeThickness = 2;
			pathShape.Fill = new SolidColorBrush(Color.FromRgb(70, 70, 74));

			BaseTileData sqrdata;
			if (tileSide == "A")
				sqrdata = Utils.tileDictionary[idNumber];
			else
				sqrdata = Utils.tileDictionaryB[idNumber];

			//store the FIRST sub-square position (pathRoot) that makes up the tile (searching top to bottom from left to right)
			//pathRoot is needed by companion app to calculate offsets
			Point[] sqrPoints = BaseTileData.ExtractCoords(sqrdata.coords);
			pathRoot = new Vector(sqrPoints[0].X, sqrPoints[0].Y);

			//local coords, example: (0,1) translated to canvas coords
			//Use sqrPoints because it is sorted from top to bottom, left to right
			Point[] sqrCenterPositions = BaseTileData.ConvertSquareCoords(String.Join(" ", sqrPoints), 
				Utils.tileDictionary[idNumber].width, Utils.tileDictionary[idNumber].height);
			Point center = sqrCenterPositions[4]; //[0]

			List<PathFigure> figures = new List<PathFigure>();
			double radius = Math.Sqrt(sqrdata.width/3d * sqrdata.width/3d + sqrdata.height/3d + sqrdata.height/3d) / 2d * img2PathScale;
			radius += 4d;
			for (int i = 0; i < sqrdata.tileCount; i++)
			{
				figures.Add(BuildRegularPolygon(sqrCenterPositions[i], radius, 4, 45));
			}
			pathShape.Data = new PathGeometry(figures, FillRule.Nonzero, null);
			pathShape.DataContext = this;

			if (printPivot)
			{
				//Pivot center for rotation - debug display
				pivotPathShape = new Path();
				pivotPathShape.Stroke = Brushes.Magenta;
				pivotPathShape.StrokeThickness = 2;
				pivotPathShape.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
				PathFigure[] pivotfigures = new PathFigure[1];
				pivotfigures[0] = BuildRectangle(new Point(0, 0), 3, 3);
				pivotPathShape.Data = new PathGeometry(pivotfigures);
				pivotPathShape.DataContext = this;
			}
		}

		/// <summary>
		/// updates shape position on canvas
		/// </summary>
		override protected void Update()
		{
			double img2PathScale = 4d / 3d; //I'm not sure why this is but the image comes out scaled bigger than the pathShape?
			double shrinkScale = 0.8d;

			//get size dimensions of PATH object
			Vector dims;
			if (tileSide == "A")
				dims = new Vector(Utils.tileDictionary[idNumber].width, Utils.tileDictionary[idNumber].height);
			else
				dims = new Vector(Utils.tileDictionaryB[idNumber].width, Utils.tileDictionary[idNumber].height);

			//calculate the SCALE of largest side (width or height)
			double scale;
			if (tileImage.Source.Width > tileImage.Source.Height)
				scale = dims.X / 512;
			else
				scale = dims.Y / 512;

			//Console.WriteLine("Image: " + dims.X + "," + dims.Y + " Scale: " + scale + " Resized: " + (dims.X * scale) + "," + (dims.Y * scale));


			//Move the pathShape drawing to the location on the screen the user has moved it to (as opposed to its basic origin at 0,0)
			//Apply translation to the pathShape
			pathShape.RenderTransformOrigin = new Point( 0, 0 );
			TranslateTransform pathTranslateTransform = new TranslateTransform(position.X, position.Y);
			RotateTransform pathRotateTransform = new RotateTransform(angle);
			pathRotateTransform.CenterX = position.X + (dims.X / 2d * img2PathScale * shrinkScale); // (pathRoot.X * 150f);
			pathRotateTransform.CenterY = position.Y + (dims.Y / 2d * img2PathScale * shrinkScale); // (pathRoot.Y * 150f);
			ScaleTransform pathScaleTransform = new ScaleTransform(img2PathScale * shrinkScale, img2PathScale * shrinkScale);
			TransformGroup sqrgrp = new TransformGroup();
			sqrgrp.Children.Add(pathScaleTransform);
			sqrgrp.Children.Add(pathTranslateTransform);
			sqrgrp.Children.Add(pathRotateTransform);
			pathShape.RenderTransform = sqrgrp;

			//Apply scale, translation, and rotation to the tile image

			TransformGroup imggrp = new TransformGroup();

			ScaleTransform scaleTransform = new ScaleTransform( scale * shrinkScale, scale * shrinkScale );

			float imgTranslateX = (float)position.X;// - 150f;
			float imgTranslateY = (float)position.Y;// - 150f;
			TranslateTransform translateTransform = new TranslateTransform(imgTranslateX, imgTranslateY);
			//Console.WriteLine("imgTranslate: " + imgTranslateX + "," + imgTranslateY);

			RotateTransform rotateTransform = new RotateTransform( angle );
			rotateTransform.CenterX = position.X + (dims.X / 2 * img2PathScale * shrinkScale); // (pathRoot.X * 150f);
			rotateTransform.CenterY = position.Y + (dims.Y / 2 * img2PathScale * shrinkScale); // (pathRoot.Y * 150f);
			//Console.WriteLine("rotateCenter: " + rotateTransform.CenterX + "," + rotateTransform.CenterY);

			imggrp.Children.Add(scaleTransform);
			imggrp.Children.Add(translateTransform);
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
			snapped.X = (int)(clickPoint.X / 25) * 25;
			snapped.Y = (int)(clickPoint.Y / 25) * 25;

			position = snapped;

			if ( snapped != new Vector( -1, -1 ) )
				Update();
		}
	}
}
