using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickGraph;

namespace JiME.Visualization.Models
{
    /// <summary>
    /// This is our custom data graph derived from BidirectionalGraph class using custom data types.
    /// Data graph stores vertices and edges data that is used by GraphArea and end-user for a variety of operations.
    /// Data graph content handled manually by user (add/remove objects). The main idea is that you can dynamicaly
    /// remove/add objects into the GraphArea layout and then use data graph to restore original layout content.
    /// </summary>
    public class Graph : BidirectionalGraph<DataVertex, DataEdge>
    {
        public static Graph Generate(Scenario scenario)
        {
            // TODO: Rename triggers???
            // TODO: Some flavor text has things like "player may use 1 token to do X or Y", this might be really hard to include in random generation...

            //Lets make new data graph instance
            var dataGraph = new Graph();

            // Prepare all vertices from the scenario
            var vertexDict = new Dictionary<string, DataVertex>();

            // Add specific START vertex to work as root node
            var startVertexId = "START";
            var startVertex = new DataVertex("START", DataVertex.Type.Start, null);
            vertexDict.Add(startVertexId, startVertex);
            dataGraph.AddVertex(startVertex);

            // TODO: Data vertex types should be handled and shown in different color / shapes etc.

            // Prepare triggers first as they are the clue that ties everything together and need to be available
            HandleCollection(scenario.triggersObserver, x =>
            {
                // Skip "None" named objects TODO: Are these a special thing that should be always included?
                if (x.dataName == "None")
                {
                    return null;
                }

                // Vertex for the trigger
                var vertex = new DataVertex(x.dataName, DataVertex.Type.Trigger, x);
                vertexDict.Add(getTriggerName(x.dataName), vertex);
                dataGraph.AddVertex(vertex);
                return vertex;
            });

            // Then prepare objectives and link to triggers
            HandleCollection(scenario.objectiveObserver, x =>
            {
                // Skip "None" named objects TODO: Are these a special thing that should be always included?
                if (x.dataName == "None")
                {
                    return null;
                }

                // Vertex for the objective
                var vertex = new DataVertex(x.dataName, DataVertex.Type.Objective, x);
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
                var vertex = new DataVertex(x.dataName, DataVertex.Type.Resolution, x);
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
                // Skip "None" named objects TODO: Are these a special thing that should be always included?
                if (x.dataName == "None")
                {
                    return null;
                }

                // Vertex for the interaction
                var vertex = new DataVertex(x.dataName, DataVertex.Type.Interaction, x);
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
                    if (x2.triggerName?.Length == 0 || x2.triggerName == "None")
                    {
                        // Special case: No initiation trigger so triggered from the start?
                        // TODO: Or is it? Could this never be triggered?
                        dataGraph.AddEdge(new DataEdge(startVertex, vertex) { Text = "initiates" });
                    }
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
                else if (x is DialogInteraction)
                {
                    var x2 = (DialogInteraction)x;
                    getTriggerOrEventVertex(vertexDict, x2.c1Trigger, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "\"" + x2.choice1 + "\"" });
                    });
                    getTriggerOrEventVertex(vertexDict, x2.c2Trigger, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "\"" + x2.choice2 + "\"" });
                    });
                    getTriggerOrEventVertex(vertexDict, x2.c3Trigger, v =>
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

                // Special case: StoryPointInteractionsa re only used during procedural generation for debug purposes
                else if (x is Procedural.StoryElements.StoryPointInteraction)
                {
                    var x2 = (Procedural.StoryElements.StoryPointInteraction)x;

                    // Endpoints are simple
                    foreach (var otherAfter in x2.OtherAfterTriggers)
                    {
                        getTriggerOrEventVertex(vertexDict, otherAfter, v =>
                        {
                            dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "\"ENDING\"" });
                        });
                    }
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
                var vertex = new DataVertex(x.dataName, DataVertex.Type.Chapter, x);
                vertexDict.Add(getChapterName(x.dataName), vertex);
                dataGraph.AddVertex(vertex);
                return vertex;
            },
            (x, vertex) =>
            {
                HandleCollection(x.tileObserver.OfType<BaseTile>(), tileX =>
                {
                    // Vertex for the scenario/tileset
                    var tileVertex = new DataVertex(tileX.idNumber.ToString() + tileX.tileSide, DataVertex.Type.Tile, tileX);
                    vertexDict.Add(getTileName(tileX.GUID.ToString()), tileVertex);
                    dataGraph.AddVertex(tileVertex);
                    return tileVertex;
                },
                (tileX, tileVertex) =>
                {
                    // Link tile to it's chapter
                    dataGraph.AddEdge(new DataEdge(vertex, tileVertex) { Text = "contains" });

                    // Object is triggered by...
                    getTriggerOrEventVertex(vertexDict, tileX.triggerName, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(tileVertex, v) { Text = "explore triggers" });
                    });

                    HandleCollection(tileX.tokenList, tokenX =>
                    {
                        // Vertex for the scenario/tileset
                        var tokenVertex = new DataVertex(tokenX.dataName, DataVertex.Type.Token, tokenX);
                        vertexDict.Add(getTokenName(tokenX.GUID.ToString()), tokenVertex);
                        dataGraph.AddVertex(tokenVertex);
                        return tokenVertex;
                    },
                    (tokenX, tokenVertex) =>
                    {
                        // Link token to it's tile
                        dataGraph.AddEdge(new DataEdge(tileVertex, tokenVertex) { Text = "contains" });

                        // Object is triggered by...
                        getTriggerOrEventVertex(vertexDict, tokenX.triggeredByName, v =>
                        {
                            dataGraph.AddEdge(new DataEdge(v, tokenVertex) { Text = "activates" });
                        });

                        // Object is triggered by...
                        getTriggerOrEventVertex(vertexDict, tokenX.triggerName, v =>
                        {
                            dataGraph.AddEdge(new DataEdge(tokenVertex, v) { Text = "triggers" });
                        });
                    });
                });

                // Start tiles
                if (x.triggeredBy == null || x.triggeredBy.Length == 0 || x.triggeredBy == "None")
                {
                    dataGraph.AddEdge(new DataEdge(startVertex, vertex) { Text = "explores" });
                }

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
            /*  TODO: Should we ever add these? Only if linked to something since this contains even the unused ones
            HandleCollection(scenario.activationsObserver, x =>
            {
                // Vertex for the activation
                var vertex = new DataVertex(x.dataName);
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
            });*/

            // Check each threat theshold (ordered by threshold)
            if (!scenario.threatNotUsed)
            {
                DataVertex prevThreat = startVertex;
                HandleCollection(scenario.threatObserver.OrderBy(x => x.threshold), x =>
                {
                    // Vertex for the threat
                    var vertex = new DataVertex("Threat Level: " + x.threshold, DataVertex.Type.ThreatLevel, x);
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
                var maxThreatVertex = new DataVertex("Max Threat: " + scenario.threatMax, DataVertex.Type.ThreatLevel, null);
                vertexDict.Add(getThreatName("MAX THREAT"), maxThreatVertex);
                dataGraph.AddVertex(maxThreatVertex);
                dataGraph.AddEdge(new DataEdge(prevThreat, maxThreatVertex, weight: 2) { Text = null });
            }

            // Return result
            return dataGraph;
        }

        private static string getTriggerName(string dataName) => "TRIG_" + dataName;
        private static string getThreatName(string dataName) => "Threat_" + dataName;
        private static string getObjectiveName(string dataName) => "OBJ_" + dataName;
        private static string getResolutionName(string dataName) => "RES_" + dataName;
        private static string getChapterName(string dataName) => "CHAP_" + dataName;
        private static string getTileName(string dataName) => "TILE_" + dataName;
        private static string getTokenName(string dataName) => "TOKEN_" + dataName;
        private static string getActivationName(string dataName) => "ACT_" + dataName;
        private static string getInteractionName(string dataName) => "INT_" + dataName;
        private static string getInteractionGroupName(string groupNameAndNumber) => "INTGRP_" + groupNameAndNumber;

        private static void getTriggerOrEventVertex(Dictionary<string, DataVertex> dict, string dataName, Action<DataVertex> action)
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

        private static void HandleCollection<T>(IEnumerable<T> collection, Func<T, DataVertex> vertexHandler, Action<T, DataVertex> edgeHandler = null)
        {
            // First add all vertices
            var res = new List<Tuple<T, DataVertex>>();
            foreach (var x in collection)
            {
                var vertex = vertexHandler(x);
                if (vertex != null)
                {
                    res.Add(Tuple.Create(x, vertex));
                }                
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

        private static DataVertex IntegrationGroupCheck(Dictionary<string, DataVertex> dict, Graph dataGraph, string dataName)
        {
            if (dataName.Contains("GRP"))
            {
                var trimmed = dataName.Trim();
                var groupNameAndNumber = trimmed.Substring(trimmed.IndexOf("GRP"));
                var groupName = getInteractionGroupName(groupNameAndNumber);
                if (!dict.ContainsKey(groupName))
                {
                    // Create the group vertex
                    var vertex = new DataVertex(groupNameAndNumber, DataVertex.Type.InteractionGroup, null);
                    dict.Add(groupName, vertex);
                    dataGraph.AddVertex(vertex);
                    return vertex;
                }
                else
                {
                    // Return existing group
                    return dict[groupName];
                }
            }
            return null;
        }
    }
}
