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


		string[] monsterNames = (Collection.CORE_SET.Monsters.Select(m => m.dataName))
			.Concat(Collection.VILLAINS_OF_ERIADOR.Monsters.Select(m => m.dataName)).ToArray()
			.Concat(Collection.SHADOWED_PATHS.Monsters.Select(m => m.dataName)).ToArray()
			.Concat(Collection.DWELLERS_IN_DARKNESS.Monsters.Select(m => m.dataName)).ToArray()
			.Concat(Collection.SPREADING_WAR.Monsters.Select(m => m.dataName)).ToArray()
			.Concat(Collection.SCOURGES_OF_THE_WASTES.Monsters.Select(m => m.dataName)).ToArray();

		public MonsterEditorWindow(Scenario s, Monster m = null )
		{
			InitializeComponent();
			DataContext = this;

			scenario = s;
			cancelButton.Visibility = m == null ? Visibility.Visible : Visibility.Collapsed;
			monster = m ?? new Monster(-1);

			if ( monster.defaultStats )
			{
				stats1.IsEnabled = false;
				stats2.IsEnabled = false;
			}
			FillDefaultStats( (int)monster.monsterType );

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
				//Console.WriteLine(index + " " + monsterRB.Content + " " + ((MonsterType)index).ToString());
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
				monster.dataName = monsterNames[(int)monster.monsterType];
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

				FillDefaultStats((int)monster.monsterType);
			}
			else
			{
				stats1.IsEnabled = true;
				stats2.IsEnabled = true;
			}
		}

		private void FillDefaultStats( string enemy )
        {
			int id = -1;
			try
            {
				id = Utils.defaultStats.Where(x => x.enumName == enemy).First().id;
			}
			catch(InvalidOperationException)
            {

            }
			FillDefaultStats(id);
		}

		private void FillDefaultStats(int id)
		{
			//Console.WriteLine("MonsterEditor FillDefaultStats " + id);
			try
			{
				defaultStats = Utils.defaultStats.Where(x => x.id == id).First();
				special.Text = string.Join(", ", defaultStats.special);
				monster.specialAbility = string.Join(", ", defaultStats.special);
			}
			catch(InvalidOperationException)
            {
				//TODO Set up the rest of the enemies in enemy-defaults.json and get rid of this
				defaultStats = new DefaultStats();
				defaultStats.id = id;
				defaultStats.dataName = "";
				defaultStats.health = 5;
				defaultStats.armor = 1;
				defaultStats.sorcery = 0;
				defaultStats.moveA = 2;
				defaultStats.moveB = 4;
				defaultStats.moveSpecial = new string[0];
				defaultStats.ranged = false;
				defaultStats.groupLimit = 3;
				defaultStats.figureLimit = 6;
				defaultStats.cost = new int[] { 7, 13, 19 };
				defaultStats.tag = new string[0];
				defaultStats.speed = "medium";
				defaultStats.damage = "light";
				defaultStats.fearsome = false;
				defaultStats.special = new string[0];
            }

			if ( monster.defaultStats )
			{
				//Only change the name if the current textbox name is empty or was the default name for the previous monster.
				if (string.IsNullOrEmpty(monster.dataName) 
					|| (monster.id >= 0 && monster.id < Collection.MONSTERS().Length && monster.dataName == Collection.MONSTERS()[monster.id].dataName)) 
				{ 
					monster.dataName = defaultStats.dataName;
				}
				monster.id = defaultStats.id;
				monster.health = defaultStats.health;
				monster.shieldValue = defaultStats.armor;
				monster.sorceryValue = defaultStats.sorcery;
				monster.moveA = defaultStats.moveA;
				monster.moveB = defaultStats.moveB;
				monster.moveSpecial = defaultStats.moveSpecial;
				monster.isRanged = defaultStats.ranged;
				monster.groupLimit = defaultStats.groupLimit;
				monster.figureLimit = defaultStats.figureLimit;
				monster.cost = defaultStats.cost;
				monster.tag = defaultStats.tag;
				monster.movementValue = defaultStats.speed == "light" ? Light - 1 : ( defaultStats.speed == "medium" ? Medium - 1 : Heavy - 1 );
				monster.damage = defaultStats.damage == "light" ? Light : ( defaultStats.damage == "medium" ? Medium : Heavy );
				monster.isFearsome = defaultStats.fearsome;
				monster.special = defaultStats.special;
				//monster.fear = defaultStats.damage == "light" ? Light : ( defaultStats.damage == "medium" ? Medium : Heavy );
			}
		}

		private void monsterType_Click( object sender, RoutedEventArgs e )
		{
			string enemy = ( (RadioButton)sender ).Content as string;
			enemy = Regex.Replace(enemy, "x[0-9]+", ""); //replace unit count info
			enemy = Regex.Replace(enemy, "of", "Of"); //replace lower case of
			enemy = enemy.Replace(" ", ""); //replace spaces
			FillDefaultStats( enemy );
		}
	}
}
