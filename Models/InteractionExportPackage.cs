using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiME
{
    public class MonsterActivationInfo
    {
        public int id;
        public string name;

        public MonsterActivationInfo() { }
        public MonsterActivationInfo(int id, string name)
        {
            this.id = id;
            this.name = name;
        }
    }

    public class MonsterModifierInfo
    {
        public int id;
        public string name;

        public MonsterModifierInfo() { }
        public MonsterModifierInfo(int id, string name)
        {
            this.id = id;
            this.name = name;
        }
    }

    public class InteractionExportPackage
    {
        public string version = Utils.formatVersion;
        public InteractionBase interaction;
        public List<Translation> translations;
        public List<MonsterActivationInfo> activationsReference;
        public List<MonsterModifierInfo> modifiersReference;
    }
}
