﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiME
{
    public enum Slot { NONE, ARMOR, HAND, TRINKET, MOUNT };

    public enum ItemSeries
    {
        NONE = 0,


        //Armor
        CLOAK = 1,
        HOARY_COAT = 2,
        PADDED_ARMOR = 3,
        PLATE_ARMOR = 4,
        RING_MAIL = 5,
        TRAVEL_GARB = 6,

        //Custom Armor
        ANCIENT_BARK = 7,
        ROBES = 8,
        HAUBERK = 9,
        DRAGON_SCALES = 10,

        //Support
        BANNER = 100,
        HARP = 101,
        HORN = 102,
        SHIELD = 103,

        //Custom Support
        POUCH = 104,

        //Weapons
        BATTLE_AXE = 200,
        DAGGER = 201,
        GREAT_BOW = 202,
        HAMMER = 203,
        HATCHET = 204,
        KNIFE = 205,
        MACE = 206,
        RENDING_CLAWS = 207,
        SHORT_BOW = 208,
        SHORT_SWORD = 209,
        SLING = 210,
        SPEAR = 211,
        STAFF = 212,
        SWORD = 213,
        WALKING_STICK = 214,

        //Custom Weapons
        BROADSWORD = 215,
        CLUB = 216,
        CURVED_SWORD = 217,
        MIGHTY_LIMB = 218,
        ROCK = 219,
        ROD = 220,
        CROSSBOW = 221,
        DRAGON_CLAWS = 222,
        DRAGON_TAIL = 223,

        //Trinkets
        BOOTS = 300,
        BROOCH = 301,
        CIRCLET = 302,
        EXTRA_RATIONS = 303,
        FANG_PENDANT = 304,
        HAMMER_AND_TONGS = 305,
        HANDKERCHIEF = 306,
        HELMET = 307,
        OLD_MAP = 308,
        OLD_PIPE = 309,
        OLD_SCEPTER = 310,
        PROVISIONS = 311,
        ROPE = 312,
        THE_CROWN_OF_SHADOWS = 313,
        TOME = 314,
        TORCH = 315,
        WATERSKIN = 316,

        //Custom Trinkets
        HERBS_AND_POULTICES = 317,
        SEEING_STONE = 318,
        THE_ONE = 319,
        BLACK_ARROWS = 320,
        THRAINS_LEGACY_POUCH = 321,

        //Mounts
        FRIENDLY_PONY = 400,
        GRUMBLE_BUM = 401,
        MEADOW_HART = 402,
        PACK_MULE = 403,
        QUICKBEAM = 404,
        SNOWBRIGHT = 405,
        SWIFT_STEED = 406,
        TRAVELLERS_HORSE = 407,
        WAR_CHARGER = 408,
        WITNESS_OF_MARANWE = 409,

        //Custom Mounts
        NORTH_TOOK_STEED = 410,
        TRUSTED_STEED = 411
    };

    public class Item: INotifyPropertyChanged
    {
        public int _id;
        public int collection;
        public Slot slotId;
        public string slot;
        public ItemSeries seriesId;
        public string seriesName;
        public string _dataName;
        public string originalName;
        public int count;
        public int tier;
        public string[] stats;
        public string trait;
        public int upgrade;
        public int tokens;
        public int handed;
        public int ranged;

        public Item() { }

        public Item(int id)
        {
            this.id = id;
        }

        public int id
        {
            get => _id;
            set
            {
                if (value != _id)
                {
                    _id = value;
                    NotifyPropertyChanged("id");
                }
            }
        }

        public string dataName
        {
            get => _dataName;
            set
            {
                if (value != _dataName)
                {
                    _dataName = value;
                    NotifyPropertyChanged("dataName");
                }
            }
        }

        public override string ToString()
        {
            return dataName;
        }

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
