﻿using System.Collections.Generic;
using System.Linq;
using Intersect_Migration_Tool.UpgradeInstructions.Upgrade_7.Intersect_Convert_Lib.GameObjects.Conditions;
using Intersect_Migration_Tool.UpgradeInstructions.Upgrade_7.Intersect_Convert_Lib.GameObjects.Events;

namespace Intersect_Migration_Tool.UpgradeInstructions.Upgrade_7.Intersect_Convert_Lib.GameObjects
{
    public class ItemBase : DatabaseObject
    {
        public new const string DatabaseTable = "items";
        public new const GameObject Type = GameObject.Item;
        protected static Dictionary<int, DatabaseObject> Objects = new Dictionary<int, DatabaseObject>();

        public string Name = "New Item";
        public string Desc = "";
        public int ItemType;
        public string Pic = "";
        public int Price;
        public int Bound;
        public int Stackable;
        public int Animation;
        public int ClassReq = -1;
        public int LevelReq;
        public int Projectile = -1;
        public int[] StatsReq;
        public ConditionLists UseReqs = new ConditionLists();
        public int[] StatsGiven;
        public int GenderReq;
        public int StatGrowth;
        public int Damage;
        public int CritChance;
        public int DamageType;
        public int ScalingStat;
        public int Scaling;
        public int AttackAnimation = -1;
        public int Speed;
        public string MalePaperdoll = "";
        public string FemalePaperdoll = "";
        public int Tool = -1;
        public int Data1;
        public int Data2;
        public int Data3;
        public int Data4;

        public ItemBase(int id) : base(id)
        {
            Speed = 10; // Set to 10 by default.
            StatsReq = new int[Options.MaxStats];
            StatsGiven = new int[Options.MaxStats];
        }

        public override void Load(byte[] data)
        {
            var myBuffer = new ByteBuffer();
            myBuffer.WriteBytes(data);
            Name = myBuffer.ReadString();
            Desc = myBuffer.ReadString();
            ItemType = myBuffer.ReadInteger();
            Pic = myBuffer.ReadString();
            Price = myBuffer.ReadInteger();
            Bound = myBuffer.ReadInteger();
            Animation = myBuffer.ReadInteger();
            ClassReq = myBuffer.ReadInteger();
            LevelReq = myBuffer.ReadInteger();
            GenderReq = myBuffer.ReadInteger();
            Projectile = myBuffer.ReadInteger();
            AttackAnimation = myBuffer.ReadInteger();

            for (var i = 0; i < Options.MaxStats; i++)
            {
                StatsReq[i] = myBuffer.ReadInteger();
                StatsGiven[i] = myBuffer.ReadInteger();
            }

            StatGrowth = myBuffer.ReadInteger();
            Damage = myBuffer.ReadInteger();
            CritChance = myBuffer.ReadInteger();
            DamageType = myBuffer.ReadInteger();
            ScalingStat = myBuffer.ReadInteger();
            Scaling = myBuffer.ReadInteger();
            Speed = myBuffer.ReadInteger();
            MalePaperdoll = myBuffer.ReadString();
            FemalePaperdoll = myBuffer.ReadString();
            Tool = myBuffer.ReadInteger();
            Data1 = myBuffer.ReadInteger();
            Data2 = myBuffer.ReadInteger();
            Data3 = myBuffer.ReadInteger();
            Data4 = myBuffer.ReadInteger();

            //Convert our reqs into a command list.
            //Gotta do stats, gender, level, class, 
            var cndList = new ConditionList();
            cndList.Name = "Migrated Requirements";
            if (ClassReq > -1)
            {
                var req = new EventCommand();
                req.Type = EventCommandType.ConditionalBranch;
                req.Ints[0] = 5; //Class Is
                req.Ints[1] = ClassReq;
                cndList.Conditions.Add(req);
            }
            if (GenderReq > 0)
            {
                var req = new EventCommand();
                req.Type = EventCommandType.ConditionalBranch;
                req.Ints[0] = 16; //Gender Is
                req.Ints[1] = GenderReq -1;
                cndList.Conditions.Add(req);
            }
            if (LevelReq > 0)
            {
                var req = new EventCommand();
                req.Type = EventCommandType.ConditionalBranch;
                req.Ints[0] = 7; //Level or Stat is
                req.Ints[1] = 1; //Greater than or equal to
                req.Ints[2] = LevelReq; //Level To Compare
                req.Ints[3] = 0; //Level not stat
                cndList.Conditions.Add(req);
            }
            for (var i = 0; i < Options.MaxStats; i++)
            {
                if (StatsReq[i] > 0)
                {
                    var req = new EventCommand();
                    req.Type = EventCommandType.ConditionalBranch;
                    req.Ints[0] = 7; //Level or Stat is
                    req.Ints[1] = 1; //Greater than or equal to
                    req.Ints[2] = StatsReq[i]; //Value To Compare
                    req.Ints[3] = i+1; //Stat index
                    cndList.Conditions.Add(req);
                }
            }
            if (cndList.Conditions.Count > 0) UseReqs.Lists.Add(cndList);
            //Check if item type was stackable using the old conditions.. if so set stackable == 1
            if ((ItemType == (int)ItemTypes.Consumable || ItemType == (int)ItemTypes.Currency ||ItemType == (int)ItemTypes.None || ItemType == (int)ItemTypes.Spell))
            {
                Stackable = 1;
            }
        }

        public byte[] ItemData()
        {
            var myBuffer = new ByteBuffer();
            myBuffer.WriteString(Name);
            myBuffer.WriteString(Desc);
            myBuffer.WriteInteger(ItemType);
            myBuffer.WriteString(Pic);
            myBuffer.WriteInteger(Price);
            myBuffer.WriteInteger(Bound);
            myBuffer.WriteInteger(Stackable);
            myBuffer.WriteInteger(Animation);
            myBuffer.WriteInteger(Projectile);
            myBuffer.WriteInteger(AttackAnimation);

            UseReqs.Save(myBuffer);

            for (var i = 0; i < Options.MaxStats; i++)
            {
                myBuffer.WriteInteger(StatsGiven[i]);
            }

            myBuffer.WriteInteger(StatGrowth);
            myBuffer.WriteInteger(Damage);
            myBuffer.WriteInteger(CritChance);
            myBuffer.WriteInteger(DamageType);
            myBuffer.WriteInteger(ScalingStat);
            myBuffer.WriteInteger(Scaling);
            myBuffer.WriteInteger(Speed);
            myBuffer.WriteString(MalePaperdoll);
            myBuffer.WriteString(FemalePaperdoll);
            myBuffer.WriteInteger(Tool);
            myBuffer.WriteInteger(Data1);
            myBuffer.WriteInteger(Data2);
            myBuffer.WriteInteger(Data3);
            myBuffer.WriteInteger(Data4);
            return myBuffer.ToArray();
        }

        public static ItemBase GetItem(int index)
        {
            if (Objects.ContainsKey(index))
            {
                return (ItemBase)Objects[index];
            }
            return null;
        }

        public static string GetName(int index)
        {
            if (Objects.ContainsKey(index))
            {
                return ((ItemBase)Objects[index]).Name;
            }
            return "Deleted";
        }

        public bool IsStackable()
        {
            //Allow Stacking on Currency, Consumable, Spell, and item types of none.
            return (ItemType == (int) ItemTypes.Consumable ||
                    ItemType == (int) ItemTypes.Currency ||
                    ItemType == (int) ItemTypes.None ||
                    ItemType == (int) ItemTypes.Spell) && Stackable > 0;
        }

        public override byte[] GetData()
        {
            return ItemData();
        }

        public override string GetTable()
        {
            return DatabaseTable;
        }

        public override GameObject GetGameObjectType()
        {
            return Type;
        }

        public static DatabaseObject Get(int index)
        {
            if (Objects.ContainsKey(index))
            {
                return Objects[index];
            }
            return null;
        }
        public override void Delete()
        {
            Objects.Remove(GetId());
        }
        public static void ClearObjects()
        {
            Objects.Clear();
        }
        public static void AddObject(int index, DatabaseObject obj)
        {
            Objects.Remove(index);
            Objects.Add(index, obj);
        }
        public static int ObjectCount()
        {
            return Objects.Count;
        }
        public static Dictionary<int, ItemBase> GetObjects()
        {
            Dictionary<int, ItemBase> objects = Objects.ToDictionary(k => k.Key, v => (ItemBase)v.Value);
            return objects;
        }
    }
}

