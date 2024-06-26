﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using GraphX;
using GraphX.Controls;
using GraphX.Controls.Models;
using JiME.Visualization;
using Newtonsoft.Json;

namespace JiME.Views
{
    public class GraphControlFactory : GraphX.Controls.Models.GraphControlFactory
    {
        public GraphControlFactory(GraphAreaBase graphArea) : base(graphArea)
        {
        }

        public override VertexControl CreateVertexControl(object vertexData)
        {
            // Check data type first
            var d = vertexData as DataVertex;
            if (d == null)
            {
                return base.CreateVertexControl(vertexData);
            }

            // For DataVertexes, we do some modifications for the default controls
            var ctrl = base.CreateVertexControl(vertexData);

            // Always use black text
            ctrl.Foreground = Brushes.Black;

            // Add a very slight opacity to the control so that it more clear if they overlap for some reason
            // NOTE: Setting Border-details do nothing to the underlying control unfortunately so used this
            ctrl.Opacity = 0.9;

            // Set max width to these controls to help with overlapping issue
            // NOTE: This is somewhat related to what is used as SimpleTreeLayoutParameters.VertexGap in GraphArea settings 
            ctrl.MaxWidth = 300;

            // Update coloring
            switch (d.VertexType)
            {
                case DataVertex.Type.Start:
                case DataVertex.Type.ThreatLevel:
                    ctrl.Background = Brushes.LightGray;
                    break;
                case DataVertex.Type.Trigger:
                    ctrl.Background = Brushes.IndianRed;
                    break;
                case DataVertex.Type.Objective:
                    ctrl.Background = Brushes.Green;
                    break;
                case DataVertex.Type.Resolution:
                    ctrl.Background = Brushes.DarkGreen;
                    break;
                case DataVertex.Type.Interaction:
                case DataVertex.Type.InteractionGroup:
                    ctrl.Background = Brushes.YellowGreen;
                    break;
                case DataVertex.Type.Chapter:
                case DataVertex.Type.Tile:
                case DataVertex.Type.Token:
                    ctrl.Background = Brushes.LightGreen;
                    break;
            }

            // Interaction depends on whether we have a click action
            if (d.ClickAction != null)
            {
                // Implememnt clicking
                // NOTE: We need to defer execution here for a small while since otherwise the re-visualization happens too close to
                // when this function is updating the graph and and we get NULL reference somewhere inside GraphX VertexControl MouseUp-event
                ctrl.Click += async (a, b) =>
                {
                    await Task.Delay(100);
                    d.ClickAction(d);
                };
                ctrl.ToolTip = d.Text + "\nClick to edit " + d.Source?.GetType().Name ?? "";
            }
            else // No click action
            {
                // Prepare JSON content display
                var jsonLines = JsonConvert.SerializeObject(d.Source, Formatting.Indented).Replace("\r", "").Split('\n').ToList();
                int similarCount = 1;
                for (int i = 1; i < jsonLines.Count; i++) // Combine identical lines
                {
                    if (jsonLines[i - 1] == jsonLines[i])
                    {
                        // Lines are identical
                        similarCount++;
                        jsonLines.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        // Lines differ, add count if more than one
                        if (similarCount > 1)
                        {
                            jsonLines[i - 1] = jsonLines[i - 1] + string.Format(" [x{0}]", similarCount);
                            similarCount = 1;
                        }
                    }
                }
                if (jsonLines.Count > 30) // Truncate if necessary
                {
                    jsonLines = jsonLines.Take(30).ToList();
                    jsonLines.Add("--- CONTENT TRUNCATED ---");
                }

                // Setup tooltip
                var preNote = "CANNOT EDIT ITEMS IN THIS VIEW - GO TO THE MAIN EDITOR AND TRY CLICKING ITEMS THERE\nType: ";
                ctrl.ToolTip = preNote + d.VertexType.ToString() + "\n" + string.Join("\n", jsonLines);

            }

            // Setup tooltip properties
            System.Windows.Controls.ToolTipService.SetInitialShowDelay(ctrl, 100);
            System.Windows.Controls.ToolTipService.SetShowDuration(ctrl, 1000000);

            return ctrl;
        }

        public override EdgeControl CreateEdgeControl(VertexControl source, VertexControl target, object edge, bool showArrows = true, Visibility visibility = Visibility.Visible)
        {
            // Check data type first
            var e= edge as DataEdge;
            if (e == null)
            {
                return base.CreateEdgeControl(source, target, edge, showArrows, visibility);
            }

            // For DataEdges, we do some modifications for the default controls
            var ctrl = base.CreateEdgeControl(source, target, edge, showArrows, visibility); // Visibility affects arrow visibility, not the balloon
            ctrl.Opacity = 0.5;
            ctrl.Foreground = Brushes.LightGray;
            if (e.Text?.Length > 0)
            {
                ctrl.ToolTip = e.Source.Text + "\n" + e.Text + "\n" + e.Target.Text;
            }
            else
            {
                ctrl.ToolTip = e.Source.Text + "\n-->\n" + e.Target.Text;
            }
            
            return ctrl;
        }
    }
}
