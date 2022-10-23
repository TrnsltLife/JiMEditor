using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Documents;
using System.IO;
using System.Text;
using System;
using System.Xml;
using System.Windows.Markup;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.Generic;
using JiME.Models;
using JiME.Views;
using System.Linq;

namespace JiME.UserControls
{
	/// <summary>
	/// Interaction logic for SidebarListView.xaml
	/// </summary>
	public partial class TokenTypeSelector : UserControl, INotifyPropertyChanged
	{
		public static readonly DependencyProperty InteractionProperty =
			DependencyProperty.Register("Interaction", typeof(InteractionBase),
			typeof(TokenTypeSelector), new FrameworkPropertyMetadata
			(null, new PropertyChangedCallback(OnInteractionChanged)));

		public static readonly DependencyProperty ScenarioProperty =
			DependencyProperty.Register("Scenario", typeof(Scenario),
			typeof(TokenTypeSelector), new FrameworkPropertyMetadata
			(null, new PropertyChangedCallback(OnScenarioChanged)));


		InteractionBase interaction;
		Scenario scenario;

		List<RadioButton> coreSetList;
		List<RadioButton> shadowedPathsList;
		List<RadioButton> spreadingWarList;
		List<RadioButton> allTerrainList;

		public TokenTypeSelector()
		{
			Debug.Log("TokenTypeSelector constructor");
			InitializeComponent();
			//DataContext = this;

			//The order of the terrrain in these lists is important to maintain unchanged because the order tells the companion app which terrain to use.
			//The order also corresponds to the order in the TerrainType enum.
			coreSetList = new List<RadioButton>() { barrelsRadio, boulderRadio, bushRadio, firePitRadio, mistRadio, pitRadio, statueRadio, streamRadio, tableRadio, wallRadio };
			shadowedPathsList = new List<RadioButton>() { elevationRadio, logRadio, rubbleRadio, webRadio };
			spreadingWarList = new List<RadioButton>() { barricadeRadio, chestRadio, fenceRadio, fountainRadio, pondRadio, trenchRadio };
			allTerrainList = new List<RadioButton>();
			allTerrainList.AddRange(coreSetList);
			allTerrainList.AddRange(shadowedPathsList);
			allTerrainList.AddRange(spreadingWarList);
		}


		protected override void OnInitialized(EventArgs e)
        {
			base.OnInitialized(e);
			Debug.Log("TokenTypeSelector OnInitalized");
			Debug.Log("interaction: " + interaction + " scenario: " + scenario);
			InitializeSelections();
		}

		public InteractionBase Interaction
		{
			get => interaction;
			set
            {
				if(interaction != value)
                {
					interaction = value;
					PropChanged("Interaction");
					InitializeSelections();
				}
			}
		}

		public Scenario Scenario
        {
			get => scenario;
			set
            {
				if (scenario != value)
				{
					scenario = value;
					PropChanged("Scenario");
					InitializeSelections();
				}
			}
        }

		public static void OnInteractionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			TokenTypeSelector tts = (TokenTypeSelector)obj;
			tts.Interaction = (InteractionBase)args.NewValue;
		}

		public static void OnScenarioChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			TokenTypeSelector tts = (TokenTypeSelector)obj;
			tts.Scenario = (Scenario)args.NewValue;
		}

		private void InitializeSelections()
        {
			Debug.Log("InitializeSelections");
			if (interaction == null || scenario == null) return;
			Debug.Log("InitializeSelections interaction and scenario not null");

			Dictionary<List<RadioButton>, Collection> checkboxCollectionMap = 
				new Dictionary<List<RadioButton>, Collection>() {
					{coreSetList, Collection.CORE_SET},
					{shadowedPathsList, Collection.SHADOWED_PATHS},
					{spreadingWarList, Collection.SPREADING_WAR},
				};

			//special tokens
			foreach(var c in scenario.collectionObserver)
            {
				if(c.DifficultGround)
                {
					difficultGroundRadio.IsEnabled = true;
					difficultGroundRadio.FontStyle = FontStyles.Normal;
				}
				if(c.Fortified)
                {
					fortifiedRadio.IsEnabled = true;
					fortifiedRadio.FontStyle = FontStyles.Normal;
				}
			}

			//terrain type radio buttons
			int index = 0;
			foreach (var terrainRadio in allTerrainList)
			{
				//Console.WriteLine(index + " " + monsterRB.Content + " " + ((MonsterType)index).ToString());
				terrainRadio.IsChecked = interaction.terrainType == (TerrainType)index;
				index++;
			}

			//Enable/Disable Collections and their monsters based on the Collections enabled for this Scenario
			foreach (KeyValuePair<List<RadioButton>, Collection> pair in checkboxCollectionMap)
			{
				if (scenario.IsCollectionEnabled(pair.Value))
				{
					foreach (var radio in pair.Key)
					{
						radio.IsEnabled = true;
						radio.FontStyle = FontStyles.Normal;
					}
				}
				else
				{
					foreach (var radio in pair.Key)
					{
						radio.IsChecked = false;
						radio.IsEnabled = false;
						radio.FontStyle = FontStyles.Italic;
					}
				}
			}

			bool isThreatTriggered = scenario.threatObserver.Any(x => x.triggerName == interaction.dataName);
			if (isThreatTriggered)
			{
				isTokenCB.IsEnabled = false;
			}

			//Special Instructions Visibility
			if(interaction is ThreatInteraction)
				threatMessage.Visibility = Visibility.Visible;
			if (interaction is PersistentTokenInteraction)
			{
				persistentEventMessage.Visibility = Visibility.Visible;
				isTokenCB.IsEnabled = false;
			}
			if (interaction is PersistentInteractionBase)
				persistentMessage.Visibility = Visibility.Visible;

			//PersonType and TerrainType Visibility
			if (interaction.isTokenInteraction && interaction.tokenType == TokenType.Person)
				personType.Visibility = Visibility.Visible;
			else if (interaction.isTokenInteraction && interaction.tokenType == TokenType.Terrain)
				terrainType.Visibility = Visibility.Visible;

			//Persistent Text Visibility
			persCB.Visibility = (interaction is PersistentInteractionBase) ? Visibility.Visible : Visibility.Collapsed;
			editPersText.Visibility = (interaction is PersistentInteractionBase) ? Visibility.Visible : Visibility.Collapsed;

			//PersonType
			humanRadio.IsChecked = interaction.personType == PersonType.Human;
			elfRadio.IsChecked = interaction.personType == PersonType.Elf;
			hobbitRadio.IsChecked = interaction.personType == PersonType.Hobbit;
			dwarfRadio.IsChecked = interaction.personType == PersonType.Dwarf;

			//TerrainType
			barrelsRadio.IsChecked = interaction.terrainType == TerrainType.Barrels && interaction.tokenType == TokenType.Terrain;
			barricadeRadio.IsChecked = interaction.terrainType == TerrainType.Barricade && interaction.tokenType == TokenType.Terrain;
			boulderRadio.IsChecked = interaction.terrainType == TerrainType.Boulder && interaction.tokenType == TokenType.Terrain;
			bushRadio.IsChecked = interaction.terrainType == TerrainType.Bush && interaction.tokenType == TokenType.Terrain;
			chestRadio.IsChecked = interaction.terrainType == TerrainType.Chest && interaction.tokenType == TokenType.Terrain;
			elevationRadio.IsChecked = interaction.terrainType == TerrainType.Elevation && interaction.tokenType == TokenType.Terrain;
			fenceRadio.IsChecked = interaction.terrainType == TerrainType.Fence && interaction.tokenType == TokenType.Terrain;
			firePitRadio.IsChecked = interaction.terrainType == TerrainType.FirePit && interaction.tokenType == TokenType.Terrain;
			fountainRadio.IsChecked = interaction.terrainType == TerrainType.Fountain && interaction.tokenType == TokenType.Terrain;
			logRadio.IsChecked = interaction.terrainType == TerrainType.Log && interaction.tokenType == TokenType.Terrain;
			mistRadio.IsChecked = interaction.terrainType == TerrainType.Mist && interaction.tokenType == TokenType.Terrain;
			pitRadio.IsChecked = interaction.terrainType == TerrainType.Pit && interaction.tokenType == TokenType.Terrain;
			pondRadio.IsChecked = interaction.terrainType == TerrainType.Pond && interaction.tokenType == TokenType.Terrain;
			rubbleRadio.IsChecked = interaction.terrainType == TerrainType.Rubble && interaction.tokenType == TokenType.Terrain;
			statueRadio.IsChecked = interaction.terrainType == TerrainType.Statue && interaction.tokenType == TokenType.Terrain;
			streamRadio.IsChecked = interaction.terrainType == TerrainType.Stream && interaction.tokenType == TokenType.Terrain;
			tableRadio.IsChecked = interaction.terrainType == TerrainType.Table && interaction.tokenType == TokenType.Terrain;
			trenchRadio.IsChecked = interaction.terrainType == TerrainType.Trench && interaction.tokenType == TokenType.Terrain;
			wallRadio.IsChecked = interaction.terrainType == TerrainType.Wall && interaction.tokenType == TokenType.Terrain;
			webRadio.IsChecked = interaction.terrainType == TerrainType.Web && interaction.tokenType == TokenType.Terrain;

			//TokenType
			personRadio.IsChecked = interaction.tokenType == TokenType.Person;
			searchRadio.IsChecked = interaction.tokenType == TokenType.Search;
			darkRadio.IsChecked = interaction.tokenType == TokenType.Darkness;
			threatRadio.IsChecked = interaction.tokenType == TokenType.Threat;
			difficultGroundRadio.IsChecked = interaction.tokenType == TokenType.DifficultGround;
			fortifiedRadio.IsChecked = interaction.tokenType == TokenType.Fortified;
			terrainRadio.IsChecked = interaction.tokenType == TokenType.Terrain && !scenario.scenarioTypeJourney;
		}

		private void isTokenCB_Click(object sender, RoutedEventArgs e)
		{
			if (isTokenCB.IsChecked == true)
			{
				interaction.triggerName = "None";
				personType.Visibility = personRadio.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
				terrainType.Visibility = terrainRadio.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
			}
			else
			{
				personType.Visibility = Visibility.Collapsed;
				terrainType.Visibility = Visibility.Collapsed;
			}
		}

		public void AssignValuesFromSelections()
        {
			//TokenType
			if (searchRadio.IsChecked.HasValue && searchRadio.IsChecked.Value)
				interaction.tokenType = TokenType.Search;
			if (personRadio.IsChecked.HasValue && personRadio.IsChecked.Value)
				interaction.tokenType = TokenType.Person;
			if (darkRadio.IsChecked.HasValue && darkRadio.IsChecked.Value)
				interaction.tokenType = TokenType.Darkness;
			if (threatRadio.IsChecked.HasValue && threatRadio.IsChecked.Value)
				interaction.tokenType = TokenType.Threat;
			if (difficultGroundRadio.IsChecked.HasValue && difficultGroundRadio.IsChecked.Value)
				interaction.tokenType = TokenType.DifficultGround;
			if (fortifiedRadio.IsChecked.HasValue && fortifiedRadio.IsChecked.Value)
				interaction.tokenType = TokenType.Fortified;
			if (terrainRadio.IsChecked.HasValue && terrainRadio.IsChecked.Value)
				interaction.tokenType = TokenType.Terrain;

			//PersonType
			if (humanRadio.IsChecked == true)
				interaction.personType = PersonType.Human;
			if (elfRadio.IsChecked == true)
				interaction.personType = PersonType.Elf;
			if (hobbitRadio.IsChecked == true)
				interaction.personType = PersonType.Hobbit;
			if (dwarfRadio.IsChecked == true)
				interaction.personType = PersonType.Dwarf;

			//TerrainType
			if (barrelsRadio.IsChecked.HasValue && barrelsRadio.IsChecked.Value)
				interaction.terrainType = TerrainType.Barrels;
			if (barricadeRadio.IsChecked.HasValue && barricadeRadio.IsChecked.Value)
				interaction.terrainType = TerrainType.Barricade;
			if (boulderRadio.IsChecked.HasValue && boulderRadio.IsChecked.Value)
				interaction.terrainType = TerrainType.Boulder;
			if (bushRadio.IsChecked.HasValue && bushRadio.IsChecked.Value)
				interaction.terrainType = TerrainType.Bush;
			if (chestRadio.IsChecked.HasValue && chestRadio.IsChecked.Value)
				interaction.terrainType = TerrainType.Chest;
			if (elevationRadio.IsChecked.HasValue && elevationRadio.IsChecked.Value)
				interaction.terrainType = TerrainType.Elevation;
			if (fenceRadio.IsChecked.HasValue && fenceRadio.IsChecked.Value)
				interaction.terrainType = TerrainType.Fence;
			if (firePitRadio.IsChecked.HasValue && firePitRadio.IsChecked.Value)
				interaction.terrainType = TerrainType.FirePit;
			if (fountainRadio.IsChecked.HasValue && fountainRadio.IsChecked.Value)
				interaction.terrainType = TerrainType.Fountain;
			if (logRadio.IsChecked.HasValue && logRadio.IsChecked.Value)
				interaction.terrainType = TerrainType.Log;
			if (mistRadio.IsChecked.HasValue && mistRadio.IsChecked.Value)
				interaction.terrainType = TerrainType.Mist;
			if (pitRadio.IsChecked.HasValue && pitRadio.IsChecked.Value)
				interaction.terrainType = TerrainType.Pit;
			if (pondRadio.IsChecked.HasValue && pondRadio.IsChecked.Value)
				interaction.terrainType = TerrainType.Pond;
			if (rubbleRadio.IsChecked.HasValue && rubbleRadio.IsChecked.Value)
				interaction.terrainType = TerrainType.Rubble;
			if (statueRadio.IsChecked.HasValue && statueRadio.IsChecked.Value)
				interaction.terrainType = TerrainType.Statue;
			if (streamRadio.IsChecked.HasValue && streamRadio.IsChecked.Value)
				interaction.terrainType = TerrainType.Stream;
			if (tableRadio.IsChecked.HasValue && tableRadio.IsChecked.Value)
				interaction.terrainType = TerrainType.Table;
			if (trenchRadio.IsChecked.HasValue && trenchRadio.IsChecked.Value)
				interaction.terrainType = TerrainType.Trench;
			if (wallRadio.IsChecked.HasValue && wallRadio.IsChecked.Value)
				interaction.terrainType = TerrainType.Wall;
			if (webRadio.IsChecked.HasValue && webRadio.IsChecked.Value)
				interaction.terrainType = TerrainType.Web;
		}

		private void tokenTypeClick(object sender, RoutedEventArgs e)
		{
			RadioButton rb = e.Source as RadioButton;
			if (((string)rb.Content) == "Person")
			{
				personType.Visibility = Visibility.Visible;
			}
			else
			{
				personType.Visibility = Visibility.Collapsed;
			}

			if (((string)rb.Content) == "Terrain")
			{
				terrainType.Visibility = Visibility.Visible;
			}
			else
			{
				terrainType.Visibility = Visibility.Collapsed;
			}
		}

		private void editPersText_Click(object sender, RoutedEventArgs e)
		{
			TextBookData tbd = new TextBookData("Persistent Text");
			PersistentInteractionBase persistent = (PersistentInteractionBase)interaction;
			tbd.pages.Add(persistent.persistentText);

			TextEditorWindow te = new TextEditorWindow(scenario, EditMode.Persistent, tbd);
			if (te.ShowDialog() == true)
			{
				persistent.persistentText = te.textBookController.pages[0];
			}
		}

		private void tokenHelp_Click(object sender, RoutedEventArgs e)
		{
			HelpWindow hw = new HelpWindow(HelpType.Token, 1);
			hw.ShowDialog();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		void PropChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
