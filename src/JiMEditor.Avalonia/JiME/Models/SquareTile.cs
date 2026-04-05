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
			//tokenCount = ( n / 100 ) % 10;
			tokenCount = 33; //This is ridiculous but would cover all 12 outside walls, all 8 inside walls, 3 additional tokens per large space, and 1 for the center space
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

			if (printClick)
			{
				//Click point for dragging
				clickPathShape = new Path();
				clickPathShape.Stroke = Brushes.Magenta;
				clickPathShape.StrokeThickness = 2;
				clickPathShape.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 0));
				PathFigure[] clickfigures = new PathFigure[1];
				clickfigures[0] = BuildRectangle(new Point(-1, -1), 3, 3);
				clickPathShape.Data = new PathGeometry(clickfigures);
				clickPathShape.DataContext = this;
			}
		}

		/// <summary>
		/// updates shape position on canvas
		/// </summary>
		override protected void Update()
		{
			//double img2PathScale = 4d / 3d; //I'm not sure why this is but the image comes out scaled bigger than the pathShape?
			double img2PathScale = 1d / (17d / 16d); //I'm not sure why this is but the image comes out scaled bigger than the pathShape?
			double shrinkScale = 0.8d;

			//Set location of the pathShape
			Canvas.SetLeft(pathShape, 0);
			Canvas.SetTop(pathShape, 0);

			//calculate the SCALE of largest side (width or height)
			double scale;
			if (tileImage.Source.Width > tileImage.Source.Height)
				scale = tileDims.X / 512;
			else
				scale = tileDims.Y / 512;

			//Apply scale to the tile image
			TransformGroup imggrp = new TransformGroup();
			ScaleTransform scaleTransform = new ScaleTransform( scale * shrinkScale * img2PathScale, scale * shrinkScale * img2PathScale);
			imggrp.Children.Add(scaleTransform);
			tileImage.RenderTransform = imggrp;


			//Add translation and rotation to the canvas
			TransformGroup canvasgrp = new TransformGroup();
			float imgTranslateX = (float)position.X;
			float imgTranslateY = (float)position.Y;
			TranslateTransform translateTransform = new TranslateTransform(imgTranslateX, imgTranslateY);
			RotateTransform rotateTransform = new RotateTransform( angle );
			double centerOffsetX = (tileDims.X / 2);
			double centerOffsetY = (tileDims.Y / 2);
			rotateTransform.CenterX = position.X + centerOffsetX;
			rotateTransform.CenterY = position.Y + centerOffsetY;
			canvasgrp.Children.Add(translateTransform);
			canvasgrp.Children.Add(rotateTransform);
			canvas.RenderTransform = canvasgrp;
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

		override public void Move(double x, double y)
		{
			position = new Vector(position.X + (x * 25), position.Y + (y * 25));

			Update();
		}
	}
}
