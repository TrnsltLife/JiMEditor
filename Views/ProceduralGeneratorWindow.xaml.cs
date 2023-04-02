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
using System.IO;

namespace JiME.Views
{

	/// <summary>
	/// Interaction logic for ScenarioWindow.xaml
	/// </summary>
	public partial class ProceduralGeneratorWindow : Window
    {
        private static readonly string RANDOM_TEXT = "RANDOM";
        private static readonly string SettingsStorageFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Your Journey", "ProcParameters.json");

        public Scenario Scenario { get; set; }
        public SimpleGenerator Generator { get; set; }
        public SimpleGeneratorParameters GeneratorParameters { get; set; }

		public ProceduralGeneratorWindow()
		{
            InitializeComponent();
            Generator = new SimpleGenerator();
            Scenario = null; // By default, we don't have a scenario ready

            // Disable buttons since we don't have a scenario yet
            visualizeButton.IsEnabled = false;
            saveButton.IsEnabled = false;
            okButton.IsEnabled = false;

            LoadParameterValues();
		}

        private  void LoadParameterValues()
        {
            // Load parameters from file if exists, otherwise use defaults
            if (File.Exists(SettingsStorageFile))
            {
                // JSON File found, read from that
                var json = File.ReadAllText(SettingsStorageFile);
                GeneratorParameters = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType<SimpleGeneratorParameters>(json, Generator.GetDefaultParameters());
            }
            else
            {
                // If file not found, use defaults
                GeneratorParameters = Generator.GetDefaultParameters();
            }

            // Set as data context so most of the parameters are updated directly
            this.DataContext = GeneratorParameters;

            // Setup archetype
            var allArchetypes = Enum.GetValues(typeof(StoryArchetype.Type)).Cast<StoryArchetype.Type?>()
                .Select(x => x.ToString())
                .OrderBy(x => x)
                .ToList();
            allArchetypes.Insert(0, RANDOM_TEXT);
            archetypeCB.ItemsSource = allArchetypes;
            archetypeCB.SelectedIndex = allArchetypes.IndexOf(GeneratorParameters.StoryArchetype == null ? RANDOM_TEXT : GeneratorParameters.StoryArchetype.Value.ToString());

            // Setup templates
            var allTemplates = StoryTemplate.GetAllKnownTemplates()
                .OrderBy(x => x)
                .ToList();
            allTemplates.Insert(0, RANDOM_TEXT);
            templateCB.ItemsSource = allTemplates;
            templateCB.SelectedIndex = allTemplates.IndexOf(GeneratorParameters.StoryTemplate?.Length > 0 ? GeneratorParameters.StoryTemplate : RANDOM_TEXT);  
        }

        private void SaveParameterValues()
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(GeneratorParameters);
            File.WriteAllText(SettingsStorageFile, json);
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
            var fileManager = new FileManager(Scenario);
            fileManager.SaveAs();
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            // Update the values that are not updated automatically to Parameters
            GeneratorParameters.StoryArchetype = (string)archetypeCB.SelectedValue == RANDOM_TEXT ? null : (StoryArchetype.Type?)Enum.Parse(typeof(StoryArchetype.Type), (string)archetypeCB.SelectedValue);
            GeneratorParameters.StoryTemplate = (string)templateCB.SelectedValue == RANDOM_TEXT ? null : (string)templateCB.SelectedValue;

            // Save the used parameters on generation
            SaveParameterValues();

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
	}
}
