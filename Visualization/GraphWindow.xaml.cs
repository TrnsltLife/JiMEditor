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
        private string getThreatName(string dataName) => "Threat_" + dataName;
        private string getObjectiveName(string dataName) => "OBJ_" + dataName;        
        private string getResolutionName(string dataName) => "RES_" + dataName;
        private string getChapterName(string dataName) => "CHAP_" + dataName;
        private string getActivationName(string dataName) => "ACT_" + dataName;
        private string getInteractionName(string dataName) => "INT_" + dataName;
        private string getInteractionGroupName(string groupNameAndNumber) => "INTGRP_" + groupNameAndNumber;

        private void getTriggerOrEventVertex(Dictionary<string, DataVertex> dict, string dataName, Action<DataVertex> action)
        {
            if (dataName?.Length > 0 && dataName != "None")
            {
                // First try trigger
                var trigName = getTriggerName(dataName);
                if (dict.ContainsKey(trigName))
                {
                    action(dict[trigName]);
                    return;
                }

                // Then try event
                var eventName = getInteractionName(dataName);
                if (dict.ContainsKey(eventName))
                {
                    action(dict[eventName]);
                    return;
                }

                // Not found
                throw new Exception("Could not find trigger or interaction with dataName: " + dataName);
            }
        }

        private void HandleCollection<T>(IEnumerable<T> collection, Func<T, DataVertex> vertexHandler, Action<T, DataVertex> edgeHandler = null)
        {
            // First add all vertices
            var res = new List<Tuple<T, DataVertex>>();
            foreach (var x in collection)
            {
                var vertex = vertexHandler(x);
                res.Add(Tuple.Create(x, vertex));
            }

            // Then do the edges (afterwards because these might refer to items in collection)
            if (edgeHandler != null)
            {
                foreach (var x in res)
                {
                    edgeHandler(x.Item1, x.Item2);
                }
            }
        }

        private DataVertex IntegrationGroupCheck(Dictionary<string, DataVertex> dict, Graph dataGraph, string dataName)
        {
            if (dataName.Contains("GRP"))
            {
                var trimmed = dataName.Trim();
                var groupNameAndNumber = trimmed.Substring(trimmed.IndexOf("GRP"));
                var groupName = getInteractionGroupName(groupNameAndNumber);
                if (!dict.ContainsKey(groupName))
                {
                    // Create the group vertex
                    var vertex = new DataVertex("G:" + groupNameAndNumber);
                    dict.Add(groupName, vertex);
                    dataGraph.AddVertex(vertex);
                    return vertex;
                } else
                {
                    // Return existing group
                    return dict[groupName];
                }
            }
            return null;
        }

        private void SetupGraph()
        {
            if (scenario == null)
            {
                return;
            }

            // TODO: Graph setup should be done in the model side (at least vertex and edge creation parts)
            // TODO: Rename triggers???
            // TODO: Some flavor text has things like "player may use 1 token to do X or Y", this might be really hard to include in random generation...

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

            // Prepare triggers first as they are the clue that ties everything together and need to be available
            HandleCollection(scenario.triggersObserver, x =>
            {
                // Vertex for the trigger
                var vertex = new DataVertex("T:" + x.dataName);
                vertexDict.Add(getTriggerName(x.dataName), vertex);
                dataGraph.AddVertex(vertex);
                return vertex;
            });

            // Then prepare objectives and link to triggers
            HandleCollection(scenario.objectiveObserver, x =>
            {
                // Vertex for the objective
                var vertex = new DataVertex("O:" + x.dataName);
                vertexDict.Add(getObjectiveName(x.dataName), vertex);
                dataGraph.AddVertex(vertex);
                return vertex;
            },
            (x, vertex) =>
            {
                // Scenario default objective
                if (scenario.objectiveName == x.dataName)
                {
                    dataGraph.AddEdge(new DataEdge(startVertex, vertex) { Text = "initiates" });
                }

                // Object is triggered by... (made available)
                getTriggerOrEventVertex(vertexDict, x.triggeredByName, v =>
                {
                    dataGraph.AddEdge(new DataEdge(v, vertex) { Text = "initiates" });
                });

                // Object is completed by...
                getTriggerOrEventVertex(vertexDict, x.triggerName, v =>
                {
                    dataGraph.AddEdge(new DataEdge(v, vertex) { Text = "completes" });
                });

                // Object causes further trigger if completed...
                getTriggerOrEventVertex(vertexDict, x.nextTrigger, v =>
                {
                    dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "on completion" });
                });
            });

            // Then prepare resolutions and link to triggers
            HandleCollection(scenario.resolutionObserver, x =>
            {
                // Vertex for the resolution
                var vertex = new DataVertex("R:" + x.dataName);
                vertexDict.Add(getResolutionName(x.dataName), vertex);
                dataGraph.AddVertex(vertex);
                return vertex;
            },
            (x, vertex) =>
            {
                // Object is completed by...
                getTriggerOrEventVertex(vertexDict, x.triggerName, v =>
                {
                    dataGraph.AddEdge(new DataEdge(v, vertex) { Text = "triggers" });
                });
            });

            // Then prepare interactions and link to triggers
            HandleCollection(scenario.interactionObserver, x =>
            {
                // Vertex for the interaction
                var vertex = new DataVertex("I:" + x.dataName);
                vertexDict.Add(getInteractionName(x.dataName), vertex);
                dataGraph.AddVertex(vertex);

                // Create interaction group if needed
                var groupVertex = IntegrationGroupCheck(vertexDict, dataGraph, x.dataName);
                if (groupVertex != null)
                {
                    dataGraph.AddEdge(new DataEdge(groupVertex, vertex) { Text = "includes" });
                }

                // Return actual vertex
                return vertex;
            },
            (x, vertex) =>
            {
                // Base interaction triggers
                if (x is InteractionBase)
                {
                    var x2 = x as InteractionBase;
                    getTriggerOrEventVertex(vertexDict, x2.triggerName, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(v, vertex) { Text = "initiates" });
                    });
                    /*else
                    {
                        // Special case: No initiation trigger so triggered from the start?
                        // TODO: Or is it? Could this never be triggered?
                        dataGraph.AddEdge(new DataEdge(startVertex, vertex) { Text = "initiates" });
                    }*/
                    getTriggerOrEventVertex(vertexDict, x2.triggerAfterName, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "triggers" });
                    });
                    // TODO: other properties?
                }

                // Rest depends on interaction type
                if (x is DecisionInteraction)
                {
                    var x2 = (DecisionInteraction)x;
                    getTriggerOrEventVertex(vertexDict, x2.choice1Trigger, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "\"" + x2.choice1 + "\"" });
                    });
                    getTriggerOrEventVertex(vertexDict, x2.choice2Trigger, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "\"" + x2.choice2 + "\"" });
                    });
                    getTriggerOrEventVertex(vertexDict, x2.choice3Trigger, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "\"" + x2.choice3 + "\"" });
                    });
                    // TODO: Other properties
                }
                else if (x is TestInteraction)
                {
                    var x2 = (TestInteraction)x;
                    getTriggerOrEventVertex(vertexDict, x2.successTrigger, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "\"PASS\"" });
                    });
                    getTriggerOrEventVertex(vertexDict, x2.failTrigger, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "\"FAIL\"" });
                    });
                    // TODO: Other properties
                }
                else if (x is ConditionalInteraction)
                {
                    var x2 = (ConditionalInteraction)x;
                    getTriggerOrEventVertex(vertexDict, x2.finishedTrigger, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "\"FINISHED\"" });
                    });
                    if (x2.triggerList != null)
                    {
                        foreach (var t in x2.triggerList)
                        {
                            getTriggerOrEventVertex(vertexDict, t, v =>
                            {
                                dataGraph.AddEdge(new DataEdge(v, vertex) { Text = "\"CONDITION\"" });
                            });
                        }
                    }
                    // TODO: Other properties
                }
                else if (x is PersistentTokenInteraction)
                {
                    var x2 = (PersistentTokenInteraction)x;
                    getTriggerOrEventVertex(vertexDict, x2.eventToActivate, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "\"ACTIVATE\"" });
                    });
                    getTriggerOrEventVertex(vertexDict, x2.alternativeTextTrigger, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "\"ALT. TEXT\"" });
                    });
                    // TODO: Other properties
                }
                else if (x is ThreatInteraction)
                {
                    var x2 = (ThreatInteraction)x;
                    getTriggerOrEventVertex(vertexDict, x2.triggerDefeatedName, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "\"DEFEATED\"" });
                    });
                    // TODO: Other properties
                }
                else if (x is TextInteraction)
                {
                    // No triggers
                    // TODO: Other properties
                }
                else // TODO: other types
                {
                    throw new NotImplementedException("Graph preparation not implemented for interaction type: " + x.GetType().Name);
                }
            });

            // Then prepare tile blocks and link to triggers
            HandleCollection(scenario.chapterObserver, x =>
            {
                // Vertex for the scenario/tileset
                var vertex = new DataVertex("C:" + x.dataName);
                vertexDict.Add(getChapterName(x.dataName), vertex);
                dataGraph.AddVertex(vertex);
                return vertex;
            },
            (x, vertex) =>
            {
                // Pre-explored from the start
                // TODO: this is not right, "start" tiles had this false
                /*if (x.isPreExplored)
                {
                    dataGraph.AddEdge(new DataEdge(startVertex, vertex) { Text = "explores" });
                }*/

                if (vertexDict.ContainsKey(getInteractionGroupName(x.randomInteractionGroup ?? "")))
                {
                    var groupVertex = vertexDict[getInteractionGroupName(x.randomInteractionGroup)];
                    dataGraph.AddEdge(new DataEdge(vertex, groupVertex) { Text = "activates" });
                }

                // Object is explored by...
                getTriggerOrEventVertex(vertexDict, x.triggeredBy, v =>
                {
                    dataGraph.AddEdge(new DataEdge(v, vertex) { Text = "reveals" });
                });
                getTriggerOrEventVertex(vertexDict, x.triggerName, v =>
                {
                    dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "triggers" });
                });

                // Object is explored by...
                getTriggerOrEventVertex(vertexDict, x.exploreTrigger, v =>
                {
                    dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "explore triggers" });
                });
            });

            // Then prepare monster activations and link to triggers
            HandleCollection(scenario.activationsObserver, x =>
            {
                // Vertex for the activation
                var vertex = new DataVertex("A:" + x.dataName);
                vertexDict.Add(getActivationName(x.dataName), vertex);
                dataGraph.AddVertex(vertex);
                return vertex;
            },
            (x, vertex) =>
            {
                // Object is activated by...
                getTriggerOrEventVertex(vertexDict, x.triggerName, v =>
                {
                    dataGraph.AddEdge(new DataEdge(v, vertex) { Text = "activates" });
                });
            });

            // Check each threat theshold (ordered by threshold)
            if (!scenario.threatNotUsed)
            {
                DataVertex prevThreat = startVertex;
                HandleCollection(scenario.threatObserver.OrderBy(x => x.threshold), x =>
                {
                // Vertex for the threat
                var vertex = new DataVertex("Threat Level: " + x.threshold);
                    vertexDict.Add(getThreatName(x.dataName + "_" + x.threshold), vertex);
                    dataGraph.AddVertex(vertex);
                    return vertex;
                },
                (x, vertex) =>
                {
                // Link to previous threat
                dataGraph.AddEdge(new DataEdge(prevThreat, vertex, weight: 2) { Text = null });
                    prevThreat = vertex;

                // Object is completed by...
                getTriggerOrEventVertex(vertexDict, x.triggerName, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "initiates" });
                    });
                });
                var maxThreatVertex = new DataVertex("Max Threat: " + scenario.threatMax);
                vertexDict.Add(getThreatName("MAX THREAT"), maxThreatVertex);
                dataGraph.AddVertex(maxThreatVertex);
                dataGraph.AddEdge(new DataEdge(prevThreat, maxThreatVertex, weight: 2) { Text = null });
            }

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
