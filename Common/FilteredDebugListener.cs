using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiME.Common
{
    public class FilteredDebugListener : DefaultTraceListener
    {
        public override void WriteLine(string message)
        {
            if (message != null && message.Contains("EdgeLabelControl_LayoutUpdated"))
                return;

            base.WriteLine(message);
        }

        public override void Write(string message)
        {
            if (message != null && message.Contains("EdgeLabelControl_LayoutUpdated"))
                return;

            base.Write(message);
        }
    }
}
