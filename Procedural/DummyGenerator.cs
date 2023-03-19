using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiME.Procedural
{
    class DummyGenerator : IProceduralGenerator
    {
        public Scenario GenerateScenario()
        {
            var s = new Scenario(); // <-- This creates the scenario with "defaults"
            s.scenarioName = "DummyScenario";

            return s;
        }
    }
}
