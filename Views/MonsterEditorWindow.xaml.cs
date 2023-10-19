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
		private static List<MonsterModifier> _monsterModifiers = MonsterModifier.Values.ToList();
		public static List<MonsterModifier> monsterModifiers 
		{
			get => _monsterModifiers;
		}

		public Scenario scenario { get; set; }
		public Monster monster { get; set; }
		public DefaultStats defaultStats { get; set; }

		int Light { get { return 2; } }
		int Medium { get { return 3; } }
		int Heavy { get { return 4; } }

		List<RadioButton> coreSetList;
		List<RadioButton> villainsOfEriajarList;
		List<RadioButton> shadedPathsList;
		List<RadioButton> denizensInDarknessList;
		List<RadioButton> unfurlingWarList;
		List<RadioButton> scorchersOfTheWildsList;
		List<RadioButton> allMonsterList;


		string[] monsterNames = (Collection.CORE_SET.Monsters.Select(m => m.dataName))
			.Concat(Collection.VILLAINS_OF_ERIAJAR.Monsters.Select(m => m.dataName)).ToArray()
			.Concat(Collection.SHADED_PATHS.Monsters.Select(m => m.dataName)).ToArray()
			.Concat(Collection.DENIZENS_IN_DARKNESS.Monsters.Select(m => m.dataName)).ToArray()
			.Concat(Collection.UNFURLING_WAR.Monsters.Select(m => m.dataName)).ToArray()
			.Concat(Collection.SCORCHERS_OF_THE_WILDS.Monsters.Select(m => m.dataName)).ToArray();

		public MonsterEditorWindow(Scenario s, Monster m = null )
		{
			InitializeComponent();
			DataContext = this;

			scenario = s;
			cancelButton.Visibility = m == null ? Visibility.Visible : Visibility.Collapsed;
			if(m != null)
            {
				//If a monster is already set, use it
				monster = m;
            }
			else
            {
				//Otherwise default to the top-left, basic enemy "Goblin" id 1, use default stats, and set the monster to appera in Easy, Normal, and Hard mode by default
				monster = new Monster(1);
				monster.defaultStats = true;
				monster.isEasy = true;
				monster.isNormal = true;
				monster.isHard = true;
				monster.count = 1; //Don't start off set to 0, it's confusing when you launch the campaign and no monsters appear.
            }

			if ( monster.defaultStats )
			{
				stats1.IsEnabled = false;
				stats2.IsEnabled = false;
			}
			FillDefaultStats( (int)monster.monsterType );

			//The order of the monsters in these lists is important to maintain unchanged because the order (0-26) tells the companion app which monster to use.
			//The order also corresponds to the order in the MonsterType enum.
			coreSetList = new List<RadioButton>() { ruffianRB, goblinScoutRB, orcHunterRB, orcMarauderRB, hungryVargRB, hillTrollRB, wightRB };
			villainsOfEriajarList = new List<RadioButton>() { atariRB, gargletargRB, chartoothRB };
			shadedPathsList = new List<RadioButton>() { giantSpiderRB, pitGoblinRB, orcTaskmasterRB, shadowmanRB, anonymousThingRB, caveTrollRB, balerockRB, spawnOfUglygiantRB };
			denizensInDarknessList = new List<RadioButton>() { supplicantOfMoreGothRB, ursulaRB, oliverRB };
			unfurlingWarList = new List<RadioButton>() { foulBeastRB, vargRiderRB, siegeEngineRB, warElephantRB, soldierRB, highOrcWarriorRB };
			scorchersOfTheWildsList = new List<RadioButton>() { lordJavelinRB, lichKingRB, endrisRB };
			allMonsterList = new List<RadioButton>();
			allMonsterList.AddRange(coreSetList);
			allMonsterList.AddRange(villainsOfEriajarList);
			allMonsterList.AddRange(shadedPathsList);
			allMonsterList.AddRange(denizensInDarknessList);
			allMonsterList.AddRange(unfurlingWarList);
			allMonsterList.AddRange(scorchersOfTheWildsList);

			Dictionary<List<RadioButton>, Collection> checkboxCollectionMap =
				new Dictionary<List<RadioButton>, Collection>() {
					{coreSetList, Collection.CORE_SET},
					{villainsOfEriajarList, Collection.VILLAINS_OF_ERIAJAR},
					{shadedPathsList, Collection.SHADED_PATHS},
					{denizensInDarknessList, Collection.DENIZENS_IN_DARKNESS},
					{unfurlingWarList, Collection.UNFURLING_WAR},
					{scorchersOfTheWildsList, Collection.SCORCHERS_OF_THE_WILDS}
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

			//Migrate movement
			if(monster.moveA == 0) { monster.moveA = monster.movementValue; }

			//negated radio buttons
			/*
			mightRB.IsChecked = monster.negatedBy == Ability.Might;
			agilityRB.IsChecked = monster.negatedBy == Ability.Agility;
			wisdomRB.IsChecked = monster.negatedBy == Ability.Wisdom;
			spiritRB.IsChecked = monster.negatedBy == Ability.Spirit;
			witRB.IsChecked = monster.negatedBy == Ability.Wit;
			*/
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
				id = Utils.defaultStats.Where(x => x.enumName == enemy).First().  id;
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
				monster.monsterType = (MonsterType)monster.id;
				monster.activationsId = defaultStats.id;
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

				AdjustEnemyCountComboBox(monster);
			}
		}

		private void AdjustEnemyCountComboBox(Monster m)
        {
			//This enforces the limits of enemy group size. e.g. there can be 3 Goblin Scouts in a group, but only 2 Cave Trolls, or only 1 Anonymous Thing
			if (monster.count > monster.groupLimit)
            {
				monster.count = monster.groupLimit;
            }

			countCB1.IsEnabled = (monster.groupLimit >= 1);
			countCB2.IsEnabled = (monster.groupLimit >= 2);
			countCB3.IsEnabled = (monster.groupLimit >= 3);
        }

		private void monsterType_Click( object sender, RoutedEventArgs e )
		{
			string enemy = ( (RadioButton)sender ).Content as string;
			enemy = Regex.Replace(enemy, "x[0-9]+", ""); //replace unit count info
			enemy = Regex.Replace(enemy, "of", "Of"); //replace lower case of
			enemy = enemy.Replace(" ", ""); //replace spaces
			FillDefaultStats( enemy );
		}


		//MonsterModifier ComboBox, ItemList, and buttons
		private void modifierCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (modifierCB.SelectedValue == null) { return; }
			addSelectedModifierButton.IsEnabled = modifierCB.SelectedValue as int? != 0;

			if (modifierCB.SelectedValue == null) { return; }
			int modId = modifierCB.SelectedValue as int? ?? default(int);
			MonsterModifier mod = MonsterModifier.FromID(modId);
			UpdateModifierDescription(mod);
		}

		private void addSelectedModifierButton_Click(object sender, RoutedEventArgs e)
		{
			if (modifierCB.SelectedValue == null) { return; }
			if (monster.modifierList.Count >= 7) 
			{
				modifierDescription.Text = "Maximum of 7 modifiers allowed.";
				return; 
			}
			int modId = modifierCB.SelectedValue as int? ?? default(int);
			if (!monster.modifierList.Contains(modId) && modId != 0)
			{
				monster.modifierList.Add(modId);
				monster.uiModifierList.Add(MonsterModifier.FromID(modId));
				Debug.Log("Add modifier " + modId + " " + MonsterModifier.FromID(modId).name);
				Debug.Log("Modifiers: " + string.Join(", ", monster.modifierList));
				Debug.Log("UiModifiers: " + string.Join(", ", monster.uiModifierList.ToList().ConvertAll(it => it.name)));
			}
		}

		private void addModifierButton_Click(object sender, RoutedEventArgs e)
		{
			/*
			MonsterModifierEditorWindow mmew = new MonsterModifierEditorWindow(scenario);
			if (mmew.ShowDialog() == true)
			{
				if (!monster.modifierList.Contains(MonsterModifier.FromID(mmew.id)))
					monster.modifierList.Add(MonsterModifier.FromID(mmew.id));
			}
			*/
		}

		private void removeModifierButton_Click(object sender, RoutedEventArgs e)
		{
			if (modifierCB.SelectedValue == null) { return; }
			int modId = ((Button)sender).DataContext as int? ?? default(int);

			if (monster.modifierList.Contains(modId))
			{
				monster.modifierList.Remove(modId);
				monster.uiModifierList.Remove(MonsterModifier.FromID(modId));
			}
		}

		private void modifierItem_MouseEnter(object sender, RoutedEventArgs e)
        {
			MonsterModifier mod = ((Border)sender).DataContext as MonsterModifier;

			UpdateModifierDescription(mod);
        }

		private void UpdateModifierDescription(MonsterModifier mod)
        {
			List<string> mods = new List<string>();
			if (mod.health != 0) { mods.Add((mod.health > 0 ? "+" : "") + mod.health + " Health"); }
			if (mod.armor != 0) { mods.Add((mod.armor > 0 ? "+" : "") + mod.armor + " Armor"); }
			if (mod.sorcery != 0) { mods.Add((mod.sorcery > 0 ? "+" : "") + mod.sorcery + " Sorcery"); }
			if (mod.damage != 0) { mods.Add((mod.damage > 0 ? "+" : "") + mod.damage + " Damage"); }
			if (mod.fear != 0) { mods.Add((mod.fear > 0 ? "+" : "") + mod.fear + " Fear"); }

			//Real immunities
			List<string> immunities = new List<string>();
			if (mod.immuneCleave) { immunities.Add("Cleave"); }
			if (mod.immuneLethal) { immunities.Add("Lethal"); }
			if (mod.immunePierce) { immunities.Add("Pierce"); }
			if (mod.immuneSmite) { immunities.Add("Smite"); }
			if (mod.immuneStun) { immunities.Add("Stun"); }
			if (mod.immuneSunder) { immunities.Add("Sunder"); }

			if (immunities.Count > 0) { mods.Add("Immune to " + string.Join(", ", immunities)); }

			//Fake immunities
			List<string> fakeImmunities = new List<string>();
			if (mod.fakeCleave) { fakeImmunities.Add("Cleave"); }
			if (mod.fakeLethal) { fakeImmunities.Add("Lethal"); }
			if (mod.fakePierce) { fakeImmunities.Add("Pierce"); }
			if (mod.fakeSmite) { fakeImmunities.Add("Smite"); }
			if (mod.fakeStun) { fakeImmunities.Add("Stun"); }
			if (mod.fakeSunder) { fakeImmunities.Add("Sunder"); }

			if (fakeImmunities.Count > 0) { mods.Add("Fake immunity to " + string.Join(", ", fakeImmunities)); }

			modifierDescription.Text = mod.name + ": " + string.Join("; ", mods);
		}

		private void modifierItem_MouseLeave(object sender, RoutedEventArgs e)
		{
			modifierDescription.Text = "";
		}

	}
}
