using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using JiME.Views;

namespace JiME
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		public Scenario scenario { get; set; }

		public MainWindow(Guid campaignGUID) : this(null)
		{
			scenario.campaignGUID = campaignGUID;
		}

		public MainWindow(Scenario s = null)
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

			//Initialize utilities
			Utils.Init();

			//Initialize scenario
			scenario = s ?? new Scenario();
			scenario.TriggerTitleChange(false);
			DataContext = scenario;
			Debug.Log(scenario.scenarioGUID);

			//We need to initialize the scenario before we initialize the component.
			InitializeComponent();

			appVersion.Text = Utils.appVersion;
			formatVersion.Text = Utils.formatVersion;

			interactionsUC.onAddEvent += OnAddEvent;
			interactionsUC.onRemoveEvent += OnRemoveEvent;
			interactionsUC.onSettingsEvent += OnSettingsInteraction;
			interactionsUC.onDuplicateEvent += OnDuplicateInteraction;
			interactionsUC.onImportEvent += OnImportInteraction;
			interactionsUC.onExportEvent += OnExportInteraction;
            interactionsUC.NewItemSelectedEvent += NewItemSelectedEvent;

            triggersUC.onAddEvent += OnAddTrigger;
			triggersUC.onRemoveEvent += OnRemoveTrigger;
			triggersUC.onSettingsEvent += OnSettingsTrigger;
			triggersUC.onDuplicateEvent += OnDuplicateTrigger;
            triggersUC.NewItemSelectedEvent += NewItemSelectedEvent;

            objectivesUC.onAddEvent += OnAddObjective;
			objectivesUC.onRemoveEvent += OnRemoveObjective;
			objectivesUC.onSettingsEvent += OnSettingsObjective;
			objectivesUC.onDuplicateEvent += OnDuplicateObjective;
            objectivesUC.NewItemSelectedEvent += NewItemSelectedEvent;

            activationsUC.onAddEvent += OnAddActivations;
			activationsUC.onRemoveEvent += OnRemoveActivations;
			activationsUC.onSettingsEvent += OnSettingsActivations;
			activationsUC.onDuplicateEvent += OnDuplicateActivations;
			activationsUC.onImportEvent += OnImportActivations;
			activationsUC.onExportEvent += OnExportActivations;

			monsterModifiersUC.onAddEvent += OnAddMonsterModifier;
			monsterModifiersUC.onRemoveEvent += OnRemoveMonsterModifier;
			monsterModifiersUC.onSettingsEvent += OnSettingsMonsterModifier;
			monsterModifiersUC.onDuplicateEvent += OnDuplicateMonsterModifier;
			monsterModifiersUC.onImportEvent += OnImportMonsterModifier;
			monsterModifiersUC.onExportEvent += OnExportMonsterModifier;

			translationsUC.onAddEvent += OnAddTranslation;
			translationsUC.onRemoveEvent += OnRemoveTranslation;
			translationsUC.onSettingsEvent += OnSettingsTranslation;
			//translationsUC.onDuplicateEvent += OnDuplicationTranslation;
			translationsUC.onImportEvent += OnImportTranslation;
			translationsUC.onExportEvent += OnExportTranslation;

			//Debug.Log( this.FindResource( "mylist" ).GetType() );

			//setup source of UI lists (scenario has to be created first!)
			((CollectionViewSource)interactionsUC.Resources["cvsListSort"]).Source = scenario.interactionObserver;
			((CollectionViewSource)triggersUC.Resources["cvsListSort"]).Source = scenario.triggersObserver;
			((CollectionViewSource)objectivesUC.Resources["cvsListSort"]).Source = scenario.objectiveObserver;
			((CollectionViewSource)activationsUC.Resources["cvsListSort"]).Source = scenario.activationsObserver;
			((CollectionViewSource)monsterModifiersUC.Resources["cvsListSort"]).Source = scenario.monsterModifierObserver;
			((CollectionViewSource)translationsUC.Resources["cvsListSort"]).Source = scenario.translationObserver;

			// Initiate visualization defer timer so it won't be done multiple times
			WpfUtils.MainThreadDispatcher = this.Dispatcher;

            // Initialize visualization
            GraphX.Controls.ZoomControl.SetViewFinderVisibility(visualizationZoomCtrl, Visibility.Visible); //Set minimap (overview) window to be visible by default
            Loaded += (object sender, RoutedEventArgs e) =>
            {
                // Show the scenario itself after the window has loaded
                // NOTE: Not deferred here since we want to zoom in after initial loading
                VisualizeScenario();

                // Set Fill zooming strategy so whole graph will be always visible (first time this is loading)
                visualizationZoomCtrl.ZoomToFill(); 
            };
            Activated += (object sender, EventArgs e) =>
            {
                // Re-visualize whenever this window regains focus since all changing of the Scenario contents happens in dialogs.
                // NOTE: This is not enough since Activated does not get triggered when we e.g. return from MessageBox dialog
                TriggerDeferredVisualizeScenario();
            };
            scenario.AddObserverChangedHandler((a, b) =>
            {
                // Any of the collections within the Scenario changed so we need to Re-visualize since this can happen through buttons
                // in the main editor window and MessageBox closing does not trigger re-visualization
                TriggerDeferredVisualizeScenario();
            });

            //debug
            //debug();
        }

        private void TriggerDeferredVisualizeScenario()
        {
            // This will wait at minimum 100ms for new triggers and only execute when no new triggers have been done during that time
            // Useful to avoid re-visualizing multiple times in short succession (e.g. due to adding a trigger also sorts it which triggers this MANY times)
            //Console.WriteLine("TRIGGER VISUALIZE SCENARIO");
            WpfUtils.DeferExecution("VisualizeScenario", 100, VisualizeScenario);

            // Show the loading indicator here so it has time to appear before load starts
            if (graphLoadingIndicator != null)
            {
                graphLoadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void VisualizeScenario()
        {
            //Console.WriteLine("VISUALIZE SCENARIO");
            try
            {
                if (scenario != null)
                {
                    var dataGraph = Visualization.Graph.Generate(scenario, VisializationItemClicked);
                    visualizationGraphArea.ShowGraph(dataGraph);
                }
            }
            finally
            {
                graphLoadingIndicator.Visibility = Visibility.Hidden;
            }
        }

        private void VisializationItemClicked(Visualization.DataVertex item)
        {
            switch (item.VertexType)
            {
                case Visualization.DataVertex.Type.Start:
                case Visualization.DataVertex.Type.Resolution:
                case Visualization.DataVertex.Type.ThreatLevel:
                    OpenScenarioEditor();
                    break;

                case Visualization.DataVertex.Type.Trigger:
                    OpenTriggerEditor(item.Source as Trigger);
                    break;

                case Visualization.DataVertex.Type.Objective:
                    OpenObjectiveEditor(item.Source as Objective);
                    break;

                case Visualization.DataVertex.Type.Interaction:
                    OpenInteractionEditor(item.Source);
                    break;

                case Visualization.DataVertex.Type.InteractionGroup:
                    MessageBox.Show("Not editable directly", "InteractionGroup", MessageBoxButton.OK);
                    break;

                case Visualization.DataVertex.Type.Chapter:
                    OpenChapterPropertiesEditor(item.Source as Chapter);
                    break;

                case Visualization.DataVertex.Type.Tile:
                    OpenTileEditor(item.Source as BaseTile);
                    break;

                case Visualization.DataVertex.Type.Token:
                    OpenTileEditor(item.Source2 as BaseTile, highlightToken: item.Source as Token);
                    break;

                default:
                    break; // No nothing
            }
        }

        void debug()
		{
			//scenario.threatObserver.Add( new Threat( "Threat 1", 10 ) { threshold = 10, triggerName = "Threat Trigger" } );
			//scenario.AddInteraction( new Interaction( "Dummy Event", false ) { interactionType = InteractionType.Text, triggerName = "Threat Trigger" } );

			//scenario.AddInteraction( new TextInteraction( "Dummy Text Interaction" ) );
		}

		#region TOOLBAR ACTIONS

		// Event / Interaction
		void OnAddEvent( object sender, EventArgs e )
		{
			ContextMenu cm = this.FindResource( "cmButton" ) as ContextMenu;
			cm.PlacementTarget = sender as Button;
			cm.IsOpen = true;
		}

		void OnRemoveEvent( object sender, EventArgs e )
		{
			//TODO check if USED by a THREAT

			if (interactionsUC.dataListView.SelectedItem is StartInteraction)
			{
				MessageBox.Show("You can't delete the Starting Position event.", "Cannot Edit Startg Position Event", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			var ret = MessageBox.Show("Are you sure you want to delete this Event?\n\nALL OF ITS DATA WILL BE DELETED.", "Delete Event", MessageBoxButton.YesNo, MessageBoxImage.Question);
			if (ret == MessageBoxResult.Yes)
			{
				int idx = interactionsUC.dataListView.SelectedIndex;
				if (idx != -1)
					scenario.RemoveData(interactionsUC.dataListView.SelectedItem);
				interactionsUC.dataListView.SelectedIndex = 0;
			}
		}

		void OnSettingsInteraction(object sender, EventArgs e)
		{
			OpenInteractionEditor(interactionsUC.dataListView.SelectedItem);
		}

		private void OpenInteractionEditor(object interactionItem)
		{
			if (interactionItem is TextInteraction)
			{
				TextInteraction castInteraction = (TextInteraction)interactionItem;
				TextInteractionWindow w = new TextInteractionWindow(scenario, castInteraction);
				castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is BranchInteraction)
			{
				BranchInteraction castInteraction = (BranchInteraction)interactionItem;
				BranchInteractionWindow w = new BranchInteractionWindow(scenario, castInteraction);
				castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is TestInteraction)
			{
				TestInteraction castInteraction = (TestInteraction)interactionItem;
				TestInteractionWindow w = new TestInteractionWindow(scenario, castInteraction);
				castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is DecisionInteraction)
			{
				DecisionInteraction castInteraction = (DecisionInteraction)interactionItem;
				DecisionInteractionWindow w = new DecisionInteractionWindow(scenario, castInteraction);
				castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is ThreatInteraction)
			{
				ThreatInteraction castInteraction = (ThreatInteraction)interactionItem;
				ThreatInteractionWindow w = new ThreatInteractionWindow(scenario, castInteraction);
				castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is MultiEventInteraction)
			{
				MultiEventInteraction castInteraction = (MultiEventInteraction)interactionItem;
				MultiEventWindow w = new MultiEventWindow(scenario, castInteraction);
				castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is PersistentTokenInteraction)
			{
				PersistentTokenInteraction castInteraction = (PersistentTokenInteraction)interactionItem;
				PersistentInteractionWindow w = new PersistentInteractionWindow(scenario, castInteraction);
				castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is ConditionalInteraction)
			{
				ConditionalInteraction castInteraction = (ConditionalInteraction)interactionItem;
				ConditionalInteractionWindow w = new ConditionalInteractionWindow(scenario, castInteraction);
				castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is DialogInteraction)
			{
				DialogInteraction castInteraction = (DialogInteraction)interactionItem;
				DialogInteractionWindow w = new DialogInteractionWindow(scenario, castInteraction);
				castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is ReplaceTokenInteraction)
			{
				ReplaceTokenInteraction castInteraction = (ReplaceTokenInteraction)interactionItem;
				ReplaceTokenInteractionWindow w = new ReplaceTokenInteractionWindow(scenario, castInteraction);
				//w.ShowDialog();
				castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is RewardInteraction)
			{
				RewardInteraction castInteraction = (RewardInteraction)interactionItem;
				RewardInteractionWindow w = new RewardInteractionWindow(scenario, castInteraction);
				castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is CorruptionInteraction)
			{
				CorruptionInteraction castInteraction = (CorruptionInteraction)interactionItem;
				CorruptionInteractionWindow w = new CorruptionInteractionWindow(scenario, castInteraction);
				castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is ItemInteraction)
			{
				ItemInteraction castInteraction = (ItemInteraction)interactionItem;
				ItemInteractionWindow w = new ItemInteractionWindow(scenario, castInteraction);
				castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is TitleInteraction)
			{
				TitleInteraction castInteraction = (TitleInteraction)interactionItem;
				TitleInteractionWindow w = new TitleInteractionWindow(scenario, castInteraction);
				castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is StartInteraction)
			{
				MessageBox.Show("You can't edit the Starting Position event. But you can place the Start token on the Starting Tile in Tile Editor -> Token Editor.", "Cannot Edit Starting Position Event", MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}

		void OnDuplicateInteraction(object sender, EventArgs e)
		{
			int idx = interactionsUC.dataListView.SelectedIndex;
			if (interactionsUC.dataListView.SelectedItem is TextInteraction)
			{
				TextInteraction interact = ((TextInteraction)interactionsUC.dataListView.Items[idx]).Clone();
				TextInteractionWindow bw = new TextInteractionWindow(scenario, interact, true);
				if (bw.ShowDialog() == true) { scenario.interactionObserver.Add(interact); }
			}
			else if (interactionsUC.dataListView.SelectedItem is BranchInteraction)
			{
				BranchInteraction interact = ((BranchInteraction)interactionsUC.dataListView.Items[idx]).Clone();
				BranchInteractionWindow bw = new BranchInteractionWindow(scenario, (BranchInteraction)interact, true);
				if (bw.ShowDialog() == true) { scenario.interactionObserver.Add(interact); }
			}
			else if (interactionsUC.dataListView.SelectedItem is TestInteraction)
			{
				TestInteraction interact = ((TestInteraction)interactionsUC.dataListView.Items[idx]).Clone();
				TestInteractionWindow bw = new TestInteractionWindow(scenario, (TestInteraction)interact, true);
				if (bw.ShowDialog() == true) { scenario.interactionObserver.Add(interact); }
			}
			else if (interactionsUC.dataListView.SelectedItem is DecisionInteraction)
			{
				DecisionInteraction interact = ((DecisionInteraction)interactionsUC.dataListView.Items[idx]).Clone();
				DecisionInteractionWindow bw = new DecisionInteractionWindow(scenario, (DecisionInteraction)interact, true);
				if (bw.ShowDialog() == true) { scenario.interactionObserver.Add(interact); }
			}
			else if (interactionsUC.dataListView.SelectedItem is ThreatInteraction)
			{
				ThreatInteraction interact = ((ThreatInteraction)interactionsUC.dataListView.Items[idx]).Clone();
				ThreatInteractionWindow bw = new ThreatInteractionWindow(scenario, (ThreatInteraction)interact, true);
				if (bw.ShowDialog() == true) { scenario.interactionObserver.Add(interact); }
			}
			else if (interactionsUC.dataListView.SelectedItem is MultiEventInteraction)
			{
				MultiEventInteraction interact = ((MultiEventInteraction)interactionsUC.dataListView.Items[idx]).Clone();
				MultiEventWindow bw = new MultiEventWindow(scenario, (MultiEventInteraction)interact, true);
				if (bw.ShowDialog() == true) { scenario.interactionObserver.Add(interact); }
			}
			else if (interactionsUC.dataListView.SelectedItem is PersistentTokenInteraction)
			{
				PersistentTokenInteraction interact = ((PersistentTokenInteraction)interactionsUC.dataListView.Items[idx]).Clone();
				PersistentInteractionWindow bw = new PersistentInteractionWindow(scenario, (PersistentTokenInteraction)interact, true);
				if (bw.ShowDialog() == true) { scenario.interactionObserver.Add(interact); }
			}
			else if (interactionsUC.dataListView.SelectedItem is ConditionalInteraction)
			{
				ConditionalInteraction interact = ((ConditionalInteraction)interactionsUC.dataListView.Items[idx]).Clone();
				ConditionalInteractionWindow bw = new ConditionalInteractionWindow(scenario, (ConditionalInteraction)interact, true);
				if (bw.ShowDialog() == true) { scenario.interactionObserver.Add(interact); }
			}
			else if (interactionsUC.dataListView.SelectedItem is DialogInteraction)
			{
				DialogInteraction interact = ((DialogInteraction)interactionsUC.dataListView.Items[idx]).Clone();
				DialogInteractionWindow bw = new DialogInteractionWindow(scenario, (DialogInteraction)interact, true);
				if (bw.ShowDialog() == true) { scenario.interactionObserver.Add(interact); }
			}
			else if (interactionsUC.dataListView.SelectedItem is ReplaceTokenInteraction)
			{
				ReplaceTokenInteraction interact = ((ReplaceTokenInteraction)interactionsUC.dataListView.Items[idx]).Clone();
				ReplaceTokenInteractionWindow bw = new ReplaceTokenInteractionWindow(scenario, (ReplaceTokenInteraction)interact, true);
				if (bw.ShowDialog() == true) { scenario.interactionObserver.Add(interact); }
			}
			else if (interactionsUC.dataListView.SelectedItem is RewardInteraction)
			{
				RewardInteraction interact = ((RewardInteraction)interactionsUC.dataListView.Items[idx]).Clone();
				RewardInteractionWindow bw = new RewardInteractionWindow(scenario, (RewardInteraction)interact, true);
				if (bw.ShowDialog() == true) { scenario.interactionObserver.Add(interact); }
			}
			else if (interactionsUC.dataListView.SelectedItem is CorruptionInteraction)
			{
				CorruptionInteraction interact = ((CorruptionInteraction)interactionsUC.dataListView.Items[idx]).Clone();
				CorruptionInteractionWindow bw = new CorruptionInteractionWindow(scenario, interact, true);
				if (bw.ShowDialog() == true) { scenario.interactionObserver.Add(interact); }
			}
			else if (interactionsUC.dataListView.SelectedItem is ItemInteraction)
			{
				ItemInteraction interact = ((ItemInteraction)interactionsUC.dataListView.Items[idx]).Clone();
				ItemInteractionWindow bw = new ItemInteractionWindow(scenario, interact, true);
				if (bw.ShowDialog() == true) { scenario.interactionObserver.Add(interact); }
			}
			else if (interactionsUC.dataListView.SelectedItem is TitleInteraction)
			{
				TitleInteraction interact = ((TitleInteraction)interactionsUC.dataListView.Items[idx]).Clone();
				TitleInteractionWindow bw = new TitleInteractionWindow(scenario, interact, true);
				if (bw.ShowDialog() == true) { scenario.interactionObserver.Add(interact); }
			}
			else if (interactionsUC.dataListView.SelectedItem is StartInteraction)
			{
				MessageBox.Show("You can't duplicate the Starting Position interaction.", "Cannot Duplicate Starting Position Event", MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}

		void HandleHandleInteractionImport(ImportCreationRegistry icr, Dictionary<string, string> dataNameMap, InteractionBase interaction, List<string> triggers, List<string> events, ObservableCollection<Monster> monsters = null)
        {
			//Create a new GUID so we can't keep importing the same item and getting the same GUID.
			//Also needed for detecting duplicate items in the Interaction editor windows.
			interaction.GUID = Guid.NewGuid();

			//Make sure the dataName we're importing is unique. Add (2), (3), etc. at the end if necessary.
			int copyIndex = 2;
			string baseDataName = interaction.dataName;
			while(dataNameMap.ContainsKey(interaction.dataName))
            {
				interaction.dataName = baseDataName + "(" + copyIndex + ")";
				copyIndex++;
            }

			//If the referenced trigger doesn't exist, create it
			foreach(string trigger in triggers)
            {
				if (trigger == null || trigger == "" || trigger == "None") { }
				else if (scenario.triggersObserver.Any(it => it.dataName == trigger)) { }
				else
				{
					Trigger t = new Trigger(trigger);
					scenario.triggersObserver.Add(t);
					icr.triggers.Add(t);
				}
            }

			//If the referenced event doesn't exist, create it as a basic TestInteraction
			foreach(string anEvent in events)
            {
				if (anEvent == null || anEvent == "" || anEvent == "None") { }
				else if (scenario.interactionObserver.Any(it => it.dataName == anEvent)) { }
				else
                {
					TextInteraction textInteraction = new TextInteraction(anEvent);
					scenario.interactionObserver.Add(textInteraction);
					icr.interactions.Add(textInteraction);
                }
            }

			//Remove MonsterModifiers and MonsterActivations that are set on the Monster but don't exist in this scenario
			if(monsters != null)
            {
				foreach(Monster monster in monsters)
                {
					Debug.Log("monster " + monster.dataName + " has " + monster.modifierList.Count() + " modifiers and " + monster.activationsId + " as activationId");
					//Remove MonsterModifiers that don't exist in this scenario
					List<MonsterModifier> removeList = new List<MonsterModifier>();
					foreach(MonsterModifier modifier in monster.modifierList)
                    {
						if(!scenario.monsterModifierObserver.Any(it => (it.dataName == modifier.dataName) || (it.id == modifier.id)))
                        {
							Debug.Log("modifier " + modifier.id + " " + modifier.dataName + " doesn't match");
							removeList.Add(modifier);
                        }
						else
                        {
							//Ids might not match up, so find matching dataName and then reassign the id to the monster
							modifier.id = scenario.monsterModifierObserver.First(it => (it.dataName == modifier.dataName) || (it.id == modifier.id)).id;
							Debug.Log("modifier " + modifier.id + " " + modifier.dataName + " does match");
                        }
                    }
					foreach(MonsterModifier modifier in removeList)
                    {
						monster.modifierList.Remove(modifier);
                    }
					Debug.Log("monster modifiers after removals has " + monster.modifierList.Count() + " items");

					//Remove MonsterActivations that don't exist in this scenario
					if(!scenario.activationsObserver.Any(it => it.id == monster.activationsId))
                    {
						Debug.Log("activationId " + monster.activationsId + " doesn't exist so setting it to " + monster.id);
						monster.activationsId = monster.id;
                    }
                }

           }
		}

		public class ImportCreationRegistry
        {
			public List<Trigger> triggers = new List<Trigger>();
			public List<InteractionBase> interactions = new List<InteractionBase>();
        }

		void OnImportInteraction(object sender, EventArgs e)
        {
			var interactionItem = new ImportManager().ImportEvent(scenario);
			if(interactionItem == null) { return; }
			bool? dialogSuccess = null;

			Dictionary<string, string> dataNameMap = new Dictionary<string, string>();
			foreach(var interaction in scenario.interactionObserver)
            {
				dataNameMap.Add(interaction.dataName, interaction.dataName);
            }

			ImportCreationRegistry icr = new ImportCreationRegistry();
			if (interactionItem is TextInteraction)
			{
				TextInteraction castInteraction = (TextInteraction)interactionItem;
				HandleHandleInteractionImport(icr,  dataNameMap, castInteraction, castInteraction.CollectTriggers(), castInteraction.CollectEvents());
				TextInteractionWindow w = new TextInteractionWindow(scenario, castInteraction);
				dialogSuccess = castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is BranchInteraction)
			{
				BranchInteraction castInteraction = (BranchInteraction)interactionItem;
				HandleHandleInteractionImport(icr,  dataNameMap, castInteraction, castInteraction.CollectTriggers(), castInteraction.CollectEvents());
				BranchInteractionWindow w = new BranchInteractionWindow(scenario, castInteraction);
				dialogSuccess = castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is TestInteraction)
			{
				TestInteraction castInteraction = (TestInteraction)interactionItem;
				HandleHandleInteractionImport(icr,  dataNameMap, castInteraction, castInteraction.CollectTriggers(), castInteraction.CollectEvents());
				TestInteractionWindow w = new TestInteractionWindow(scenario, castInteraction);
				dialogSuccess = castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is DecisionInteraction)
			{
				DecisionInteraction castInteraction = (DecisionInteraction)interactionItem;
				HandleHandleInteractionImport(icr,  dataNameMap, castInteraction, castInteraction.CollectTriggers(), castInteraction.CollectEvents());
				DecisionInteractionWindow w = new DecisionInteractionWindow(scenario, castInteraction);
				dialogSuccess = castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is ThreatInteraction)
			{
				ThreatInteraction castInteraction = (ThreatInteraction)interactionItem;
				HandleHandleInteractionImport(icr,  dataNameMap, castInteraction, castInteraction.CollectTriggers(), castInteraction.CollectEvents(), castInteraction.monsterCollection);
				ThreatInteractionWindow w = new ThreatInteractionWindow(scenario, castInteraction);
				dialogSuccess = castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is MultiEventInteraction)
			{
				MultiEventInteraction castInteraction = (MultiEventInteraction)interactionItem;
				HandleHandleInteractionImport(icr,  dataNameMap, castInteraction, castInteraction.CollectTriggers(), castInteraction.CollectEvents());
				MultiEventWindow w = new MultiEventWindow(scenario, castInteraction);
				dialogSuccess = castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is PersistentTokenInteraction)
			{
				PersistentTokenInteraction castInteraction = (PersistentTokenInteraction)interactionItem;
				HandleHandleInteractionImport(icr,  dataNameMap, castInteraction, castInteraction.CollectTriggers(), castInteraction.CollectEvents());
				PersistentInteractionWindow w = new PersistentInteractionWindow(scenario, castInteraction);
				dialogSuccess = castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is ConditionalInteraction)
			{
				ConditionalInteraction castInteraction = (ConditionalInteraction)interactionItem;
				HandleHandleInteractionImport(icr,  dataNameMap, castInteraction, castInteraction.CollectTriggers(), castInteraction.CollectEvents());
				ConditionalInteractionWindow w = new ConditionalInteractionWindow(scenario, castInteraction);
				dialogSuccess = castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is DialogInteraction)
			{
				DialogInteraction castInteraction = (DialogInteraction)interactionItem;
				HandleHandleInteractionImport(icr,  dataNameMap, castInteraction, castInteraction.CollectTriggers(), castInteraction.CollectEvents());
				DialogInteractionWindow w = new DialogInteractionWindow(scenario, castInteraction);
				dialogSuccess = castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is ReplaceTokenInteraction)
			{
				ReplaceTokenInteraction castInteraction = (ReplaceTokenInteraction)interactionItem;
				HandleHandleInteractionImport(icr,  dataNameMap, castInteraction, castInteraction.CollectTriggers(), castInteraction.CollectEvents());
				ReplaceTokenInteractionWindow w = new ReplaceTokenInteractionWindow(scenario, castInteraction);
				//w.ShowDialog();
				castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is RewardInteraction)
			{
				RewardInteraction castInteraction = (RewardInteraction)interactionItem;
				HandleHandleInteractionImport(icr,  dataNameMap, castInteraction, castInteraction.CollectTriggers(), castInteraction.CollectEvents());
				RewardInteractionWindow w = new RewardInteractionWindow(scenario, castInteraction);
				dialogSuccess = castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is CorruptionInteraction)
			{
				CorruptionInteraction castInteraction = (CorruptionInteraction)interactionItem;
				HandleHandleInteractionImport(icr,  dataNameMap, castInteraction, castInteraction.CollectTriggers(), castInteraction.CollectEvents());
				CorruptionInteractionWindow w = new CorruptionInteractionWindow(scenario, castInteraction);
				dialogSuccess = castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is ItemInteraction)
			{
				ItemInteraction castInteraction = (ItemInteraction)interactionItem;
				HandleHandleInteractionImport(icr,  dataNameMap, castInteraction, castInteraction.CollectTriggers(), castInteraction.CollectEvents());
				ItemInteractionWindow w = new ItemInteractionWindow(scenario, castInteraction);
				dialogSuccess = castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is TitleInteraction)
			{
				TitleInteraction castInteraction = (TitleInteraction)interactionItem;
				HandleHandleInteractionImport(icr,  dataNameMap, castInteraction, castInteraction.CollectTriggers(), castInteraction.CollectEvents());
				TitleInteractionWindow w = new TitleInteractionWindow(scenario, castInteraction);
				dialogSuccess = castInteraction.HandleWindow(w, scenario.translationObserver);
			}
			else if (interactionItem is StartInteraction)
			{
				MessageBox.Show("You can't import a Starting Position event.", "Cannot Edit Starting Position Event", MessageBoxButton.OK, MessageBoxImage.Information);
				dialogSuccess = false;
			}

			if(dialogSuccess == true)
            {
				//TODO Check that dataName doesn't already exist, if so, modify it
				scenario.interactionObserver.Add(interactionItem);
            }
			else
            {
				//Remove any triggers or interactions that were created during the import
				foreach(Trigger trigger in icr.triggers)
                {
					scenario.triggersObserver.Remove(trigger);
                }
				foreach(InteractionBase interaction in icr.interactions)
                {
					scenario.interactionObserver.Remove(interaction);
                }
            }
		}


		void OnExportInteraction(object sender, EventArgs e)
        {
			bool exported = new ExportManager().ExportEvent(scenario, (InteractionBase)interactionsUC.dataListView.SelectedItem);
		}


		// Trigger
		void OnAddTrigger( object sender, EventArgs e )
		{
			AddTrigger();
		}

		void OnRemoveTrigger( object sender, EventArgs e )
		{
			string selected = ( (Trigger)triggersUC.dataListView.SelectedItem ).dataName;
			var strigger = (Trigger)triggersUC.dataListView.SelectedItem;

			Tuple<string, string> used = scenario.IsTriggerUsed( selected );

			if ( used != null )
			{
				MessageBox.Show( $"The selected Trigger [{selected}] is being used by [{used.Item2}] called [{used.Item1}].", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
				return;
			}

			if ( strigger.isCampaignTrigger )
			{
				MessageBox.Show( "Campaign Triggers cannot be removed.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
				return;
			}

			var ret = MessageBox.Show("Are you sure you want to delete this Trigger?", "Delete Trigger", MessageBoxButton.YesNo, MessageBoxImage.Question);
			if (ret == MessageBoxResult.Yes)
			{
				int idx = triggersUC.dataListView.SelectedIndex;
				if (idx != -1)
					scenario.RemoveData(triggersUC.dataListView.SelectedItem);
				triggersUC.dataListView.SelectedIndex = 0;
            }
		}

		void OnDuplicateTrigger(object sender, EventArgs e)
		{
			int idx = triggersUC.dataListView.SelectedIndex;
			Trigger trig = ((Trigger)triggersUC.dataListView.Items[idx]).Clone();

			TriggerEditorWindow tew = new TriggerEditorWindow(scenario, trig, true);
			if (tew.ShowDialog() == true)
			{
				//scenario.triggersObserver.Add(trig);
				//The TriggerEditorWindow's OK button actually handles adding the Trigger to the scenario.triggersObserver.
			}
		}

		void OnSettingsTrigger(object sender, EventArgs e)
		{
			OpenTriggerEditor((Trigger)triggersUC.dataListView.SelectedItem);
		}

		private void OpenTriggerEditor(Trigger trigger)
		{
			TriggerEditorWindow tw = new TriggerEditorWindow(scenario, trigger.dataName);
			tw.ShowDialog();
		}


		// Objective
		void OnAddObjective( object sender, EventArgs e )
		{
			AddObjective();
		}

		void OnRemoveObjective( object sender, EventArgs e )
		{
			if (scenario.objectiveObserver.Count > 1)
			{
				var ret = MessageBox.Show("Are you sure you want to delete this Objective?\n\nALL ITS INFORMATION WILL BE DELETED.", "Delete Objective", MessageBoxButton.YesNo, MessageBoxImage.Question);
				if (ret == MessageBoxResult.Yes)
				{
					int idx = objectivesUC.dataListView.SelectedIndex;
					if (idx != -1)
						scenario.RemoveData(objectivesUC.dataListView.Items[idx]);
					objectivesUC.dataListView.SelectedIndex = 0;
                }
			}
			else
			{
				MessageBox.Show("There must be at least one Objective.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		void OnDuplicateObjective(object sender, EventArgs e)
		{
			int idx = objectivesUC.dataListView.SelectedIndex;
			Objective obj = ((Objective)objectivesUC.dataListView.Items[idx]).Clone();

			ObjectiveEditorWindow oew = new ObjectiveEditorWindow(scenario, obj, true);
			if (oew.ShowDialog() == true)
			{
				scenario.objectiveObserver.Add(obj);
            }
		}

		void OnSettingsObjective( object sender, EventArgs e )
        {
            OpenObjectiveEditor((Objective)objectivesUC.dataListView.SelectedItem);
        }

        private void OpenObjectiveEditor(Objective o)
		{
			ObjectiveEditorWindow ow = new ObjectiveEditorWindow( scenario, o, false );
			o.HandleWindow(ow, scenario.translationObserver);
		}


		// Monster Activations
		void OnAddActivations(object sender, EventArgs e)
		{
			AddActivations();
		}

		void OnRemoveActivations(object sender, EventArgs e)
		{
			int idx = activationsUC.dataListView.SelectedIndex;
			MonsterActivations act = (MonsterActivations)activationsUC.dataListView.Items[idx];
			if (idx != -1 && act.id >= MonsterActivations.START_OF_CUSTOM_ACTIVATIONS) //Don't allow removing the basic enemy activations. Only allow removing built-in custom activations and user custom activations.
			{
				var ret = MessageBox.Show("Are you sure you want to delete this Enemy Attack Group?\n\nALL DESCRIPTIONS AND DAMAGE WILL BE DELETED.", "Delete Enemy Attack Group", MessageBoxButton.YesNo, MessageBoxImage.Question);
				if (ret == MessageBoxResult.Yes)
				{
					scenario.RemoveData(activationsUC.dataListView.Items[idx]);
					activationsUC.dataListView.SelectedIndex = Math.Max(idx - 1, 0);
				}
			}
			else
            {
				MessageBox.Show("You can't delete the default Enemy Attack Groups, but you can copy them and modify the copy.", "Cannot Delete Enemy Attack Group", MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}

		void OnSettingsActivations(object sender, EventArgs e)
		{
			int idx = activationsUC.dataListView.SelectedIndex;
			MonsterActivations act = (MonsterActivations)activationsUC.dataListView.Items[idx];
			if (idx != -1 && act.id >= MonsterActivations.START_OF_CUSTOM_ACTIVATIONS) //Don't allow modifying the basic enemy activations. Only allow modifying built-in custom activations and user custom activations.
			{
				ActivationsEditorWindow ow = new ActivationsEditorWindow(scenario, ((MonsterActivations)activationsUC.dataListView.SelectedItem), false);
				ow.ShowDialog();
			}
			else
			{
				MessageBox.Show("You can't modify the default Enemy Attack Groups, but you can copy them and modify the copy.", "Cannot Modify Enemy Attack Group", MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}

		void OnDuplicateActivations(object sender, EventArgs e)
		{
			//Get next id starting at 1000 to create the new item
			int maxId = scenario.activationsObserver.Max(a => a.id);
			int newId = Math.Max(maxId+1, MonsterActivations.START_OF_CUSTOM_ACTIVATIONS); //Get the next id over 1000
			//Get the selected item and clone it
			int idx = activationsUC.dataListView.SelectedIndex;
			MonsterActivations act = ((MonsterActivations)activationsUC.dataListView.Items[idx]).Clone(newId);

			ActivationsEditorWindow aew = new ActivationsEditorWindow(scenario, act, true);
			if(aew.ShowDialog() == true)
            {
				scenario.activationsObserver.Add(act);
			}
		}

		void OnImportActivations(object sender, EventArgs e)
		{

		}

		void OnExportActivations(object sender, EventArgs e)
		{
			bool exported = new ExportManager().ExportMonsterActivation(scenario, (MonsterActivations)activationsUC.dataListView.SelectedItem);
		}


		// Monster Modifier
		void OnAddMonsterModifier(object sender, EventArgs e)
		{
			AddMonsterModifier();
		}

		void OnRemoveMonsterModifier(object sender, EventArgs e)
		{
			int idx = monsterModifiersUC.dataListView.SelectedIndex;
			MonsterModifier mod = (MonsterModifier)monsterModifiersUC.dataListView.Items[idx];
			if (idx != -1 && mod.id >= MonsterModifier.START_OF_CUSTOM_MODIFIERS) //Don't allow removing the built-in enemy bonuses. Only allow removing custom enemy bonuses
			{
				var ret = MessageBox.Show("Are you sure you want to delete this Enemy Bonus?", "Delete Enemy Bonus", MessageBoxButton.YesNo, MessageBoxImage.Question);
				if (ret == MessageBoxResult.Yes)
				{
					scenario.RemoveData(monsterModifiersUC.dataListView.Items[idx]);
					monsterModifiersUC.dataListView.SelectedIndex = Math.Max(idx - 1, 0);
				}
			}
			else
			{
				MessageBox.Show("You can't delete the default Enemy Bonuses, but you can copy them and modify the copy.", "Cannot Delete Enemy Bonus", MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}

		void OnSettingsMonsterModifier(object sender, EventArgs e)
		{
			int idx = monsterModifiersUC.dataListView.SelectedIndex;
			MonsterModifier mod = (MonsterModifier)monsterModifiersUC.dataListView.Items[idx];
			if (idx != -1 && mod.id >= MonsterModifier.START_OF_CUSTOM_MODIFIERS) //Don't allow modifying the built-in enemy bonuses. Only allow modifying custom enemy bonuses.
			{
				MonsterModifierEditorWindow ow = new MonsterModifierEditorWindow(scenario, ((MonsterModifier)monsterModifiersUC.dataListView.SelectedItem), false);
				ow.ShowDialog();
			}
			else
			{
				MessageBox.Show("You can't modify the default Enemy Bonuses, but you can copy them and modify the copy.", "Cannot Modify Enemy Bonus", MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}

		void OnDuplicateMonsterModifier(object sender, EventArgs e)
		{
			//Get next id starting at 1000 to create the new item
			int maxId = scenario.monsterModifierObserver.Max(a => a.id);
			int newId = Math.Max(maxId + 1, MonsterModifier.START_OF_CUSTOM_MODIFIERS); //Get the next id over 1000
			//Get the selected item and clone it
			int idx = monsterModifiersUC.dataListView.SelectedIndex;
			MonsterModifier mod = ((MonsterModifier)monsterModifiersUC.dataListView.Items[idx]).Clone(newId);

			MonsterModifierEditorWindow mmew = new MonsterModifierEditorWindow(scenario, mod, true);
			if (mmew.ShowDialog() == true)
			{
				scenario.monsterModifierObserver.Add(mod);
			}
		}

		void OnImportMonsterModifier(object sender, EventArgs e)
		{
			//TODO
		}

		void OnExportMonsterModifier(object sender, EventArgs e)
		{
			bool exported = new ExportManager().ExportMonsterModifier(scenario, (MonsterModifier)monsterModifiersUC.dataListView.SelectedItem);
		}

		// Translations
		void OnAddTranslation(object sender, EventArgs e)
		{
			AddTranslation();
		}

		void OnRemoveTranslation(object sender, EventArgs e)
		{
			int idx = translationsUC.dataListView.SelectedIndex;
			Translation translation = (Translation)translationsUC.dataListView.Items[idx];
			var ret = MessageBox.Show("Are you sure you want to delete this Translation?\n\n" + translation.langName + " / " + translation.dataName + "\n\nALL TRANSLATION TEXT FOR THIS LANGUAGE WILL BE DELETED.\n\nThis cannot be undone.", "Delete Translation: " + translation.langName + " / " + translation.dataName, MessageBoxButton.YesNo, MessageBoxImage.Question);
			if (ret == MessageBoxResult.Yes)
			{
				var ret2 = MessageBox.Show("Please Reconfirm:\n\nAre you REALLY, REALLY sure you want to delete this Translation?\n\n" + translation.langName + " / " + translation.dataName + "\n\nALL TRANSLATION TEXT FOR THIS LANGUAGE WILL BE DELETED.\n\nThis cannot be undone.", "Delete Translation: " + translation.langName + " / " + translation.dataName, MessageBoxButton.YesNo, MessageBoxImage.Question);
				if (ret2 == MessageBoxResult.Yes)
				{
					scenario.RemoveData(translationsUC.dataListView.Items[idx]);
					translationsUC.dataListView.SelectedIndex = Math.Max(idx - 1, 0);
				}
			}
		}

		void OnSettingsTranslation(object sender, EventArgs e)
		{
			Dictionary<string, TranslationItem> defaultTranslation = scenario.CollectTranslationItemsAsDictionary();

			TranslationEditorWindow tw = new TranslationEditorWindow(scenario, defaultTranslation, ((Translation)translationsUC.dataListView.SelectedItem), false);
			tw.ShowDialog();
		}

		void OnImportTranslation(object sender, EventArgs e)
		{
			new ImportManager().ImportTranslation(scenario.translationObserver);
		}

		void OnExportTranslation(object sender, EventArgs e)
		{
			Translation selectedTranslation = ((Translation)translationsUC.dataListView.SelectedItem);

			List<TranslationItemForExport> translationItemsForExport = scenario.CollectTranslationItemsForExport(selectedTranslation);

			TranslationForExport translationForExport = new TranslationForExport(selectedTranslation.dataName, selectedTranslation.langName, translationItemsForExport);

			bool exported = new ExportManager().ExportTranslation(scenario.scenarioName, translationForExport);
		}
		#endregion

		private void scenarioName_PreviewMouseDown( object sender, System.Windows.Input.MouseButtonEventArgs e )
		{
			if ( e.ButtonState == System.Windows.Input.MouseButtonState.Pressed )
			{
				if ( scenarioName.Visibility == Visibility.Visible )
				{
					scenarioName.Visibility = Visibility.Collapsed;
					scenarioNameEdit.Visibility = Visibility.Visible;
					scenarioNameEdit.Text = scenarioName.Text;
					scenarioNameEdit.Focus();
					scenarioNameEdit.SelectAll();
				}
				else
				{
					scenarioName.Visibility = Visibility.Visible;
					scenarioNameEdit.Visibility = Visibility.Collapsed;
				}
			}
		}

		private void ScenarioNameEdit_PreviewKeyDown( object sender, System.Windows.Input.KeyEventArgs e )
		{
			if ( e.Key == System.Windows.Input.Key.Enter )
			{
				onScenarioNameFocusLost();
			}
		}

		private void scenarioNameEdit_LostFocus( object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e )
		{
			onScenarioNameFocusLost();
		}

		void onScenarioNameFocusLost()
		{
			if ( scenarioNameEdit.Text.Trim() != string.Empty )
			{
				scenario.scenarioName = scenarioNameEdit.Text;
			}
			else
				MessageBox.Show( "The Scenario name cannot be an empty string.", "Invalid Scenario Name", MessageBoxButton.OK, MessageBoxImage.Information );
			scenarioName.Visibility = Visibility.Visible;
			scenarioNameEdit.Visibility = Visibility.Collapsed;
		}

		private void ScenarioSettingsButton_Click( object sender, RoutedEventArgs e )
        {
            OpenScenarioEditor();
        }

        private void OpenScenarioEditor()
		{
			Dictionary<string, string> originalValues = scenario.CaptureStartingValues();
			string originalKeyName = scenario.TranslationKeyName();
			ScenarioWindow sw = new ScenarioWindow( scenario );
			sw.Owner = this;
			if ( sw.ShowDialog() == true )
			{
				scenario.scenarioName = sw.scenarioName;
			}
			scenario.DecertifyChangedValues(scenario.translationObserver, originalValues, originalKeyName);
		}   

        private void Window_Closing( object sender, System.ComponentModel.CancelEventArgs e )
		{
			if ( scenario.isDirty )
			{
				if ( MessageBox.Show( "The Project has changes that haven't been saved.  Are you sure you want to exit without saving?", "Project Changes Not Saved", MessageBoxButton.YesNo, MessageBoxImage.Question ) == MessageBoxResult.No )
					e.Cancel = true;
			}
		}

		#region COMMANDS
		void AddObjective()
		{
			ObjectiveEditorWindow ow = new ObjectiveEditorWindow( scenario, new Objective( "Default Short Name - Change This" ) );
			if ( ow.ShowDialog() == true )
			{
				scenario.AddObjective( ow.objective );
            }
		}

		void AddActivations()
		{
			//Console.WriteLine("Add Enemy Activations...");
			//Get next id starting at 1000 to create the new item
			int maxId = scenario.activationsObserver.Max(a => a.id);
			int newId = Math.Max(maxId + 1, MonsterActivations.START_OF_CUSTOM_ACTIVATIONS); //Get the next id over 1000
			ActivationsEditorWindow aew = new ActivationsEditorWindow(scenario, new MonsterActivations(newId), false);
			if (aew.ShowDialog() == true)
			{
				scenario.AddActivations(aew.activations);
			}
		}

		void AddMonsterModifier()
		{
			//Get next id starting at 1000 to create the new item
			int maxId = scenario.monsterModifierObserver.Max(a => a.id);
			int newId = Math.Max(maxId + 1, MonsterModifier.START_OF_CUSTOM_MODIFIERS); //Get the next id over 1000
			MonsterModifierEditorWindow mmew = new MonsterModifierEditorWindow(scenario, new MonsterModifier(newId), false);
			if (mmew.ShowDialog() == true)
			{
				scenario.AddMonsterModifier(mmew.modifier);
			}
		}

		void AddTranslation()
        {
			Dictionary<string, TranslationItem> defaultTranslation = scenario.CollectTranslationItemsAsDictionary();

			TranslationEditorWindow tw = new TranslationEditorWindow(scenario, defaultTranslation, new Translation());
			if(tw.ShowDialog() == true)
            {
				scenario.AddTranslation(tw.translation);
            }
        }


		void AddTrigger()
		{
			TriggerEditorWindow tw = new TriggerEditorWindow( scenario );
			tw.ShowDialog();
		}

		private void CommandExit_Executed( object sender, System.Windows.Input.ExecutedRoutedEventArgs e )
		{
			if ( scenario.isDirty )
			{
				if ( MessageBox.Show( "The Project has changes that haven't been saved.  Are you sure you want to exit without saving?", "Project Changes Not Saved", MessageBoxButton.YesNo, MessageBoxImage.Question ) == MessageBoxResult.No )
					return;
			}
			Application.Current.Shutdown();
		}

		private void CommandExit_CanExecute( object sender, System.Windows.Input.CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = true;
		}

		private void CommandNewObjective_Executed( object sender, System.Windows.Input.ExecutedRoutedEventArgs e )
		{
			AddObjective();
		}

		private void CommandNewObjective_CanExecute( object sender, System.Windows.Input.CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = true;
		}

		private void CommandNewTrigger_Executed( object sender, System.Windows.Input.ExecutedRoutedEventArgs e )
		{
			AddTrigger();
		}

		private void CommandNewTrigger_CanExecute( object sender, System.Windows.Input.CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = true;
		}

		private void CommandNewEvent_Executed( object sender, System.Windows.Input.ExecutedRoutedEventArgs e )
		{
			ContextMenu cm = this.FindResource( "cmButton" ) as ContextMenu;
			cm.PlacementTarget = sender as Window;
			cm.IsOpen = true;
		}

		private void CommandNewEvent_CanExecute( object sender, System.Windows.Input.CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = true;
		}

		private void CommandNewProject_Executed( object sender, System.Windows.Input.ExecutedRoutedEventArgs e )
		{
			if ( MessageBox.Show( "Are you sure you want to close this Project and start a new one?", "New Project", MessageBoxButton.YesNo, MessageBoxImage.Question ) == MessageBoxResult.Yes )
			{
				ProjectWindow mainWindow = new ProjectWindow();
				mainWindow.Show();
				Close();
			}
		}

		private void CommandNewProject_CanExecute( object sender, System.Windows.Input.CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = true;
		}

		private void CommandOpenProject_Executed( object sender, System.Windows.Input.ExecutedRoutedEventArgs e )
		{
			if ( MessageBox.Show( "Are you sure you want to close this Project and open a different one?", "Open Project", MessageBoxButton.YesNo, MessageBoxImage.Question ) == MessageBoxResult.Yes )
			{
				ProjectWindow mainWindow = new ProjectWindow();
				mainWindow.Show();
				Close();
			}
		}

		private void CommandOpenProject_CanExecute( object sender, System.Windows.Input.CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = true;
		}

		private void CommandSaveProject_Executed( object sender, System.Windows.Input.ExecutedRoutedEventArgs e )
		{
			FileManager fm = new FileManager( scenario );
			if ( fm.Save() )
			{
				scenario.fileName = fm.fileName;
				scenario.saveDate = fm.saveDate;
				scenario.TriggerTitleChange();
			}
		}

		private void CommandSaveProject_CanExecute( object sender, System.Windows.Input.CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = true;
		}

		private void CommandScenarioSettings_Executed( object sender, System.Windows.Input.ExecutedRoutedEventArgs e )
		{
			ScenarioWindow sw = new ScenarioWindow( scenario );
			sw.Owner = this;
			if ( sw.ShowDialog() == true )
			{
				scenario.scenarioName = sw.scenarioName;
			}
		}

		private void CommandScenarioSettings_CanExecute( object sender, System.Windows.Input.CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = true;
		}

		private void CommandSaveProjectAs_Executed( object sender, System.Windows.Input.ExecutedRoutedEventArgs e )
		{
			FileManager fm = new FileManager( scenario );
			if ( fm.SaveAs() )
			{
				scenario.fileName = fm.fileName;
				scenario.saveDate = fm.saveDate;
				scenario.TriggerTitleChange();
			}
		}

		private void CommandSaveProjectAs_CanExecute( object sender, System.Windows.Input.CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = true;
		}

		private void CommandNewChapter_Executed( object sender, System.Windows.Input.ExecutedRoutedEventArgs e )
		{
			ChapterPropertiesWindow cw = new ChapterPropertiesWindow( scenario );
			if ( cw.ShowDialog() == true )
			{
				scenario.AddChapter( cw.chapter );
			}
		}

		private void CommandNewChapter_CanExecute( object sender, System.Windows.Input.CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = true;
			//Only allow adding new tile blocks if we're on the journey map, no the battle map
			//e.CanExecute = scenario.scenarioTypeJourney;
		}

		//Interaction popup commands
		private void CommandNewTextInteraction_Executed( object sender, System.Windows.Input.ExecutedRoutedEventArgs e )
		{
			TextInteractionWindow ew = new TextInteractionWindow( scenario );
			if ( ew.ShowDialog() == true )
			{
				scenario.AddInteraction( ew.interaction );
			}
		}
		private void CommandNewTextInteraction_CanExecute( object sender, System.Windows.Input.CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = true;
		}
		private void CommandNewBranchInteraction_Executed( object sender, System.Windows.Input.ExecutedRoutedEventArgs e )
		{
			BranchInteractionWindow ew = new BranchInteractionWindow( scenario );
			if ( ew.ShowDialog() == true )
			{
				scenario.AddInteraction( ew.interaction );
			}
		}
		private void CommandNewBranchInteraction_CanExecute( object sender, System.Windows.Input.CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = true;
		}
		private void CommandNewThreatInteraction_Executed( object sender, System.Windows.Input.ExecutedRoutedEventArgs e )
		{
			ThreatInteractionWindow ew = new ThreatInteractionWindow( scenario );
			if ( ew.ShowDialog() == true )
			{
				scenario.AddInteraction( ew.interaction );
			}
		}
		private void CommandNewThreatInteraction_CanExecute( object sender, System.Windows.Input.CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = true;
		}
		private void CommandNewTestInteraction_Executed( object sender, System.Windows.Input.ExecutedRoutedEventArgs e )
		{
			TestInteractionWindow ew = new TestInteractionWindow( scenario );
			if ( ew.ShowDialog() == true )
			{
				scenario.AddInteraction( ew.interaction );
			}
		}
		private void CommandNewTestInteraction_CanExecute( object sender, System.Windows.Input.CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = true;
		}
		private void CommandNewDecisionInteraction_Executed( object sender, System.Windows.Input.ExecutedRoutedEventArgs e )
		{
			DecisionInteractionWindow ew = new DecisionInteractionWindow( scenario );
			if ( ew.ShowDialog() == true )
			{
				scenario.AddInteraction( ew.interaction );
			}
		}
		private void CommandNewDecisionInteraction_CanExecute( object sender, System.Windows.Input.CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = true;
		}
		private void CommandNewMultiInteraction_Executed( object sender, System.Windows.Input.ExecutedRoutedEventArgs e )
		{
			MultiEventWindow ew = new MultiEventWindow( scenario );
			if ( ew.ShowDialog() == true )
			{
				scenario.AddInteraction( ew.interaction );
			}
		}
		private void CommandNewMultiInteraction_CanExecute( object sender, System.Windows.Input.CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = true;
		}
		private void CommandNewPersistentInteraction_Executed( object sender, System.Windows.Input.ExecutedRoutedEventArgs e )
		{
			PersistentInteractionWindow ew = new PersistentInteractionWindow( scenario );
			if ( ew.ShowDialog() == true )
			{
				scenario.AddInteraction( ew.interaction );
			}
		}
		private void CommandNewPersistentInteraction_CanExecute( object sender, System.Windows.Input.CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = true;
		}
		private void CommandNewConditionalInteraction_Executed( object sender, System.Windows.Input.ExecutedRoutedEventArgs e )
		{
			ConditionalInteractionWindow ew = new ConditionalInteractionWindow( scenario );
			if ( ew.ShowDialog() == true )
			{
				scenario.AddInteraction( ew.interaction );
			}
		}
		private void CommandNewConditionalInteraction_CanExecute( object sender, System.Windows.Input.CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = true;
		}
		private void CommandNewDialogInteraction_Executed( object sender, System.Windows.Input.ExecutedRoutedEventArgs e )
		{
			DialogInteractionWindow ew = new DialogInteractionWindow( scenario );
			if ( ew.ShowDialog() == true )
			{
				scenario.AddInteraction( ew.interaction );
			}
		}
		private void CommandNewDialogInteraction_CanExecute( object sender, System.Windows.Input.CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = true;
		}
		private void CommandNewReplaceTokenInteraction_Executed( object sender, System.Windows.Input.ExecutedRoutedEventArgs e )
		{
			ReplaceTokenInteractionWindow ew = new ReplaceTokenInteractionWindow( scenario );
			if ( ew.ShowDialog() == true )
			{
				scenario.AddInteraction( ew.interaction );
			}
		}
		private void CommandNewReplaceTokenInteraction_CanExecute( object sender, System.Windows.Input.CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = true;
		}
		private void CommandNewRewardInteraction_Executed( object sender, System.Windows.Input.ExecutedRoutedEventArgs e )
		{
			RewardInteractionWindow ew = new RewardInteractionWindow( scenario );
			if ( ew.ShowDialog() == true )
			{
				scenario.AddInteraction( ew.interaction );
			}
		}
		private void CommandNewRewardInteraction_CanExecute( object sender, System.Windows.Input.CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = true;
		}

		private void CommandNewCorruptionInteraction_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{
			CorruptionInteractionWindow ew = new CorruptionInteractionWindow(scenario);
			if (ew.ShowDialog() == true)
			{
				scenario.AddInteraction(ew.interaction);
			}
		}
		private void CommandNewCorruptionInteraction_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void CommandNewItemInteraction_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{
			ItemInteractionWindow ew = new ItemInteractionWindow(scenario);
			if (ew.ShowDialog() == true)
			{
				scenario.AddInteraction(ew.interaction);
			}
		}
		private void CommandNewItemInteraction_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void CommandNewTitleInteraction_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{
			TitleInteractionWindow ew = new TitleInteractionWindow(scenario);
			if (ew.ShowDialog() == true)
			{
				scenario.AddInteraction(ew.interaction);
			}
		}
		private void CommandNewTitleInteraction_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}
		#endregion

		private void RemoveChapterButton_Click( object sender, RoutedEventArgs e )
		{
			var ret = MessageBox.Show("Are you sure you want to delete this Tile Block?\n\nALL TILES AND TOKENS WILL BE DELETED.", "Delete Tile Block", MessageBoxButton.YesNo, MessageBoxImage.Question);
			if (ret == MessageBoxResult.Yes)
			{
				Chapter c = ((Button)e.Source).DataContext as Chapter;
				foreach (var tile in c.tileObserver)
					scenario.globalTilePool.Add(tile.idNumber);
				TileSorter sorter = new TileSorter();
				List<int> foo = scenario.globalTilePool.ToList();
				foo.Sort(sorter);
				scenario.globalTilePool.Clear();
				foreach (int s in foo)
					scenario.globalTilePool.Add(s);
				scenario.RemoveData(c);
			}
		}

		private void ChapterPropsButton_Click( object sender, RoutedEventArgs e )
		{
			Chapter c = ( (Button)e.Source ).DataContext as Chapter;
            OpenChapterPropertiesEditor(c);
		}

        private void TileEditButton_Click(object sender, RoutedEventArgs e)
        {
            OpenChapterTileEditor(((Button)e.Source).DataContext as Chapter);
        }

        private void OpenTokenEditor(Token t)
        {
            // TODO: cannot rely on parent tile
            var tile = scenario.chapterObserver.SelectMany(ch => ch.tileObserver.OfType<BaseTile>()).FirstOrDefault(t2 => t2.idNumber == t.parentTile.idNumber && t2.tileSide == t.parentTile.tileSide);
            if (tile != null)
            {
                OpenTileEditor(tile);
            }
        }

        private void OpenTileEditor(BaseTile t, Token highlightToken = null)
        {
            var c = scenario.chapterObserver.FirstOrDefault(ch => ch.tileObserver.OfType<BaseTile>().Any(tile => tile.idNumber == t.idNumber && tile.tileSide == t.tileSide));
            if (c != null)
            {
                TokenEditorWindow tp = new TokenEditorWindow(t, scenario, fromRandom: true, highlightToken: highlightToken);
                tp.ShowDialog();
            }
        }

        private void OpenChapterPropertiesEditor(Chapter c)
        {
            ChapterPropertiesWindow cw = new ChapterPropertiesWindow(scenario, c);
            cw.ShowDialog();
        }

        private void OpenChapterTileEditor(Chapter c)
        {
			if (scenario.scenarioTypeJourney)
			{
				if (c.isRandomTiles)
				{
					TilePoolEditorWindow tp = new TilePoolEditorWindow(scenario, c);
					tp.ShowDialog();
				}
				else
				{
					TileEditorWindow tw = new TileEditorWindow(scenario, c);
					tw.ShowDialog();
				}
			}
			else
            {
				//BattleTileEditor bte = new BattleTileEditor(scenario, c);
				//bte.ShowDialog();
				TileEditorWindow tw = new TileEditorWindow(scenario, c);
				tw.ShowDialog();
			}
		}

        private void activationsUC_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void GraphVisibleCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (visualizationZoomCtrl != null)
            {
                visualizationZoomCtrl.Visibility = Visibility.Visible;
                TriggerDeferredVisualizeScenario();
            }
        }

        private void GraphVisibleCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (visualizationZoomCtrl != null)
            {
                visualizationZoomCtrl.Visibility = Visibility.Hidden;
            }
        }

        private void NewItemSelectedEvent(string dataName)
        {
            // Focus visualization if Autofocus is enabled (and if graph is even visible)
            if (graphAutofocusCheckbox?.IsChecked == true && graphVisibleCheckbox?.IsChecked == true)
            {
                var rect = visualizationGraphArea.FindGraphNodeRect(dataName);
                if (rect.HasValue)
                {
                    // The rectangle shows exactly the item --> we like to expand it a bit
                    int multiplier = 7;
                    var r = rect.Value;
                    var largerRect = new System.Windows.Rect(
                        x: r.X - (multiplier / 2) * r.Width,
                        y: r.Y - (multiplier / 2) * r.Height,
                        width: multiplier * r.Width,
                        height: multiplier * r.Height);

                    visualizationZoomCtrl.ZoomToContent(largerRect, usingContentCoordinates: true);
                }
                
            }
            
        }
    }
}
