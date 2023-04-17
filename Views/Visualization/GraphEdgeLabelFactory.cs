using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GraphX.Controls;
using GraphX.Controls.Models;

namespace JiME.Views
{
    /// <summary>
    /// UIElement creation factory for DataEdge class
    /// </summary>
    public class GraphEdgeLabelFactory : DefaultEdgelabelFactory
    {
        public override IEnumerable<AttachableEdgeLabelControl> CreateLabel<TCtrl>(TCtrl control)
        {
            var edgeLabels = base.CreateLabel(control);
            foreach (var edgeLabel in edgeLabels)
            {
                // Reduce the font size so that the edge labels are more distinguishable from vertex labels
                edgeLabel.FontSize = 7; // Default is 12

                // Rotate edge labels to the edge arrow
                edgeLabel.AlignToEdge = true;

                // Label visibility is controlled by the label text
                var edgeText = (edgeLabel.AttachNode?.Edge as Visualization.DataEdge)?.Text;
                if (edgeText?.Length > 0)
                {
                    edgeLabel.Visibility = Visibility.Visible;
                    edgeLabel.ToolTip = edgeText; // <-- Also show as tooltip since the edge arrow already has the same tooltip
                }
                else
                {
                    edgeLabel.Visibility = Visibility.Hidden;
                }
            }
            return edgeLabels;
        }
    }
}
