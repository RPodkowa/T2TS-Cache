#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using DBDef;
using TheHoney;
using Thea2.Common;
using Thea2.Server;
using Thea2.General;
using Thea2;

namespace GameScript
{
    public class GameplayScripts : ScriptBase
	{
        /// <summary>
        /// post processing attributes to ensure ability to influence some statistics can increase other.
        /// In effect this produces "Final" attributes which are result of "Base" attributes plus "Changes"
        /// Attribute Process:
        /// Base - permanent attributes of the item/character, which are not recalculated often, but can be change in game
        /// Changes - modifiers applied to the base. Eg traits, effects, item bonuses...
        /// Final - AttributesProcessing function which ensures there is a way to post process special connections between tags.
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        static public object AttributesProcessing(Attributes a, Character c, bool returnChangelog)
        {
            a.finalAttributes = new NetDictionary<DBReference<Tag>, FInt>();

            //from, to, how much change occurred. This is so that it can be later processed 
            //for additional information on "extra info" popups
            //string -  ID of the skill or item which influenced change
            //Tag - attribute changed by the process
            //FInt - amount of the change
            List<Multitype<long, Tag, FInt, bool>> changelog = null;
            if(returnChangelog)
            {
                changelog = new List<Multitype<long, Tag, FInt, bool>>();
            }

            foreach (var pair in a.baseAttributes)
            {
                a.finalAttributes[pair.Key] = pair.Value;
            }

            Dictionary<Tag, FInt> addons = new Dictionary<Tag, FInt>();
            Dictionary<Tag, float> multipliers = new Dictionary<Tag, float>();
            if (c != null)
            {                
                //this is a character therefore trigger passive skills which influence attributes
                //Process skills in slots
                foreach (var v in c.learnedSkills)
                {
                    foreach (var k in v.source.Get().skillSubskills)
                    {
                        if (k.trigger.triggerGroup == ETriggerGroupType.Passive)
                        {
                            Globals.CallFunction(k.activation.scriptName,
                                                v.GetCurrentSkillAttributes()[k],
                                                addons,
                                                multipliers,
                                                DescriptionInfoUtils.GetInfoID(v.source.Get().descriptionInfo),
                                                changelog, 
                                                c );
                        }
                    }
                }

                //Process skills in effects on character
                foreach (var v in c.effects)
                {
                    foreach(var k in v.source.Get().skillSubskills)
                    {
                        if (k.trigger.triggerGroup == ETriggerGroupType.Passive)
                        {
                            Globals.CallFunction(k.activation.scriptName,
                                                v.GetCurrentSkillAttributes()[k],
                                                addons, 
                                                multipliers,
                                                DescriptionInfoUtils.GetInfoID(v.source.Get().descriptionInfo),
                                                changelog,
                                                c);
                        }
                    }
                }

                //Process skills from items
                foreach (SkillInstance v in c.equipmentEffects)
                {
                    foreach (Subskill k in v.source.Get().skillSubskills)
                    {
                        if (k.trigger.triggerGroup == ETriggerGroupType.Passive)
                        {
//                             string itemID = null;
//                             foreach (var i in c.equipment)
//                             {
//                                 if (i != null && 
//                                     i.Get() != null && 
//                                     i.Get().skill == v)
//                                 {
//                                     itemID = i.Get().GetDescriptionInfo().GetInfoID().ToString();
//                                     break;
//                                 }
//                             }                            

                            Globals.CallFunction(k.activation.scriptName,
                                                v.GetCurrentSkillAttributes()[k],
                                                addons,
                                                multipliers,
                                                DescriptionInfoUtils.GetInfoID(v.source.Get().descriptionInfo),
                                                changelog,
                                                c);
                        }
                    }
                }
                foreach (var add in addons)
                {
                    if(a.finalAttributes.ContainsKey(add.Key))
                    {
                        a.finalAttributes[add.Key] += add.Value;
                    }
                    else
                    {
                        a.finalAttributes[add.Key] = add.Value;
                    }                    
                }
                foreach (var mul in multipliers)
                {
                    if (a.finalAttributes.ContainsKey(mul.Key))
                    {
                        a.finalAttributes[mul.Key] = a.finalAttributes[mul.Key] *  mul.Value;
                    }
                }
            }

            //main attributes to secondary conversion
            //NOTE: attributes which gets processing should not be parent tags for any other tag as they are 
            //add for this reason only at late stage

            Tag tag = Globals.GetInstanceFromDB<Tag>(TAG.WITS);
            if (a.finalAttributes.ContainsKey(tag))
            {
                FInt wits = a.finalAttributes[tag];
                //use special function to calculate delay                
                FInt delay;
                if (wits.ToFloat() <= 16f)
                {
                    delay = 4f - (wits / 4f);
                }
                else
                {
                    // - Sqrt( x/2 - 8) / 2
                    //delay 0 at wits 16, 
                    //delay -1 at wits 24,    = sqrt(4) / 2
                    //delay -2 at wits 48,    = sqrt(16) / 2
                    //delay -3 at wits 88,    = sqrt(36) / 2
                    //delay -4 at wits 144,   = sqrt(64) / 2
                    //delay -5 at wits 216,   = sqrt(100) / 2
                    delay = (FInt)Mathf.Pow(wits.ToFloat()*0.5f - 8f, 0.5f) *(-0.5f);
                }
                    
                SimpleApplyModifier(a.finalAttributes, tag,
                                Globals.GetInstanceFromDB<Tag>(TAG.DELAY),
                                delay, multipliers, changelog);
                
            }
            else
            {
                //write down without calculation if wits are not on character attribute list
                SimpleApplyModifier(a.finalAttributes, tag,
                                Globals.GetInstanceFromDB<Tag>(TAG.DELAY),
                                new FInt(4), multipliers, changelog);
            }

            tag = Globals.GetInstanceFromDB<Tag>(TAG.STRENGTH);
            if (a.finalAttributes.ContainsKey(tag))
            {
                FInt value = a.finalAttributes[tag];
                SimpleApplyModifier(a.finalAttributes, tag, 
                                    Globals.GetInstanceFromDB<Tag>(TAG.PERSONAL_CARRY_LIMIT), 
                                    GameplayUtils.GetCarryValue(value.ToInt()), multipliers, changelog);
                SimpleApplyModifier(a.finalAttributes, tag,
                                    Globals.GetInstanceFromDB<Tag>(TAG.MAX_HEALTH_PHYSICAL),
                                    1f * value, multipliers, changelog);
            }

            tag = Globals.GetInstanceFromDB<Tag>(TAG.PERCEPTION);
            if (a.finalAttributes.ContainsKey(tag))
            {
                FInt value = a.finalAttributes[tag];
                SimpleApplyModifier(a.finalAttributes, tag,
                                    Globals.GetInstanceFromDB<Tag>(TAG.MAX_HEALTH_PHYSICAL),
                                    0.5f * value, multipliers, changelog);
                SimpleApplyModifier(a.finalAttributes, tag,
                                    Globals.GetInstanceFromDB<Tag>(TAG.GATHERING),
                                    0.4f * value, multipliers, changelog);
                SimpleApplyModifier(a.finalAttributes, tag,
                                    Globals.GetInstanceFromDB<Tag>(TAG.LUCK),
                                    0.2f * value, multipliers, changelog);
            }

            tag = Globals.GetInstanceFromDB<Tag>(TAG.INTELLIGENCE);
            if (a.finalAttributes.ContainsKey(tag))
            {
                FInt value = a.finalAttributes[tag];
                SimpleApplyModifier(a.finalAttributes, tag,
                                    Globals.GetInstanceFromDB<Tag>(TAG.MAX_HEALTH_MENTAL),
                                    1f * value, multipliers, changelog);
                SimpleApplyModifier(a.finalAttributes, tag,
                                    Globals.GetInstanceFromDB<Tag>(TAG.RESEARCH),
                                    0.4f * value, multipliers, changelog);
                SimpleApplyModifier(a.finalAttributes, tag,
                                    Globals.GetInstanceFromDB<Tag>(TAG.CRAFTING),
                                    0.2f * value, multipliers, changelog);
            }
            tag = Globals.GetInstanceFromDB<Tag>(TAG.WISDOM);
            if (a.finalAttributes.ContainsKey(tag))
            {
                FInt value = a.finalAttributes[tag];
                SimpleApplyModifier(a.finalAttributes, tag,
                                    Globals.GetInstanceFromDB<Tag>(TAG.MAX_HEALTH_MENTAL),
                                    0.5f * value, multipliers, changelog);
                SimpleApplyModifier(a.finalAttributes, tag,
                                    Globals.GetInstanceFromDB<Tag>(TAG.LUCK),
                                    0.1f * value, multipliers, changelog);
                SimpleApplyModifier(a.finalAttributes, tag,
                                    Globals.GetInstanceFromDB<Tag>(TAG.RITUALS),
                                    0.2f * value, multipliers, changelog);
                SimpleApplyModifier(a.finalAttributes, tag,
                                    Globals.GetInstanceFromDB<Tag>(TAG.CRAFTING),
                                    0.4f * value, multipliers, changelog);
                SimpleApplyModifier(a.finalAttributes, tag,
                                    Globals.GetInstanceFromDB<Tag>(TAG.GATHERING),
                                    0.2f * value, multipliers, changelog);
            }

            tag = Globals.GetInstanceFromDB<Tag>(TAG.MYSTICISM);
            if (a.finalAttributes.ContainsKey(tag))
            {
                FInt value = a.finalAttributes[tag];
                SimpleApplyModifier(a.finalAttributes, tag,
                                    Globals.GetInstanceFromDB<Tag>(TAG.MAX_HEALTH_SPIRIT),
                                    1f * value, multipliers, changelog);
                SimpleApplyModifier(a.finalAttributes, tag,
                                    Globals.GetInstanceFromDB<Tag>(TAG.RESEARCH),
                                    0.2f * value, multipliers, changelog);
                SimpleApplyModifier(a.finalAttributes, tag,
                                    Globals.GetInstanceFromDB<Tag>(TAG.RITUALS),
                                    0.4f * value, multipliers, changelog);
            }
            tag = Globals.GetInstanceFromDB<Tag>(TAG.DESTINY);
            if (a.finalAttributes.ContainsKey(tag))
            {
                FInt value = a.finalAttributes[tag];
                SimpleApplyModifier(a.finalAttributes, tag,
                                    Globals.GetInstanceFromDB<Tag>(TAG.MAX_HEALTH_SPIRIT),
                                    .5f * value, multipliers, changelog);
                SimpleApplyModifier(a.finalAttributes, tag,
                                    Globals.GetInstanceFromDB<Tag>(TAG.GATHERING),
                                    0.1f * value, multipliers, changelog);
                SimpleApplyModifier(a.finalAttributes, tag,
                                    Globals.GetInstanceFromDB<Tag>(TAG.RESEARCH),
                                    0.1f * value, multipliers, changelog);
                SimpleApplyModifier(a.finalAttributes, tag, (Tag)TAG.LUCK,
                                    0.4f * value, multipliers, changelog);
                SimpleApplyModifier(a.finalAttributes, tag,
                                    Globals.GetInstanceFromDB<Tag>(TAG.RITUALS),
                                    0.1f * value, multipliers, changelog);
                SimpleApplyModifier(a.finalAttributes, tag,
                                    Globals.GetInstanceFromDB<Tag>(TAG.CRAFTING),
                                    0.1f * value, multipliers, changelog);
            }

            
            
            //Apply all values to their parents so that they can be sampled easy later
            //we are using presorted list which ensures that we will process leafs first, therefore
            //no parent would be processed before its child to ensure that parent can influence 
            //its own parents including this bonus without double processing
            List<Tag> tags = DBTypeUtils<Tag>.GetParentSortedType();
            foreach(Tag t in tags)
            {
                if (a.finalAttributes.ContainsKey(t))
                {
                    FInt bonus = a.finalAttributes[t];
                    HashSet<Tag> parents = DBTypeUtils<Tag>.GetParentsOf(t);
                    foreach(Tag parent in parents)
                    {
                        if(parent == null)
                        {
                            Debug.LogError("[ERROR]" + parent);
                        }
                        if (a.finalAttributes.ContainsKey(parent))
                        {
                            a.finalAttributes[parent] += bonus;
                        }
                        else
                        {
                            a.finalAttributes[parent] = bonus;
                        }
                    }
                }
            }

            if(c != null)
            {
                c.MaxCarry = a.finalAttributes[(Tag)TAG.PERSONAL_CARRY_LIMIT];
            }

            a.ClampAndScaleHP();
            a.ClearDirty();
           

            return changelog;
        }
        static void SimpleApplyModifier(NetDictionary<DBReference<Tag>, FInt> dict,
                                        Tag source, Tag target, FInt change,
                                        Dictionary<Tag, float> multiplier,
                                        List<Multitype<long, Tag, FInt, bool>> changelog)
        {            

            FInt changeMP = change * multiplier.MHGetValueOrDefault(target, 1f);
            if (dict.ContainsKey(target))
            {
                dict[target] += changeMP;
            }
            else
            {
                dict[target] = changeMP;
            }

            if (changelog != null)
            {
                changelog.Add(new Multitype<long, Tag, FInt, bool>(DescriptionInfoUtils.GetInfoID(source.descriptionInfo), target, change, false));
            }
        }
        
        /// <summary>
        /// Post process newly created character, for example by adding tag Male if character is neither Female nor Other
        /// This would not be called for clones
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        static public object PostCreateCharacter(Character character)
        {
            
            Tag female = Globals.GetInstanceFromDB<Tag>("TAG-GENDER_FEMALE");
            Tag other = Globals.GetInstanceFromDB<Tag>("TAG-GENDER_OTHER");
            
            //if not female nor other then should get mare tag
            if (character.attributes.GetBase(female) == 0f &&
                character.attributes.GetBase(other) == 0f)
            {
                Tag male = Globals.GetInstanceFromDB<Tag>("TAG-GENDER_MALE");
                character.attributes.SetBaseTo(male, 1f);
            }

            return null;
        }


        /// <summary>
        /// This script calculates chances of getting an item as a drop 
        /// </summary>
        /// <param name="targetGroup"></param>
        /// <param name="targetItems"></param>
        /// <returns></returns>
        static public float DropItemChance(Thea2.Server.Group targetGroup, float defaultChance)
        {
            Tag dropTag = Globals.GetInstanceFromDB<Tag>("TAG-DROP");

            List<float> dropAttributes = new List<float>();

            foreach(Character c in targetGroup.characters)
            {
                dropAttributes.Add( c.attributes.GetFinal(dropTag).ToFloat() );
            }

            dropAttributes.Sort(delegate (float a, float b)
            {
                return -a.CompareTo(b);
            });

            float cumulativeChances = defaultChance;

            for(int i=0; i< dropAttributes.Count; i++)
            {
                if (i == 0)
                {
                    cumulativeChances += dropAttributes[i] * 0.01f;
                }
                else
                {
                    cumulativeChances += dropAttributes[i] * 0.005f;
                }
            }

            return cumulativeChances;
        }

        static public object UpdateGroupAttributes(Thea2.Server.Group targetGroup, GameInstance gameInstance)
        {
            targetGroup.mp                  = FInt.ZERO;
            targetGroup.maxMp               = FInt.ZERO;
            targetGroup.viewRadius          = new FInt(-20);
            targetGroup.detectDangerRadius  = FInt.ZERO;
            targetGroup.detectability       = FInt.ZERO;
            targetGroup.seaMaxMP            = FInt.ZERO;
            targetGroup.seaCarryLimit       = FInt.ZERO;
            targetGroup.gatheringRange      = FInt.ONE;
            targetGroup.maxCraftingWorkersPerTask  = new FInt(targetGroup.settlement ? 3 : 2);
            targetGroup.maxGatheringWorkersPerTask = new FInt(targetGroup.settlement ? 3 : 2);
            targetGroup.maxResearchWorkersPerTask = new FInt(targetGroup.settlement ? 3 : 2);
            targetGroup.maxRitualsWorkersPerTask = new FInt(targetGroup.settlement ? 3 : 2);
            targetGroup.viewRangeBonus      = FInt.ZERO;
            
            Tag tDay = DBHelpers.GetDBInstance<Tag>(TAG.DAY);
            bool day = gameInstance.sharedAttributes.Contains(tDay, FInt.ONE);

            foreach (var v in targetGroup.items)
            {
                if (v.GetItem() != null && v.GetItem().skill != null)
                {
                    SkillInstance si = v.GetItem().skill;
                    foreach (var k in si.source.Get().skillSubskills)
                    {
                        if (k.trigger.triggerGroup == ETriggerGroupType.GroupPassive)
                        {
                            Globals.CallFunction(k.activation.scriptName, v, si.GetCurrentSkillAttributes()[k], targetGroup);
                        }
                    }
                }
            }
            if(targetGroup.seaMaxMP < FInt.ONE)
            {
                targetGroup.seaMaxMP = FInt.ONE;
            }
            else if(targetGroup.ownerID >0)
            {
                SPlayer player = GameInstance.Get().GetPlayer(targetGroup.ownerID);
                FInt v = player.attributes.GetBase(TAG.SEA_MOVEMENT_RANGE);

                targetGroup.seaMaxMP += v;
            }

            if(targetGroup.characters != null &&
                targetGroup.seaCarryLimit < targetGroup.characters.Count * 40)
            {
                targetGroup.seaCarryLimit = new FInt( targetGroup.characters.Count * 40);
            }

            bool isLand = World.IsLand(targetGroup.position);

            if (targetGroup.characters != null)
            {
                for (int i = 0; i < targetGroup.characters.Count; i++)
                {
                    Character c = targetGroup.characters[i];
                    if (i == 0)
                    {
                        if (!targetGroup.settlement && isLand)
                        {
                            targetGroup.mp = c.GetCurentMP();
                            targetGroup.maxMp = c.GetMaxMP();
                        }

                        if (day)
                        {
                            targetGroup.viewRadius = c.attributes.GetFinal(TAG.RANGE_OF_SIGHT_DAY) + targetGroup.viewRangeBonus;
                        }
                        else
                        {
                            targetGroup.viewRadius = c.attributes.GetFinal(TAG.RANGE_OF_SIGHT_NIGHT) + targetGroup.viewRangeBonus;
                        }
                    }
                    else
                    {
                        if (!targetGroup.settlement && isLand)
                        {
                            targetGroup.mp = FInt.Min(targetGroup.mp, c.GetCurentMP());
                            targetGroup.maxMp = FInt.Min(targetGroup.maxMp, c.GetMaxMP());
                        }

                        if (day)
                        {
                            FInt f = c.attributes.GetFinal(TAG.RANGE_OF_SIGHT_DAY) + targetGroup.viewRangeBonus;
                            targetGroup.viewRadius = FInt.Max(targetGroup.viewRadius, f);
                        }
                        else
                        {
                            FInt f = c.attributes.GetFinal(TAG.RANGE_OF_SIGHT_NIGHT) + targetGroup.viewRangeBonus;
                            targetGroup.viewRadius = FInt.Max(targetGroup.viewRadius, f);
                        }
                    }
                }
            }
            
            targetGroup.attributesDirty = false;

            return null;
        }
    }
}
#endif