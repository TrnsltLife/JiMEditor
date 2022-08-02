using JiME.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace JiME.Views
{
	/// <summary>
	/// Interaction logic for MonsterEditorWindow.xaml
	/// </summary>
	public partial class MonsterEditorWindow : Window
	{
		public Scenario scenario { get; set; }
		public Monster monster { get; set; }
		public DefaultStats defaultStats { get; set; }

		int Light { get { return 2; } }
		int Medium { get { return 3; } }
		int Heavy { get { return 4; } }

		List<RadioButton> coreSetList;
		List<RadioButton> villainsOfEriadorList;
		List<RadioButton> shadowedPathsList;
		List<RadioButton> dwellersInDarknessList;
		List<RadioButton> spreadingWarList;
		List<RadioButton> scourgesOfTheWastesList;
		List<RadioButton> allMonsterList;

		public MonsterEditorWindow(Scenario s, Monster m = null )
		{
			InitializeComponent();
			DataContext = this;

			scenario = s;
			cancelButton.Visibility = m == null ? Visibility.Visible : Visibility.Collapsed;
			monster = m ?? new Monster( "" );

			if ( monster.defaultStats )
			{
				stats1.IsEnabled = false;
				stats2.IsEnabled = false;
			}
			FillDefaultStats( monster.monsterType.ToString() );

			//The order of the monsters in these lists is important to maintain unchanged because the order (0-26) tells the companion app which monster to use.
			//The order also corresponds to the order in the MonsterType enum.
			coreSetList = new List<RadioButton>() { ruffianRB, goblinScoutRB, orcHunterRB, orcMarauderRB, hungryWargRB, hillTrollRB, wightRB };
			villainsOfEriadorList = new List<RadioButton>() { atarinRB, gulgotarRB, coalfangRB };
			shadowedPathsList = new List<RadioButton>() { giantSpiderRB, pitGoblinRB, orcTaskmasterRB, shadowmanRB, namelessThingRB, caveTrollRB, balrogRB, spawnOfUngoliantRB };
			dwellersInDarknessList = new List<RadioButton>() { supplicantOfMorgothRB, ursaRB, ollieRB };
			spreadingWarList = new List<RadioButton>() { fellBeastRB, wargRiderRB, siegeEngineRB, warOliphauntRB, soldierRB, urukWarriorRB };
			scourgesOfTheWastesList = new List<RadioButton>() { lordAngonRB, witchKingOfAngmarRB, eadrisRB };
			allMonsterList = new List<RadioButton>();
			allMonsterList.AddRange(coreSetList);
			allMonsterList.AddRange(villainsOfEriadorList);
			allMonsterList.AddRange(shadowedPathsList);
			allMonsterList.AddRange(dwellersInDarknessList);
			allMonsterList.AddRange(spreadingWarList);
			allMonsterList.AddRange(scourgesOfTheWastesList);

			Dictionary<List<RadioButton>, Collection> checkboxCollectionMap =
				new Dictionary<List<RadioButton>, Collection>() {
					{coreSetList, Collection.CORE_SET},
					{villainsOfEriadorList, Collection.VILLAINS_OF_ERIADOR},
					{shadowedPathsList, Collection.SHADOWED_PATHS},
					{dwellersInDarknessList, Collection.DWELLERS_IN_DARKNESS},
					{spreadingWarList, Collection.SPREADING_WAR},
					{scourgesOfTheWastesList, Collection.SCOURGES_OF_THE_WASTES}
			};

			//monster type radio buttons
			int index = 0;
			foreach(var monsterRB in allMonsterList)
            {
				Console.WriteLine(index + " " + monsterRB.Content + " " + ((MonsterType)index).ToString());
				monsterRB.IsChecked = monster.monsterType == (MonsterType)index;
				index++;
            }

			//Enable/Disable Collections and their monsters based on the Collections enabled for this Scenario
			foreach (KeyValuePair<List<RadioButton>, Collection> pair in checkboxCollectionMap)
			{
				if (scenario.IsCollectionEnabled(pair.Value))
				{
					foreach (var checkbox in pair.Key)
					{
						checkbox.IsEnabled = true;
						checkbox.FontStyle = FontStyles.Normal;
					}
				}
				else
				{
					foreach (var checkbox in pair.Key)
					{
						checkbox.IsChecked = false;
						checkbox.IsEnabled = false;
						checkbox.FontStyle = FontStyles.Italic;
					}
				}
			}

			//negated radio buttons
			mightRB.IsChecked = monster.negatedBy == Ability.Might;
			agilityRB.IsChecked = monster.negatedBy == Ability.Agility;
			wisdomRB.IsChecked = monster.negatedBy == Ability.Wisdom;
			spiritRB.IsChecked = monster.negatedBy == Ability.Spirit;
			witRB.IsChecked = monster.negatedBy == Ability.Wit;
		}

		private void OkButton_Click( object sender, RoutedEventArgs e )
		{
			TryClose();
			DialogResult = true;
		}

		private void CancelButton_Click( object sender, RoutedEventArgs e )
		{
			DialogResult = false;
		}

		void TryClose()
		{
			int index = 0;
			foreach (var monsterRB in allMonsterList)
			{
				if(monsterRB.IsChecked == true)
                {
					monster.monsterType = (MonsterType)index;
					break;
                };
				index++;
			}

			if ( string.IsNullOrEmpty( nameTB.Text.Trim() ) )
				monster.dataName = Monster.monsterNames[(int)monster.monsterType];
		}

		private void Window_Closing( object sender, System.ComponentModel.CancelEventArgs e )
		{
			TryClose();
		}

		private void useDefaultCB_Click( object sender, RoutedEventArgs e )
		{
			if ( ( (CheckBox)sender ).IsChecked == true )
			{
				stats1.IsEnabled = false;
				stats2.IsEnabled = false;

				int index = 0;
				foreach (var monsterRB in allMonsterList)
				{
					if (monsterRB.IsChecked == true)
					{
						monster.monsterType = (MonsterType)index;
						break;
					};
					index++;
				}

				Console.WriteLine("FillDefaultStats: " + monster.monsterType.ToString());
				FillDefaultStats( monster.monsterType.ToString() );
			}
			else
			{
				stats1.IsEnabled = true;
				stats2.IsEnabled = true;
			}
		}

		private void FillDefaultStats( string enemy )
		{
			try
			{
				defaultStats = Utils.defaultStats.Where(x => x.name == enemy).First();
				special.Text = defaultStats.special;
				monster.specialAbility = defaultStats.special;
			}
			catch(InvalidOperationException e)
            {
				//TODO Set up the rest of the enemies in enemy-defaults.json and get rid of this
				defaultStats = new DefaultStats();
				defaultStats.name = enemy;
				defaultStats.health = 5;
				defaultStats.speed = "light";
				defaultStats.damage = "light";
				defaultStats.armor = 1;
				defaultStats.sorcery = 0;
				defaultStats.special = "";
            }

			if ( monster.defaultStats )
			{
				monster.health = defaultStats.health;
				monster.movementValue = defaultStats.speed == "light" ? Light - 1 : ( defaultStats.speed == "medium" ? Medium - 1 : Heavy - 1 );
				monster.damage = defaultStats.damage == "light" ? Light : ( defaultStats.damage == "medium" ? Medium : Heavy );
				//monster.fear = defaultStats.damage == "light" ? Light : ( defaultStats.damage == "medium" ? Medium : Heavy );
				monster.shieldValue = defaultStats.armor;
				monster.sorceryValue = defaultStats.sorcery;
			}
		}

		private void monsterType_Click( object sender, RoutedEventArgs e )
		{
			string enemy = ( (RadioButton)sender ).Content as string;
			Console.WriteLine("monsterType_Click enemy 1: " + enemy);
			enemy = Regex.Replace(enemy, "x[0-9]+", ""); //replace unit count info
			enemy = Regex.Replace(enemy, "of", "Of"); //replace lower case of
			enemy = enemy.Replace(" ", ""); //replace spaces
			Console.WriteLine("monsterType_Click enemy 2: " + enemy);
			FillDefaultStats( enemy );
		}
	}
}
