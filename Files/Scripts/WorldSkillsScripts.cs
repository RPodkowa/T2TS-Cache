#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using DBDef;
using Thea2.Common;
using Thea2.General;
using TheHoney;
using Thea2.Server;
using System;

namespace GameScript
{
    public class WorldSkillsScripts : ScriptBase
    {
        #region ACT - Activation (Passive Scripts), WACT - character skills, GACT - group skills (eg buildings)
        #region WACT - character skills
        /// <summary>
        /// Basic method to add or multiply
        /// </summary>
        /// <param name="skillAttributes"> attributes of the subskill </param>
        /// <param name="addons"> additions to the tags/attributes of the character </param>
        /// <param name="multipliers"> multiplications to the tags/attributes of the character </param>        
        /// <param name="changeID"> ID of the skill or item which influenced change </param>
        /// <param name="changeLog"> changelog for attribute detailed changes </param>
        /// <returns></returns>
        static public object WAct_AttributeChange(InterpolatedSkillAttributes skillAttributes,
                                                Dictionary<Tag, FInt> addons, 
                                                Dictionary<Tag, float> multipliers,
                                                Int64 changeID,
                                                List<Multitype<Int64, Tag, FInt, bool>> changeLog,
                                                Character c)
        {
            if (skillAttributes.attributes != null)
            {
                foreach(var v in skillAttributes.attributes)
                {                    
                    
                    Tag t = Globals.GetInstanceFromDB<Tag>(v.Key);
                    if (t != null)
                    {
                        bool additive = v.Value.t0 == "+";
                        ApplyChange(additive, t, v.Value.t1, addons, multipliers, changeID, changeLog);
                    }                    
                }                                
            }            

            return null;
        }
        static public object WAct_ShieldChange(InterpolatedSkillAttributes skillAttributes,
                                                Dictionary<Tag, FInt> addons,
                                                Dictionary<Tag, float> multipliers,
                                                Int64 changeID,
                                                List<Multitype<Int64, Tag, FInt, bool>> changeLog,
                                                Character c)
        {
            if (skillAttributes.attributes != null)
            {                
                foreach (var v in skillAttributes.attributes)
                {
                    Tag t = Globals.GetInstanceFromDB<Tag>(v.Key);
                    if (t != null)
                    {
                        bool allShield = t == (Tag)TAG.CA_SHIELD;
                        bool additive = v.Value.t0 == "+";
                        
                        if (allShield)
                        {
                            ApplyChange(additive, (Tag)TAG.SHIELDING_PHYSICAL, v.Value.t1, addons, multipliers, changeID, changeLog);
                            ApplyChange(additive, (Tag)TAG.SHIELDING_MENTAL, v.Value.t1, addons, multipliers, changeID, changeLog);
                            ApplyChange(additive, (Tag)TAG.SHIELDING_SPIRIT, v.Value.t1, addons, multipliers, changeID, changeLog);
                        }
                        else
                        {
                            ApplyChange(additive, t, v.Value.t1, addons, multipliers, changeID, changeLog);
                        }
                        
                    }
                }                                
            }

            return null;
        }

        static public object WAct_NightAttributeChange(InterpolatedSkillAttributes skillAttributes,
                                                Dictionary<Tag, FInt> addons,
                                                Dictionary<Tag, float> multipliers,
                                                Int64 changeID,
                                                List<Multitype<Int64, Tag, FInt, bool>> changeLog,
                                                Character c
                                                )
        {
            if (!GameInstance.Get().sharedAttributes.Contains(TAG.NIGHT))
            {
                return null;
            }

            return WAct_AttributeChange(skillAttributes, addons, multipliers, changeID, changeLog, c);
        }

        static public object WAct_DayAttributeChange(InterpolatedSkillAttributes skillAttributes,
                                                Dictionary<Tag, FInt> addons,
                                                Dictionary<Tag, float> multipliers,
                                                Int64 changeID,
                                                List<Multitype<Int64, Tag, FInt, bool>> changeLog,
                                                Character c
                                                )
        {
            if (!GameInstance.Get().sharedAttributes.Contains(TAG.DAY))
            {
                return null;
            }

            return WAct_AttributeChange(skillAttributes, addons, multipliers, changeID, changeLog, c);
        }
        static public object WAct_DievannaAnimalPath(InterpolatedSkillAttributes skillAttributes,
                                                Dictionary<Tag, FInt> addons,
                                                Dictionary<Tag, float> multipliers,
                                                Int64 changeID,
                                                List<Multitype<Int64, Tag, FInt, bool>> changeLog,
                                                Character c)
        {
            if (c.subrace.Get().race == (Race)RACE.BEAST ||
                c.subrace.Get().race == (Race)RACE.ELF ||
                c.attributes.GetBase(TAG.FOREST_DEMON) > 0)
            {
                if (skillAttributes.attributes != null)
                {
                    foreach (var v in skillAttributes.attributes)
                    {
                        Tag t = Globals.GetInstanceFromDB<Tag>(v.Key);
                        if (t != null)
                        {
                            bool additive = v.Value.t0 == "+";
                            ApplyChange(additive, t, v.Value.t1, addons, multipliers, changeID, changeLog);
                        }
                    }
                }
            }
            return null;
        }
        static public object WAct_SmallWeaponHate(InterpolatedSkillAttributes skillAttributes,
                                                Dictionary<Tag, FInt> addons,
                                                Dictionary<Tag, float> multipliers,
                                                Int64 changeID,
                                                List<Multitype<Int64, Tag, FInt, bool>> changeLog,
                                                Character c
                                                )
        {
            if (skillAttributes.attributes != null)
            {
                ItemCraftedEquipment lHand = c.GetEquipment(EInventorySlot.LeftHand);
                ItemCraftedEquipment rHand = c.GetEquipment(EInventorySlot.RightHand);
                if (lHand != rHand)
                {
                    WAct_ShieldChange(skillAttributes, addons, multipliers, changeID, changeLog, c);
                }
            }
            return null;
        }
        #endregion
        #region GACT - group skills (eg buildings)
        static public object GAct_SeaMovement(CountEntityBase source, InterpolatedSkillAttributes skillAttributes, Thea2.Server.Group target)
        {
            //if (World.IsLand(target.position)) return null;

            foreach (var v in skillAttributes.attributes)
            {
                if("TAG-SEA_MOVEMENT_RANGE" == v.Key)
                {
                    
                    if (target.seaMaxMP == 0)
                    {
                        //if group have multiple not calculated its MP it would use speed from this skill
                        target.seaMaxMP = v.Value.t1;
                    }
                    else
                    {
                        //if group have multiple sources producing sea speed, only smallest would be applied
                        target.seaMaxMP = FInt.Min(target.seaMaxMP, v.Value.t1);
                    }
                }
            }
            return null;
        }
        static public object GAct_GatheringRange(CountEntityBase source, InterpolatedSkillAttributes skillAttributes, Thea2.Server.Group target)
        {
            foreach (var v in skillAttributes.attributes)
            {
                if ("GatheringRange" == v.Key)
                {
                    //target.gatheringRange = FInt.Max(target.gatheringRange, v.Value.t1);  // get bigger of "base" and modifier
                    target.gatheringRange += v.Value.t1;                                    // "base" + modifier
                }
            }
            return null;
        }
        static public object GAct_SightRange(CountEntityBase source, InterpolatedSkillAttributes skillAttributes, Thea2.Server.Group target)
        {
            foreach (var v in skillAttributes.attributes)
            {
                if ("SightRange" == v.Key)
                {
                    target.viewRangeBonus = v.Value.t1;
                }
            }
            return null;
        }
        static public object GAct_ExtraCraftingSlot(CountEntityBase source, InterpolatedSkillAttributes skillAttributes, Thea2.Server.Group target)
        {
            foreach (var v in skillAttributes.attributes)
            {
                if ("ExtraCraftingSlot" == v.Key)
                {
                    target.maxCraftingWorkersPerTask += v.Value.t1;
                }
            }
            return null;
        }
        static public object GAct_ExtraGatheringSlot(CountEntityBase source, InterpolatedSkillAttributes skillAttributes, Thea2.Server.Group target)
        {
            foreach (var v in skillAttributes.attributes)
            {
                if ("ExtraGatheringSlot" == v.Key)
                {
                    target.maxGatheringWorkersPerTask += v.Value.t1;
                }
            }
            return null;
        }
        static public object GAct_ExtraResearchSlot(CountEntityBase source, InterpolatedSkillAttributes skillAttributes, Thea2.Server.Group target)
        {
            foreach (var v in skillAttributes.attributes)
            {
                if ("ExtraReasearchSlot" == v.Key)
                {
                    target.maxResearchWorkersPerTask += v.Value.t1;
                }
            }
            return null;
        }
        static public object GAct_ExtraRitualsSlot(CountEntityBase source, InterpolatedSkillAttributes skillAttributes, Thea2.Server.Group target)
        {
            foreach (var v in skillAttributes.attributes)
            {
                if ("ExtraRitualsSlot" == v.Key)
                {
                    target.maxRitualsWorkersPerTask += v.Value.t1;
                }
            }
            return null;
        }
        static public object GAct_ExtraSmallRp(CountEntityBase source, InterpolatedSkillAttributes skillAttributes, Thea2.Server.Group target)
        {
            SPlayer p = GameInstance.Get().GetPlayer(target.ownerID);

            if (p != null)
            {
                //Apply bonus only for the group which contains building
                if (target != source.item.Get().GetGroup()) return null;

                foreach (var v in skillAttributes.attributes)
                {
                    if ("ExtraEssence" == v.Key)
                    {
                        p.researchProgress += v.Value.t1;
                    }
                }
            }

            return null;
        }
        static public object GAct_GroupWaterCarry(CountEntityBase source, InterpolatedSkillAttributes skillAttributes, Thea2.Server.Group target)
        {            
            //Apply bonus only for the group which contains building
            if (target != source.item.Get().GetGroup()) return null;

            foreach (var v in skillAttributes.attributes)
            {
                if ("TAG-GROUP_CARRY_LIMIT" == v.Key)
                {
                    // sources which increase water carry limit influence carry by adding to each other(eg multiple ships :P)
                    target.seaCarryLimit += v.Value.t1;                    
                }
            }
            return null;
        }
        #endregion
        #endregion
        #region ACTA - Activation (Non passive Scripts), WACTA - character skills, GACTA - group skills (eg buildings)
        #region WACTA - character skills
        static public object WActA_AttributeChange(InterpolatedSkillAttributes skillAttributes,                                                
                                                SkillInstance si,
                                                Subskill ss,
                                                Character owner
                                                )
        {            
            if (skillAttributes.attributes != null)
            {
                foreach (var v in skillAttributes.attributes)
                {
                    if (v.Value.t0 == "+")
                    {
                        Tag t = Globals.GetInstanceFromDB<Tag>(v.Key);
                        if (t != null)
                        {
                            owner.attributes.AddToBase(t, v.Value.t1);                            
                        }
                    }
                    else if (v.Value.t0 == "*")
                    {
                        Tag t = Globals.GetInstanceFromDB<Tag>(v.Key);
                        if (t != null)
                        {
                            FInt was = owner.attributes.GetBase(t);
                            owner.attributes.SetBaseTo(t, was * v.Value.t1 * 0.01f);                            
                        }
                    }
                    else
                    {
                        Debug.LogError("[ERROR]Skill contained attribute which is neither additive nor multiplicative for attribute change!");
                    }
                }
            }

            return null;
        }
        static public object WActA_NightDamageOrHeal(InterpolatedSkillAttributes skillAttributes,
                                                SkillInstance si,
                                                Subskill ss,
                                                Character owner
                                                )
        {
            if (!GameInstance.Get().sharedAttributes.Contains(TAG.NIGHT))
            {
                return null;
            }

            return WActA_DamageOrHeal(skillAttributes, si, ss, owner);
        }
        static public object WActA_DayDamageOrHeal(InterpolatedSkillAttributes skillAttributes,
                                                SkillInstance si,
                                                Subskill ss,
                                                Character owner
                                                )
        {
            if (!GameInstance.Get().sharedAttributes.Contains(TAG.DAY))
            {
                return null;
            }

            return WActA_DamageOrHeal(skillAttributes, si, ss, owner);
        }
        static public object WActA_DamageOrHeal(InterpolatedSkillAttributes skillAttributes,
                                                SkillInstance si,
                                                Subskill ss,
                                                Character owner
                                              )
        {
            if (skillAttributes.attributes != null)
            {
                foreach (var v in skillAttributes.attributes)
                {
                    Tag t = Globals.GetInstanceFromDB<Tag>(v.Key);
                    bool add = v.Value.t0 == "+";
                    bool percent = v.Value.t0 == "%";                    

                    FInt cur = FInt.ZERO;
                    FInt max = FInt.ZERO;

                    if (t == (Tag)TAG.HEALTH_PHYSICAL)
                    {
                        cur = owner.attributes.GetBase(TAG.HEALTH_PHYSICAL);
                        max = owner.attributes.GetFinal(TAG.MAX_HEALTH_PHYSICAL);
                    }
                    else if (t == (Tag)TAG.HEALTH_MENTAL)
                    {
                        cur = owner.attributes.GetBase(TAG.HEALTH_MENTAL);
                        max = owner.attributes.GetFinal(TAG.MAX_HEALTH_MENTAL);
                    }
                    else if (t == (Tag)TAG.HEALTH_SPIRIT)
                    {
                        cur = owner.attributes.GetBase(TAG.HEALTH_SPIRIT);
                        max = owner.attributes.GetFinal(TAG.MAX_HEALTH_SPIRIT);
                    }

                    if (add)
                    {
                        FInt healValue = FInt.Min(max, cur + v.Value.t1);
                        owner.attributes.SetBaseTo(t, healValue);
                    }
                    else if (percent)
                    {
                        FInt healValue = FInt.Min(max, cur + (max * v.Value.t1) / 100f);
                        owner.attributes.AddToBase(t, healValue);
                    }
                }
            }

            return null;
        }
        static public object WActA_Damage(InterpolatedSkillAttributes skillAttributes,
                                                SkillInstance si,
                                                Subskill ss,
                                                Character owner
                                              )
        {
            if (skillAttributes.attributes != null)
            {
                foreach (var v in skillAttributes.attributes)
                {
                    Tag t = Globals.GetInstanceFromDB<Tag>(v.Key);
                    bool add = v.Value.t0 == "+";
                    bool percent = v.Value.t0 == "*";                    

                    FInt cur = FInt.ZERO;
                    FInt max = FInt.ZERO;

                    if (t == (Tag)TAG.HEALTH_PHYSICAL)
                    {
                        cur = owner.attributes.GetBase(TAG.HEALTH_PHYSICAL);
                        max = owner.attributes.GetFinal(TAG.MAX_HEALTH_PHYSICAL);
                    }
                    else if (t == (Tag)TAG.HEALTH_MENTAL)
                    {
                        cur = owner.attributes.GetBase(TAG.HEALTH_MENTAL);
                        max = owner.attributes.GetFinal(TAG.MAX_HEALTH_MENTAL);
                    }
                    else if (t == (Tag)TAG.HEALTH_SPIRIT)
                    {
                        cur = owner.attributes.GetBase(TAG.HEALTH_SPIRIT);
                        max = owner.attributes.GetFinal(TAG.MAX_HEALTH_SPIRIT);
                    }

                    if (add)
                    {
                        FInt healValue = FInt.Min(max, cur - v.Value.t1);
                        owner.attributes.SetBaseTo(t, healValue);
                    }
                    else if (percent)
                    {
                        FInt healValue = FInt.Min(max, cur - (max * v.Value.t1) / 100f);
                        owner.attributes.SetBaseTo(t, healValue);
                    }
                }
            }

            return null;
        }
        static public object WActA_HealGroup(InterpolatedSkillAttributes skillAttributes,
                                                SkillInstance si,
                                                Subskill ss,
                                                Character owner
                                              )
        {
            var group = owner.group.Get();

            foreach (var c in group.GetCharactersAsList())
            {
                if (c.attributes.GetBase(TAG.HEALTH_PHYSICAL) < c.GetPhysicalHealthMAX() ||
                    c.attributes.GetBase(TAG.HEALTH_MENTAL) < c.GetMentalHealthMAX() ||
                    c.attributes.GetBase(TAG.HEALTH_SPIRIT) < c.GetSpiritHealthMAX())
                {
                    WActA_DamageOrHeal(skillAttributes, si, ss, c);
                }
            }

            return null;
        }
        static public object WActA_StrigaBite(InterpolatedSkillAttributes skillAttributes,
                                                SkillInstance si,
                                                Subskill ss,
                                                Character owner
                                              )
        {
            Thea2.Server.Group g = owner.GetGroup();

            if (skillAttributes.attributes != null && g != null)
            {
                FInt value = FInt.ZERO;
                foreach (var v in skillAttributes.attributes)
                {
                    if (v.Key == "StrigaBite")
                    {
                        value = v.Value.t1;
                    }
                }

                List<Character> chList = new List<Character>();
                foreach (Character ch in g.GetCharactersAsList())
                {
                    if (!ch.attributes.Contains(TAG.STRIGA))
                    {
                        chList.Add(ch);
                    }
                }

                if (chList.Count > 0)
                {
                    int r = UnityEngine.Random.Range(0, chList.Count - 1);

                    chList[r].attributes.SetBaseTo((TAG.HEALTH_PHYSICAL), chList[r].attributes.GetBase(TAG.HEALTH_PHYSICAL) - value);
                }
            }
            
            return null;

        }
        static public object WActA_Foodie(InterpolatedSkillAttributes skillAttributes,
                                                SkillInstance si,
                                                Subskill ss,
                                                Character owner
                                              )
        {
            Thea2.Server.Group g = owner.GetGroup();

            if (skillAttributes.attributes != null && g != null)
            {
                FInt value = FInt.ZERO;
                foreach (var v in skillAttributes.attributes)
                {
                    if (v.Key == "Foodie")
                    {
                        value = v.Value.t1;
                    }
                }
                
                int r = UnityEngine.Random.Range(0, 100);
                if(r <= value)
                {
                    var cebsFood = g.items.FindAll(o => g.GetForbidden().IsAllowedFood(o.GetItem()) &&
                                                        o.GetItem().attributes.Contains(TAG.FOOD));

                    if (cebsFood.Count > 0)
                    {
                        int indexX = UnityEngine.Random.Range(0, cebsFood.Count);
                        if (cebsFood[indexX].count > 1)
                        {
                            cebsFood[indexX].count--;
                        }
                        else
                        {
                            g.RemoveItem(cebsFood[indexX].GetItem(), 1);
                            cebsFood.RemoveAt(indexX);
                        }
                    }
                }
            }
            
            return null;
        }
        static public object WActA_Greed(InterpolatedSkillAttributes skillAttributes,
                                                SkillInstance si,
                                                Subskill ss,
                                                Character owner
                                              )
        {
            Thea2.Server.Group g = owner.GetGroup();

            if (skillAttributes.attributes != null && g != null)
            {
                FInt value = FInt.ZERO;
                foreach (var v in skillAttributes.attributes)
                {
                    if (v.Key == "Greed")
                    {
                        value = v.Value.t1;
                    }
                }
                
                var cebsMetal = g.items.FindAll(o => o.GetItem().attributes.Contains(TAG.RESOURCE) && o.GetItem().attributes.Contains(TAG.IRON) ||
                                                     o.GetItem().attributes.Contains(TAG.RESOURCE) && o.GetItem().attributes.Contains(TAG.STEEL));

                if (cebsMetal.Count > 0)
                {
                    cebsMetal.Sort(delegate (CountEntityBase a, CountEntityBase b)
                    {
                        return a.GetItem().rarity.CompareTo(b.GetItem().rarity);
                    });

                    int indexX = 0;
                    if (cebsMetal[indexX].count > value)
                    {
                        cebsMetal[indexX].count -= value.ToInt();
                    }
                    else
                    {
                        g.RemoveItem(cebsMetal[indexX].GetItem(), value.ToInt());
                    }

                    Deg_SimpleCountdown(si, owner, false);
                }
            }
            
            return null;
        }
        static public object WActA_NaturePurity(InterpolatedSkillAttributes skillAttributes,
                                                SkillInstance si,
                                                Subskill ss,
                                                Character owner
                                              )
        {
            foreach (var v in skillAttributes.attributes)
            {
                Tag t = Globals.GetInstanceFromDB<Tag>(v.Key);
                if (t != null)
                {
                    if (!owner.attributes.Contains(t, FInt.ONE))
                    {
                        owner.attributes.AddToBase(t, v.Value.t1);
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            for (int i = 0; i < (int)EInventorySlot.None; i++)
            {
                EInventorySlot slot = (EInventorySlot)i;
                TAG t = TagUtils.inventorySlotToBlockTag[slot];
                FInt f = owner.attributes.GetBase(Globals.GetInstanceFromDB<Tag>(t));

                if(f != FInt.ZERO && owner.GetEquipment(slot) != null)
                {
                    owner.UnEquip(owner.GetEquipment(slot), true);
                }
            }

            return null;
        }
        static public object WActA_UnlockBlockedSlots(InterpolatedSkillAttributes skillAttributes,
                                                SkillInstance si,
                                                Subskill ss,
                                                Character owner
                                              )
        {
            for (int i = 0; i < (int)EInventorySlot.None; i++)
            {
                EInventorySlot slot = (EInventorySlot)i;
                TAG t = TagUtils.inventorySlotToBlockTag[slot];
                FInt f = owner.attributes.GetBase(Globals.GetInstanceFromDB<Tag>(t));
                bool blockediInSubrace = false;

                if (f != FInt.ZERO)
                {
                    if (owner.subrace.Get().blockedItemSlot != null)
                    {
                        foreach (var y in owner.subrace.Get().blockedItemSlot)
                        {
                            if (y == (Tag)t)
                            {
                                blockediInSubrace = true;
                                break;
                            }        
                        }

                        if(!blockediInSubrace)
                        {
                            owner.attributes.SetBaseTo(t, FInt.ZERO);
                        }
                    }
                    else
                    {
                        owner.attributes.SetBaseTo(t, FInt.ZERO);
                    }
                }    
            }
                
            return null;
        }
        static public object WActA_GrowUp(InterpolatedSkillAttributes skillAttributes,
                                                SkillInstance si,
                                                Subskill ss,
                                                Character owner)
        {
            if (owner == null)
            {
                Debug.LogError("[ERROR]Grow up is missing character!");
            }

            CountedTag ct = new CountedTag();
            ct.tag = (Tag)TAG.GROWING_UP;
            ct.amount = 1f;

            owner.attributes.AddToBase(ct);

            return null;
        }
        static public object WActA_AddDeathCharacterTag(InterpolatedSkillAttributes skillAttributes,
                                               SkillInstance si,
                                               Subskill ss,
                                               Character owner)
        {
            if (owner == null)
            {
                Debug.LogError("[ERROR]Grow up is missing character!");
            }
            
            owner.attributes.AddToBase((Tag)TAG.DEATH_CHARACTER, FInt.ONE);

            return null;
        }
        static public object WActA_AddXp(InterpolatedSkillAttributes skillAttributes,
                                                SkillInstance si,
                                                Subskill ss,
                                                Character owner)
        {
            if (owner == null)
            {
                Debug.LogError("[ERROR] WActA_AddXp is missing character!");
            }

            FInt value = FInt.ZERO;

            foreach (var v in skillAttributes.attributes)
            {
                if ("AddXp" == v.Key)
                {
                    value = v.Value.t1;
                }
            }

            if(value != FInt.ZERO)
            {
                owner.AddXP(value);
            }

            return null;
        }
        static public object WActA_AddItemCargo(InterpolatedSkillAttributes skillAttributes,
                                                SkillInstance si,
                                                Subskill ss,
                                                Character owner)
        {
            if (owner == null)
            {
                Debug.LogError("[ERROR] WActA_AddItemCargo is missing character!");
            }
            Thea2.Server.Group target = owner.GetGroup();

            if (skillAttributes.attributes != null && target != null)
            {
                foreach (var v in skillAttributes.attributes)
                {
                    if (v.Key == "ItemCargo")
                    {
                        DBClass t = Globals.GetInstanceFromDB(v.Value.t0);
                        if (t != null)
                        {
                            Type tp = t.GetType();

                            if (tp == typeof(ItemCargo))
                            {
                                CountEntityBase ceb = ItemBase.InstantaiteFrom(t as ItemCargo);
                                int min = (t as ItemCargo).numberOfItems.minimumCount;
                                int max = (t as ItemCargo).numberOfItems.maximumCount;

                                int count = max;
                                if (max != min)
                                {
                                    //excluding max forces random param to be bigger by 1
                                    count = UnityEngine.Random.Range(min, max + 1);
                                }
                                ceb.count = count;

                                target.AddItem(ceb);
                            }
                        }
                    }
                }
            }

            return null;
        }
        static public object WActA_DemonBind(InterpolatedSkillAttributes skillAttributes,
                                                SkillInstance si,
                                                Subskill ss,
                                                Character owner)
        {
            if (owner == null)
            {
                Debug.LogError("[ERROR]WActA_DemonBind is missing character!");
            }

            FInt value = FInt.ZERO;

            if (owner.superConnection == null || !owner.superConnection.Valid())
            {
                owner.Destroy();
            }

            return null;
        }
        static public object WActA_ExtraSkillDawn(InterpolatedSkillAttributes skillAttributes,
                                                SkillInstance si,
                                                Subskill ss,
                                                Character owner)
        {
            int turn = ServerManager.GetTurnManager().tunrIndex;

            if(turn % 18 == 1)
            {
                WActA_ExtraSkill(skillAttributes, si, ss, owner);
            }
            return null;
        }
        static public object WActA_ExtraSkill(InterpolatedSkillAttributes skillAttributes,
                                                SkillInstance si,
                                                Subskill ss,
                                                Character owner)
        {
            String skillName = null;
            FInt lvl = FInt.ZERO;
           
            foreach (var v in skillAttributes.attributes)
            {
                if ("NewSkill" == v.Key)
                {
                    skillName = v.Value.t0;
                    lvl = (FInt)v.Value.t1;
                }
            }

            if (lvl > 0)
            {
                owner.LearnSkill(Character.SkillCategory.Effect, (Skill)skillName, lvl);
            }

            return null;
        }
        static public object WActA_ExtraSkillNewDay(InterpolatedSkillAttributes skillAttributes,
                                                SkillInstance si,
                                                Subskill ss,
                                                Character owner)
        {
            String skillName = "SKILL-NEW_DAY";
            FInt lvl = FInt.ONE;

            owner.LearnSkill(Character.SkillCategory.Effect, (Skill)skillName, lvl);

            return null;
        }
        static public object WActA_ExtraSkillEternalLight(InterpolatedSkillAttributes skillAttributes,
                                                SkillInstance si,
                                                Subskill ss,
                                                Character owner)
        {
            String skillName = "SKILL-ETERNAL_LIGHT";
            FInt lvl = FInt.ONE;

            owner.LearnSkill(Character.SkillCategory.Effect, (Skill)skillName, lvl);

            return null;
        }
            #endregion

            #region GACTA - group skills (eg buildings)
            static public object GActA_ExtraXp(CountEntityBase source, InterpolatedSkillAttributes skillAttributes, Thea2.Server.Group target)
        {
            FInt value = FInt.ZERO;

            //Apply bonus only for the group which contains building
            if (target != source.item.Get().GetGroup()) return null;

            foreach (var v in skillAttributes.attributes)
            {
                if ("AddXp" == v.Key)
                {
                    value = v.Value.t1;
                }
            }

            foreach (var c in target.characters)
            {
                if (value != null)
                {
                    c.Get().AddXP(value);
                }
            }

            return null;
        }
        static public object GActA_ExtraSmallRp(CountEntityBase source, InterpolatedSkillAttributes skillAttributes, Thea2.Server.Group target)
        {
            SPlayer p = GameInstance.Get().GetPlayer(target.ownerID);

            if (p != null)
            {
                //Apply bonus only for the group which contains building
                if (target != source.item.Get().GetGroup()) return null;

                foreach (var v in skillAttributes.attributes)
                {
                    if ("AddRp" == v.Key)
                    {
                        p.researchProgress += v.Value.t1;
                    }
                }
            }

            return null;
        }
        static public object GActA_MoraleTreshold(CountEntityBase source, InterpolatedSkillAttributes skillAttributes, Thea2.Server.Group target)
        {
            FInt treshold = FInt.ZERO;

            foreach (var v in skillAttributes.attributes)
            {
                if ("MoraleTreshold" == v.Key)
                {
                    treshold = v.Value.t1;
                }
            }

            foreach (Character c in target.characters)
            {
                if (c.attributes.GetFinal(TAG.MORALE) >= treshold)
                {
                    continue;
                }
                else
                {
                    c.attributes.SetBaseTo(TAG.MORALE, treshold);
                }
            }

            return null;
        }
        static public object GActA_ExtraSkill(CountEntityBase source, InterpolatedSkillAttributes skillAttributes, Thea2.Server.Group target)
        {
            String skillName = null;
            FInt lvl = FInt.ZERO;
            SkillInstance si = new SkillInstance();

            foreach (var v in skillAttributes.attributes)
            {
                if ("NewSkill" == v.Key)
                {
                    skillName = v.Value.t0;
                    lvl = (FInt)v.Value.t1;
                }
            }

            if (lvl > 0)
            {
                foreach (Character c in target.characters)
                {

                    c.LearnSkill(Character.SkillCategory.Effect, (Skill)skillName, lvl);
                }
            }

            return null;
        }
        static public object GActA_ChildExtraSkill(CountEntityBase source, InterpolatedSkillAttributes skillAttributes, Thea2.Server.Group target)
        {
            String skillName = null;
            FInt lvl = FInt.ZERO;
            SkillInstance si = new SkillInstance();

            foreach (var v in skillAttributes.attributes)
            {
                if ("NewSkill" == v.Key)
                {
                    skillName = v.Value.t0;
                    lvl = (FInt)v.Value.t1;
                }
            }

            if (lvl > 0)
            {
                foreach (Character c in target.characters)
                {
                    if (c.attributes.Contains(TAG.CHILD))
                    {
                        c.LearnSkill(Character.SkillCategory.Effect, (Skill)skillName, lvl);
                    }
                }
            }

            return null;
        }
        static public object GActA_SummonCharacter(CountEntityBase source, InterpolatedSkillAttributes skillAttributes, Thea2.Server.Group target)
        {
            Subrace summon = null;
            int level = 1;
            bool useEq = false;

            if (source == null || source.GetItem() == null || target == null) return null;

            //Apply bonus only for the group which contains building
            if (target != source.item.Get().GetGroup()) return null;

            if (source.GetItem().superConnection != null && source.GetItem().superConnection.Valid()) return null;

            Tag summonUsed = (Tag)"TAG-SUMMON_USED";
            if (source.GetItem().attributes.GetBase(summonUsed) > 0) return null;

            foreach (var v in skillAttributes.attributes)
            {
                if ("Summon" == v.Key)
                {
                    summon = (Subrace)v.Value.t0;
                }

                if ("SummonLevel" == v.Key)
                {
                    level = v.Value.t1.ToInt();
                }

                if ("SummonEquipment" == v.Key)
                {
                    useEq = v.Value.t1.ToInt() >= 1;
                }
            }

            if (summon != null)
            {
                var c = Character.Instantiate(target, summon, level, useEq);
                if (c != null)
                {
                    c.SuperConnectWith(source.GetItem());
                    source.GetItem().attributes.SetBaseTo(summonUsed, FInt.ONE);
                }
            }

            return null;
        }
        static public object GActA_AddItemCargoWithChance(CountEntityBase source, InterpolatedSkillAttributes skillAttributes, Thea2.Server.Group target)
        {
            //Apply bonus only for the group which contains building
            if (target != source.item.Get().GetGroup()) return null;
            
            if (skillAttributes.attributes != null && target != null)
            {
                FInt chance = skillAttributes.GetFInt("ItemCargo");
                int random = UnityEngine.Random.Range(0, 100);
                if(random <= chance)
                {
                    GActA_AddItemCargo(source, skillAttributes, target);
                }
            }
            return null;
        }
        static public object GActA_AddItemCargo(CountEntityBase source, InterpolatedSkillAttributes skillAttributes, Thea2.Server.Group target)
        {
            //Apply bonus only for the group which contains building
            if (target != source.item.Get().GetGroup()) return null;

            if (skillAttributes.attributes != null && target != null)
            {
                foreach (var v in skillAttributes.attributes)
                {
                    if (v.Key == "ItemCargo")
                    {
                        DBClass t = Globals.GetInstanceFromDB(v.Value.t0);
                        if (t != null)
                        {
                            Type tp = t.GetType();

                            if (tp == typeof(ItemCargo))
                            {
                                CountEntityBase ceb = ItemBase.InstantaiteFrom(t as ItemCargo);
                                int min = (t as ItemCargo).numberOfItems.minimumCount;
                                int max = (t as ItemCargo).numberOfItems.maximumCount;

                                int count = max;
                                if (max != min)
                                {
                                    //excluding max forces random param to be bigger by 1
                                    count = UnityEngine.Random.Range(min, max + 1);
                                }
                                ceb.count = count;

                                target.AddItem(ceb);
                            }
                        }
                    }
                }
            }

            return null;
        }
        #endregion
        #endregion
        #region W - Weather scripts
        static public object W_Wind()
        {
            Tag m = (Tag)TAG.HEALTH_MENTAL;
            Tag s = (Tag)TAG.HEALTH_SPIRIT;
            foreach (var v in GameInstance.Get().GetPlayerGroups())
            {
                //consume 2 MHP and 1 SHP from characters
                if (v.characters != null)
                {
                    foreach(var c in v.characters)
                    {
                        c.Get().attributes.AddToBase(m, -2f, false);
                        c.Get().attributes.AddToBase(s, -1f);
                    }
                }

                //Increase Sailing speed this turn
                //NOTE! moved to NewTurnRefresh
                //                 if(!World.IsLand(v.position))
                //                 {
                //                     v.mp += FInt.ONE*2;
                //                     v.currentMp += FInt.ONE*2;
                //                 }
            }
            return null;
        }
        static public object W_Rain()
        {
            Skill s = (Skill)"SKILL-WEATHER_VIS";

            foreach (var v in GameInstance.Get().GetPlayerGroups())
            {
                if (v.characters != null)
                {
                    foreach (var c in v.characters)
                    {
                        c.Get().LearnSkill(Character.SkillCategory.Effect, s, FInt.ONE);
                    }
                }
            }
            return null;
        }
        static public object W_Snow()
        {
            Skill s = (Skill)"SKILL-WEATHER_SNOW";

            foreach (var v in GameInstance.Get().GetPlayerGroups())
            {
                if (v.characters != null)
                {
                    foreach (var c in v.characters)
                    {
                        c.Get().LearnSkill(Character.SkillCategory.Effect, s, new FInt(1));
                    }
                }
            }
            return null;
        }
        static public object W_SnowStorm()
        {
            Skill s = (Skill)"SKILL-WEATHER_SNOW";

            foreach (var v in GameInstance.Get().GetPlayerGroups())
            {
                if (v.characters != null)
                {
                    foreach (var c in v.characters)
                    {
                        c.Get().LearnSkill(Character.SkillCategory.Effect, s, new FInt(3));
                    }
                }
            }
            return null;
        }
        static public object W_SunnyDay()
        {
            Skill s = (Skill)"SKILL-WEATHER_SUNNY_DAY";

            foreach (var v in GameInstance.Get().GetPlayerGroups())
            {
                if (v.characters != null)
                {
                    foreach (var c in v.characters)
                    {
                        c.Get().LearnSkill(Character.SkillCategory.Effect, s, new FInt(5));
                    }
                }
            }
            return null;
        }

        static public object W_Scorch()
        {
            Tag p = (Tag)TAG.HEALTH_PHYSICAL;
            
            foreach (var v in GameInstance.Get().GetPlayerGroups())
            {
                //physical random damage to the characters outside village
                //possible curse
                if (v.characters != null && !v.settlement)
                {
                    foreach (var c in v.characters)
                    {
                        c.Get().attributes.AddToBase(p, UnityEngine.Random.Range(1f, 5f) * -1f );

                        if(UnityEngine.Random.Range(0f, 1f) < 0.4f && c.Get().attributes.GetFinal(p) <= 4)
                        {
                            c.Get().AddSkillEffect((SkillPack)SKILL_PACK.CURSE_P, true, 1);
                        }
                    }
                }
            }
            return null;
        }
        static public object W_Darkness()
        {
            Tag p = (Tag)TAG.HEALTH_SPIRIT;

            foreach (var v in GameInstance.Get().GetPlayerGroups())
            {
                //spiritual random damage to the characters outside village
                //possible curse
                if (v.characters != null && !v.settlement)
                {
                    foreach (var c in v.characters)
                    {
                        c.Get().attributes.AddToBase(p, UnityEngine.Random.Range(1f, 5f) * -1f);

                        if (UnityEngine.Random.Range(0f, 1f) < 0.4f && c.Get().attributes.GetFinal(p) <= 4)
                        {
                            c.Get().AddSkillEffect((SkillPack)SKILL_PACK.CURSE_S, true, 1);
                        }
                    }
                }
            }
            return null;
        }
        #endregion
        #region DEG - Charges Degradation, Group skills does not support degradation as of yet
        static public object Deg_SimpleCountdown(
                                    SkillInstance si,
                                    Character owner,
                                    bool justReturnDegradationSpeed)
        {
            if (justReturnDegradationSpeed)
            {
                return 1;
            }

            si.charges -= 1;

            if (si.charges <= 0)
            {
                if (si.source.Get().chargesCounter != null &&
                    si.source.Get().chargesCounter.finalScript != null)
                {
                    Globals.CallFunction(si.source.Get().chargesCounter.finalScript, null, si, null, owner);
                }

                if (!owner.RemoveSkill(si))
                {
                    Debug.LogError("[ERROR]SKill removal failed! "+si.source.Get().dbName);
                }           
            }

            return null;
        }
        static public object Deg_NoCountdown(
                                    SkillInstance si,
                                    Character owner,
                                    bool justReturnDegradationSpeed)
        {
            if(justReturnDegradationSpeed)
            {
                return 1;
            }
            return null;
        }
        static public object Deg_InfiniteCountdown(
                                    SkillInstance si,
                                    Character owner,
                                    bool justReturnDegradationSpeed)
        {
            if(justReturnDegradationSpeed)
            {
                return 0;
            }
            return null;
        }
        static public object Deg_InverseLevelCountdown(
                                    SkillInstance si,
                                    Character owner,
                                    bool justReturnDegradationSpeed)
        {
            //this will provide bleed by 
            // 5 points at LEVEL 1 (10 charges skill will last 2 turns)
            // 4 points at LEVEL 2 (10 charges skill will last 3 turns)
            // 3 points at LEVEL 3 (10 charges skill will last 4 turns)
            // 2 points at LEVEL 4 (10 charges skill will last 5 turns)
            // 1 point at LEVEL 5 (10 charges skill will last 10 turns)
            int bleedSpeed = 6 - FInt.Max(si.GetLevel(), FInt.ONE).ToInt();
            if (justReturnDegradationSpeed)
            {
                return bleedSpeed;
            }

            si.charges -= bleedSpeed;            

            if (si.charges <= 0)
            {
                if (si.source.Get().chargesCounter != null &&
                    si.source.Get().chargesCounter.finalScript != null)
                {
                    Globals.CallFunction(si.source.Get().chargesCounter.finalScript, null, si, null, owner);
                }

                if (!owner.RemoveSkill(si))
                {
                    Debug.LogError("[ERROR]SKill removal failed! " + si.source.Get().dbName);
                }
            }

            return bleedSpeed;
        }
        #endregion
        #region UTILS
        static void ApplyChange(bool additive, Tag t, FInt change, 
                        Dictionary<Tag, FInt> addons,
                        Dictionary<Tag, float> multipliers,
                        Int64 changeID,
                        List<Multitype<Int64, Tag, FInt, bool>> changeLog)
        {
            if (additive)
            {
                addons[t] = addons.MHGetValueOrDefault(t, FInt.ZERO) + change;

                if (changeLog != null)
                {
                    changeLog.Add(new Multitype<Int64, Tag, FInt, bool>(changeID, t, change, false));
                }
            }
            else 
            {
                multipliers[t] = multipliers.MHGetValueOrDefault(t, 1f) * change.ToFloat() * 0.01f;

                if (changeLog != null)
                {
                    changeLog.Add(new Multitype<Int64, Tag, FInt, bool>(changeID, t, change, true));
                }
            }

        }
        #endregion
    }
}
#endif