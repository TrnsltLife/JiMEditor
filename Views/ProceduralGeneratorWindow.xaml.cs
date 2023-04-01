using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using JiME.Models;
using JiME.Procedural.StoryElements;
using JiME.Procedural;
using System.Windows.Media;

namespace JiME.Views
{

	/// <summary>
	/// Interaction logic for ScenarioWindow.xaml
	/// </summary>
	public partial class ProceduralGeneratorWindow : Window
	{
		bool closing = false;

        public Scenario Scenario { get; set; }
        public SimpleGenerator Generator { get; set; }
        public SimpleGeneratorParameters GeneratorParameters { get; set; }

		public ProceduralGeneratorWindow()
		{


            InitializeComponent();
            Generator = new SimpleGenerator();
            Scenario = null; // By default, we don't have a scenario ready
			DataContext = this;

            // Disable buttons since we don't have a scenario yet
            visualizeButton.IsEnabled = false;
            saveButton.IsEnabled = false;
            okButton.IsEnabled = false;

            LoadValues();
		}

        private  void LoadValues()
        {
            // TODO: load latest from file
            GeneratorParameters = Generator.GetDefaultParameters();

            // If file not found, use defaults
            {
                // Setup archetype
                var allArchetypes = Enum.GetValues(typeof(StoryArchetype.Type)).Cast<StoryArchetype.Type?>().ToList();
                allArchetypes.Insert(0, null);
                archetypeCB.ItemsSource = allArchetypes;

                // Setup collections
                coreSetCB.IsChecked = true;
                villainsOfEriadorCB.IsChecked = GeneratorParameters.Has_VILLAINS_OF_ERIADOR;
                shadowedPathsCB.IsChecked = GeneratorParameters.Has_SHADOWED_PATHS;
                dwellersInDarknessCB.IsChecked = GeneratorParameters.Has_VILLAINS_OF_ERIADOR;
                spreadingWarCB.IsChecked = GeneratorParameters.Has_VILLAINS_OF_ERIADOR;
                scourgesOfTheWastesCB.IsChecked = GeneratorParameters.Has_VILLAINS_OF_ERIADOR;
            }
        }

        private void SaveValues()
        {
            // TODO: save current to file
        }

        private void OkButton_Click( object sender, RoutedEventArgs e )
		{
			if ( !closing && TryClose() )
				DialogResult = closing = true;
		}

        private void VisualizeButton_Click(object sender, RoutedEventArgs e)
        {
            var sw = new Visualization.Views.GraphWindow(Scenario);
            //sw.Owner = this; 
            sw.WindowState = WindowState.Maximized;
            sw.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            sw.Show();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO:
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO:  Make sure selected values are used / updated correctly

            // Generate the scenario
            var generatorContext = Generator.GenerateScenario(GeneratorParameters);
            Scenario = generatorContext.Scenario;

            // Display possible errors
            if (generatorContext.GeneratorWarnings.Count > 0)
            {
                // TODO: Show these nicely
                MessageBox.Show(string.Join(Environment.NewLine, generatorContext.GeneratorWarnings), "WARNINGS", MessageBoxButton.OK);
            }

            // Enable buttons that need the scenario
            visualizeButton.IsEnabled = true;
            saveButton.IsEnabled = true;
            okButton.IsEnabled = true;
        }


        bool TryClose()
        { 
			return true;
		}

		private void collection_Click( object sender, RoutedEventArgs e )
        {
            // TODO: update collections?

			CheckBox checkbox = (CheckBox)sender;
			string name = checkbox.Content as string;
			bool? check = checkbox.IsChecked;

			Collection collection = Collection.FromName(name);
			
			if(!check.GetValueOrDefault(false))
			{
				//TODO Do a warning that we're removing a Collection and for them to remove tiles and monsters that use it.
				//TODO List Tile Maps and Events that use the Collection we're disabling.
				//TODO Automatically remove Collection resources from Tile Maps and Events.
				Scenario.collectionObserver.Remove(collection);
			}
            else
			{
				Scenario.collectionObserver.Add(collection);
			}

			Scenario.RefilterGlobalTilePool();
		}
	}
}
