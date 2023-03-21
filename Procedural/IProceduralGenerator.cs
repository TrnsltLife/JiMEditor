using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiME.Procedural
{
    public interface IProceduralGenerator<TGeneratorParameters> : IProceduralGenerator
    {
        TGeneratorParameters GetDefaultParameters();
        Scenario GenerateScenario(TGeneratorParameters parameters);
    }

    public interface IProceduralGenerator
    {
    }
}
