﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiME.Models
{
    public class Collection
    {
        public static readonly Collection CORE_SET = new Collection("Core Set", "Ring", "/JiME;component/Assets/Collection-White-Ring.png",
            new string[] { "Ruffian", "Goblin Scout", "Orc Hunter", "Orc Marauder", "Hungry Warg", "Hill Troll", "Wight" },
            new int[] { 0, 1, 2, 3, 4, 5, 6 }, //monsterId
            new int[] { 6, 6, 3, 6, 3, 1, 3 }, //monsterCount
            new int[] { 7, 4, 10, 9, 14, 25, 17 }, //monsterCost
            new int[] { 3, 3, 3, 3, 3, 1, 3 }, //groupLimit
            new int[] { 100, 101, 200, 201, 202, 203, 204, 205, 206, 207, 208, 209, 300, 301, 302, 303, 304, 305, 306, 307, 308, 400 }, //tileId
            false, false
        );

        public static readonly Collection VILLAINS_OF_ERIADOR = new Collection("Villains of Eriador", "Wolf", "/JiME;component/Assets/Collection-White-Wolf.png",
            new string[] { "Atarin", "Gulgotar", "Coalfang" },
            new int[] { 7, 8, 9 }, //monsterId
            new int[] { 1, 1, 1 }, //monsterCount
            new int[] { 14, 22, 28 }, //monsterCost
            new int[] { 1, 1, 1 }, //groupLimit
            new int[] {}, //tileId
            false, false
        );

        public static readonly Collection SHADOWED_PATHS = new Collection("Shadowed Paths", "Web", "/JiME;component/Assets/Collection-White-Web.png",
            new string[] { "Giant Spider", "Pit Goblin", "Orc Taskmaster", "Shadowman", "Nameless Thing", "Cave Troll", "Balrog", "Spawn of Ungoliant" },
            new int[] { 10, 11, 12, 13, 14, 15, 16, 17 }, //monsterId
            new int[] { 6, 6, 3, 3, 3, 2, 1, 1 }, //monsterCount
            new int[] { 5, 4, 14, 17, 27, 20, 50, 36 }, //monsterCost
            new int[] { 3, 3, 3, 3, 1, 2, 1, 1 }, //groupLimit
            new int[] { 102, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219, 220, 221, 309, 310, 311, 312, 313, 401, 402 }, //tileId
            true, false
        );

        public static readonly Collection DWELLERS_IN_DARKNESS = new Collection("Dwellers in Darkness", "Crown", "/JiME;component/Assets/Collection-White-Crown.png",
            new string[] { "Supplicant of Morgoth", "Ursa", "Ollie" },
            new int[] { 18, 19, 20 }, //monsterId
            new int[] { 34, 28, 40 }, //monsterCost
            new int[] { 1, 1, 1 }, //monsterCount
            new int[] { 1, 1, 1 }, //groupLimit
            new int[] { }, //tileId
            false, false
        );

        public static readonly Collection SPREADING_WAR = new Collection("Spreading War", "War", "/JiME;component/Assets/Collection-White-War.png",
            new string[] { "Fell Beast", "Warg Rider",  "Siege Engine", "War Oliphaunt", "Soldier", "Uruk Warrior", },
            new int[] { 21, 22, 23, 24, 25, 26, }, //monsterId
            new int[] { 3, 3, 2, 1, 6, 6, }, //monsterCost
            new int[] { 24, 14, 22, 30, 8, 11, }, //monsterCount
            new int[] { 1, 3, 1, 1, 3, 3, }, //groupLimit
            new int[] {  }, //tileId
            true, true
        );

        public static IEnumerable<Collection> Values
        {
            get
            {
                yield return CORE_SET;
                yield return VILLAINS_OF_ERIADOR;
                yield return SHADOWED_PATHS;
                yield return DWELLERS_IN_DARKNESS;
                yield return SPREADING_WAR;
            }
        }

        public string Name { get; private set; }
        public string Symbol { get; private set; }
        public string Icon { get; private set; }
        public string[] MonsterNames { get; private set; }
        public int[] MonsterIds { get; private set; }
        public int[] MonsterCosts { get; private set; }
        public int[] MonsterCounts { get; private set; }
        public int[] GroupLimits { get; private set; }
        public int[] TileNumbers { get; private set; }
        public Boolean DifficultGround { get; private set; }
        public Boolean Fortified { get; private set; }

        Collection(string name, string symbol, string icon, string[] monsterNames, int[] monsterIds, int[] monsterCosts, int[] monsterCounts, int[] groupLimits, int[] tileNumbers, Boolean difficultGround, Boolean fortified) =>
            (Name, Symbol, Icon, MonsterNames, MonsterIds, MonsterCosts, MonsterCounts, GroupLimits, TileNumbers, DifficultGround, Fortified) = 
            (name, symbol, icon, monsterNames, monsterIds, monsterCosts, monsterCounts, groupLimits, tileNumbers, difficultGround, fortified);

        public override string ToString() => Name;

        public static Collection FromName(string name)
        {
            switch (name)
            {
                case "Core Set":
                    return Collection.CORE_SET;
                case "Villains of Eriador":
                    return Collection.VILLAINS_OF_ERIADOR;
                case "Shadowed Paths":
                    return Collection.SHADOWED_PATHS;
                case "Dwellers in Darkness":
                    return Collection.DWELLERS_IN_DARKNESS;
                case "Spreading War":
                    return Collection.SPREADING_WAR;
                default:
                    throw new Exception("Collection not recognized:\r\n" + name);
            }
        }
    }
}