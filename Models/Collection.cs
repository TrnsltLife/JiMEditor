﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiME.Models
{
    public class Collection
    {
        public static readonly Collection CORE_SET = new Collection(1, "Core Set", "r",
            //new string[] { "Ruffian", "Goblin Scout", "Orc Hunter", "Orc Marauder", "Hungry Warg", "Hill Troll", "Wight" },
            new Monster[] {new Monster(0), new Monster(1), new Monster(2), new Monster(3), new Monster(4), new Monster(5), new Monster(6)}, 
            new int[] { 100, 101, 200, 201, 202, 203, 204, 205, 206, 207, 208, 209, 300, 301, 302, 303, 304, 305, 306, 307, 308, 400 }, //tileId
            false, false
        );

        public static readonly Collection VILLAINS_OF_ERIADOR = new Collection(2, "Villains of Eriador", "v",
            //new string[] { "Atarin", "Gulgotar", "Coalfang" },
            new Monster[] {new Monster(7), new Monster(8), new Monster(9)},
            new int[] {}, //tileId
            false, false
        );

        public static readonly Collection SHADOWED_PATHS = new Collection(3, "Shadowed Paths", "p",
            //new string[] { "Giant Spider", "Pit Goblin", "Orc Taskmaster", "Shadowman", "Nameless Thing", "Cave Troll", "Balrog", "Spawn of Ungoliant" },
            new Monster[] {new Monster(10), new Monster(11), new Monster(12), new Monster(13), new Monster(14), new Monster(15), new Monster(16), new Monster(17)},
            new int[] { 102, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219, 220, 221, 309, 310, 311, 312, 313, 401, 402 }, //tileId
            true, false
        );

        public static readonly Collection DWELLERS_IN_DARKNESS = new Collection(4, "Dwellers in Darkness", "d",
            //new string[] { "Supplicant of Morgoth", "Ursa", "Ollie" },
            new Monster[] {new Monster(18), new Monster(19), new Monster(20)},
            new int[] { }, //tileId
            false, false
        );

        public static readonly Collection SPREADING_WAR = new Collection(5, "Spreading War", "w",
            //new string[] { "Fell Beast", "Warg Rider",  "Siege Engine", "War Oliphaunt", "Soldier", "Uruk Warrior", },
            new Monster[] {new Monster(21), new Monster(22), new Monster(23), new Monster(24), new Monster(25), new Monster(26)},
            new int[] {  }, //tileId
            true, true
        );

        public static readonly Collection SCOURGES_OF_THE_WASTES = new Collection(6, "Scourges of the Wastes", "c",
            //new string[] { "Lord Angon", "Witch-king of Angmar", "Eadris" },
            new Monster[] {new Monster(27), new Monster(28), new Monster(29)},
            new int[] { }, //tileId
            false, false
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
                yield return SCOURGES_OF_THE_WASTES;
            }
        }

        public static Monster[] _MONSTERS;
        public static Monster[] MONSTERS()
        {
            if(_MONSTERS == null)
            {
                _MONSTERS = (Collection.CORE_SET.Monsters)
                    .Concat(Collection.VILLAINS_OF_ERIADOR.Monsters).ToArray()
                    .Concat(Collection.SHADOWED_PATHS.Monsters).ToArray()
                    .Concat(Collection.DWELLERS_IN_DARKNESS.Monsters).ToArray()
                    .Concat(Collection.SPREADING_WAR.Monsters).ToArray()
                    .Concat(Collection.SCOURGES_OF_THE_WASTES.Monsters).ToArray();

                //Console.WriteLine("Init MONSTERS:");
                //foreach(var monster in _MONSTERS)
                //{
                //    Console.WriteLine(monster.id + " " + monster.health + " " + monster.dataName);
                //}
            }
            return _MONSTERS;
        }

        public int ID { get; private set; }
        public string Name { get; private set; }
        public string FontCharacter { get; private set; }
        //public string[] MonsterNames { get; private set; }
        public Monster[] Monsters { get; private set; }
        public int[] TileNumbers { get; private set; }
        public Boolean DifficultGround { get; private set; }
        public Boolean Fortified { get; private set; }

        Collection(int id, string name, string fontCharacter, Monster[] monsters, int[] tileNumbers, Boolean difficultGround, Boolean fortified) =>
            (ID, Name, FontCharacter, Monsters, TileNumbers, DifficultGround, Fortified) = 
            (id, name, fontCharacter, monsters, tileNumbers, difficultGround, fortified);

        public override string ToString() => Name;

        public static Collection FromID(int id)
        {
            switch (id)
            {
                case 1:
                    return Collection.CORE_SET;
                case 2:
                    return Collection.VILLAINS_OF_ERIADOR;
                case 3:
                    return Collection.SHADOWED_PATHS;
                case 4:
                    return Collection.DWELLERS_IN_DARKNESS;
                case 5:
                    return Collection.SPREADING_WAR;
                case 6:
                    return Collection.SCOURGES_OF_THE_WASTES;
                default:
                    throw new Exception("Collection not recognized: " + id);
            }
        }

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
                case "Scourges of the Wastes":
                    return Collection.SCOURGES_OF_THE_WASTES;
                default:
                    throw new Exception("Collection not recognized: " + name);
            }
        }

        public static Collection FromTileNumber(int tileId)
        {
            if(Collection.CORE_SET.TileNumbers.Contains(tileId)) { return Collection.CORE_SET; }
            else if(Collection.SHADOWED_PATHS.TileNumbers.Contains(tileId)) { return Collection.SHADOWED_PATHS; }
            else if (Collection.SPREADING_WAR.TileNumbers.Contains(tileId)) { return Collection.SPREADING_WAR; }
            return null;
        }
    }
}