using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;

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
		List<CheckBox> villainsOfEriajarList;
		List<CheckBox> shadedPathsList;
		List<CheckBox> denizensInDarknessList;
		List<CheckBox> unfurlingWarList;
		List<CheckBox> scorchersOfTheWildsList;
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

		public ThreatInteractionWindow( Scenario s, ThreatInteraction inter = null , bool showCancelButton = false)
		{
			scenario = s;
			interaction = inter ?? new ThreatInteraction("New Threat Event");

			InitializeComponent();
			DataContext = this;

			cancelButton.Visibility = (inter == null || showCancelButton) ? Visibility.Visible : Visibility.Collapsed;
			updateUIForEventGroup();

			isThreatTriggered = scenario.threatObserver.Any( x => x.triggerName == interaction.dataName );
			if ( isThreatTriggered )
			{
				addMainTriggerButton.IsEnabled = false;
				triggeredByCB.IsEnabled = false;
				//isTokenCB.IsEnabled = false;
				interaction.isTokenInteraction = false;
			}



			oldName = interaction.dataName;

			//The order of the monsters in these lists is important to maintain unchanged because the order (0-26) tells the companion app which monster to use.
			//The order also corresponds to the order in the MonsterType enum.
			coreSetList = new List<CheckBox>() { ruffianCB, goblinScoutCB, orcHunterCB, orcMarauderCB, hungryVargCB, hillTrollCB, wightCB };
			villainsOfEriajarList = new List<CheckBox>() { atariCB, gargletargCB, chartoothCB };
			shadedPathsList = new List<CheckBox>() { giantSpiderCB, pitGoblinCB, orcTaskmasterCB, shadowmanCB, anonymousThingCB, caveTrollCB, balerockCB, spawnOfUglygiantCB };
			denizensInDarknessList = new List<CheckBox>() { supplicantOfMoreGothCB, ursulaCB, oliverCB };
			unfurlingWarList = new List<CheckBox>() { foulBeastCB, vargRiderCB, siegeEngineCB, warElephantCB, soldierCB, highOrcWarriorCB };
			scorchersOfTheWildsList = new List<CheckBox>() { lordJavelinCB, lichKingCB, endrisCB };

			allMonsterList = new List<CheckBox>();
			allMonsterList.AddRange(coreSetList);
			allMonsterList.AddRange(villainsOfEriajarList);
			allMonsterList.AddRange(shadedPathsList);
			allMonsterList.AddRange(denizensInDarknessList);
			allMonsterList.AddRange(unfurlingWarList);
			allMonsterList.AddRange(scorchersOfTheWildsList);

			Dictionary<List<CheckBox>, Collection> checkboxCollectionMap =
				new Dictionary<List<CheckBox>, Collection>() {
					{coreSetList, Collection.CORE_SET},
					{villainsOfEriajarList, Collection.VILLAINS_OF_ERIAJAR},
					{shadedPathsList, Collection.SHADED_PATHS},
					{denizensInDarknessList, Collection.DENIZENS_IN_DARKNESS},
					{unfurlingWarList, Collection.UNFURLING_WAR},
					{scorchersOfTheWildsList, Collection.SCORCHERS_OF_THE_WILDS},

					{new List<CheckBox>(){coreSetCB}, Collection.CORE_SET },
					{new List<CheckBox>(){villainsOfEriajarCB}, Collection.VILLAINS_OF_ERIAJAR },
					{new List<CheckBox>(){shadedPathsCB}, Collection.SHADED_PATHS },
					{new List<CheckBox>(){denizensInDarknessCB}, Collection.DENIZENS_IN_DARKNESS },
					{new List<CheckBox>(){unfurlingWarCB}, Collection.UNFURLING_WAR },
					{new List<CheckBox>(){scorchersOfTheWildsCB}, Collection.SCORCHERS_OF_THE_WILDS },
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
			if ( interaction.dataName == "New Threat Event" || scenario.interactionObserver.Count(x => x.dataName == interaction.dataName && x.GUID != interaction.GUID) > 0)
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

			tokenTypeSelector.AssignValuesFromSelections();

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
				me.monster.index = (interaction.monsterCollection.Count + 1);
				interaction.AddMonster( me.monster );
			}
		}

		private void EditButton_Click( object sender, RoutedEventArgs e )
		{
			Monster m = ( (Button)sender ).DataContext as Monster;
			MonsterEditorWindow me = new MonsterEditorWindow(scenario, m);
			//me.ShowDialog();
			m.translationKeyParents = interaction.dataName;
			m.HandleWindow(me, scenario.translationObserver);
		}

		private void DeleteButton_Click( object sender, RoutedEventArgs e )
		{
			var ret = MessageBox.Show("Are you sure you want to delete this Scripted Enemy?\n\nTHIS CANNOT BE UNDONE.", "Delete Scripted Enemy", MessageBoxButton.YesNo, MessageBoxImage.Question);
			if (ret == MessageBoxResult.Yes)
			{
				Monster m = ((Button)sender).DataContext as Monster;
				if(m.GUID == Guid.Empty) { m.GUID = Guid.NewGuid(); }
				interaction.UpdateKeysStartingWith(scenario.translationObserver, interaction.TranslationKeyPrefix() + "monster." + m.index.ToString() + ".", interaction.TranslationKeyPrefix() + "monster.deleted." + m.GUID + "."); //update translation key of deleted item to use its GUID
				interaction.monsterCollection.Remove(m);
				interaction.RenumberMonsters(scenario.translationObserver);
			}
		}

		void PropChanged( string name )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}

		private void groupHelp_Click( object sender, RoutedEventArgs e )
		{
			HelpWindow hw = new HelpWindow( HelpType.Grouping );
			hw.ShowDialog();
		}

		private void nameTB_TextChanged( object sender, TextChangedEventArgs e )
		{
			interaction.dataName = ( (TextBox)sender ).Text;
			updateUIForEventGroup();
		}

		private void updateUIForEventGroup()
		{
			Regex rx = new Regex(@"\sGRP\d+$");
			MatchCollection matches = rx.Matches(interaction.dataName);
			if (matches.Count > 0)
			{
				groupInfo.Text = "This Event is in the following group: " + matches[0].Value.Trim();
				isReusableCB.Visibility = Visibility.Visible;
			}
			else
			{
				groupInfo.Text = "This Event is in the following group: None";
				isReusableCB.Visibility = Visibility.Collapsed;
			}
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
			else if(collection.StartsWith("villainsOfEriajar"))
            {
				monsterList = villainsOfEriajarList;
            }
			else if(collection.StartsWith("shadedPaths"))
            {
				monsterList = shadedPathsList;
            }
			else if(collection.StartsWith("denizensInDarkness"))
            {
				monsterList = denizensInDarknessList;
            }
			else if(collection.StartsWith("unfurlingWar"))
            {
				monsterList = unfurlingWarList;
            }
			else if(collection.StartsWith("scorchersOfTheWilds"))
            {
				monsterList = scorchersOfTheWildsList;
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
