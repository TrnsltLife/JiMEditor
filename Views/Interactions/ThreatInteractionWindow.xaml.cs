using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using JiME.Models;

namespace JiME.Views
{
	/// <summary>
	/// Interaction logic for ThreatInteractionWindow.xaml
	/// </summary>
	public partial class ThreatInteractionWindow : Window, INotifyPropertyChanged
	{
		string oldName;

		public Scenario scenario { get; set; }
		public ThreatInteraction interaction { get; set; }
		bool closing = false;

		List<CheckBox> coreSetList;
		List<CheckBox> villainsOfEriadorList;
		List<CheckBox> shadowedPathsList;
		List<CheckBox> dwellersInDarknessList;
		List<CheckBox> spreadingWarList;
		List<CheckBox> scourgesOfTheWastesList;
		List<CheckBox> allMonsterList;

		public event PropertyChangedEventHandler PropertyChanged;
		bool _isThreatTriggered;
		public bool isThreatTriggered
		{
			get => _isThreatTriggered;
			set
			{
				_isThreatTriggered = value;
				PropChanged( "isThreatTriggered" );
			}
		}

		public ThreatInteractionWindow( Scenario s, ThreatInteraction inter = null )
		{
			InitializeComponent();
			DataContext = this;

			scenario = s;
			cancelButton.Visibility = inter == null ? Visibility.Visible : Visibility.Collapsed;
			interaction = inter ?? new ThreatInteraction( "New Threat Event" );

			isThreatTriggered = scenario.threatObserver.Any( x => x.triggerName == interaction.dataName );
			if ( isThreatTriggered )
			{
				addMainTriggerButton.IsEnabled = false;
				triggeredByCB.IsEnabled = false;
				isTokenCB.IsEnabled = false;
				interaction.isTokenInteraction = false;
			}

			if ( interaction.isTokenInteraction && interaction.tokenType == TokenType.Person )
				personType.Visibility = Visibility.Visible;
			else if (interaction.isTokenInteraction && interaction.tokenType == TokenType.Terrain)
				terrainType.Visibility = Visibility.Visible;

			//PersonType
			humanRadio.IsChecked = interaction.personType == PersonType.Human && interaction.tokenType == TokenType.Person;
			elfRadio.IsChecked = interaction.personType == PersonType.Elf && interaction.tokenType == TokenType.Person;
			hobbitRadio.IsChecked = interaction.personType == PersonType.Hobbit && interaction.tokenType == TokenType.Person;
			dwarfRadio.IsChecked = interaction.personType == PersonType.Dwarf && interaction.tokenType == TokenType.Person;

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


			oldName = interaction.dataName;

			//The order of the monsters in these lists is important to maintain unchanged because the order (0-26) tells the companion app which monster to use.
			//The order also corresponds to the order in the MonsterType enum.
			coreSetList = new List<CheckBox>() { ruffianCB, goblinScoutCB, orcHunterCB, orcMarauderCB, hungryWargCB, hillTrollCB, wightCB };
			villainsOfEriadorList = new List<CheckBox>() { atarinCB, gulgotarCB, coalfangCB };
			shadowedPathsList = new List<CheckBox>() { giantSpiderCB, pitGoblinCB, orcTaskmasterCB, shadowmanCB, namelessThingCB, caveTrollCB, balrogCB, spawnOfUngoliantCB };
			dwellersInDarknessList = new List<CheckBox>() { supplicantOfMorgothCB, ursaCB, ollieCB };
			spreadingWarList = new List<CheckBox>() { fellBeastCB, wargRiderCB, siegeEngineCB, warOliphauntCB, soldierCB, urukWarriorCB };
			scourgesOfTheWastesList = new List<CheckBox>() { lordAngonCB, witchKingOfAngmarCB, eadrisCB };

			allMonsterList = new List<CheckBox>();
			allMonsterList.AddRange(coreSetList);
			allMonsterList.AddRange(villainsOfEriadorList);
			allMonsterList.AddRange(shadowedPathsList);
			allMonsterList.AddRange(dwellersInDarknessList);
			allMonsterList.AddRange(spreadingWarList);
			allMonsterList.AddRange(scourgesOfTheWastesList);

			Dictionary<List<CheckBox>, Collection> checkboxCollectionMap =
				new Dictionary<List<CheckBox>, Collection>() {
					{coreSetList, Collection.CORE_SET},
					{villainsOfEriadorList, Collection.VILLAINS_OF_ERIADOR},
					{shadowedPathsList, Collection.SHADOWED_PATHS},
					{dwellersInDarknessList, Collection.DWELLERS_IN_DARKNESS},
					{spreadingWarList, Collection.SPREADING_WAR},
					{scourgesOfTheWastesList, Collection.SCOURGES_OF_THE_WASTES},

					{new List<CheckBox>(){coreSetCB}, Collection.CORE_SET },
					{new List<CheckBox>(){villainsOfEriadorCB}, Collection.VILLAINS_OF_ERIADOR },
					{new List<CheckBox>(){shadowedPathsCB}, Collection.SHADOWED_PATHS },
					{new List<CheckBox>(){dwellersInDarknessCB}, Collection.DWELLERS_IN_DARKNESS },
					{new List<CheckBox>(){spreadingWarCB}, Collection.SPREADING_WAR },
					{new List<CheckBox>(){scourgesOfTheWastesCB}, Collection.SCOURGES_OF_THE_WASTES },
				};

			//Help handle a situation where an old file type had less enemies available than we do now.
			if (allMonsterList.Count > interaction.includedEnemies.Length) { interaction.ResizeIncludedEnemies(); }

			int index = 0;
			foreach (var monsterCheckbox in allMonsterList)
            {
				monsterCheckbox.IsChecked = interaction.includedEnemies[index];
				index++;
            }

			//Enable/Disable Collections and their monsters based on the Collections enabled for this Scenario
			foreach (KeyValuePair<List<CheckBox>, Collection> pair in checkboxCollectionMap)
			{
				if(scenario.IsCollectionEnabled(pair.Value))
                {
					foreach (var checkbox in pair.Key)
					{
						checkbox.IsEnabled = true;
						checkbox.FontStyle = FontStyles.Normal;
					}
				}
				else 
				{
					foreach(var checkbox in pair.Key)
                    {
						checkbox.IsChecked = false;
						checkbox.IsEnabled = false;
						checkbox.FontStyle = FontStyles.Italic;
                    }
                }
            }

			biasLight.IsChecked = interaction.difficultyBias == DifficultyBias.Light;
			biasMedium.IsChecked = interaction.difficultyBias == DifficultyBias.Medium;
			biasHeavy.IsChecked = interaction.difficultyBias == DifficultyBias.Heavy;
		}

		private void isTokenCB_Click( object sender, RoutedEventArgs e )
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

		private void EditFlavorButton_Click( object sender, RoutedEventArgs e )
		{
			TextEditorWindow tw = new TextEditorWindow( scenario, EditMode.Flavor, interaction.textBookData );
			if ( tw.ShowDialog() == true )
			{
				interaction.textBookData.pages = tw.textBookController.pages;
			}
		}

		private void EditEventButton_Click( object sender, RoutedEventArgs e )
		{
			TextEditorWindow tw = new TextEditorWindow( scenario, EditMode.Event, interaction.eventBookData );
			if ( tw.ShowDialog() == true )
			{
				interaction.eventBookData.pages = tw.textBookController.pages;
			}
		}

		bool TryClosing()
		{
			//check for dupe name
			if ( interaction.dataName == "New Threat Event" || scenario.interactionObserver.Count( x => x.dataName == interaction.dataName ) > 1 )
			{
				MessageBox.Show( "Give this Event a unique name.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
				return false;
			}

			return true;
		}

		private void Window_Closing( object sender, CancelEventArgs e )
		{
			if ( !closing )
				e.Cancel = true;
		}

		private void addMainTriggerAfterButton_Click( object sender, RoutedEventArgs e )
		{
			TriggerEditorWindow tw = new TriggerEditorWindow( scenario );
			if ( tw.ShowDialog() == true )
			{
				interaction.triggerAfterName = tw.triggerName;
			}
		}

		private void addMainTriggerButton_Click( object sender, RoutedEventArgs e )
		{
			TriggerEditorWindow tw = new TriggerEditorWindow( scenario );
			if ( tw.ShowDialog() == true )
			{
				interaction.triggerName = tw.triggerName;
			}
		}

		private void OkButton_Click( object sender, RoutedEventArgs e )
		{
			if ( !TryClosing() )
				return;

			//TokenType
			if ( searchRadio.IsChecked.HasValue && searchRadio.IsChecked.Value )
				interaction.tokenType = TokenType.Search;
			if ( personRadio.IsChecked.HasValue && personRadio.IsChecked.Value )
				interaction.tokenType = TokenType.Person;
			if ( darkRadio.IsChecked.HasValue && darkRadio.IsChecked.Value )
				interaction.tokenType = TokenType.Darkness;
			if ( threatRadio.IsChecked.HasValue && threatRadio.IsChecked.Value )
				interaction.tokenType = TokenType.Threat;
			if (difficultGroundRadio.IsChecked.HasValue && difficultGroundRadio.IsChecked.Value)
				interaction.tokenType = TokenType.DifficultGround;
			if (fortifiedRadio.IsChecked.HasValue && fortifiedRadio.IsChecked.Value)
				interaction.tokenType = TokenType.Fortified;
			if (terrainRadio.IsChecked.HasValue && terrainRadio.IsChecked.Value)
				interaction.tokenType = TokenType.Terrain;

			//PersonType
			if ( humanRadio.IsChecked == true )
				interaction.personType = PersonType.Human;
			if ( elfRadio.IsChecked == true )
				interaction.personType = PersonType.Elf;
			if ( hobbitRadio.IsChecked == true )
				interaction.personType = PersonType.Hobbit;
			if ( dwarfRadio.IsChecked == true )
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

			scenario.UpdateEventReferences( oldName, interaction );

			int index = 0;
			foreach (var monsterCheckbox in allMonsterList)
			{
				interaction.includedEnemies[index] = monsterCheckbox.IsChecked.Value;
				index++;
			}

			if ( biasLight.IsChecked == true )
				interaction.difficultyBias = DifficultyBias.Light;
			if ( biasMedium.IsChecked == true )
				interaction.difficultyBias = DifficultyBias.Medium;
			if ( biasHeavy.IsChecked == true )
				interaction.difficultyBias = DifficultyBias.Heavy;

			closing = true;
			DialogResult = true;
		}

		private void CancelButton_Click( object sender, RoutedEventArgs e )
		{
			closing = true;
			DialogResult = false;
		}

		private void Window_ContentRendered( object sender, System.EventArgs e )
		{
			nameTB.Focus();
			nameTB.SelectAll();
		}

		private void AddMonsterButton_Click( object sender, RoutedEventArgs e )
		{
			MonsterEditorWindow me = new MonsterEditorWindow(scenario);
			if ( me.ShowDialog() == true )
			{
				interaction.AddMonster( me.monster );
			}
		}

		private void EditButton_Click( object sender, RoutedEventArgs e )
		{
			Monster m = ( (Button)sender ).DataContext as Monster;
			MonsterEditorWindow me = new MonsterEditorWindow(scenario, m);
			me.ShowDialog();
		}

		private void DeleteButton_Click( object sender, RoutedEventArgs e )
		{
			Monster m = ( (Button)sender ).DataContext as Monster;
			interaction.monsterCollection.Remove( m );
		}

		void PropChanged( string name )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}

		private void tokenHelp_Click( object sender, RoutedEventArgs e )
		{
			HelpWindow hw = new HelpWindow( HelpType.Token, 1 );
			hw.ShowDialog();
		}

		private void groupHelp_Click( object sender, RoutedEventArgs e )
		{
			HelpWindow hw = new HelpWindow( HelpType.Grouping );
			hw.ShowDialog();
		}

		private void nameTB_TextChanged( object sender, TextChangedEventArgs e )
		{
			interaction.dataName = ( (TextBox)sender ).Text;
			Regex rx = new Regex( @"\sGRP\d+$" );
			MatchCollection matches = rx.Matches( interaction.dataName );
			if ( matches.Count > 0 )
				groupInfo.Text = "This Event is in the following group: " + matches[0].Value.Trim();
			else
				groupInfo.Text = "This Event is in the following group: None";
		}

		private void addDefeatedTriggerButton_Click( object sender, RoutedEventArgs e )
		{
			TriggerEditorWindow tw = new TriggerEditorWindow( scenario );
			if ( tw.ShowDialog() == true )
			{
				interaction.triggerDefeatedName = tw.triggerName;
			}
		}

		private void help_Click( object sender, RoutedEventArgs e )
		{
			HelpWindow hw = new HelpWindow( HelpType.Enemies, 0 );
			hw.ShowDialog();
		}

		private void tokenTypeClick( object sender, RoutedEventArgs e )
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

		private void collection_Click(object sender, RoutedEventArgs e)
		{
			CheckBox checkbox = ((CheckBox)sender);
			string collection = checkbox.Name as string;
			bool? check = checkbox.IsChecked;

			List<CheckBox> monsterList = null;
			if(collection.StartsWith("coreSet"))
            {
				monsterList = coreSetList;
            }
			else if(collection.StartsWith("villainsOfEriador"))
            {
				monsterList = villainsOfEriadorList;
            }
			else if(collection.StartsWith("shadowedPaths"))
            {
				monsterList = shadowedPathsList;
            }
			else if(collection.StartsWith("dwellersInDarkness"))
            {
				monsterList = dwellersInDarknessList;
            }
			else if(collection.StartsWith("spreadingWar"))
            {
				monsterList = spreadingWarList;
            }
			else if(collection.StartsWith("scourgesOfTheWastes"))
            {
				monsterList = scourgesOfTheWastesList;
            }
			monsterList.ForEach(it => it.IsChecked = check);
		}

		private void simulateBtn_Click( object sender, RoutedEventArgs e )
		{
			var sd = new SimulatorData()
			{
				poolPoints = interaction.basePoolPoints,
				difficultyBias = biasLight.IsChecked == true ? DifficultyBias.Light : ( biasMedium.IsChecked == true ? DifficultyBias.Medium : DifficultyBias.Heavy )
			};


			int index = 0;
			foreach (var monsterCheckbox in allMonsterList)
			{
				sd.includedEnemies[index] = monsterCheckbox.IsChecked.Value;
				index++;
			}

			var sim = new EnemyCalculator( sd );
			sim.ShowDialog();
		}
	}
}
