using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

		string _dataName;
		string _triggerName, _triggeredByName;
		TokenType _tokenType;
		PersonType _personType;

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
		public int idNumber { get; set; }
		public Vector position { get; set; }
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
		public Ellipse tokenPathShape;
		Point clickV;
		Vector lastPos;
		static SolidColorBrush[] fillColors = { 
			Brushes.ForestGreen, //Search
			Brushes.BlueViolet, //Person
			Brushes.DarkRed, //Threat
			Brushes.Black, //Darkness
			Brushes.Gray, //Exploration
			Brushes.Yellow, //DifficultTerrain
			Brushes.OrangeRed, //Fortified
			Brushes.Gray //None
		};

		public Token( TokenType ttype )
		{
			dataName = ttype.ToString();
			GUID = Guid.NewGuid();
			triggerName = "None";
			triggeredByName = "None";
			tokenType = ttype;
			personType = PersonType.Human;
			position = new Vector( 256, 256 );

			BuildShape();
			Update();
		}

		public void ReColor()
		{
			tokenPathShape.Fill = fillColors[(int)tokenType];
		}

		void BuildShape()
		{
			tokenPathShape = new Ellipse();
			tokenPathShape.Fill = fillColors[(int)tokenType];
			tokenPathShape.StrokeThickness = 4;
			tokenPathShape.Stroke = Brushes.White;
			tokenPathShape.Width = 50;
			tokenPathShape.Height = 50;
			tokenPathShape.DataContext = this;
		}

		void Update()
		{
			if(parentTile != null)
            {
				//tokenPathShape.RenderTransformOrigin = new Point(.5d, .5d);
				TransformGroup grp = new TransformGroup();
				ScaleTransform sc = new ScaleTransform(0.5d, 0.5d);
				grp.Children.Add(sc);
				tokenPathShape.RenderTransform = grp;

				//The passed in canvas element has its size set the same as the tileImage.
				//Calculate the scale based on the longest .png dimension of 512 pixels. Used to properly position tokens.
				double scale = Math.Max(parentCanvas.Width, parentCanvas.Height) / 512d;
				//The TokenEditorWindow has the short dimension of the image centered in the frame, so we have to offset that dimension
				double widthOffset = parentCanvas.Width > parentCanvas.Height ? 0 : (parentCanvas.Height - parentCanvas.Width) / 2;
				double heightOffset = parentCanvas.Height > parentCanvas.Width ? 0 : (parentCanvas.Width - parentCanvas.Height) / 2;
				Canvas.SetLeft(tokenPathShape, (position.X - 25) * scale - widthOffset);
				Canvas.SetTop(tokenPathShape, (position.Y - 25) * scale - heightOffset);
			}
			else
            {
				tokenPathShape.RenderTransformOrigin = new Point(.5d, .5d);
				TranslateTransform tf = new TranslateTransform(position.X - 25, position.Y - 25);
				tokenPathShape.RenderTransform = tf;
			}
		}

		public void Rehydrate( Canvas canvas )
		{
			BuildShape();
			tokenPathShape.Fill = fillColors[(int)tokenType];
			tokenPathShape.DataContext = this;
			canvas.Children.Add( tokenPathShape );
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
			Vector clickPoint = new Vector( e.GetPosition( canvas ).X - clickV.X, e.GetPosition( canvas ).Y - clickV.Y );

			//Vector snapped = new Vector();
			//snapped.X = ( from snapx in Utils.dragSnapX where clickPoint.X.WithinTolerance( snapx, Utils.tolerance ) select snapx ).FirstOr( -1 );
			//snapped.Y = ( from snapy in Utils.dragSnapY where clickPoint.Y.WithinTolerance( snapy, Utils.tolerance ) select snapy ).FirstOr( -1 );

			position = clickPoint;
			if ( position.X - 25 < 0 || position.Y - 25 < 0 ||
				position.X + 25 > 512 || position.Y + 25 > 512 )
				position = lastPos;
			lastPos = position;
			Update();
		}

		void Prop( string name )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}
	}
}
