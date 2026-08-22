// Ported from: JiMEditor/Models/BaseTile.cs — WPF Canvas/Path visuals stripped for Mac MVP (2026-08-09).
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Newtonsoft.Json;

namespace JiME
{
	public class BaseTile : Translatable, INotifyPropertyChanged, ITile
	{
		override public string TranslationKeyName() { return idNumber.ToString(); }
		override public string TranslationKeyPrefix() { return idNumber.ToString(); }

		override protected void DefineTranslationAccessors()
		{
			translationAccessors = new List<TranslationAccessor>();
		}

		string _tileSide = "A", _triggerName = "None";
		bool _isStartTile;
		Collection? _collection;

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
		public Vector2 position { get; set; }
		public TileType tileType { get; set; }
		public ObservableCollection<Token> tokenList { get; set; } = new();
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
		public Vector2 pathRoot { get; set; }
		public string triggerName
		{
			get => _triggerName;
			set
			{
				_triggerName = value;
				PropChanged( "triggerName" );
			}
		}

		[JsonIgnore]
		public double rotationAngle = 15;
		[JsonIgnore]
		public bool useGraphic;

		[JsonIgnore]
		public Collection collection
		{
			get
			{
				if ( _collection == null )
					_collection = Collection.FromTileNumber( idNumber );
				return _collection;
			}
			set => _collection = value;
		}

		[JsonIgnore]
		public string idNumberAndCollection => "" + idNumber + collection.FontCharacter;

		public event PropertyChangedEventHandler? PropertyChanged;

		public static BaseTile CreateTile( int n, bool skipBuild = true )
		{
			if ( n == 998 || n == 999 )
				return new SquareTile( n, skipBuild );
			return new HexTile( n, skipBuild );
		}

		public BaseTile() : base() { }

		public BaseTile( int n, bool skipBuild = true ) : base()
		{
			InitCommon( n );
			_ = skipBuild; // Avalonia editors own visuals; WPF BuildShape/BuildImage removed.
		}

		protected void InitCommon( int n )
		{
			idNumber = n;
			tokenCount = ( n / 100 ) % 10;
			GUID = Guid.NewGuid();
			tileSide = "A";
			position = new Vector2( Utils.dragSnapX[5], Utils.dragSnapY[5] );
			tokenList = new ObservableCollection<Token>();
			isStartTile = false;
			triggerName = "None";
			collection = Collection.FromTileNumber( idNumber );
		}

		/// <summary>Stores chapter color index; Avalonia UI applies brushes.</summary>
		virtual public void ChangeColor( int idx ) => color = idx;

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
