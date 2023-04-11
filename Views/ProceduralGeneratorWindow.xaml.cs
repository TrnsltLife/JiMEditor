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

        public bool AllowDirectSaving { get; private set; }

        public Scenario Scenario { get; set; }
        public ProceduralGenerator Generator { get; set; }
        public ProceduralGeneratorParameters GeneratorParameters { get; set; }

        public ProceduralGeneratorWindow() : this(allowDirectSaving: true)
        {
        }

        public ProceduralGeneratorWindow(bool allowDirectSaving)
        {
            AllowDirectSaving = allowDirectSaving;

            InitializeComponent();
            Generator = new ProceduralGenerator();
            Scenario = null; // By default, we don't have a scenario ready

            // Disable buttons since we don't have a scenario yet
            visualizeButton.IsEnabled = false;
            saveButton.IsEnabled = false;
            okButton.IsEnabled = false;

            // If direct saving is not allowed, the button is permanently hidden
            saveButton.Visibility = allowDirectSaving ? Visibility.Visible : Visibility.Collapsed;

            // Add instructions
            logListBox.ItemsSource = new List<ProceduralGeneratorContext.LogItem>()
            {
                new ProceduralGeneratorContext.LogItem() { Type = ProceduralGeneratorContext.LogType.Info, Message = "Click the Die to generate..." }
            };

            LoadParameterValues();
		}

        private  void LoadParameterValues()
        {
            // Load parameters from file if exists, otherwise use defaults
            if (File.Exists(SettingsStorageFile))
            {
                // JSON File found, read from that
                try
                {
                    var json = File.ReadAllText(SettingsStorageFile);
                    GeneratorParameters = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType<ProceduralGeneratorParameters>(json, Generator.GetDefaultParameters());
                }
                catch
                {
                    // Something failed in reading, use default value
                    GeneratorParameters = Generator.GetDefaultParameters();
                }
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
            UpdateTemplateSelections(GeneratorParameters.StoryTemplate);

            // Setup debug values
            debugDontFillStoriesCB.IsChecked = GeneratorParameters.DebugSkipStoryPointsFillIn;
            debugVerboseStoryElementCheck.IsChecked = GeneratorParameters.DebugVerboseStoryElementCheck;
        }

        private void UpdateTemplateSelections(string overrideTemplateSelection)
        {
            // Find out which templates are valid based on archetype selection
            StoryArchetype.Type? fixedArchetype = ((string)archetypeCB.SelectedItem == RANDOM_TEXT) ? null
                : (StoryArchetype.Type?)Enum.Parse(typeof(StoryArchetype.Type), (string)archetypeCB.SelectedItem);
            var validTemplates = StoryTemplate.GetAllKnownTemplates()
                .OrderBy(x => x)
                .Where(x => fixedArchetype == null || StoryTemplate.GetTemplate(x).SupportedArchetypes.ContainsKey(fixedArchetype.Value))
                .ToList();
            validTemplates.Insert(0, RANDOM_TEXT);

            // Set the selections
            var oldSelection = (string)templateCB.SelectedItem;
            templateCB.ItemsSource = validTemplates;
            if (overrideTemplateSelection?.Length > 0)
            {
                templateCB.SelectedIndex = validTemplates.IndexOf(GeneratorParameters.StoryTemplate?.Length > 0 ? GeneratorParameters.StoryTemplate : RANDOM_TEXT);
            }
            else if (validTemplates.Contains(oldSelection))
            {
                templateCB.SelectedIndex = validTemplates.IndexOf(oldSelection);
            }
            else
            {
                templateCB.SelectedIndex = 0;
            }            
        }

        private void ArchetypeCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Archetype changed, update possible values for Templates
            UpdateTemplateSelections(null);
        }

        private void SaveParameterValues()
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(GeneratorParameters);
            File.WriteAllText(SettingsStorageFile, json);
        }

        private void VisualizeButton_Click(object sender, RoutedEventArgs e)
        {
            var sw = new Views.GraphWindow(Scenario);
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

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            // Update the values that are not updated automatically to Parameters
            GeneratorParameters.StoryArchetype = (string)archetypeCB.SelectedValue == RANDOM_TEXT ? null : (StoryArchetype.Type?)Enum.Parse(typeof(StoryArchetype.Type), (string)archetypeCB.SelectedValue);
            GeneratorParameters.StoryTemplate = (string)templateCB.SelectedValue == RANDOM_TEXT ? null : (string)templateCB.SelectedValue;
            GeneratorParameters.DebugSkipStoryPointsFillIn = debugDontFillStoriesCB.IsChecked ?? false;
            GeneratorParameters.DebugVerboseStoryElementCheck = debugVerboseStoryElementCheck.IsChecked ?? false;

            // Save the used parameters on generation
            SaveParameterValues();

            // Generate the scenario
            ProceduralGeneratorContext generatorContext = Generator.GenerateScenario(GeneratorParameters);
            Scenario = generatorContext.Scenario;

            // Display logs
            if (generatorContext.Scenario != null)
            {
                if (AllowDirectSaving)
                {
                    generatorContext.LogInfo("Use buttons below to Visualize, Save or Accept the Scenario...");
                }
                else
                {
                    generatorContext.LogInfo("Use buttons below to Visualize or Accept the Scenario...");
                }
            }
            logListBox.ItemsSource = generatorContext.GeneratorLogs;
            logScrollViewer.ScrollToBottom();

            // Enable buttons that need the scenario
            visualizeButton.IsEnabled = Scenario != null;
            saveButton.IsEnabled = Scenario != null;
            okButton.IsEnabled = Scenario != null;
        }

        private void TextBlock_AcceptOnlyNonNegativeInteger(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            int integer;
            if (!int.TryParse(e.Text, out integer))
            {
                // Not an integer, not allowed
                e.Handled = true;
                return;
            }
            if (integer < 0)
            {
                e.Handled = true;
            }
        }
    }
}
