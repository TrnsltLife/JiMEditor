using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using JiME.Visualization;
using GraphX.Common.Enums;
using GraphX.Logic.Algorithms.LayoutAlgorithms;
using GraphX.Controls;

namespace JiME.Views
{
    /// <summary>
    /// Interaction logic for GraphWindow.xaml
    /// </summary>
    public partial class GraphWindow : Window
    {
        Scenario scenario;

        public GraphWindow()
        {
            InitializeComponent();
        }

        public GraphWindow(Scenario s) : this()
        {
            scenario = s;

            //Customize Zoombox a bit
            //Set minimap (overview) window to be visible by default
            ZoomControl.SetViewFinderVisibility(zoomctrl, Visibility.Visible);
            //Set Fill zooming strategy so whole graph will be always visible
            zoomctrl.ZoomToFill();
            Loaded += (object sender, RoutedEventArgs e) =>
            {
                VisualizeScenario(); // Show the scenario itself after the window has loaded
                zoomctrl.ZoomToFill(); // Set Fill zooming strategy so whole graph will be always visible (first time this is loading)
            };
        }

        private void VisualizeScenario()
        {
            if (scenario != null)
            {
                var dataGraph = Graph.Generate(scenario);
                graphArea.ShowGraph(dataGraph);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            graphArea.ExportAsPng();
        }
    }
}
