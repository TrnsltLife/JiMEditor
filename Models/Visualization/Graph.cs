using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickGraph;

namespace JiME.Visualization
{
    /// <summary>
    /// This is our custom data graph derived from BidirectionalGraph class using custom data types.
    /// Data graph stores vertices and edges data that is used by GraphArea and end-user for a variety of operations.
    /// Data graph content handled manually by user (add/remove objects). The main idea is that you can dynamicaly
    /// remove/add objects into the GraphArea layout and then use data graph to restore original layout content.
    /// </summary>
    public class Graph : BidirectionalGraph<DataVertex, DataEdge>
    {
        public static Graph Generate(Scenario scenario, Action<DataVertex> clickAction)
        {
            //Lets make new data graph instance
            var dataGraph = new Graph();

            // Prepare all vertices from the scenario
            var vertexDict = new Dictionary<string, DataVertex>();

            // Add specific START vertex to work as root node
            var startVertexId = "START";
            var startVertex = new DataVertex("START", DataVertex.Type.Start, null, null, clickAction);
            vertexDict.Add(startVertexId, startVertex);
            dataGraph.AddVertex(startVertex);

            // Prepare triggers first as they are the clue that ties everything together and need to be available
            HandleCollection(scenario.triggersObserver, x =>
            {
                // Skip "None" named objects
                if (x.dataName == "None")
                {
                    return null;
                }

                // Vertex for the trigger
                var vertex = new DataVertex(x.dataName, DataVertex.Type.Trigger, x, null, clickAction);
                vertexDict.Add(getTriggerName(x.dataName), vertex);
                dataGraph.AddVertex(vertex);
                return vertex;
            });

            // Then prepare objectives and link to triggers
            HandleCollection(scenario.objectiveObserver, x =>
            {
                // Skip "None" named objects
                if (x.dataName == "None")
                {
                    return null;
                }

                // Vertex for the objective
                var vertex = new DataVertex(x.dataName, DataVertex.Type.Objective, x, null, clickAction);
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
                    getTriggerVertex(vertexDict, x.triggeredByName, v =>
                        {
                            dataGraph.AddEdge(new DataEdge(v, vertex) { Text = "initiates" });
                        });

                    // Object is completed by...
                    getTriggerVertex(vertexDict, x.triggerName, v =>
                        {
                            dataGraph.AddEdge(new DataEdge(v, vertex) { Text = "completes" });
                        });

                    // Object causes further trigger if completed...
                    getTriggerVertex(vertexDict, x.nextTrigger, v =>
                        {
                            dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "on completion" });
                        });
                });

            // Then prepare resolutions and link to triggers
            HandleCollection(scenario.resolutionObserver, x =>
            {
                // Vertex for the resolution
                var vertex = new DataVertex(x.dataName, DataVertex.Type.Resolution, x, null, clickAction);
                vertexDict.Add(getResolutionName(x.dataName), vertex);
                dataGraph.AddVertex(vertex);
                return vertex;
            },
            (x, vertex) =>
            {
                // Object is completed by...
                getTriggerVertex(vertexDict, x.triggerName, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(v, vertex) { Text = "triggers" });
                    });
            });

            // Then prepare interactions and link to triggers
            HandleCollection(scenario.interactionObserver, x =>
            {
                // Skip "None" named objects
                if (x.dataName == "None")
                {
                    return null;
                }

                // Vertex for the interaction
                var vertex = new DataVertex(x.dataName, DataVertex.Type.Interaction, x, null, clickAction);
                vertexDict.Add(getInteractionName(x.dataName), vertex);
                dataGraph.AddVertex(vertex);

                // Create interaction group if needed (if this Interaction is part of an random integration group)
                var groupVertex = IntegrationGroupCheck(vertexDict, dataGraph, x.dataName, clickAction);
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
                    getTriggerVertex(vertexDict, x2.triggerName, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(v, vertex) { Text = "initiates" });
                    });
                    getTriggerVertex(vertexDict, x2.triggerAfterName, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "triggers" });
                    });
                }

                // Rest depends on interaction type
                if (x is DecisionInteraction)
                {
                    var x2 = (DecisionInteraction)x;
                    getTriggerVertex(vertexDict, x2.choice1Trigger, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "\"" + x2.choice1 + "\"" });
                    });
                    getTriggerVertex(vertexDict, x2.choice2Trigger, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "\"" + x2.choice2 + "\"" });
                    });
                    getTriggerVertex(vertexDict, x2.choice3Trigger, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "\"" + x2.choice3 + "\"" });
                    });
                }
                else if (x is DialogInteraction)
                {
                    var x2 = (DialogInteraction)x;
                    getTriggerVertex(vertexDict, x2.c1Trigger, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "\"" + x2.choice1 + "\"" });
                    });
                    getTriggerVertex(vertexDict, x2.c2Trigger, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "\"" + x2.choice2 + "\"" });
                    });
                    getTriggerVertex(vertexDict, x2.c3Trigger, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "\"" + x2.choice3 + "\"" });
                    });
                }
                else if (x is TestInteraction)
                {
                    var x2 = (TestInteraction)x;
                    getTriggerVertex(vertexDict, x2.successTrigger, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "\"PASS\"" });
                    });
                    getTriggerVertex(vertexDict, x2.failTrigger, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "\"FAIL\"" });
                    });
                }
                else if (x is ConditionalInteraction)
                {
                    var x2 = (ConditionalInteraction)x;
                    getTriggerVertex(vertexDict, x2.finishedTrigger, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "finished" });
                    });
                    if (x2.triggerList != null)
                    {
                        foreach (var t in x2.triggerList)
                        {
                            getTriggerVertex(vertexDict, t, v =>
                            {
                                dataGraph.AddEdge(new DataEdge(v, vertex) { Text = "condition" });
                            });
                        }
                    }
                }
                else if (x is PersistentTokenInteraction)
                {
                    var x2 = (PersistentTokenInteraction)x;
                    getEventVertex(vertexDict, x2.eventToActivate, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "\"activate\"" });
                    });
                    getTriggerVertex(vertexDict, x2.alternativeTextTrigger, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "\"alt.text\"" });
                    });
                }
                else if (x is ThreatInteraction)
                {
                    var x2 = (ThreatInteraction)x;
                    getTriggerVertex(vertexDict, x2.triggerDefeatedName, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "\"defeated\"" });
                    });
                }
                else if (x is MultiEventInteraction)
                {
                    var x2 = (MultiEventInteraction)x;
                    // Triggers and Events are mutually exclusive and that is reflected here even if the lists themselves could both have items
                    if (x2.triggerList != null && x2.usingTriggers)
                    {
                        foreach (var t in x2.triggerList)
                        {
                            getTriggerVertex(vertexDict, t, v =>
                            {
                                dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "triggers" });
                            });
                        }                        
                    }
                    if (x2.eventList != null && !x2.usingTriggers)
                    {
                        foreach (var t in x2.eventList)
                        {
                            getEventVertex(vertexDict, t, v =>
                            {
                                dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "triggers" });
                            });
                        }
                    }
                }
                else if (x is BranchInteraction)
                {
                    var x2 = (BranchInteraction)x;

                    // First draw out the source trigger
                    getTriggerVertex(vertexDict, x2.triggerTest, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(v, vertex) { Text = "input" });
                    });

                    // Then check the outputs
                    if (x2.branchTestEvent)
                    {
                        // Triggering events
                        getEventVertex(vertexDict, x2.triggerIsSet, v =>
                        {
                            dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "set" });
                        });
                        getEventVertex(vertexDict, x2.triggerNotSet, v =>
                        {
                            dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "not set" });
                        });
                    }
                    else
                    {
                        // Triggering triggers
                        getTriggerVertex(vertexDict, x2.triggerIsSetTrigger, v =>
                        {
                            dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "set" });
                        });
                        getTriggerVertex(vertexDict, x2.triggerNotSetTrigger, v =>
                        {
                            dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "not set" });
                        });
                    }
                }
                else if (x is ReplaceTokenInteraction)
                {
                    var x2 = (ReplaceTokenInteraction)x;
                    getEventVertex(vertexDict, x2.eventToReplace, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "replace this" });
                    });
                    getEventVertex(vertexDict, x2.replaceWithEvent, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "with this" });
                    });
                }
                else if (x is ItemInteraction)
                {
                    var x2 = (ItemInteraction)x;
                    getTriggerVertex(vertexDict, x2.fallbackTrigger, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "\"fallback\"" });
                    });
                }
                else if (x is TitleInteraction)
                {
                    var x2 = (TitleInteraction)x;
                    getTriggerVertex(vertexDict, x2.fallbackTrigger, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "\"fallback\"" });
                    });
                }
                else // TextInteraction, NoneInteraction, RewardInteraction, DarknessInteraction
                {
                    // These types do not have any special connections to add
                }
            });

            // Then prepare tile blocks and link to triggers
            HandleCollection(scenario.chapterObserver, x =>
            {
                // Vertex for the scenario/tileset
                var vertex = new DataVertex(x.dataName, DataVertex.Type.Chapter, x, null, clickAction);
                vertexDict.Add(getChapterName(x.dataName), vertex);
                dataGraph.AddVertex(vertex);
                return vertex;
            },
            (x, vertex) =>
            {
                HandleCollection(x.tileObserver.OfType<BaseTile>(), tileX =>
                {
                    // Vertex for the scenario/tileset
                    var tileVertex = new DataVertex(tileX.idNumber.ToString() + tileX.tileSide, DataVertex.Type.Tile, tileX, null, clickAction);
                    vertexDict.Add(getTileName(tileX.GUID.ToString()), tileVertex);
                    dataGraph.AddVertex(tileVertex);
                    return tileVertex;
                },
                (tileX, tileVertex) =>
                {
                    // Link tile to it's chapter
                    dataGraph.AddEdge(new DataEdge(vertex, tileVertex) { Text = "contains" });

                    // Object is triggered by...
                    getTriggerVertex(vertexDict, tileX.triggerName, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(tileVertex, v) { Text = "explore triggers" });
                    });

                    HandleCollection(tileX.tokenList, tokenX =>
                    {
                        // Vertex for the scenario/tileset
                        var tokenVertex = new DataVertex(tokenX.dataName, DataVertex.Type.Token, tokenX, tileX, clickAction);
                        vertexDict.Add(getTokenName(tokenX.GUID.ToString()), tokenVertex);
                        dataGraph.AddVertex(tokenVertex);
                        return tokenVertex;
                    },
                    (tokenX, tokenVertex) =>
                    {
                        // Link token to it's tile
                        dataGraph.AddEdge(new DataEdge(tileVertex, tokenVertex) { Text = "contains" });

                        // Object is triggered by...
                        getTriggerVertex(vertexDict, tokenX.triggeredByName, v =>
                        {
                            dataGraph.AddEdge(new DataEdge(v, tokenVertex) { Text = "activates" });
                        });

                        // Object is triggered by... (note: this can only link to Events!)
                        getEventVertex(vertexDict, tokenX.triggerName, v =>
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
                getTriggerVertex(vertexDict, x.triggeredBy, v =>
                {
                    dataGraph.AddEdge(new DataEdge(v, vertex) { Text = "reveals" });
                });
                getTriggerVertex(vertexDict, x.triggerName, v =>
                {
                    dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "triggers" });
                });

                // Object is explored by...
                getTriggerVertex(vertexDict, x.exploreTrigger, v =>
                {
                    dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "explore triggers" });
                });


                getTriggerVertex(vertexDict, x.exploredAllTilesTrigger, v =>
                {
                    dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "fully explored triggers" });
                });

            });

            // Check each threat theshold (ordered by threshold)
            if (!scenario.threatNotUsed)
            {
                DataVertex prevThreat = startVertex;
                HandleCollection(scenario.threatObserver.OrderBy(x => x.threshold), x =>
                {
                    // Vertex for the threat
                    var vertex = new DataVertex("Threat Level: " + x.threshold, DataVertex.Type.ThreatLevel, x, null, clickAction);
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
                    getEventVertex(vertexDict, x.triggerName, v =>
                    {
                        dataGraph.AddEdge(new DataEdge(vertex, v) { Text = "initiates" });
                    });
                });
                var maxThreatVertex = new DataVertex("Max Threat: " + scenario.threatMax, DataVertex.Type.ThreatLevel, null, null, clickAction);
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

        private static void getEventVertex(Dictionary<string, DataVertex> dict, string dataName, Action<DataVertex> action)
        {
            if (dataName == null || dataName == "None")
            {
                return;
            }
            var eventName = getInteractionName(dataName);
            if (dict.ContainsKey(eventName))
            {
                action(dict[eventName]);
                return;
            }
            // Not found, just log this since this might happen if triggers are removed before all connections are done
            Console.WriteLine("Graph: Could not find interaction with dataName: " + dataName);
        }

        private static void getTriggerVertex(Dictionary<string, DataVertex> dict, string dataName, Action<DataVertex> action)
        {
            if (dataName == null || dataName == "None")
            {
                return;
            }
            var trigName = getTriggerName(dataName);
            if (dict.ContainsKey(trigName))
            {
                action(dict[trigName]);
                return;
            }
            // Not found, just log this since this might happen if triggers are removed before all connections are done
            Console.WriteLine("Graph: Could not find trigger with dataName: " + dataName);
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

        private static DataVertex IntegrationGroupCheck(Dictionary<string, DataVertex> dict, Graph dataGraph, string dataName, Action<DataVertex> clickAction)
        {
            if (dataName.Contains("GRP"))
            {
                var trimmed = dataName.Trim();
                var groupNameAndNumber = trimmed.Substring(trimmed.IndexOf("GRP"));
                var groupName = getInteractionGroupName(groupNameAndNumber);
                if (!dict.ContainsKey(groupName))
                {
                    // Create the group vertex
                    var vertex = new DataVertex(groupNameAndNumber, DataVertex.Type.InteractionGroup, null, null, clickAction);
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
