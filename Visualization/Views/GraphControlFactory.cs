using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using GraphX;
using GraphX.Controls;
using GraphX.Controls.Models;
using JiME.Visualization.Models;
using Newtonsoft.Json;

namespace JiME.Visualization.Views
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

            // Setup tooltip
            var jsonLines = JsonConvert.SerializeObject(d.Source, Formatting.Indented).Split('\n').ToList();
            if (jsonLines.Count > 30)
            {
                jsonLines = jsonLines.Take(30).ToList();
                jsonLines.Add("--- CONTENT TRUNCATED ---");
            }
            ctrl.ToolTip = d.VertexType.ToString() + "\n" + string.Join("\n", jsonLines);
            System.Windows.Controls.ToolTipService.SetInitialShowDelay(ctrl, 0);

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
            return ctrl;
        }
    }
}
