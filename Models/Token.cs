using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace JiME
{
	public class Token : INotifyPropertyChanged, ICommonData
	{
		[JsonIgnore]
		public BaseTile parentTile = null;
		[JsonIgnore]
		public Canvas parentCanvas = null;
		[JsonIgnore]
		public double rotationAngle = 15;

		string _dataName;
		string _triggerName, _triggeredByName;
		TokenType _tokenType;
		PersonType _personType;
		TerrainType _terrainType;

		public string dataName
		{
			get { return _dataName; }
			set
			{
				if ( value != _dataName )
				{
					_dataName = value;
					Prop( "dataName" );
				}
			}
		}
		public Guid GUID { get; set; }
		public bool isEmpty { get; set; }
		/// <summary>
		/// The name of the Event to trigger
		/// </summary>
		public string triggerName
		{
			get => _triggerName;
			set
			{
				if ( value != _triggerName )
				{
					_triggerName = value;
					Prop( "triggerName" );
				}
			}
		}
		public TokenType tokenType
		{
			get => _tokenType;
			set
			{
				_tokenType = value;
				Prop( "tokenType" );
			}
		}

		public PersonType personType
		{
			get => _personType;
			set
			{
				_personType = value;
				Prop( "personType" );
			}
		}

		public TerrainType terrainType
		{
			get => _terrainType;
			set
			{
				_terrainType = value;
				Prop("terrainType");
			}
		}

		public int idNumber { get; set; }
		public Vector position { get; set; }
		public double angle { get; set; }
		public string triggeredByName
		{
			get => _triggeredByName;
			set
			{
				if ( value != _triggeredByName )
				{
					_triggeredByName = value;
					Prop( "triggeredByName" );
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		//shape stuff
		[JsonIgnore]
		public Shape tokenPathShape;
		Point clickV;
		Vector lastPos;

		static Brush[] personBrushes = {
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Tokens/Human.png"))),
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Tokens/Elf.png"))),
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Tokens/Hobbit.png"))),
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Tokens/Dwarf.png")))
		};

		static Brush[] tokenBrushes = {
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Tokens/Search.png"))),
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Tokens/Person.png"))),
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Tokens/Threat.png"))),
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Tokens/Darkness.png"))),
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Tokens/DifficultGround.png"))),
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Tokens/Fortified.png"))),
			Brushes.Brown, //Terrain
			Brushes.Gray //None
		};

		static Brush[] terrainBrushes = {
			Brushes.Black, //None
			//Core Set
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Terrain/Barrels.png"))),
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Terrain/Boulder.png"))),
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Terrain/Bush.png"))),
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Terrain/FirePit.png"))),
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Terrain/Mist.png"))),
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Terrain/Pit.png"))),
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Terrain/Statue.png"))),
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Terrain/Stream.png"))),
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Terrain/Table.png"))),
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Terrain/Wall.png"))),

			//Shadowed Paths
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Terrain/Elevation.png"))),
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Terrain/Log.png"))),
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Terrain/Rubble.png"))),
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Terrain/Web.png"))),

			//Spreading War
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Terrain/Barricade.png"))),
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Terrain/Chest.png"))),
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Terrain/Fence.png"))),
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Terrain/Fountain.png"))),
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Terrain/Pond.png"))),
			new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JiME;component/Assets/Terrain/Trench.png")))
		};


		public Token( TokenType ttype )
		{
			dataName = ttype.ToString();
			GUID = Guid.NewGuid();
			triggerName = "None";
			triggeredByName = "None";
			tokenType = ttype;
			personType = PersonType.None;
			terrainType = TerrainType.None;
			position = new Vector( 256, 256 );

			BuildShape();
			Update();
		}

		public void Rehydrate(Canvas canvas)
		{
			canvas.Children.Remove(tokenPathShape);
			BuildShape();
			tokenPathShape.DataContext = this;
			canvas.Children.Add(tokenPathShape);
			if (parentTile != null)
			{
				Canvas.SetZIndex(tokenPathShape, 104);
			}
			else
			{
				//Rely on transform in Update()
			}
			Update();
		}

		public void ReColor()
		{
			if (tokenType == TokenType.Person)
			{
				tokenPathShape.Fill = personBrushes[(int)personType];
			}
			else if (tokenType == TokenType.Terrain)
			{
				tokenPathShape.Fill = terrainBrushes[(int)terrainType];
			}
			else
			{
				tokenPathShape.Fill = tokenBrushes[(int)tokenType];
			}
		}

		void BuildShape()
		{
			if (tokenType == TokenType.Terrain)
			{
				if(new List<TerrainType>() { TerrainType.Barrels, TerrainType.Barricade, TerrainType.Chest, TerrainType.Elevation, TerrainType.Log, TerrainType.Table }.Contains(terrainType))
                {
					//31mm x 70mm rectangle
					tokenPathShape = new Rectangle();
					ReColor();
					tokenPathShape.StrokeThickness = 4;
					tokenPathShape.Stroke = Brushes.White;
					tokenPathShape.Width = 62;
					tokenPathShape.Height = 140;
					tokenPathShape.DataContext = this;
				}
				else if (new List<TerrainType>() { TerrainType.Fence, TerrainType.Stream, TerrainType.Trench, TerrainType.Wall }.Contains(terrainType))
                {
					//15mm x 94mm rectangle
					tokenPathShape = new Rectangle();
					ReColor();
					tokenPathShape.StrokeThickness = 4;
					tokenPathShape.Stroke = Brushes.White;
					tokenPathShape.Width = 27; //30;
					tokenPathShape.Height = 170; //188;
					tokenPathShape.DataContext = this;
				}
				else if (new List<TerrainType>() { TerrainType.Boulder, TerrainType.Bush, TerrainType.FirePit, TerrainType.Rubble, TerrainType.Statue, TerrainType.Web }.Contains(terrainType))
				{
					//37mm diameter ellipse
					tokenPathShape = new Ellipse();
					ReColor();
					tokenPathShape.StrokeThickness = 4;
					tokenPathShape.Stroke = Brushes.White;
					tokenPathShape.Width = 74;
					tokenPathShape.Height = 74;
					tokenPathShape.DataContext = this;
				}
				else if (new List<TerrainType>() { TerrainType.Fountain, TerrainType.Mist, TerrainType.Pit, TerrainType.Pond }.Contains(terrainType))
				{
					//75mm x 75mm rounded rectangle
					tokenPathShape = new Rectangle();
					ReColor();
					tokenPathShape.StrokeThickness = 4;
					tokenPathShape.Stroke = Brushes.White;
					tokenPathShape.Width = 150;
					tokenPathShape.Height = 150;
					((Rectangle)tokenPathShape).RadiusX = 50;
					((Rectangle)tokenPathShape).RadiusY = 50;
					tokenPathShape.DataContext = this;
				}
			}
			else //Round/Hex tokens simply displayed with Ellipse in the editor
			{
				//25mm diameter
				tokenPathShape = new Ellipse();
				ReColor();
				tokenPathShape.StrokeThickness = 4;
				tokenPathShape.Stroke = Brushes.White;
				tokenPathShape.Width = 50;
				tokenPathShape.Height = 50;
				tokenPathShape.DataContext = this;
			}
		}

		void Update()
		{
			if(parentTile != null) //TileEditorWindow
            {
				//tokenPathShape.RenderTransformOrigin = new Point(.5d, .5d);
				double scale = 0.5d; //TileType.hex
				if (parentTile.tileType == TileType.Square)
				{
					scale = 0.8d;
				}
				ScaleTransform sc = new ScaleTransform(scale, scale);

				RotateTransform rotateTransform = new RotateTransform(angle);
				double centerOffsetX = (tokenPathShape.Width / 2) * scale;
				double centerOffsetY = (tokenPathShape.Height / 2) * scale;
				rotateTransform.CenterX = centerOffsetX;
				rotateTransform.CenterY = centerOffsetY;

				TransformGroup grp = new TransformGroup();
				grp.Children.Add(sc);
				grp.Children.Add(rotateTransform);
				tokenPathShape.RenderTransform = grp;

				//The passed in canvas element has its size set the same as the tileImage.
				//Calculate the scale based on the longest .png dimension of 512 pixels. Used to properly position tokens.
				double positionScale = Math.Max(parentCanvas.Width, parentCanvas.Height) / 512d;
				//The TokenEditorWindow has the short dimension of the image centered in the frame, so we have to offset that dimension
				double widthOffset = parentCanvas.Width > parentCanvas.Height ? 0 : (parentCanvas.Height - parentCanvas.Width) / 2;
				double heightOffset = parentCanvas.Height > parentCanvas.Width ? 0 : (parentCanvas.Width - parentCanvas.Height) / 2;
				Canvas.SetLeft(tokenPathShape, (position.X) * positionScale - widthOffset);
				Canvas.SetTop(tokenPathShape, (position.Y) * positionScale - heightOffset);
			}
			else //TokenEditorWindow
            {
				tokenPathShape.RenderTransformOrigin = new Point(.5d, .5d);
				TranslateTransform tf = new TranslateTransform(position.X, position.Y);

				RotateTransform rotateTransform = new RotateTransform(angle);
				rotateTransform.CenterX = position.X;
				rotateTransform.CenterY = position.Y;

				TransformGroup grp = new TransformGroup();
				grp.Children.Add(tf);
				grp.Children.Add(rotateTransform);
				tokenPathShape.RenderTransform = grp;
			}
		}

		public void Select()
		{
			tokenPathShape.Stroke = new SolidColorBrush( Colors.Red );
			Canvas.SetZIndex( tokenPathShape, 105 );
		}

		public void Unselect()
		{
			tokenPathShape.Stroke = new SolidColorBrush( Colors.White );
			Canvas.SetZIndex( tokenPathShape, 104 );
		}

		public void SetClickV( MouseButtonEventArgs e, Canvas canvas )
		{
			clickV = new Point();
			TransformGroup grp = tokenPathShape.RenderTransform as TransformGroup;
			if ( grp?.Children.Count == 1 )
			{
				Vector gv = new Vector( ( (TranslateTransform)grp.Children[0] ).X, ( (TranslateTransform)grp.Children[0] ).Y );
				//Debug.Log( "shape position(center):" + gv.X + "," + gv.Y );
				//Debug.Log( $"dims: {hexPathShape.ActualWidth},{hexPathShape.ActualHeight}" );
				clickV = e.GetPosition( canvas );
				clickV.X -= gv.X;
				clickV.Y -= gv.Y;
				//Debug.Log( "clickV:" + clickV );
				lastPos = e.GetPosition( canvas ).ToVector();
			}
		}

		public void Drag( MouseEventArgs e, Canvas canvas )
		{
			Vector clickPoint = new Vector( e.GetPosition( canvas ).X - clickV.X - tokenPathShape.Width/2, e.GetPosition( canvas ).Y - clickV.Y - tokenPathShape.Height/2);

			//Vector snapped = new Vector();
			//snapped.X = ( from snapx in Utils.dragSnapX where clickPoint.X.WithinTolerance( snapx, Utils.tolerance ) select snapx ).FirstOr( -1 );
			//snapped.Y = ( from snapy in Utils.dragSnapY where clickPoint.Y.WithinTolerance( snapy, Utils.tolerance ) select snapy ).FirstOr( -1 );

			position = clickPoint;
			if ((position.X + tokenPathShape.Width/2) < 0 || (position.Y + tokenPathShape.Height/2) < 0 ||
				(position.X - tokenPathShape.Width/2) > 512 || (position.Y - tokenPathShape.Height/2) > 512)
			{
				position = lastPos;
			}
			lastPos = position;
			Update();
		}

		virtual public void Rotate(double direction, Canvas parentCanvas)
		{
			//Only allow rotating Terrain tokens
			if (tokenType == TokenType.Terrain)
			{
				angle += direction * rotationAngle;
				angle %= 360;
				Update();
				Select();
			}
		}

		virtual public void Move(double x, double y)
        {
			position = new Vector(position.X + x, position.Y + y);

			if ((position.X + tokenPathShape.Width / 2) < 0 || (position.Y + tokenPathShape.Height / 2) < 0 ||
				(position.X - tokenPathShape.Width / 2) > 512 || (position.Y - tokenPathShape.Height / 2) > 512)
			{
				position = lastPos;
			}
			lastPos = position;
			Update();
		}

		void Prop( string name )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}
	}
}
