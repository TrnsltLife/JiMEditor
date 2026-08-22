// Ported from: JiMEditor/Models/Token.cs — WPF Shape/Canvas visuals stripped for Mac MVP (2026-08-09).
using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace JiME
{
	public class Token : INotifyPropertyChanged, ICommonData
	{
		[JsonIgnore]
		public BaseTile? parentTile = null;

		[JsonIgnore]
		public double rotationAngle = 15;

		string _dataName = "";
		string _triggerName = "None", _triggeredByName = "None";
		TokenType _tokenType;
		PersonType _personType;
		TerrainType _terrainType;

		public string dataName
		{
			get => _dataName;
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
		/// <summary>The name of the Event to trigger</summary>
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
				UpdateDataName();
			}
		}

		public PersonType personType
		{
			get => _personType;
			set
			{
				_personType = value;
				Prop( "personType" );
				UpdateDataName();
			}
		}

		public TerrainType terrainType
		{
			get => _terrainType;
			set
			{
				_terrainType = value;
				Prop( "terrainType" );
				UpdateDataName();
			}
		}

		public int idNumber { get; set; }
		public Vector2 position { get; set; }
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

		public void UpdateDataName()
		{
			if ( tokenType == TokenType.Person )
				dataName = tokenType.ToString() + ": " + personType.ToString();
			else if ( tokenType == TokenType.Terrain )
				dataName = tokenType.ToString() + ": " + terrainType.ToString();
			else
				dataName = tokenType.ToString();
		}

		public void ApplyInteraction( IInteraction interaction )
		{
			if ( interaction is StartInteraction start )
				start.ApplyStartInteractionDefaults();

			triggerName = interaction.dataName;
			if ( interaction is StartInteraction || interaction.dataName == StartInteraction.DEFAULT_NAME )
			{
				tokenType = TokenType.Start;
				personType = PersonType.None;
				terrainType = TerrainType.None;
			}
			else
			{
				tokenType = interaction.tokenType;
				personType = interaction.personType;
				terrainType = interaction.terrainType;
			}
		}

		public void RepairStartPositionFromTriggerName()
		{
			if ( triggerName == StartInteraction.DEFAULT_NAME && tokenType != TokenType.Start )
			{
				tokenType = TokenType.Start;
				personType = PersonType.None;
				terrainType = TerrainType.None;
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public Token( TokenType ttype, PersonType ptype = PersonType.None, TerrainType tetype = TerrainType.None )
		{
			tokenType = ttype;
			personType = ptype;
			terrainType = tetype;

			UpdateDataName();

			GUID = Guid.NewGuid();
			triggerName = "None";
			triggeredByName = "None";
			position = new Vector2( 256, 256 );
		}

		void Prop( string name )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}
	}
}
