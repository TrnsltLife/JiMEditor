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
    /// UIElement creation factory for DataVertex class
    /// </summary>
    public class GraphVertexLabelFactory : DefaultVertexlabelFactory
    {
        public override IEnumerable<AttachableVertexLabelControl> CreateLabel<TCtrl>(TCtrl control)
        {
            return base.CreateLabel(control);
        }
    }
}
