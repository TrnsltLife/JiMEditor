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
            var s = new Scenario("DummyScenario");

            s.AddInteraction(new DecisionInteraction("What to do?")
            { 
                   isThreeChoices = false,
                   choice1 = "Complete objective",
                   choice1Trigger = "Complete objective",
                   choice2 = "Fail objective",
                   choice2Trigger = "End In Failure"
            });

            s.AddTrigger("Complete objective");
            
            s.AddObjective(new Objective("DummyObjective") { triggerName = "Complete objective", nextTrigger = "End In Success" });
            s.objectiveName = s.objectiveObserver.Where(o => o.dataName != "None").First().dataName;

            s.AddTrigger("End In Success");
            s.AddTrigger("End In Failure");

            s.AddResolution(new TextBookData("Success!") { triggerName = "End In Success" } , true);
            s.AddResolution(new TextBookData("Failure!") { triggerName = "End In Failure" }, false);


            return s;
        }
    }
}
