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
     * UIElement creation factory for DataVertex class
     */
    public class GraphVertexLabelFactory : ILabelFactory<UIElement>
    {
        public IEnumerable<UIElement> CreateLabel<TCtrl>(TCtrl control)
        {
            throw new NotImplementedException();
        }
    }
}
