using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GraphX.Controls.Models;

namespace JiME.Visualization.Views
{
    /**
     * UIElement creation factory for DataEdge class
     */
    public class GraphEdgeLabelFactory : ILabelFactory<UIElement>
    {
        public IEnumerable<UIElement> CreateLabel<TCtrl>(TCtrl control)
        {
            throw new NotImplementedException();
        }
    }
}
