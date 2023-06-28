using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using JiME.Models;
using Newtonsoft.Json;

namespace JiME
{
	public enum ScenarioType { Journey, Battle }
	public enum InteractionType { Text, Threat, StatTest, Decision, Branch, Darkness, MultiEvent, Persistent, Conditional, Dialog, Replace, Reward }
	/// <summary>
	/// The order of the monsters in this enum is important to maintain unchanged because the order (0-29) tells the companion app which monster to use.
	/// The order for the first 7 (core set) was set by GlowPuff and is kept for backwards compatibility.
	/// The order for the other sets is kept in the same order as the order in the original JiME engine, although not sharing the same enum int values.
	/// </summary>
	public enum MonsterType { Ruffian, GoblinScout, OrcHunter, OrcMarauder, Varg, HillTroll, Wight, //0 - 6
							  Atari, Gargletarg, Chartooth, //7-9
							  GiantSpider, PitGoblin, OrcTaskmaster, Shadowman, AnonymousThing, CaveTroll, Balerock, SpawnOfUglygiant, //10-17
							  SupplicantOfMoreGoth, Ursula, Oliver, //18-20
							  FoulBeast, VargRider, SiegeEngine, WarElephant, Soldier, HighOrcWarrior, //21-26
							  LordJavelin, LichKing, Endris //27-29
	}
	public enum TileType { Hex, Battle, Square }
	public enum ThreatAttributes { }//armor, elite, etc
	public enum ProjectType { Standalone, Campaign }
	public enum EditMode { Intro, Resolution, Objective, Flavor, Pass, Fail, Progress, Dialog, Special, Persistent, Story, Event }
	public enum EditorMode { Information, Threat, Decision, Test, Branch }
	public enum Ability { Might, Agility, Wisdom, Spirit, Wit, Wild, Random, None}
	public enum TerrainToken { None, Pit, Mist, Barrels, Table, FirePit, Statue }
	public enum TokenType { Search, Person, Threat, Darkness, DifficultGround, Fortified, Terrain, None }
	public enum PersonType { Human, Elf, Hobbit, Dwarf, None }

	/// <summary>
	/// The order of the terrain in this enum is important to maintain unchanged and the order correlates with the data in Collection.cs and in the companion app.
	/// </summary>
	public enum TerrainType { None, Barrels, Boulder, Bush, FirePit, Mist, Pit, Statue, Stream, Table, Wall, //Core Set
							  Elevation, Log, Rubble, Web, //Shaded Paths
							  Barricade, Chest, Fence, Fountain, Pond, Trench //Unfurling War
							}
	public enum HelpType { Token, Grouping, Enemies, Triggers }
	public enum DifficultyBias { Light, Medium, Heavy }


	public interface ITile
	{
		TileType tileType { get; set; }
		int idNumber { get; set; }
		Vector position { get; set; }
	}

	public interface ICommonData
	{
		Guid GUID { get; set; }
		string dataName { get; set; }
		bool isEmpty { get; set; }
		string triggerName { get; set; }
	}


	public interface IInteraction
	{
		string dataName { get; set; }
		Guid GUID { get; set; }
		InteractionType interactionType { get; set; }
		void RenameTrigger( string oldName, string newName );
		int loreReward { get; set; }
		int xpReward { get; set; }
		int threatReward { get; set; }
		bool isTokenInteraction { get; set; }
		string triggerName { get; set; }
		string tokenInteractionText { get; set; }
		TokenType tokenType { get; set; }
		PersonType personType { get; set; }

		TerrainType terrainType { get; set; }
		bool isReusable { get; set; }
	}

	public interface ITranslationCollector
    {
		List<TranslationItem> CollectTranslationItems();
		string DefaultStringForTranslationKey(string key);

	}

	class Debug
	{
		public static void Log( object p )
		{
#if DEBUG
			Console.WriteLine( p );
#endif
		}
	}

	public class ProjectItem
	{
		public string Title { get; set; }
		public string Date { get; set; }
		public string Description { get; set; }
		public ProjectType projectType { get; set; }
		public string fileName { get; set; }
		public string fileVersion { get; set; }
		public string collectionIcons { get; set; }
	}

	public class CampaignItem
	{
		public string scenarioName { get; set; }
		///file NAME only, not path
		public string fileName { get; set; }

        [JsonIgnore]
		public List<Collection> collectionList { get; set; }
        [JsonIgnore]
		public string collectionIcons { get; set; }
	}

	class TileSorter : IComparer<int>
	{
		public int Compare( int x, int y )
		{
			return x.CompareTo( y );
		}
	}

	public class GalleryTile : INotifyPropertyChanged
	{
		bool _selected, _enabled;
		string _side;
		int _id;
		public int id { get { return _id; } 
			set 
			{
				_id = value; 
				collection = Collection.FromTileNumber(value); 
				collectionChar = collection.FontCharacter;
			} 
		}
		public ImageSource source { get; set; }
		public Collection collection { get; set; }
		public string collectionChar { get; set; }
		public bool selected
		{
			get { return _selected; }
			set
			{
				_selected = value;
				PropChanged( "selected" );
			}
		}
		public bool enabled
		{
			get { return _enabled; }
			set
			{
				_enabled = value;
				PropChanged( "enabled" );
			}
		}
		public SolidColorBrush color;
		public string side
		{
			get { return _side; }
			set
			{
				_side = value;
				PropChanged( "side" );
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public GalleryTile()
		{
			enabled = true;
			selected = false;
			side = "A";
		}

		void PropChanged( string name )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}
	}

	public class DefaultStats
	{
		public int id { get; set; }
		public string enumName { get; set; }
		public string dataName { get; set; }
		public int health { get; set; }
		public int armor { get; set; }
		public int sorcery { get; set; }
		public int moveA { get; set; }
		public int moveB { get; set; }
		public string[] moveSpecial { get; set; }
			//Encumbered: +1 to moveB
			//Predatory: move to most damaged hero
			//Incorporeal: ignore terrain and black borders when moving
			//Flying: ignore terrain when moving
			//Tunneler: doesn't move through spaces but is placed in the space. Doesn't trigger Move events. Ignores terrain and black borders.
			//Reinforcements: Each hero in a space with an enemy suffers 2 facedown {Damage}, {Might} negates.
			//Stampede: Move all other enemies in its space to the adjacent space with the most heroes.
		public bool ranged { get; set; }
		public int groupLimit { get; set; }
		public int figureLimit { get; set; }
		public int[] cost { get; set; }
		public string[] tag { get; set; }
		public string speed { get; set; }
		public string damage { get; set; }
		public bool fearsome { get; set; } //Used for what used to be "Fear Bias"
		public string[] special { get; set; }
			//Cleave: attacks hero in a space
	}

	public class DefaultActivations
    {
		public string name { get; set; }
		public int id { get; set; }
		[JsonConverter(typeof(CollectionConverter))]
		public Collection collection { get; set; }
		public DefaultActivationItem[] activations { get; set; }
    }

	public class DefaultActivationItem
    {
		public int id { get; set; }
		public Ability negate { get; set; }
		public bool[] valid { get; set; }
		public int[] damage { get; set; }
		public int[] fear { get; set; }
		public string text { get; set; }
		public string effect { get; set; }
    }

	public class DefaultTerrainInteractions
    {
		[JsonConverter(typeof(InteractionConverter))]
		public List<IInteraction> interactions { get; set; }
	}

	public static class Utils
	{
		/// <summary>
		/// AKA "Engine Version" in the companion app
		/// Update this number every time the file format changes with new features
		/// </summary>
		public static string formatVersion = "1.13";
		public static string appVersion = "0.25";
		public static Dictionary<int, BaseTileData> tileDictionary { get; set; } = new Dictionary<int, BaseTileData>();
		public static Dictionary<int, BaseTileData> tileDictionaryB { get; set; } = new Dictionary<int, BaseTileData>();
		public static int tolerance = 25;
		public static double[] dragSnapX, dragSnapY;
		//hexSnap is used to convert local to absolute canvas coords (world)
		public static double[] hexSnapX, hexSnapY;//used in BaseTileData.cs
		//sqrSnap is used to convert local to absolute canvas coords (world)
		public static double[] sqrSnapX, sqrSnapY;//used in BaseTileData.cs
		public static SolidColorBrush[] hexColors;
		public static bool isLoaded = false;
		//public static GalleryTile[] galleryTilesA;
		//public static GalleryTile[] galleryTilesB;
		public static ImageSource[] tileSourceA, tileSourceB;
		public static DefaultStats[] defaultStats;
		public static DefaultActivations[] defaultActivations;
		public static IInteraction[] defaultTerrainInteractions;

		public static void Init()
		{
			//create the hex grid snaps on canvas
			dragSnapX = new double[60];
			dragSnapY = new double[40];
			//grid snap is STAGGERED
			//x to x is NOT whole width of hex
			//1 hex is 64 wide
			//NEXT x grid is 32(radius)+16(HALF of radius)=48 units away
			for ( int i = 0; i < dragSnapX.Length; i++ )
				dragSnapX[i] = i * 48;
			//each y grid snap is just half a hex height away
			for ( int i = 0; i < dragSnapY.Length; i++ )
				dragSnapY[i] = 55.4256256d / 2 * i;//27.7128128

			//max hex area of any composite shape
			hexSnapX = new double[35];//25//20
			hexSnapY = new double[36];//26//21

			//hexSnapX = new double[8];
			//hexSnapY = new double[9];
			//double xx = 36d, yy = 30;//offset onto canvas
			double xx = -480, yy = -277.128128d;//offset onto canvas
			for ( int c = 0; c < hexSnapX.Length; c++ )
				hexSnapX[c] = xx + ( 48d * c );
			for ( int c = 0; c < hexSnapY.Length; c++ )
				hexSnapY[c] = yy + ( 55.4256256d / 2 * c );

			//sqrSnap grid
			sqrSnapX = new double[35];
			sqrSnapY = new double[35];
			double sqrxx = 0, sqryy = 0d;//offset onto canvas
			for (int i = 0; i < sqrSnapX.Length; i++)
				sqrSnapX[i] = sqrxx + (33d * i);
			for (int i = 0; i < sqrSnapY.Length; i++)
				sqrSnapY[i] = sqryy + (33d * i);

			hexColors = new SolidColorBrush[5];
			hexColors[0] = new SolidColorBrush( Colors.SaddleBrown );
			hexColors[1] = new SolidColorBrush( Colors.DodgerBlue );
			hexColors[2] = new SolidColorBrush( Colors.SeaGreen );
			hexColors[3] = new SolidColorBrush( Colors.DarkMagenta );
			hexColors[4] = new SolidColorBrush( Colors.DarkOrange );

			//create gallery tiles
			int[] ids = LoadTiles().ToArray();
			int tileCount = ids.Length;
			tileSourceA = new ImageSource[tileCount];
			tileSourceB = new ImageSource[tileCount];

			for ( int i = 0; i < tileCount; i++ )
			{
				//A
				tileSourceA[i] = new BitmapImage( new Uri( $"pack://application:,,,/JiME;component/Assets/TilesA/{ids[i]}.png" ) );
				//B
				tileSourceB[i] = new BitmapImage( new Uri( $"pack://application:,,,/JiME;component/Assets/TilesB/{ids[i]}.png" ) );
			}

			//load default enemy stats
			defaultStats = LoadDefaultStats().ToArray();

			//load default enemy activation stats
			defaultActivations = LoadDefaultActivations().ToArray();

			//load default terrain interactions
			defaultTerrainInteractions = LoadDefaultTerrainInteractions().ToArray();
		}

		public static float RemapValue( float value, float low1, float high1, float low2, float high2 )
		{
			//value = Math.Clamp( value, low1, high1 );
			if ( low2 < high2 )
				return low2 + ( value - low1 ) * ( high2 - low2 ) / ( high1 - low1 );
			else
				return high2 + ( value - high1 ) * ( low2 - high2 ) / ( low1 - high1 );
		}

		public static bool WithinTolerance( this Vector value1, Vector value2, float tolerance )
		{
			return Math.Abs( value1.X - value2.X ) <= tolerance
				&& Math.Abs( value1.Y - value2.Y ) <= tolerance;
		}

		public static bool WithinTolerance( this double value1, double value2, float tolerance )
		{
			return Math.Abs( value1 - value2 ) <= tolerance;
		}

		/// <summary>
		/// returns tile ID data in an array
		/// </summary>
		public static List<int> LoadTiles()
		{
			var assembly = Assembly.GetExecutingAssembly();
			string resourceName = assembly.GetManifestResourceNames()
	.Single( str => str.Contains( ".tiles.json" ) );

			using ( Stream stream = assembly.GetManifestResourceStream( resourceName ) )
			using ( StreamReader reader = new StreamReader( stream ) )
			{
				string json = reader.ReadToEnd();
				var list = JObject.Parse( json );
				JToken token = list.SelectToken( "tiles" );
				List<int> ret = token.ToObject( typeof( List<int> ) ) as List<int>;
				TileSorter sorter = new TileSorter();
				ret.Sort( sorter );
				return ret;
			}
		}

		static List<DefaultStats> LoadDefaultStats()
		{
			var assembly = Assembly.GetExecutingAssembly();
			string resourceName = assembly.GetManifestResourceNames()
				.Single( str => str.Contains( ".enemy-defaults.json" ) );

			using ( Stream stream = assembly.GetManifestResourceStream( resourceName ) )
			using ( StreamReader reader = new StreamReader( stream ) )
			{
				string json = reader.ReadToEnd();
				var list = JObject.Parse( json );
				JToken token = list.SelectToken( "defaults" );
				List<DefaultStats> ret = token.ToObject( typeof( List<DefaultStats> ) ) as List<DefaultStats>;
				Console.WriteLine("DefaultStats Loaded:");
				//foreach( DefaultStats item in ret )
                //{
				//	Console.WriteLine(item.id + " " + item.enumName + " " + item.dataName );
                //}
				return ret;
			}
		}
		static List<DefaultActivations> LoadDefaultActivations()
		{
			var assembly = Assembly.GetExecutingAssembly();
			string resourceName = assembly.GetManifestResourceNames()
				.Single(str => str.Contains(".enemy-activation-defaults.json"));

			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			using (StreamReader reader = new StreamReader(stream))
			{
				string json = reader.ReadToEnd();
				var list = JObject.Parse(json);
				JToken token = list.SelectToken("activations");
				List<DefaultActivations> ret = token.ToObject(typeof(List<DefaultActivations>)) as List<DefaultActivations>;
				Console.WriteLine("Default Activations Loaded:");
				return ret;
			}
		}

		static List<IInteraction> LoadDefaultTerrainInteractions()
        {
			var assembly = Assembly.GetExecutingAssembly();
			string resourceName = assembly.GetManifestResourceNames()
				.Single(str => str.Contains(".terrain-interactions.json"));

			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			using (StreamReader reader = new StreamReader(stream))
			{
				string json = reader.ReadToEnd();
				var list = JObject.Parse(json);
				JToken token = list.SelectToken("interactions");
				var defTerInt = list.ToObject(typeof(DefaultTerrainInteractions)) as DefaultTerrainInteractions;
				List<IInteraction> ret = defTerInt.interactions.ToList<IInteraction>();
				Console.WriteLine("Default Terrain Interactions Loaded:");
				return ret;
			}
		}

		public static void LoadHexData()
		{
			if ( isLoaded )
				return;
			isLoaded = true;

			var assembly = Assembly.GetExecutingAssembly();
			foreach ( var item in new string[] { "A", "B" } )
			{
				string resourceName = assembly.GetManifestResourceNames()
					.Single( str => str.Contains( ".hextiles" + item + ".json" ) );

				using ( Stream stream = assembly.GetManifestResourceStream( resourceName ) )
				using ( StreamReader reader = new StreamReader( stream ) )
				{
					string json = reader.ReadToEnd();
					var list = JObject.Parse( json );
					JToken token = list.SelectToken( "tiles" );
					List<BaseTileData> ret = token.ToObject( typeof( List<BaseTileData> ) ) as List<BaseTileData>;
					foreach ( BaseTileData data in ret )
					{
						data.Init();
						if ( item == "A" )
						{
							tileDictionary.Add( data.id, data );
						}
						else
						{
							tileDictionaryB.Add( data.id, data );
						}
					}
				}
			}
		}
	}

	internal static class WindowExtensions
	{
		// from winuser.h
		private const int GWL_STYLE = -16,
											WS_MAXIMIZEBOX = 0x10000,
											WS_MINIMIZEBOX = 0x20000;

		[DllImport( "user32.dll" )]
		extern private static int GetWindowLong( IntPtr hwnd, int index );

		[DllImport( "user32.dll" )]
		extern private static int SetWindowLong( IntPtr hwnd, int index, int value );

		internal static void HideMinimizeAndMaximizeButtons( this Window window )
		{
			IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper( window ).Handle;
			var currentStyle = GetWindowLong( hwnd, GWL_STYLE );

			SetWindowLong( hwnd, GWL_STYLE, ( currentStyle & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX ) );
		}
	}

	public static class ObservableExtensions
	{
		/// <summary>
		/// Is the given trigger name used in this collection?
		/// </summary>
		public static Tuple<string, string> IsTriggerUsed<T>( this ObservableCollection<T> collection, string name )
		{
			foreach ( ICommonData item in collection )
			{
				if ( item.triggerName == name )
				{
					string n = collection.ToString();
					int idx = n.IndexOf( "[" );
					n = n.Substring( idx + 6, n.Length - idx - 7 );
					return new Tuple<string, string>( item.dataName, n );
				}
			}
			return null;
		}

		/// <summary>
		/// Return the data model with the given name
		/// </summary>
		public static ICommonData GetData<T>( this ObservableCollection<T> collection, string name )
		{
			foreach ( ICommonData item in collection )
			{
				if ( item.dataName == name )
					return item;
			}
			return null;
		}
	}

	public static class Extensions
	{
		public static Vector ToVector( this Point p )
		{
			return new Vector( p.X, p.Y );
		}

		public static T[] Fill<T>( this T[] arr, T value )
		{
			for ( int i = 0; i < arr.Length; i++ )
			{
				arr[i] = value;
			}
			return arr;
		}

		public static T FirstOr<T>( this IEnumerable<T> source, T alternate )
		{
			foreach ( T t in source )
				return t;
			return alternate;
		}
	}

	public class SimulatorData
	{
		public bool[] includedEnemies = new bool[30];
		public DifficultyBias difficultyBias;
		public float poolPoints;
	}

	public class MonsterSim
	{
		public string dataName { get; set; }
		public int health { get; set; }
		public int shieldValue { get; set; }
		public int sorceryValue { get; set; }
		//public int damage { get; set; }
		public string specialAbility
		{
			get
			{
				string ret = "";
				for ( int i = 0; i < modList.Count; i++ )
				{
					ret += modList[i];
					if ( i < Math.Max( 0, modList.Count - 1 ) )
						ret += ", ";
				}
				return ret;
			}
		}
		public int count { get; set; }
		public float cost { get; set; }
		public float singlecost { get; set; }
		public List<string> modList { get; set; } = new List<string>();
	}

	//public class DebugTraceListener : TraceListener
	//{
	//	public override void Write( string message )
	//	{
	//	}

	//	public override void WriteLine( string message )
	//	{
	//		Debug.Log( message );
	//		Debugger.Break();
	//	}
	//}
}
