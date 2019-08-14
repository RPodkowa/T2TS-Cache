#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using DBDef;
using Thea2.Common;
using Thea2.General;
using TheHoney;
using Thea2.Server;
using System;
using Thea2.Client;
using System.Text;

namespace GameScript
{
    public class DebugScripts : ScriptBase
    {

        static public object DEV_AddRP(string data)
        {
            int c = 10;
            if (!string.IsNullOrEmpty(data))
            {
                try
                {
                    c = Convert.ToInt32(data);
                }
                catch
                {
                    Debug.LogError("[ERROR]data was not a number! " + data);
                }
            }
            var v = GameInstance.Get().GetPlayers().Find(o => o.ID == RootNetworkManager.GetMyPlayerID());

            if (v != null)
            {
                v.unlockPoints += c;
            }

            return null;
        }

        static public object DEV_AddResources(string data)
        {
            var v = GroupSelectionManager.Get().GetSelectedGroup();

            if (v == null)
            {
                Debug.LogError("[ERROR]this script requires selected group");
                return null;
            }

            int c = 10;
            if (!string.IsNullOrEmpty(data))
            {
                try
                {
                    c = Convert.ToInt32(data);
                }
                catch
                {
                    Debug.LogError("[ERROR]data was not a number! " + data);
                }
            }

            if (v != null && c > 0)
            {
                var group = EntityManager.Get<Thea2.Server.Group>(v.GetID());

                foreach (var r in ItemBase.resourceToItem)
                {
                    group.AddItem(r.Value.Clone<ItemResource>(true), c, true);
                }

                //request update / changes made to the group
                NOCSRequestGroupDetails.ServerSelfRequestOneGroup(v.GetID());
            }

            return null;
        }
        static public object DEV_AddToxines(string data)
        {
            var v = GroupSelectionManager.Get().GetSelectedGroup();

            if (v == null)
            {
                Debug.LogError("[ERROR]this script requires selected group");
                return null;
            }

            if (v != null)
            {
                var group = EntityManager.Get<Thea2.Server.Group>(v.GetID());
                if (group.characters == null) return null;

                SkillPack sp = Globals.GetInstanceFromDB<SkillPack>(SKILL_PACK.TOXINE);

                foreach (var c in group.characters)
                {
                    c.Get().AddSkillEffect(sp, false, 5);
                }

                //request update / changes made to the group
                NOCSRequestGroupDetails.ServerSelfRequestOneGroup(v.GetID());
            }

            return null;
        }
        static public object DEV_Add(string data)
        {
            var v = GroupSelectionManager.Get().GetSelectedGroup();

            if (v == null || string.IsNullOrEmpty(data))
            {
                Debug.LogError("[ERROR]this script requires selected group and data containing Item cargo ID or subrace");
                return null;
            }

            if (v != null)
            {
                var group = EntityManager.Get<Thea2.Server.Group>(v.GetID());
                if (group == null)
                {
                    Debug.LogError("[ERROR]Group not found");
                    return null;
                }

                for (int i = 0; i < 40; i++)
                {
                    var b = Globals.GetInstanceFromDB(data);

                    if (b is ItemCargo)
                    {
                        ItemCargo ic = b as ItemCargo;
                        group.AddItem(ItemBase.InstantaiteFrom(ic));
                    }
                    else if (b is Subrace)
                    {
                        Character.Instantiate(group, b as Subrace, 1);
                    }
                }
                //request update / changes made to the group
                NOCSRequestGroupDetails.ServerSelfRequestOneGroup(v.GetID());
            }

            return null;
        }

        static public object DEV_DebugGroupsPower(string data)
        {
            //parameter can be group database reference and result will be put into LOG 
            //or
            //parameter can be number which would tell how many times each group will be produced to csv file

            int c = 3;
            if (!string.IsNullOrEmpty(data))
            {
                DBDef.Group g = Globals.GetInstanceFromDB<DBDef.Group>(data);
                if (g != null)
                {
                    HoneyDebug.DebugGroups(g);
                    return null;
                }
                else
                {
                    try
                    {
                        c = Convert.ToInt32(data);
                    }
                    catch
                    {
                        Debug.LogError("[ERROR]data was not a number! " + data);
                    }
                }
            }

            HoneyDebug.DebugGroups(c);
            return null;
        }
        static public object DEV_DebugSelectedGroup(string data)
        {
            var v = GroupSelectionManager.Get().GetSelectedGroup();

            if (v == null)
            {
                Debug.LogError("[ERROR]this script requires selected group");
                return null;
            }

            Thea2.Server.Group g = EntityManager.Get<Thea2.Server.Group>(v.GetID());            

            HoneyDebug.DebugGroups(g);

            if(g != null && g.characters != null)
            {
                StringBuilder sb = new StringBuilder();

                foreach(var c in g.characters)
                {
                    Character ch = c.Get();
                    sb.AppendLine("Name: " +ch.name);

                    var d = ch.attributes.GetFinalDictionary();
                    foreach(var a in d)
                    {
                        sb.AppendLine(a.Key.Get().dbName + " : " + a.Value.ToString(true));
                    }

                    sb.AppendLine("-------------");
                }

                Debug.Log(sb.ToString());
            }

            return null;
        }


        static public object DEV_AddMP(string data)
        {
            var v = GroupSelectionManager.Get().GetSelectedGroup();

            if (v == null)
            {
                Debug.LogError("[ERROR]this script requires selected group");
                return null;
            }

            int c = 10;
            if (!string.IsNullOrEmpty(data))
            {
                try
                {
                    c = Convert.ToInt32(data);
                }
                catch
                {
                    Debug.LogError("[ERROR]data was not a number! " + data);
                }
            }

            if (v != null && c > 0)
            {
                var group = EntityManager.Get<Thea2.Server.Group>(v.GetID());
                if (group.characters == null) return null;


                Tag mp = (Tag)TAG.MOVEMENT_RANGE;
                //Tag mp2 = Globals.GetInstanceFromDB<Tag>("TAG-MOVEMENT_RANGE");   // alternative method to access Tag

                foreach (var character in group.characters)
                {

                    character.Get().attributes.AddToBase(mp, c, true);
                    character.Get().attributes.GetFinal(mp);
                }

                group.NewTurnRefresh();

                //request update / changes made to the group
                NOCSRequestGroupDetails.ServerSelfRequestOneGroup(v.GetID());
            }
            
            return null;
        }
        static public object DEV_HealGroup(string data)
        {
            var v = GroupSelectionManager.Get().GetSelectedGroup();

            if (v == null)
            {
                Debug.LogError("[ERROR]this script requires selected group");
                return null;
            }


            if (v != null)
            {
                var group = EntityManager.Get<Thea2.Server.Group>(v.GetID());
                if (group.characters == null) return null;
                

                foreach (var character in group.characters)
                {
                    character.Get().HealFully();
                }

                group.NewTurnRefresh();

                //request update / changes made to the group
                NOCSRequestGroupDetails.ServerSelfRequestOneGroup(v.GetID());
            }

            return null;
        }

        static public object DEV_AddXP(string data)
        {
            var v = GroupSelectionManager.Get().GetSelectedGroup();

            if (v == null)
            {
                Debug.LogError("[ERROR]this script requires selected group");
                return null;
            }

            int c = 10;
            if (!string.IsNullOrEmpty(data))
            {
                try
                {
                    c = Convert.ToInt32(data);
                }
                catch
                {
                    Debug.LogError("[ERROR]data was not a number! " + data);
                }
            }

            if (v != null && c > 0)
            {
                var group = EntityManager.Get<Thea2.Server.Group>(v.GetID());
                if (group.characters == null) return null;

                foreach (var character in group.characters)
                {
                    character.Get().Xp = character.Get().Xp + c;
                }

                //request update / changes made to the group
                NOCSRequestGroupDetails.ServerSelfRequestOneGroup(v.GetID());
            }
            return null;
        }
    }
}
#endif