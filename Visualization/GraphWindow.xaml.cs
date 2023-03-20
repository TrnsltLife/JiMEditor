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
using JiME.Visualization.Models;
using GraphX.Common.Enums;
using GraphX.Logic.Algorithms.LayoutAlgorithms;
using GraphX.Controls;

namespace JiME.Visualization
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

            SetupGraph();

            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //lets create graph
            //Note that you can't create it in class constructor as there will be problems with visuals

            //Lets generate configured graph using pre-created data graph assigned to LogicCore object.
            //Optionaly we set first method param to True (True by default) so this method will automatically generate edges
            //  If you want to increase performance in cases where edges don't need to be drawn at first you can set it to False.
            //  You can also handle edge generation by calling manually Area.GenerateAllEdges() method.
            //Optionaly we set second param to True (True by default) so this method will automaticaly checks and assigns missing unique data ids
            //for edges and vertices in _dataGraph.
            //Note! Area.Graph property will be replaced by supplied _dataGraph object (if any).
            graphArea.GenerateGraph(true, true);

            /* 
             * After graph generation is finished you can apply some additional settings for newly created visual vertex and edge controls
             * (VertexControl and EdgeControl classes).
             * 
             */

            //This method sets the dash style for edges. It is applied to all edges in Area.EdgesList. You can also set dash property for
            //each edge individually using EdgeControl.DashStyle property.
            //For ex.: Area.EdgesList[0].DashStyle = GraphX.EdgeDashStyle.Dash;
            graphArea.SetEdgesDashStyle(EdgeDashStyle.Solid);

            //This method sets edges arrows visibility. It is also applied to all edges in Area.EdgesList. You can also set property for
            //each edge individually using property, for ex: Area.EdgesList[0].ShowArrows = true;
            graphArea.ShowAllEdgesArrows(true);

            //This method sets edges labels visibility. It is also applied to all edges in Area.EdgesList. You can also set property for
            //each edge individually using property, for ex: Area.EdgesList[0].ShowLabel = true;
            graphArea.ShowAllEdgesLabels(true);

            zoomctrl.ZoomToFill();
        }

        private string getTriggerName(string dataName) => "TRIG_" + dataName;
        private string getObjectiveName(string dataName) => "OBJ_" + dataName;
        private string getResolutionName(string dataName) => "RES_" + dataName;
        private string getInteractionName(string dataName) => "INT_" + dataName;

        private void SetupGraph()
        {
            if (scenario == null)
            {
                return;
            }

            // TODO: Graph setup should be done in the model side (at least vertex and edge creation parts)

            // SETUP GRAPH

            //Lets make new data graph instance
            var dataGraph = new Graph();
            
            // Prepare all vertices from the scenario
            var vertexDict = new Dictionary<string, DataVertex>();

            // Add specific START vertex to work as root node
            var startVertexId = "START";
            var startVertex = new DataVertex("START");
            vertexDict.Add(startVertexId, startVertex);
            dataGraph.AddVertex(startVertex);

            // TODO: Data vertex types should be handled and shown in different color / shapes etc.

            // Prepare triggers first as they don't have any edge definitions
            foreach (var trigger in scenario.triggersObserver)
            {
                // Vertex for the trigger
                var vertex = new DataVertex("T:" + trigger.dataName);
                vertexDict.Add(getTriggerName(trigger.dataName), vertex);
                dataGraph.AddVertex(vertex);
            }

            // Then prepare objectives and link to triggers
            foreach (var objective in scenario.objectiveObserver)
            {
                // Vertex for the objective
                var vertex = new DataVertex("O:" + objective.dataName);
                vertexDict.Add(getObjectiveName(objective.dataName), vertex);
                dataGraph.AddVertex(vertex);

                // Scenario default objective
                if (scenario.objectiveName == objective.dataName)
                {
                    dataGraph.AddEdge(new DataEdge(startVertex, vertex) { Text = "initiates" });
                }

                // Object is triggered by... (made available)
                if (objective.triggeredByName?.Length > 0 && objective.triggeredByName != "None")
                {
                    dataGraph.AddEdge(new DataEdge(vertexDict[getTriggerName(objective.triggeredByName)], vertex) { Text = "initiates" });
                }

                // Object is completed by...
                if (objective.triggerName?.Length > 0 && objective.triggerName != "None")
                {
                    dataGraph.AddEdge(new DataEdge(vertexDict[getTriggerName(objective.triggerName)], vertex) { Text = "completes" });
                }

                // Object causes further trigger if completed...
                if (objective.nextTrigger?.Length > 0 && objective.nextTrigger != "None")
                {
                    dataGraph.AddEdge(new DataEdge(vertex, vertexDict[getTriggerName(objective.nextTrigger)]) { Text = "on completion" });
                }
            }

            // Then prepare objectives and link to triggers
            foreach (var resolution in scenario.resolutionObserver)
            {
                // Vertex for the objective
                var vertex = new DataVertex("R:" + resolution.dataName);
                vertexDict.Add(getResolutionName(resolution.dataName), vertex);
                dataGraph.AddVertex(vertex);

                // Object is completed by...
                if (resolution.triggerName?.Length > 0 && resolution.triggerName != "None")
                {
                    dataGraph.AddEdge(new DataEdge(vertexDict[getTriggerName(resolution.triggerName)], vertex) { Text = "triggers" });
                }
            }

            // Then prepare interactions and link to triggers
            foreach (var interaction in scenario.interactionObserver)
            {
                // Vertex for the objective
                var vertex = new DataVertex("I:" + interaction.dataName);
                vertexDict.Add(getInteractionName(interaction.dataName), vertex);
                dataGraph.AddVertex(vertex);

                // Rest depends on interaction type
                switch(interaction.interactionType)
                {
                    case InteractionType.Decision:
                        {
                            var x = (DecisionInteraction)interaction;
                            if (x.triggerName?.Length > 0 && x.triggerName != "None")
                            {
                                dataGraph.AddEdge(new DataEdge(vertexDict[getTriggerName(x.triggerName)], vertex) { Text = "initiates" });
                            }
                            else
                            {
                                // Special case: No initiation trigger so triggered from the start?
                                // TODO: Or is it? Could this never be triggered?
                                dataGraph.AddEdge(new DataEdge(startVertex, vertex) { Text = "initiates" });
                            }
                            if (x.triggerAfterName?.Length > 0 && x.triggerAfterName != "None")
                            {
                                dataGraph.AddEdge(new DataEdge(vertex, vertexDict[getTriggerName(x.triggerAfterName)]) { Text = "triggers" });
                            }
                            if (x.choice1Trigger?.Length > 0 && x.choice1Trigger != "None")
                            {
                                dataGraph.AddEdge(new DataEdge(vertex, vertexDict[getTriggerName(x.choice1Trigger)]) { Text = "\"" + x.choice1 + "\"" });
                            }
                            if (x.choice2Trigger?.Length > 0 && x.choice2Trigger != "None")
                            {
                                dataGraph.AddEdge(new DataEdge(vertex, vertexDict[getTriggerName(x.choice2Trigger)]) { Text = "\"" + x.choice2 + "\"" });
                            }
                            if (x.choice3Trigger?.Length > 0 && x.choice3Trigger != "None")
                            {
                                dataGraph.AddEdge(new DataEdge(vertex, vertexDict[getTriggerName(x.choice3Trigger)]) { Text = "\"" + x.choice3 + "\"" });
                            }
                            // TODO: Other properties
                            break;
                        }

                    // TODO: other types
                    default:
                        throw new NotImplementedException("Graph preparation not implemented for interaction type: " + interaction.interactionType.ToString());
                }
            }


            // TODO: other stuff like MonsterActivations, Threats etc.

            // SETUP GRAPHAREA
            //Lets create logic core and filled data graph with edges and vertices
            var logicCore = new GraphLogic() { Graph = dataGraph };
            //This property sets layout algorithm that will be used to calculate vertices positions
            //Different algorithms uses different values and some of them uses edge Weight property.
            logicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.Tree;
            //Now we can set parameters for selected algorithm using AlgorithmFactory property. This property provides methods for
            //creating all available algorithms and algo parameters.
            logicCore.DefaultLayoutAlgorithmParams = logicCore.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.Tree);
            //Unfortunately to change algo parameters you need to specify params type which is different for every algorithm.
            //((KKLayoutParameters)logicCore.DefaultLayoutAlgorithmParams).MaxIterations = 100;
            ((SimpleTreeLayoutParameters)logicCore.DefaultLayoutAlgorithmParams).LayerGap = 100;
            ((SimpleTreeLayoutParameters)logicCore.DefaultLayoutAlgorithmParams).VertexGap = 100;

            //This property sets vertex overlap removal algorithm.
            //Such algorithms help to arrange vertices in the layout so no one overlaps each other.
            logicCore.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
            //Default parameters are created automaticaly when new default algorithm is set and previous params were NULL
            logicCore.DefaultOverlapRemovalAlgorithmParams.HorizontalGap = 50;
            logicCore.DefaultOverlapRemovalAlgorithmParams.VerticalGap = 50;

            //This property sets edge routing algorithm that is used to build route paths according to algorithm logic.
            //For ex., SimpleER algorithm will try to set edge paths around vertices so no edge will intersect any vertex.
            //Bundling algorithm will try to tie different edges that follows same direction to a single channel making complex graphs more appealing.
            logicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;

            //This property sets async algorithms computation so methods like: Area.RelayoutGraph() and Area.GenerateGraph()
            //will run async with the UI thread. Completion of the specified methods can be catched by corresponding events:
            //Area.RelayoutFinished and Area.GenerateGraphFinished.
            logicCore.AsyncAlgorithmCompute = false;

            //Finally assign logic core to GraphArea object
            graphArea.LogicCore = logicCore;
        }
    }
}
