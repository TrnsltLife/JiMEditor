using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiME.Procedural.StoryElements
{
    /// <summary>
    /// Dummy interaction for Procedural generator purposes
    /// </summary>
    class StoryPointInteraction : InteractionBase
    {
        public List<string> OtherAfterTriggers = new List<string>();

        public StoryPointInteraction(string dataName) : base(dataName)
        {
        }
    }
}
