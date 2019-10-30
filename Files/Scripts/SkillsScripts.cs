#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR
using System.Collections.Generic;
using DBDef;
using Thea2.Common;
using Thea2.Server;
using Thea2.General;
using TheHoney;
using Thea2;

namespace GameScript
{
    public class SubskillScript : ScriptBase
    {
        #region SI - Short Info
        //SSSD (Short Sub Skill Description) produces some short symbolic text which helps identify potential power of the skill
        // eg: 12-16
        static public string SI_Damage(object info)
        {
            Multitype<NetCard, NetSkill, NetBattlefield> data = info as Multitype<NetCard, NetSkill, NetBattlefield>;
            if (data != null)
            {
                return GameplayUtils.GetDamageFor(data.t1, data.t0).ToString(false);    
            }

            Multitype<SkillInstance, Subskill, ClientEntityCharacter> dInfo = info as Multitype<SkillInstance, Subskill, ClientEntityCharacter>;
            SkillInstance si = dInfo.t0;
            Subskill ss = dInfo.t1;
            ClientEntityCharacter character = dInfo.t2;

            var skillAttributes = si.GetCurrentSkillAttributes()[ss];
            int flags = skillAttributes.GetFInt("ProceduralFlags").ToInt();

            string st = null;

            FInt dmgBase = GameplayUtils.GetDamageFor(si, ss, null);

            if (ss.challengeTypes == null || character == null)
            {
                if ((flags & (int)EActivatorBlocks.Additive) > 0)
                {
                    st = "(+)" + dmgBase;
                }
                else
                {
                    st = dmgBase.ToString();
                }
            }
            else
            {
                foreach (var v in ss.challengeTypes)
                {
                    string color = "";
                    switch (v)
                    {
                        case EChallengeType.TypePhysical:
                            color = "XML_COLOR-PHYSICAL";
                            break;
                        case EChallengeType.TypeMental:
                            color = "XML_COLOR-MENTAL";
                            break;
                        case EChallengeType.TypeSpirit:
                            color = "XML_COLOR-SPIRITUAL";
                            break;
                    }

                    if (st == null) st = "";
                    else st += "|";
                    
                    if ((flags & (int)EActivatorBlocks.Additive) > 0)
                    {
                        st += "(+)" + dmgBase + " (" +
                                ColorUtils.GetColorAsTextTag(color) +
                                GameplayUtils.GetDamageFor(si, ss, character, v) +
                                ColorUtils.GetColorAsTextTag("XML_COLOR-NORMAL_FONT") + ")";
                    }
                    else
                    {
                        st += dmgBase + " (" +
                                ColorUtils.GetColorAsTextTag(color) +
                                GameplayUtils.GetDamageFor(si, ss, character, v) +
                                ColorUtils.GetColorAsTextTag("XML_COLOR-NORMAL_FONT") + ")";
                    }
                }
            }
            return st;
        }
        static public string SI_StunDamage(object info)
        {
            Multitype<NetCard, NetSkill, NetBattlefield> data = info as Multitype<NetCard, NetSkill, NetBattlefield>;
            if (data != null)
            {
                return data.t0.GetSkillCastingStrength(data.t1).ToString(false) +
                        " " +
                        "<sprite name=DelayIcon>" +
                        "+" + data.t1.GetFloatAttribute("Stun").ToStringScaled(0.8f);
            }

            Multitype<SkillInstance, Subskill, ClientEntityCharacter> dInfo = info as Multitype<SkillInstance, Subskill, ClientEntityCharacter>;
            SkillInstance si = dInfo.t0;
            Subskill ss = dInfo.t1;
            ClientEntityCharacter character = dInfo.t2;
            var dataInWorld = si.GetCurrentSkillAttributes()[ss];

            string mainName = dataInWorld.GetMainName(EChallengeType.TypePhysical);
            if (string.IsNullOrEmpty(mainName))
            {
                return "E:N!!";
            }

            if (character == null) return dataInWorld.GetFIntMain().ToString();

            FInt baseDMg = dataInWorld.GetFIntMain();
            FInt modifier = GameplayUtils.GetDamageMultiplierFInt(character.GetAttribute(mainName));
            FInt delay = dataInWorld.GetFInt("Stun");

            string st = null;
            foreach (var v in ss.challengeTypes)
            {
                string color = "";
                switch (v)
                {
                    case EChallengeType.TypePhysical:
                        color = "XML_COLOR-PHYSICAL";
                        break;
                    case EChallengeType.TypeMental:
                        color = "XML_COLOR-MENTAL";
                        break;
                    case EChallengeType.TypeSpirit:
                        color = "XML_COLOR-SPIRITUAL";
                        break;
                }

                if (st == null) st = "";
                else st += "|";

                st += baseDMg + " (" +
                    ColorUtils.GetColorAsTextTag(color) +
                    GameplayUtils.GetDamageFor(si, ss, character, v) +
                    ColorUtils.GetColorAsTextTag("XML_COLOR-NORMAL_FONT") + " " +
                    "<sprite name=DelayIcon>" +
                    "+" + delay.ToString() +
                    ")";
            }


            return st;
        }
        static public string SI_Stun(object info)
        {
            Multitype<NetCard, NetSkill, NetBattlefield> data = info as Multitype<NetCard, NetSkill, NetBattlefield>;
            if (data != null)
            {
                return "+" + data.t1.GetFloatAttribute("Stun").ToStringScaled(0.8f);
            }

            Multitype<SkillInstance, Subskill, ClientEntityCharacter> dInfo = info as Multitype<SkillInstance, Subskill, ClientEntityCharacter>;
            SkillInstance si = dInfo.t0;
            Subskill ss = dInfo.t1;
            ClientEntityCharacter character = dInfo.t2;
            var dataInWorld = si.GetCurrentSkillAttributes()[ss];

            FInt delay = dataInWorld.GetFInt("Stun");
            string st = "+" + delay.ToString();

            return st;
        }


        static public string SI_HurryUp(object info)
        {
            Multitype<NetCard, NetSkill, NetBattlefield> data = info as Multitype<NetCard, NetSkill, NetBattlefield>;
            if (data != null)
            {
                return "-" + data.t1.GetFloatAttribute("InspireSpeed").ToStringScaled(0.8f);
            }

            Multitype<SkillInstance, Subskill, ClientEntityCharacter> dInfo = info as Multitype<SkillInstance, Subskill, ClientEntityCharacter>;
            SkillInstance si = dInfo.t0;
            Subskill ss = dInfo.t1;
            ClientEntityCharacter character = dInfo.t2;
            var dataInWorld = si.GetCurrentSkillAttributes()[ss];

            FInt delay = dataInWorld.GetFInt("InspireSpeed");
            string st = "-" + delay.ToString();

            return st;
        }
        static public string SI_HurryUpShielding(object info)
        {
            return SI_DefaultAttributeTrioChange(info) + "<sprite name=DelayIcon>" + SI_HurryUp(info);
        }
        static public string SI_BlessedArmor(object info)
        {
            Multitype<NetCard, NetSkill, NetBattlefield> data = info as Multitype<NetCard, NetSkill, NetBattlefield>;
            
            if (data != null)
            {
                FInt inspirePower = data.t1.GetFloatAttribute("InspirePower");
                if (data.t1.MainAtt != null)
                {
                    //pow is main attribtue strength * its local multiplier if applicable
                    //attribute is applicable in scale 0.1 so eg. 25 Strength  provides multiplier 2.5 to the InspirePower
                    FInt pow = data.t0.GetSkillCastingStrength(data.t1);
                    inspirePower = inspirePower * pow;
                    inspirePower *= 0.1f;
                    inspirePower.CutToInt();
                }
                string inspireMod = "<sprite name=DamageInspire>" + inspirePower;

                return SI_DefaultAttributeTrioChange(info) + inspireMod;
            }

            {
                Multitype<SkillInstance, Subskill, ClientEntityCharacter> dInfo = info as Multitype<SkillInstance, Subskill, ClientEntityCharacter>;
                SkillInstance si = dInfo.t0;
                Subskill ss = dInfo.t1;
                ClientEntityCharacter character = dInfo.t2;
                var dataInWorld = si.GetCurrentSkillAttributes()[ss];
                
                FInt modifier = FInt.ONE;
                FInt pow = FInt.ONE;
                
                FInt inspirePower = dataInWorld.GetFInt("InspirePower");
                
                modifier = dataInWorld.GetFIntMain();
                string mainAtt = dataInWorld.GetMainName(EChallengeType.TypePhysical);
                if (character == null)
                {
                    pow = FInt.ONE;
                }
                else
                {
                    pow = GameplayUtils.GetDamageMultiplierFInt(character.GetAttribute(mainAtt));
                }
                FInt baseValue = inspirePower * modifier;
                baseValue.CutToInt();
                FInt finalValue = (modifier * inspirePower * pow) * 0.1f;
                finalValue.CutToInt();

                return SI_DefaultAttributeTrioChange(info) 
                    + "<sprite name=DamageInspire>" + baseValue + "(" + finalValue + ")";
            }
        }

        static public string SI_DefaultAttributeChange(object info)
        {
            Multitype<NetCard, NetSkill, NetBattlefield> data = info as Multitype<NetCard, NetSkill, NetBattlefield>;
            if (data != null)
            {
                return data.t0.GetSkillCastingStrength(data.t1).ToString(false);
            }

            Multitype<SkillInstance, Subskill, ClientEntityCharacter> dInfo = info as Multitype<SkillInstance, Subskill, ClientEntityCharacter>;
            SkillInstance si = dInfo.t0;
            Subskill ss = dInfo.t1;
            ClientEntityCharacter character = dInfo.t2;
            var dataInWorld = si.GetCurrentSkillAttributes()[ss];

            if (dataInWorld.attributes != null && dataInWorld.attributes.Count > 0)
            {
                string answer = "";
                string name = dataInWorld.attributes.GetKey(0);

                //                 Tag t = Globals.GetInstanceFromDB<Tag>(name);
                //                 if(t != null && t.visible)
                //                 {
                //                     answer = "<sprite name=" + t.descriptionInfo.iconName + "> ";
                //                 }

                bool multiplier = false;
                if (dataInWorld.attributes.GetValue(0).t0 == "*")
                {
                    answer += "x";
                    multiplier = true;
                }
                answer += dataInWorld.attributes.GetValue(0).t1.ToString();

                if (multiplier)
                {
                    answer += "%";
                }

                return answer;
            }

            return "";
        }
        static public string SI_DefaultAttributeTrioChange(object info)
        {
            Multitype<NetCard, NetSkill, NetBattlefield> data = info as Multitype<NetCard, NetSkill, NetBattlefield>;
            if (data != null)
            {
                string answer = "";
                string sign = "";
                switch (data.t2.ChallengeType)
                {
                    case EChallengeType.TypePhysical:
                        answer = data.t1.GetFloatAttribute(((Tag)TAG.SHIELDING_PHYSICAL).dbName).ToString(false);
                        sign = data.t1.GetStringAttribute(((Tag)TAG.SHIELDING_PHYSICAL).dbName);
                        break;

                    case EChallengeType.TypeMental:
                        answer = data.t1.GetFloatAttribute(((Tag)TAG.SHIELDING_MENTAL).dbName).ToString(false);
                        sign = data.t1.GetStringAttribute(((Tag)TAG.SHIELDING_MENTAL).dbName);
                        break;

                    case EChallengeType.TypeSpirit:
                        answer = data.t1.GetFloatAttribute(((Tag)TAG.SHIELDING_SPIRIT).dbName).ToString(false);
                        sign = data.t1.GetStringAttribute(((Tag)TAG.SHIELDING_SPIRIT).dbName);
                        break;

                    default:
                        return "[ERROR]";
                }

                if(sign == "*")
                {
                    answer += "%";
                }

                return answer;
            }

            Multitype<SkillInstance, Subskill, ClientEntityCharacter> dInfo = info as Multitype<SkillInstance, Subskill, ClientEntityCharacter>;
            SkillInstance si = dInfo.t0;
            Subskill ss = dInfo.t1;
            ClientEntityCharacter character = dInfo.t2;
            var dataInWorld = si.GetCurrentSkillAttributes()[ss];

            /*if (dataInWorld.GetFIntMain() != FInt.ZERO)
            {
                return dataInWorld.GetFIntMain().ToString();
            }
            else */if (dataInWorld.attributes != null /*&& dataInWorld.attributes.Keys.Count >= 3*/)
            {
                string answer = "";

                if (string.IsNullOrEmpty(dataInWorld.GetFInt(((Tag)TAG.SHIELDING_PHYSICAL).dbName).ToString()))
                {
                    answer = dataInWorld.GetFInt(((Tag)TAG.SHIELDING_PHYSICAL).dbName).ToString();
                }
                if (!string.IsNullOrEmpty(dataInWorld.GetFInt(((Tag)TAG.SHIELDING_MENTAL).dbName).ToString()))
                {
                    answer = dataInWorld.GetFInt(((Tag)TAG.SHIELDING_MENTAL).dbName).ToString();
                }
                if (!string.IsNullOrEmpty(dataInWorld.GetFInt(((Tag)TAG.SHIELDING_SPIRIT).dbName).ToString()))
                {
                    answer = dataInWorld.GetFInt(((Tag)TAG.SHIELDING_SPIRIT).dbName).ToString();
                }

                if(!string.IsNullOrEmpty(answer))
                {
                    if (dataInWorld.attributes.GetValue(0).t0 == "*")
                    {
                        answer += "%";
                    }
                    return answer;
                }
            }

            return "";
        }
        static public string SI_DeathChanceModification(object info)
        {
            Multitype<NetCard, NetSkill, NetBattlefield> data = info as Multitype<NetCard, NetSkill, NetBattlefield>;
            if (data != null)
            {
                return data.t0.GetSkillCastingStrength(data.t1).ToString(false);
            }

            Multitype<SkillInstance, Subskill, ClientEntityCharacter> dInfo = info as Multitype<SkillInstance, Subskill, ClientEntityCharacter>;
            SkillInstance si = dInfo.t0;
            Subskill ss = dInfo.t1;
            ClientEntityCharacter character = dInfo.t2;
            var dataInWorld = si.GetCurrentSkillAttributes()[ss];

            if (dataInWorld.attributes != null && dataInWorld.attributes.Count > 0)
            {
                if(dataInWorld.attributes.GetKey(0) == "TAG-DEATH_CHANCE_MODIFIER")
                {
                    var value = System.Math.Abs(dataInWorld.attributes.GetValue(0).t1.ToInt());
                    return value.ToString() + "%";
                }
            }

            return "";
        }
        static public string SI_DievannaAnimalPath(object info)
        {
            Multitype<NetCard, NetSkill, NetBattlefield> data = info as Multitype<NetCard, NetSkill, NetBattlefield>;
            if (data != null)
            {
                return data.t0.GetSkillCastingStrength(data.t1).ToString(false);
            }

            Multitype<SkillInstance, Subskill, ClientEntityCharacter> dInfo = info as Multitype<SkillInstance, Subskill, ClientEntityCharacter>;
            SkillInstance si = dInfo.t0;
            Subskill ss = dInfo.t1;
            ClientEntityCharacter character = dInfo.t2;
            

            var dataInWorld = si.GetCurrentSkillAttributes()[ss];

            if (dataInWorld.attributes != null && dataInWorld.attributes.Count > 0)
            {
                string answer = dataInWorld.attributes.GetValue(0).t1.ToString();

                if (character == null)
                {
                    return answer;
                }

                string sr = character.SubraceName;
                var subrace = Globals.GetInstanceFromDB<Subrace>(sr);
                
                if (subrace.race == (Race)RACE.BEAST ||
                    subrace.race == (Race)RACE.ELF ||
                    character.GetAttribute(TAG.FOREST_DEMON) > 0)
                {
                    return answer + " (" + answer +")";
                }
                
                return answer + " (" + FInt.ZERO.ToString() +")";
            }
            return "";
        }
        static public string SI_Foodie(object info)
        {
            FInt value = FInt.ONE;
            string answer = SI_DefaultAttributeChange(info) + " (-" + value.ToString() + ")";

            return answer;
        }
        static public string SI_NaturePurity(object info)
        {
            Multitype<SkillInstance, Subskill, ClientEntityCharacter> dInfo = info as Multitype<SkillInstance, Subskill, ClientEntityCharacter>;
            SkillInstance si = dInfo.t0;
            Subskill ss = dInfo.t1;
            var dataInWorld = si.GetCurrentSkillAttributes()[ss];

            return dataInWorld.attributes.Count.ToString();
        }
        static public string SI_AddItemCargo(object info)
        {
            Multitype<SkillInstance, Subskill, ClientEntityCharacter> dInfo = info as Multitype<SkillInstance, Subskill, ClientEntityCharacter>;
            SkillInstance si = dInfo.t0;
            Subskill ss = dInfo.t1;
            var dataInWorld = si.GetCurrentSkillAttributes()[ss];

            DBClass t = Globals.GetInstanceFromDB(dataInWorld.GetString("ItemCargo"));
            if (t != null)
            {
                System.Type tp = t.GetType();

                if (tp == typeof(ItemCargo))
                {
                    int min = (t as ItemCargo).numberOfItems.minimumCount;
                    int max = (t as ItemCargo).numberOfItems.maximumCount;

                    if (min == max)
                        return min.ToString();

                    return min.ToString() + " - " + max.ToString();
                }
            }
            return "";
        }
        static public string SI_AddItemCargoWithChance(object info)
        {
            Multitype<SkillInstance, Subskill, ClientEntityCharacter> dInfo = info as Multitype<SkillInstance, Subskill, ClientEntityCharacter>;
            SkillInstance si = dInfo.t0;
            Subskill ss = dInfo.t1;
            var dataInWorld = si.GetCurrentSkillAttributes()[ss];

            DBClass t = Globals.GetInstanceFromDB(dataInWorld.GetString("ItemCargo"));
            if (t != null)
            {
                System.Type tp = t.GetType();

                if (tp == typeof(ItemCargo))
                {
                    int min = (t as ItemCargo).numberOfItems.minimumCount;
                    int max = (t as ItemCargo).numberOfItems.maximumCount;
                    return dataInWorld.attributes.GetValue(0).t1.ToString() + "%";
                    if (min == max)
                        return min.ToString();

                    return min.ToString() + " - " + max.ToString() + " % ";
                }
            }
            return "";
        }
        static public string SI_HealPercentage(object info)
        {
            Multitype<SkillInstance, Subskill, ClientEntityCharacter> dInfo = info as Multitype<SkillInstance, Subskill, ClientEntityCharacter>;
            if (dInfo != null)
            {
                SkillInstance si = dInfo.t0;
                Subskill ss = dInfo.t1;
                var dataInWorld = si.GetCurrentSkillAttributes()[ss];

                return (dataInWorld.GetFIntMain() * 100).ToString() + "%";
            }
            return "";
        }
        static public string SI_LoseHealPercentage(object info)
        {
            Multitype<SkillInstance, Subskill, ClientEntityCharacter> dInfo = info as Multitype<SkillInstance, Subskill, ClientEntityCharacter>;
            if (dInfo != null)
            {
                SkillInstance si = dInfo.t0;
                Subskill ss = dInfo.t1;
                var dataInWorld = si.GetCurrentSkillAttributes()[ss];

                return ("-" + dataInWorld.GetFIntMain() * 100).ToString() + "%";
            }
            return "";
        }

        static public object SI_SummonX1(object info)
        {
            return "x1";
        }

        static public object SI_SummonX2(object info)
        {
            return "x1-2";
        }

        static public object SI_SummonX3(object info)
        {
            return "x1-3";
        }

        static public object SI_SummonX4(object info)
        {
            return "x1-4";
        }

        static public object SI_SummonX5(object info)
        {
            return "x1-5";
        }

        static public string SI_Inspire(object info)
        {
            Multitype<NetCard, NetSkill, NetBattlefield> data = info as Multitype<NetCard, NetSkill, NetBattlefield>;
            if (data != null)
            {
                FInt inspireSpeed = -1 * data.t1.GetFloatAttribute("InspireSpeed");
                FInt inspirePower = data.t1.GetFloatAttribute("InspirePower");
                if (data.t1.MainAtt != null)
                {
                    //pow is main attribtue strength * its local multiplier if applicable
                    //attribute is applicable in scale 0.1 so eg. 25 Strength  provides multiplier 2.5 to the InspirePower
                    FInt pow = data.t0.GetSkillCastingStrength(data.t1);
                    inspirePower = inspirePower * pow;
                    inspirePower *= 0.1f;
                    inspirePower.CutToInt();
                }
                return inspirePower + "<sprite name=DelayIcon>" + inspireSpeed;
            }

            {
                Multitype<SkillInstance, Subskill, ClientEntityCharacter> dInfo = info as Multitype<SkillInstance, Subskill, ClientEntityCharacter>;
                SkillInstance si = dInfo.t0;
                Subskill ss = dInfo.t1;
                ClientEntityCharacter character = dInfo.t2;
                var dataInWorld = si.GetCurrentSkillAttributes()[ss];

                FInt modifier = FInt.ONE;
                FInt pow = FInt.ONE;

                FInt inspireSpeed = -1 * dataInWorld.GetFInt("InspireSpeed");
                FInt inspirePower = dataInWorld.GetFInt("InspirePower");

                if (dataInWorld.mainAtt != null)
                {
                    modifier = dataInWorld.GetFIntMain();

                    string mainAtt = dataInWorld.GetMainName(EChallengeType.TypePhysical);
                    if (character == null)
                    {
                        pow = FInt.ONE;
                    }
                    else
                    {
                        pow = GameplayUtils.GetDamageMultiplierFInt(character.GetAttribute(mainAtt));
                    }
                }
                FInt baseValue = inspirePower * modifier;
                baseValue.CutToInt();
                FInt finalValue = (modifier * inspirePower * pow) * 0.1f;
                finalValue.CutToInt();
                return baseValue + "("+ finalValue + ")<sprite name=DelayIcon>" + inspireSpeed;
            }
        }

        static public string SI_Swarm(object info)
        {
            Multitype<NetCard, NetSkill, NetBattlefield> data = info as Multitype<NetCard, NetSkill, NetBattlefield>;
            if (data != null)
            {
                FInt inspireSpeed = -1 * data.t1.GetFloatAttribute("InspireSpeed");
                FInt inspirePower = data.t1.GetFloatAttribute("InspirePower");
                if (data.t1.MainAtt != null)
                {
                    //pow is main attribtue strength * its local multiplier if applicable
                    //attribute is applicable in scale 0.1 so eg. 25 Strength  provides multiplier 2.5 to the InspirePower
                    FInt pow = data.t0.GetSkillCastingStrength(data.t1);
                    inspirePower = inspirePower * pow;
                    inspirePower *= 0.1f;
                }
                return inspirePower + " <sprite name=DelayIcon>" + inspireSpeed;
            }

            {
                Multitype<SkillInstance, Subskill, ClientEntityCharacter> dInfo = info as Multitype<SkillInstance, Subskill, ClientEntityCharacter>;
                SkillInstance si = dInfo.t0;
                Subskill ss = dInfo.t1;
                ClientEntityCharacter character = dInfo.t2;
                var dataInWorld = si.GetCurrentSkillAttributes()[ss];

                FInt modifier = FInt.ONE;
                FInt inspireSpeed = -1 * dataInWorld.GetFInt("InspireSpeed");
                FInt inspirePower = dataInWorld.GetFInt("InspirePower");

                if (dataInWorld.mainAtt != null)
                {
                    modifier = dataInWorld.GetFIntMain();
                }

                return inspirePower * modifier + " <sprite name=DelayIcon>" + inspireSpeed;
            }
        }


        static public string SI_UnInspire(object info)
        {
            Multitype<NetCard, NetSkill, NetBattlefield> data = info as Multitype<NetCard, NetSkill, NetBattlefield>;
            if (data != null)
            {
                FInt inspireSpeed = 1 * data.t1.GetFloatAttribute("UnInspireSpeed");
                FInt inspirePower = data.t1.GetFloatAttribute("UnInspirePower");
                if (data.t1.MainAtt != null)
                {
                    //pow is main attribtue strength * its local multiplier if applicable
                    //attribute is applicable in scale 0.1 so eg. 25 Strength  provides multiplier 2.5 to the InspirePower
                    FInt pow = data.t0.GetSkillCastingStrength(data.t1);
                    inspirePower = inspirePower * pow;
                    inspirePower *= 0.1f;
                    inspirePower.CutToInt();
                }
                return inspirePower  + "<sprite name=DelayIcon>" + "+" + inspireSpeed;
            }

            {
                Multitype<SkillInstance, Subskill, ClientEntityCharacter> dInfo = info as Multitype<SkillInstance, Subskill, ClientEntityCharacter>;
                SkillInstance si = dInfo.t0;
                Subskill ss = dInfo.t1;
                ClientEntityCharacter character = dInfo.t2;
                var dataInWorld = si.GetCurrentSkillAttributes()[ss];

                FInt modifier = FInt.ONE;
                FInt pow = FInt.ONE;

                FInt inspireSpeed = 1 * dataInWorld.GetFInt("UnInspireSpeed");
                FInt inspirePower = dataInWorld.GetFInt("UnInspirePower");

                if (dataInWorld.mainAtt != null)
                {
                    modifier = dataInWorld.GetFIntMain();

                    string mainAtt = dataInWorld.GetMainName(EChallengeType.TypePhysical);
                    if (character == null)
                    {
                        pow = FInt.ONE;
                    }
                    else
                    {
                        pow = GameplayUtils.GetDamageMultiplierFInt(character.GetAttribute(mainAtt));
                    }
                }

                FInt baseValue = inspirePower * modifier;
                baseValue.CutToInt();
                FInt finalValue = (modifier * inspirePower * pow) * 0.1f;
                finalValue.CutToInt();
                return baseValue + "(" + finalValue + ")<sprite name=DelayIcon>" + inspireSpeed;
            }
        }
        static public string SI_BloodySacrifice(object info)
        {
            Multitype<NetCard, NetSkill, NetBattlefield> data = info as Multitype<NetCard, NetSkill, NetBattlefield>;
            if (data != null)
            {
                NetCard nc = data.t0;
                NetSkill ns = data.t1;
                FInt sacrifice = ns.GetFloatAttribute("HealthSacrifice");
                FInt multipier = ns.GetFloatAttribute("DmgMulti");
                FInt sacrificeHp = (nc.GetCA_HEALTH() + nc.GetCA_SHIELD()) * sacrifice;
                sacrificeHp.CutToInt();
                FInt damage = sacrificeHp * multipier;
                return damage + " <sprite name=HealthPhysicalIcon>" + "-" + sacrificeHp;
            }

            {
                Multitype<SkillInstance, Subskill, ClientEntityCharacter> dInfo = info as Multitype<SkillInstance, Subskill, ClientEntityCharacter>;
                SkillInstance si = dInfo.t0;
                Subskill ss = dInfo.t1;
                ClientEntityCharacter character = dInfo.t2;
                var dataInWorld = si.GetCurrentSkillAttributes()[ss];

                if (dataInWorld.attributes != null && dataInWorld.attributes.Count > 0)
                {
                    FInt sacrifice = dataInWorld.GetFInt("HealthSacrifice");
                    FInt multipier = dataInWorld.GetFInt("DmgMulti");

                    if(character != null)
                    {
                        FInt shield = character.GetAttribute((Tag)TAG.SHIELDING_PHYSICAL);
                        shield.CutToInt();
                        FInt sacrificeHp = (character.GetPHYSICAL_HP() + shield) * sacrifice;
                        sacrificeHp.CutToInt();
                        FInt damage = sacrificeHp * multipier;
                        return damage + " <sprite name=HealthPhysicalIcon>" + "-" + sacrificeHp;
                    }
                    else
                    {
                        return (sacrifice * multipier) + "(x)" + "\n<sprite name=HealthPhysicalIcon>" + "-" + multipier + "(x)";
                    }
                }

                return "";
            }
        }
        static public string SI_DestroyShielding(object info)
        {
            Multitype<NetCard, NetSkill, NetBattlefield> data = info as Multitype<NetCard, NetSkill, NetBattlefield>;
            if (data != null)
            {
                string value = data.t1.GetFloatAttribute("ShieldDestroy").ToString();
                return value;
            }

            {
                return SI_ExtraBonus(info);
            }
        }

        static public string SI_Range(object info)
        {
            Multitype<SkillInstance, Subskill, ClientEntityCharacter> dInfo = info as Multitype<SkillInstance, Subskill, ClientEntityCharacter>;
            SkillInstance si = dInfo.t0;
            Subskill ss = dInfo.t1;

            var skillAttributes = si.GetCurrentSkillAttributes()[ss];
            string sign = skillAttributes.attributes.GetValue(0).t0;
            int modifier = skillAttributes.attributes.GetValue(0).t1.ToInt(); 
            string st = sign + modifier.ToString();

            return st;
        }
        static public string SI_ExtraSlot(object info)
        {
            Multitype<SkillInstance, Subskill, ClientEntityCharacter> dInfo = info as Multitype<SkillInstance, Subskill, ClientEntityCharacter>;
            SkillInstance si = dInfo.t0;
            Subskill ss = dInfo.t1;

            var skillAttributes = si.GetCurrentSkillAttributes()[ss];
            string sign = skillAttributes.attributes.GetValue(0).t0;
            int modifier = skillAttributes.attributes.GetValue(0).t1.ToInt();
            string st = sign + modifier.ToString();

            return st;
        }
        static public string SI_ExtraBonus(object info)
        {
            Multitype<SkillInstance, Subskill, ClientEntityCharacter> dInfo = info as Multitype<SkillInstance, Subskill, ClientEntityCharacter>;
            SkillInstance si = dInfo.t0;
            Subskill ss = dInfo.t1;

            var skillAttributes = si.GetCurrentSkillAttributes()[ss];
            string sign = skillAttributes.attributes.GetValue(0).t0;
            FInt modifier = skillAttributes.attributes.GetValue(0).t1;
            string st = sign + modifier.ToString();

            return st;
        }
        static public string SI_ExtraSkill(object info)
        {
            Multitype<SkillInstance, Subskill, ClientEntityCharacter> dInfo = info as Multitype<SkillInstance, Subskill, ClientEntityCharacter>;
            SkillInstance buildingSi = dInfo.t0;
            Subskill buildingSubskill = dInfo.t1;

            var buildingSSAttributes = buildingSi.GetCurrentSkillAttributes()[buildingSubskill];
            FInt charSkillLvl = buildingSSAttributes.GetFInt("NewSkill");
            string charSkillName = buildingSSAttributes.GetString("NewSkill");
            Skill newSkill = (Skill)charSkillName;

            if (newSkill == null) return charSkillName;
            SkillInstance charSi = SkillInstance.Instantiate(newSkill, charSkillLvl);
            Subskill charSubskill = newSkill.skillSubskills[0];

            var charSSAttributes = charSi.GetCurrentSkillAttributes()[charSubskill];
            string bonusTag = charSSAttributes.attributes.GetKey(0);
            FInt bonusValue = charSSAttributes.GetFInt(bonusTag);
            string st = bonusValue.ToString();

            if (charSSAttributes.GetString(bonusTag) == "*")
                st += "%";
            
            return st;
        }
        static public string SI_ExtraSkillPercent(object info)
        {
            Multitype<SkillInstance, Subskill, ClientEntityCharacter> dInfo = info as Multitype<SkillInstance, Subskill, ClientEntityCharacter>;
            SkillInstance buildingSi = dInfo.t0;
            Subskill buildingSubskill = dInfo.t1;

            var buildingSSAttributes = buildingSi.GetCurrentSkillAttributes()[buildingSubskill];
            FInt charSkillLvl = buildingSSAttributes.GetFInt("NewSkill");
            string charSkillName = buildingSSAttributes.GetString("NewSkill");
            Skill newSkill = (Skill)charSkillName;

            if (newSkill == null) return charSkillName;
            SkillInstance charSi = SkillInstance.Instantiate(newSkill, charSkillLvl);
            Subskill charSubskill = newSkill.skillSubskills[0];

            var charSSAttributes = charSi.GetCurrentSkillAttributes()[charSubskill];
            string bonusTag = charSSAttributes.attributes.GetKey(0);
            FInt bonusValue = charSSAttributes.GetFInt(bonusTag);
            string st = (bonusValue).ToString() + "%";

            return st;

        }

        static public object SI_Empty(object info)
        {
            return " ";
        }
        #endregion

        #region TRI - Trigger Scripts
        // Trigger work for spells, items and character skills

        ///This is default behavior which ensures that it is triggered 
        ///only when trigger source is owner of this skill as well
        ///EG: OnAttack is done by skill owner or OnPlayCard is done by skill owner etc.
        static public bool Tri_OnOwnEffectDefault(NetBattlefield bf, NetSkill skill, Multitype<object, object, object, object> data)
        {
            NetCard reciver = data.t0 as NetCard;// target of the previous skill
            int reciverPosition = (int)data.t1; //position of the target of the previous skill
            //we do not use all data provided,
            //but data is there in case this is part of some other trigger reasoning
            //NetQueueItem damageSource = data.t2 as NetQueueItem;
            //FInt damageSize = (FInt)data.t3;

            if (reciver == null || skill.OwnerCardID != reciver.CardID) return false;

            //Add new effect of this skill on stack
            //reciver position is new caster position            
            NetQueueItem q2 = new NetQueueItem(bf, skill, null, null, FInt.ZERO, reciverPosition);
            bf.PutSkillOnQueue_Local(q2);

            return true;
        }
        static public bool Tri_OnEnemyDominance(NetBattlefield bf, NetSkill skill, Multitype<object, object, object, object> data)
        {
            NetCard reciver = data.t0 as NetCard;// target of the previous skill
            int reciverPosition = (int)data.t1; //position of the target of the previous skill
            //we do not use all data provided,
            //but data is there in case this is part of some other trigger reasoning
            
            if (bf.GetOpposingLivingCardsBFPositions(reciver.PlayerOwnerID).Count <= bf.GetOwnLivingCardsBFPositions(reciver.PlayerOwnerID).Count ||
                reciver == null || skill.OwnerCardID != reciver.CardID) return false;

            NetListInt targets = new NetListInt(new List<int>(bf.GetOwnLivingCardsBFPositions(reciver.PlayerOwnerID)));
            NetQueueItem q2 = new NetQueueItem(bf, skill, targets, null, FInt.ZERO, reciver.CardID);
            q2.TargetsSelected = true;
            bf.PutSkillOnQueue_Local(q2);

            return true;
        }
        static public bool Tri_OnOwnEnterBf(NetBattlefield bf, NetSkill skill, Multitype<object, object, object, object> data)
        {
            NetCard reciver = data.t0 as NetCard;// target of the previous skill
            int reciverPosition = (int)data.t1; //position of the target of the previous skill
            //we do not use all data provided,
            //but data is there in case this is part of some other trigger reasoning
            //NetQueueItem damageSource = data.t2 as NetQueueItem;
            //FInt damageSize = (FInt)data.t3;

            if (reciver == null || skill.OwnerCardID != reciver.CardID || reciverPosition == -1) return false;

            NetCard nc = skill.GetOwner(bf);
            //Add new effect of this skill on stack
            //reciver position is new caster position            
            NetQueueItem q2 = new NetQueueItem(bf, skill, null, null, FInt.ZERO, nc.CardID);
            bf.PutSkillOnQueue_Local(q2);

            return true;
        }
        /// <summary>
        /// Triggers when owner is on battlefield and reciver is different than owner. Sets owner as a new target and reciver as a new secondary target
        /// </summary>
        static public bool Tri_PainCommission(NetBattlefield bf, NetSkill skill, Multitype<object, object, object, object> data)
        {
            NetCard reciver = data.t0 as NetCard;// target of the previous skill
            int reciverPosition = (int)data.t1; //position of the target of the previous skill
                                                //we do not use all data provided,
                                                //but data is there in case this is part of some other trigger reasoning
                                                //NetQueueItem damageSource = data.t2 as NetQueueItem;
            FInt damageSize = (FInt)data.t3;

            int owner = skill.OwnerCardID;

            //check if its the owner who was targeted and if reciver is on battlefield
            if (reciver == null || !bf.IsBattleSlot(reciverPosition) || 
                reciver.CardID == owner) return false;

            // check if owner is on battlefield
            if (!bf.BattlefieldPositions.value.Contains(owner)) return false;

            // check if reciver is a owner of any instance of skill
            if (!NetType.IsNullOrEmpty(reciver.ActiveEffects))
            {
                foreach (NetSkill ae in reciver.ActiveEffects.value)
                {
                    if (ae.SubSkillDbName == skill.SubSkillDbName)
                    {
                        return false;
                    }
                }
            }

            NetCard nc = skill.GetOwner(bf);

            // check if reciver is on player side and if owner is different than reciver
            bool playersSide = bf.IsFriendlySlot(reciverPosition);
            if ((playersSide && nc.PlayerOwnerID == -1) ||
                (!playersSide && nc.PlayerOwnerID > -1) ||
                nc == reciver)
            {
                //cast was not on the same battle side
                return false;
            }

            NetListInt target = new NetListInt(new List<int>());
            NetListInt sTarget = new NetListInt(new List<int>());
            var friendlyPositions = bf.GetOwnLivingCardsBFPositions(nc.PlayerOwnerID);

            // searching for positions of owner on battlefield
            foreach (var pos in friendlyPositions)
            {
                if (bf.ConvertPositionIDToCard(pos).CardID == skill.OwnerCardID)
                {
                    target.value.Add(pos);
                    break;
                }
            }
            if (target.value.Count == 0)
            {
                return false;
            }

            sTarget.value.Add(reciverPosition);

            //Add new effect of this skill on stack
            //reciver position is new caster position            
            NetQueueItem q2 = new NetQueueItem(bf, skill, target, sTarget, FInt.ZERO, reciverPosition);
            q2.TargetsSelected = true;
            q2.Data = damageSize;
            bf.PutSkillOnQueue_Local(q2);

            return true;
        }
        static public bool Tri_OnAllyEffectWhenSelfOnBattlefieldDefault(NetBattlefield bf, NetSkill skill, Multitype<object, object, object, object> data)
        {
            NetCard reciver = data.t0 as NetCard;// target of the previous skill
            int reciverPosition = (int)data.t1; //position of the target of the previous skill
            //we do not use all data provided,
            //but data is there in case this is part of some other trigger reasoning
            //NetQueueItem damageSource = data.t2 as NetQueueItem;
            //FInt damageSize = (FInt)data.t3;
            
            if (reciver == null || !bf.IsBattleSlot(reciverPosition)) return false;
            
            int owner = skill.OwnerCardID;
            if (!bf.BattlefieldPositions.value.Contains(owner) )
            {
                return false;
            }

            NetCard nc = skill.GetOwner(bf);

            bool playersSide = bf.IsFriendlySlot(reciverPosition);
            if ((playersSide && nc.PlayerOwnerID == -1) || 
                (!playersSide && nc.PlayerOwnerID > -1))
            {
                //cast was not on the same battle side
                return false;
            }

            NetListInt target = new NetListInt(new List<int>());
            var friendlyPositions = bf.GetOwnLivingCardsBFPositions(nc.PlayerOwnerID);

            // searching for positions of owner on battlefield
            foreach (var pos in friendlyPositions)
            {
                if (bf.ConvertPositionIDToCard(pos).CardID == skill.OwnerCardID && pos != reciverPosition)
                {
                    target.value.Add(pos);
                }
            }
            if(target.value.Count == 0)
            {
                return false;
            }

            //Add new effect of this skill on stack
            //reciver position is new caster position            
            NetQueueItem q2 = new NetQueueItem(bf, skill, target, null, FInt.ZERO, nc.CardID);
            q2.TargetsSelected = true;
            bf.PutSkillOnQueue_Local(q2);

            return true;
        }
        static public bool Tri_OnEnemyEffectWhenSelfOnBattlefieldDefault(NetBattlefield bf, NetSkill skill, Multitype<object, object, object, object> data)
        {
            NetCard reciver = data.t0 as NetCard;// target of the previous skill
            int reciverPosition = (int)data.t1; //position of the target of the previous skill
                                                //we do not use all data provided,
                                                //but data is there in case this is part of some other trigger reasoning
                                                //NetQueueItem damageSource = data.t2 as NetQueueItem;
                                                //FInt damageSize = (FInt)data.t3;

            if (reciver == null || !bf.IsBattleSlot(reciverPosition)) return false;

            int owner = skill.OwnerCardID;
            if (!bf.BattlefieldPositions.value.Contains(owner))
            {
                return false;
            }

            NetCard nc = skill.GetOwner(bf);

            bool playersSide = bf.IsFriendlySlot(reciverPosition);
            if ((!playersSide && nc.PlayerOwnerID == -1) ||
                (playersSide && nc.PlayerOwnerID > -1))
            {
                return false;
            }

            NetListInt target = new NetListInt(new List<int>());
            target.value.Add(reciverPosition);
            
            //Add new effect of this skill on stack
            //reciver position is new caster position            
            NetQueueItem q2 = new NetQueueItem(bf, skill, target, null, FInt.ZERO, nc.CardID);
            q2.TargetsSelected = true;
            bf.PutSkillOnQueue_Local(q2);

            return true;
        }
        static public bool Tri_OnEnemyEffectWhenSelfOnBattlefield(NetBattlefield bf, NetSkill skill, Multitype<object, object, object, object> data)
        {
            NetCard reciver = data.t0 as NetCard;// target of the previous skill
            int reciverPosition = (int)data.t1; //position of the target of the previous skill
                                                //we do not use all data provided,
                                                //but data is there in case this is part of some other trigger reasoning
                                                //NetQueueItem damageSource = data.t2 as NetQueueItem;
                                                //FInt damageSize = (FInt)data.t3;

            if (reciver == null || !bf.IsBattleSlot(reciverPosition)) return false;
            NetCard nc = skill.GetOwner(bf);

            int owner = nc.CardID;
            if (!bf.BattlefieldPositions.value.Contains(owner))
            {
                return false;
            }

            bool playersSide = bf.IsFriendlySlot(reciverPosition);
            if ((!playersSide && nc.PlayerOwnerID == -1) ||
                (playersSide && nc.PlayerOwnerID > -1))
            {
                return false;
            }

            NetListInt target = new NetListInt(new List<int>());
            var friendlyPositions = bf.GetOwnLivingCardsBFPositions(nc.PlayerOwnerID);

            // searching for positions of owner on battlefield
            foreach (var pos in friendlyPositions)
            {
                if (bf.ConvertPositionIDToCard(pos).CardID == skill.OwnerCardID)
                {
                    target.value.Add(pos);
                }
            }
            if (target.value.Count == 0)
            {
                return false;
            }

            //Add new effect of this skill on stack
            //reciver position is new caster position            
            NetQueueItem q2 = new NetQueueItem(bf, skill, target, null, FInt.ZERO, nc.CardID);
            q2.TargetsSelected = true;
            bf.PutSkillOnQueue_Local(q2);

            return true;
        }
        static public bool Tri_OnAttackFromMe(NetBattlefield bf, NetSkill skill, Multitype<object, object, object, object> data)
        {
            NetCard reciver = data.t0 as NetCard;
            int reciverPosition = (int)data.t1;
            NetQueueItem damageSource = data.t2 as NetQueueItem;
            FInt damageSize = (FInt)data.t3;

            //NOTE! Because we need to be careful here we do additional limitation, 
            //(its possible to start circular trigger if eg. this does damage in response )
            NetSkill ns = damageSource.GetNetSkill(bf);
            ETriggerGroupType trigger = ns.TriggerGroup;
            if (NetType.IsNullOrEmpty(damageSource.Targets) ||
                ns.OwnerCardID != skill.OwnerCardID ||
                trigger != ETriggerGroupType.DoAttack)
            {
                //source is not regular action therefore we would go safety route and NOT do any further damages!
                return false;
            }

            //Add new effect of this skill on stack
            //reciver position is new caster position
            NetQueueItem q2 = new NetQueueItem(bf, skill, null, null, FInt.ZERO, damageSource.CastingPosition);
            q2.Data = damageSize;
            q2.Targets = damageSource.Targets != null ? ProtoBuf.Serializer.DeepClone(damageSource.Targets) : null;
            q2.TargetsSelected = true;
            bf.PutSkillOnQueue_Local(q2);

            return true;
        }
        static public bool Tri_OnEnemyKilled(NetBattlefield bf, NetSkill skill, Multitype<object, object, object, object> data)
        {
            NetCard reciver = data.t0 as NetCard;
            int reciverPosition = (int)data.t1;
            NetQueueItem damageSource = data.t2 as NetQueueItem;
            FInt damageSize = (FInt)data.t3;

            //NOTE! Because we need to be careful here we do additional limitation, 
            //(its possible to start circular trigger if eg. this does damage in response )
            NetSkill ns = damageSource.GetNetSkill(bf);
            ETriggerGroupType trigger = ns.TriggerGroup;
            if (trigger != ETriggerGroupType.DoAttack ||
                NetType.IsNullOrEmpty(damageSource.Targets) ||
                ns.OwnerCardID != skill.OwnerCardID ||
                reciver.GetCA_HEALTH() > 0)
            {
                return false;
            }

            //Add new effect of this skill on stack
            //reciver position is new caster position
            NetQueueItem q2 = new NetQueueItem(bf, skill, null, null, FInt.ZERO, reciverPosition);
            bf.PutSkillOnQueue_Local(q2);

            return true;
        }
        
        static public bool Tri_OnAttackFromMeEffctOnMeWounded(NetBattlefield bf, NetSkill skill, Multitype<object, object, object, object> data)
        {
            NetCard reciver = skill.GetOwner(bf);
            NetQueueItem damageSource = data.t2 as NetQueueItem;
            int reciverPosition = damageSource.CastingPosition;

            //NOTE! Because we need to be careful here we do additional limitation, 
            //(its possible to start circular trigger if eg. this does damage in response )
            NetSkill ns = damageSource.GetNetSkill(bf);
            ETriggerGroupType trigger = ns.TriggerGroup;
            if (NetType.IsNullOrEmpty(damageSource.Targets) ||
                ns.OwnerCardID != skill.OwnerCardID ||
                trigger != ETriggerGroupType.DoAttack ||
                reciver.GetCA_MAX_HEALTH() == reciver.GetCA_HEALTH())
            {
                //source is not regular action therefore we would go safety route and NOT do any further damages!
                return false;
            }

            //Add new effect of this skill on stack
            //reciver position is new caster position            
            NetQueueItem q2 = new NetQueueItem(bf, skill, null, null, FInt.ZERO, reciverPosition);
            bf.PutSkillOnQueue_Local(q2);

            return true;
        }
        static public bool Tri_OnAttackFromMeEffctOnMe(NetBattlefield bf, NetSkill skill, Multitype<object, object, object, object> data)
        {
            NetCard reciver = skill.GetOwner(bf);
            NetQueueItem damageSource = data.t2 as NetQueueItem;
            int reciverPosition = damageSource.CastingPosition;

            //NOTE! Because we need to be careful here we do additional limitation, 
            //(its possible to start circular trigger if eg. this does damage in response )
            NetSkill ns = damageSource.GetNetSkill(bf);
            ETriggerGroupType trigger = ns.TriggerGroup;
            if (NetType.IsNullOrEmpty(damageSource.Targets) ||
                ns.OwnerCardID != skill.OwnerCardID ||
                trigger != ETriggerGroupType.DoAttack)
            {
                //source is not regular action therefore we would go safety route and NOT do any further damages!
                return false;
            }

            //Add new effect of this skill on stack
            //reciver position is new caster position            
            NetQueueItem q2 = new NetQueueItem(bf, skill, null, null, FInt.ZERO, reciverPosition);
            bf.PutSkillOnQueue_Local(q2);

            return true;
        }
        static public bool Tri_OnDamage(NetBattlefield bf, NetSkill skill, Multitype<object, object, object, object> data)
        {
            NetCard reciver = data.t0 as NetCard;
            int reciverPosition = (int)data.t1;
            NetQueueItem damageSource = data.t2 as NetQueueItem;
            FInt damageSize = (FInt)data.t3;

            //NOTE! Because we need to be careful here we do additional limitation, 
            //(its possible to start circular trigger if eg. this does damage in response )
            ETriggerGroupType trigger = damageSource.GetNetSkill(bf).TriggerGroup;
            if (trigger != ETriggerGroupType.DoAttack &&
                 trigger != ETriggerGroupType.DoAlternateAttack &&
                 trigger != ETriggerGroupType.DoCast &&
                 trigger != ETriggerGroupType.DoCastInstant)
            {
                //source is not regular action therefore we would go safety route and NOT do any further damages!
                return false;
            }

            //OnDamage skill owner
            int id = skill.GetOwner(bf).CardID;
            //check if its the owner who was targeted
            if (reciver == null || reciver.CardID != id) return false;

            //Add new effect of this skill on stack
            //reciver position is new caster position
            NetQueueItem q2 = new NetQueueItem(bf, skill, null, null, FInt.ZERO, reciverPosition);
            q2.Data = damageSize;
            bf.PutSkillOnQueue_Local(q2);

            return true;
        }
        /// <summary>
        /// Triggers when target receives poison damage and send damage size as a Data in new skill
        /// </summary>
        static public bool Tri_OnPoisonDamage(NetBattlefield bf, NetSkill skill, Multitype<object, object, object, object> data)
        {
            NetCard reciver = data.t0 as NetCard;
            int reciverPosition = (int)data.t1;
            NetQueueItem damageSource = data.t2 as NetQueueItem;
            FInt damageSize = (FInt)data.t3;
            
            // if reciver have full HP poison effect wont work and skill won't trigger
            if (reciver.GetCA_MAX_HEALTH() == reciver.GetCA_HEALTH()) return false;
            
            SubSkillShortInfo si = damageSource.GetNetSkill(bf).GetSubSkill().shortInfo;
            if (si == null) return false;

            Tag dmgIcon = si.damageIcon; 
            DescriptionInfo di = dmgIcon.descriptionInfo;
            if (di == null) return false;

            string poisonIconName = di.iconName;
            if (string.IsNullOrEmpty(poisonIconName)) return false;

            if (!poisonIconName.Contains("DamagePoison")) return false;

            //NOTE! Because we need to be careful here we do additional limitation, 
            //(its possible to start circular trigger if eg. this does damage in response )
            ETriggerGroupType trigger = damageSource.GetNetSkill(bf).TriggerGroup;
            if (trigger != ETriggerGroupType.DoAttack &&
                 trigger != ETriggerGroupType.DoAlternateAttack &&
                 trigger != ETriggerGroupType.DoCast &&
                 trigger != ETriggerGroupType.DoCastInstant)
            {
                //source is not regular action therefore we would go safety route and NOT do any further damages!
                return false;
            }

            //OnDamage skill owner
            int id = skill.GetOwner(bf).CardID;
            //check if its the owner who was targeted
            if (reciver == null || reciver.CardID != id) return false;

            //check damage dealt with poison effect
            var ncDmgSource = bf.GetCardFromBattleSlot(damageSource.CastingPosition);
            var baseDmg = ncDmgSource.GetSkillCastingStrength(damageSource.GetNetSkill(bf));
            var poisonDamage = damageSize - baseDmg;
            if (poisonDamage <= 0 || reciver.GetCA_HEALTH() + poisonDamage <= 0) return false;

            //Add new effect of this skill on stack
            NetListInt target = new NetListInt(new List<int>());
            target.value.Add(reciverPosition);
            NetQueueItem q2 = new NetQueueItem(bf, skill, target, null, FInt.ZERO, reciverPosition);
            q2.Data = poisonDamage;
            q2.TargetsSelected = true;
            bf.PutSkillOnQueue_Local(q2);

            return true;
        }
        static public bool Tri_OnAttackDamageRetaliation(NetBattlefield bf, NetSkill skill, Multitype<object, object, object, object> data)
        {
            NetCard reciver = data.t0 as NetCard;
            int reciverPosition = (int)data.t1;
            NetQueueItem damageSource = data.t2 as NetQueueItem;
            FInt damageSize = (FInt)data.t3;
            //we do not use all data provided,
            //but data is there in case this is part of some other trigger reasoning

            //NOTE! Be careful, its possible to start circular trigger if eg. this does damage in response            

            NetSkill sourceSkill = damageSource.GetNetSkill(bf);

            //we are going to retaliate only toward enemy which have done normal form of the attack
            //not to magical attacks or other like that
            if (sourceSkill.TriggerGroup != ETriggerGroupType.DoAttack) return false;

            //OnDamage skill owner
            int id = skill.GetOwner(bf).CardID;
            //check if its the owner who was targeted
            if (reciver == null || reciver.CardID != id) return false;

            //Add new effect of this skill on stack
            //also override its targets so that it knows who it supposed to react with
            //reciver position is new caster position
            NetListInt target = new NetListInt(new List<int>());
            target.value.Add(damageSource.CastingPosition);
            NetQueueItem q2 = new NetQueueItem(bf, skill, target, null, FInt.ZERO, reciverPosition);
            q2.Data = damageSize;
            q2.TargetsSelected = true;

            bf.PutSkillOnQueue_Local(q2);

            return true;
        }
        static public bool Tri_OnFirstTwoCardsOnBf(NetBattlefield bf, NetSkill skill, Multitype<object, object, object, object> data)
        {
            NetCard reciver = data.t0 as NetCard;
            int reciverPosition = (int)data.t1;
            NetQueueItem damageSource = data.t2 as NetQueueItem;
            NetCard skillOwner = skill.GetOwner(bf);

            //negative ID and positive ID would only happen when both cards are from opposing sides
            if (skillOwner.PlayerOwnerID * reciver.PlayerOwnerID > 0) return false; 

            int skillOwnerBfPosition = bf.GetFirstBfPositionOf(skillOwner.CardID);
            if (skillOwnerBfPosition == -1) return false;

            int skillOwnerPlayerID = skillOwner.PlayerOwnerID;
            List<int> livingEnemies = bf.GetOpposingLivingCardsBFPositions(skillOwnerPlayerID);

            if (livingEnemies.Count > 2)
            {
                return false;
            }

            //Add new effect of this skill on stack
            NetListInt target = new NetListInt(new List<int>());
            target.value.Add(reciverPosition);
            NetQueueItem q2 = new NetQueueItem(bf, skill, target, null, FInt.ZERO, skillOwnerBfPosition);
            q2.TargetsSelected = true;
            bf.PutSkillOnQueue_Local(q2);

            return true;
        }
        static public bool Tri_OnMoreThanTwoCardsOnBf(NetBattlefield bf, NetSkill skill, Multitype<object, object, object, object> data)
        {
            NetCard reciver = data.t0 as NetCard;
            int reciverPosition = (int)data.t1;
            NetQueueItem damageSource = data.t2 as NetQueueItem;
            NetCard skillOwner = skill.GetOwner(bf);

            //negative ID and positive ID would only happen when both cards are from opposing sides
            if (skillOwner.PlayerOwnerID * reciver.PlayerOwnerID > 0) return false;

            int skillOwnerBfPosition = bf.GetFirstBfPositionOf(skillOwner.CardID);
            if (skillOwnerBfPosition == -1) return false;

            int skillOwnerPlayerID = skillOwner.PlayerOwnerID;
            List<int> livingEnemies = bf.GetOpposingLivingCardsBFPositions(skillOwnerPlayerID);

            if (livingEnemies.Count <= 2)
            {
                return false;
            }

            //Add new effect of this skill on stack
            //reciver position is new caster position
            NetListInt target = new NetListInt(new List<int>());
            target.value.Add(reciverPosition);
            NetQueueItem q2 = new NetQueueItem(bf, skill, target, null, FInt.ZERO, skillOwnerBfPosition);
            q2.TargetsSelected = true;
            bf.PutSkillOnQueue_Local(q2);

            return true;
        }
        #endregion

        #region TRG - Targets Scripts

        // Targeting work for spells, items and character skills
        static public List<int> Trg_WoundedFriendlyTarget(NetBattlefield bf, NetSkill ns, int bfPosition)
        {
            if (bf == null || ns == null) return null;

            NetCard nc = bf.GetCardByID(ns.OwnerCardID);

            if (nc == null) return null;

            List<int> livingFriendlies = bf.GetOwnLivingCardsBFPositions(nc.PlayerOwnerID);
            List<int> targets = null;
            foreach (var v in livingFriendlies)
            {
                NetCard nc2 = bf.GetCardFromBattleSlot(v);
                if (nc2 != null && nc2.GetCA_MAX_HEALTH() > nc2.GetCA_HEALTH())
                {
                    if (targets == null) targets = new List<int>();
                    targets.Add(v);
                }
            }

            return targets;
        }
        static public List<int> Trg_FriendlyTarget(NetBattlefield bf, NetSkill ns, int bfPosition)
        {
            if (bf == null || ns == null) return null;

            NetCard nc = bf.GetCardByID(ns.OwnerCardID);

            if (nc == null) return null;

            return bf.GetOwnLivingCardsBFPositions(nc.PlayerOwnerID);
        }
        static public List<int> Trg_OwnBattlefield(NetBattlefield bf, NetSkill ns, int bfPosition)
        {
            if (bf == null || ns == null) return null;

            NetCard nc = bf.GetCardByID(ns.OwnerCardID);

            if (nc == null) return null;

            List<int> livingFriendlies = bf.GetOwnLivingCardsBFPositions(nc.PlayerOwnerID);

            return livingFriendlies;
        }
        static public List<int> Trg_Self(NetBattlefield bf, NetSkill ns, int bfPosition)
        {
            if (bf == null || ns == null) return null;

            if (bf.IsBattleSlot(bfPosition))
            {
                return new List<int>() { bfPosition };
            }

            return new List<int>() { ns.OwnerCardID };
        }

        static public List<int> Trg_SelfAllBfPositions(NetBattlefield bf, NetSkill ns, int bfPosition)
        {
            if (bf == null || ns == null) return null;

            NetCard nc = bf.GetCardByID(ns.OwnerCardID);
            List<int> selfPositions = new List<int>();
            var ownPositions = bf.GetOwnLivingCardsBFPositions(nc.PlayerOwnerID);

            // searching for positions of owner on battlefield
            foreach(var pos in ownPositions)
            {
                if (bf.ConvertPositionIDToCard(pos).CardID == ns.OwnerCardID)
                {
                    selfPositions.Add(pos);
                }
            }

            if (selfPositions.Count > 0)
            {
                return selfPositions;
            }

            return new List<int>() { ns.OwnerCardID };
        }

        static public List<int> Trg_MeleeTargeting(NetBattlefield bf, NetSkill ns, int bfPosition)
        {
            if (bf == null || ns == null) return null;

            NetCard nc = bf.GetCardByID(ns.OwnerCardID);

            if (nc == null) return null;

            //this skill should work only when activated from battlefield
            if (!bf.IsBattleSlot(bfPosition)) return null;

            int meleeEnemySlotInFront = bf.GetOpposingMeleeSlotInFront(bfPosition);

            NetCard ncEnemy = bf.GetCardFromBattleSlot(meleeEnemySlotInFront);

            if (ncEnemy == null)
            {
                int rangedEnemySlotInFront = bf.GetOpposingRangedSlotInFront(bfPosition);
                ncEnemy = bf.GetCardFromBattleSlot(rangedEnemySlotInFront);
                if (ncEnemy == null)
                {
                    return TrgU_FallbackTargets(bf, ns, nc);
                }
                else
                {
                    return new List<int>() { rangedEnemySlotInFront };
                }
            }
            else
            {
                return new List<int>() { meleeEnemySlotInFront };
            }
        }

        static public List<int> Trg_SpearOnlyTargeting(NetBattlefield bf, NetSkill ns, int bfPosition)
        {
            if (bf == null || ns == null) return null;

            NetCard nc = bf.GetCardByID(ns.OwnerCardID);

            if (nc == null) return null;

            //this skill should work only when activated from battlefield
            if (!bf.IsBattleSlot(bfPosition)) return null;

            int meleeEnemySlotInFront = bf.GetOpposingMeleeSlotInFront(bfPosition);
            NetCard ncEnemy = bf.GetCardFromBattleSlot(meleeEnemySlotInFront);
            if (ncEnemy != null)
            {
                return new List<int>() { meleeEnemySlotInFront }; 
            }

            int back = bf.GetOwnSlotRangedForMelee(meleeEnemySlotInFront);
            ncEnemy = bf.GetCardFromBattleSlot(back);
            if (ncEnemy != null)
            {
                return new List<int>() { back };
            }

            return null;
        }
        static public List<int> Trg_RangeTargeting(NetBattlefield bf, NetSkill ns, int bfPosition)
        {
            if (bf == null || ns == null) return null;

            NetCard nc = bf.GetCardByID(ns.OwnerCardID);

            if (nc == null) return null;

            //this skill should work only when activated from battlefield
            if (!bf.IsBattleSlot(bfPosition)) return null;

            //defender in front of the shooter
            int meleeEnemySlotInFront = bf.GetOpposingMeleeSlotInFront(bfPosition);
            NetCard ncEnemy = bf.GetCardFromBattleSlot(meleeEnemySlotInFront);
            if (ncEnemy != null)
            {
                return new List<int>() { meleeEnemySlotInFront };
            }

            return TrgU_FallbackTargets(bf, ns, nc);
        }
        static public List<int> Trg_OpponentBattlefieldOrHandPositions(NetBattlefield bf, NetSkill ns, int bfPosition)
        {
            if (bf == null || ns == null) return null;

            NetCard nc = bf.GetCardByID(ns.OwnerCardID);

            if (nc == null) return null;

            var oppBfPos = bf.GetOpposingLivingCardsBFPositions(nc.PlayerOwnerID);
            if(oppBfPos.Count > 0)
            {
                return oppBfPos;
            }
            return TrgU_FallbackTargets(bf, ns, nc);
        }
        static public List<int> Trg_AllBattlefieldPositions(NetBattlefield bf, NetSkill ns, int bfPosition)
        {
            if (bf == null || ns == null) return null;

            NetCard nc = bf.GetCardByID(ns.OwnerCardID);

            if (nc == null) return null;

            List<int> a = bf.GetOpposingLivingCardsBFPositions(nc.PlayerOwnerID);
            List<int> b = bf.GetOwnLivingCardsBFPositions(nc.PlayerOwnerID);
            List<int> list = new List<int>(a);
            list.AddRange(b);

            return list;
        }
        static public List<int> Trg_AllBattlefieldPositionsExceptCaster(NetBattlefield bf, NetSkill ns, int bfPosition)
        {
            if (bf == null || ns == null) return null;

            NetCard nc = bf.GetCardByID(ns.OwnerCardID);

            if (nc == null) return null;

            List<int> a = bf.GetOpposingLivingCardsBFPositions(nc.PlayerOwnerID);
            List<int> b = bf.GetOwnLivingCardsBFPositions(nc.PlayerOwnerID, nc.CardID);

            if (a.Count == 0) return null;

            List<int> list = new List<int>(a);
            list.AddRange(b);
            
            return list;
        }
        static public List<int> Trg_OwnFreePositions(NetBattlefield bf, NetSkill ns, int bfPosition)
        {
            if (bf == null || ns == null) return null;

            NetCard nc = bf.GetCardByID(ns.OwnerCardID);
            if (nc == null) return null;

            if (nc.PlayerOwnerID == -1)
            {
                List<int> l = new List<int>(bf.GetFreeEnemyRangedSlots(false));
                l.AddRange(bf.GetFreeEnemyMeleeSlots(false));
                return l;
            }
            else
            {
                List<int> l = new List<int>(bf.GetFreeFriendlyMeleeSlots(false));
                l.AddRange(bf.GetFreeFriendlyRangedSlots(false));
                return l;
            }
        }
        static public List<int> Trg_OwnFreePositionsSummonOnBF(NetBattlefield bf, NetSkill ns, int bfPosition)
        {
            if (bf == null || ns == null) return null;

            NetCard nc = bf.GetCardByID(ns.OwnerCardID);
            if (nc == null) return null;

            var list = bf.GetCardsOnBattelfield(nc.PlayerOwnerID > 0);
            foreach (NetCard v in list)
            {
                if (v.SummonedBySkillID == ns.SkillInBattleID)
                {
                    //summon already on bf
                    return null;
                }
            }

            if (nc.PlayerOwnerID == -1)
            {
                List<int> l = new List<int>(bf.GetFreeEnemyRangedSlots(false));
                l.AddRange(bf.GetFreeEnemyMeleeSlots(false));
                return l;
            }
            else
            {
                List<int> l = new List<int>(bf.GetFreeFriendlyMeleeSlots(false));
                l.AddRange(bf.GetFreeFriendlyRangedSlots(false));
                return l;
            }
        }
        static public List<int> Trg_OwnFreePositionsCasterOnBF(NetBattlefield bf, NetSkill ns, int bfPosition)
        {
            if (bf == null || ns == null) return null;

            NetCard nc = bf.GetCardByID(ns.OwnerCardID);
            if (nc == null) return null;

            // check if caster Card is on Battlefield
            HashSet<NetCard> livingFrendlyCreatures = bf.GetCardsOnBattelfield(nc.PlayerOwnerID);
            bool isOnBF = false;
            foreach (NetCard creature in livingFrendlyCreatures)
            {
                if (nc == creature) isOnBF = true;
            }
            if (!isOnBF) return null;

            if (nc.PlayerOwnerID == -1)
            {
                List<int> l = new List<int>(bf.GetFreeEnemyRangedSlots(false));
                l.AddRange(bf.GetFreeEnemyMeleeSlots(false));
                return l;
            }
            else
            {
                List<int> l = new List<int>(bf.GetFreeFriendlyMeleeSlots(false));
                l.AddRange(bf.GetFreeFriendlyRangedSlots(false));
                return l;
            }
        }
        static public List<int> Trg_OwnFreeMeleePositions(NetBattlefield bf, NetSkill ns, int bfPosition)
        {
            if (bf == null || ns == null) return null;

            NetCard nc = bf.GetCardByID(ns.OwnerCardID);
            if (nc == null) return null;

            if (nc.PlayerOwnerID == -1)
            {
                List<int> l = new List<int>(bf.GetFreeEnemyMeleeSlots(false));
                return l;
            }
            else
            {
                List<int> l = new List<int>(bf.GetFreeFriendlyMeleeSlots(false));
                return l;
            }
        }
        static public List<int> Trg_OwnFreeMeleePositionsCasterOnBF(NetBattlefield bf, NetSkill ns, int bfPosition)
        {
            if (bf == null || ns == null) return null;

            NetCard nc = bf.GetCardByID(ns.OwnerCardID);
            if (nc == null) return null;

            // check if caster Card is on Battlefield
            HashSet<NetCard> livingFrendlyCreatures = bf.GetCardsOnBattelfield(nc.PlayerOwnerID);
            bool isOnBF = false;
            foreach (NetCard creature in livingFrendlyCreatures)
            {
                if (nc == creature) isOnBF = true;
            }
            if (!isOnBF) return null;

            if (nc.PlayerOwnerID == -1)
            {
                List<int> l = new List<int>(bf.GetFreeEnemyMeleeSlots(false));
                return l;
            }
            else
            {
                List<int> l = new List<int>(bf.GetFreeFriendlyMeleeSlots(false));
                return l;
            }
        }
        static public List<int> Trg_OwnFreeRangedPositions(NetBattlefield bf, NetSkill ns, int bfPosition)
        {
            if (bf == null || ns == null) return null;

            NetCard nc = bf.GetCardByID(ns.OwnerCardID);
            if (nc == null) return null;

            if (nc.PlayerOwnerID == -1)
            {
                List<int> l = new List<int>(bf.GetFreeEnemyRangedSlots(false));
                return l;
            }
            else
            {
                List<int> l = new List<int>(bf.GetFreeFriendlyRangedSlots(false));
                return l;
            }
        }
        static public List<int> Trg_OwnFreeRangePositionsCasterOnBF(NetBattlefield bf, NetSkill ns, int bfPosition)
        {
            if (bf == null || ns == null) return null;

            NetCard nc = bf.GetCardByID(ns.OwnerCardID);
            if (nc == null) return null;

            // check if caster Card is on Battlefield
            HashSet<NetCard> livingFrendlyCreatures = bf.GetCardsOnBattelfield(nc.PlayerOwnerID);
            bool isOnBF = false;
            foreach (NetCard creature in livingFrendlyCreatures)
            {
                if (nc == creature) isOnBF = true;
            }
            if (!isOnBF) return null;

            if (nc.PlayerOwnerID == -1)
            {
                List<int> l = new List<int>(bf.GetFreeEnemyRangedSlots(false));
                return l;
            }
            else
            {
                List<int> l = new List<int>(bf.GetFreeFriendlyRangedSlots(false));
                return l;
            }
        }
        static public List<int> Trg_OwnFreeRangePositionsSummonNotOnBF(NetBattlefield bf, NetSkill ns, int bfPosition)
        {
            if (bf == null || ns == null) return null;

            NetCard nc = bf.GetCardByID(ns.OwnerCardID);
            if (nc == null) return null;

            var list = bf.GetCardsOnBattelfield(nc.PlayerOwnerID > 0);
            foreach (NetCard v in list)
            {
                if (v.SummonedBySkillID == ns.SkillInBattleID)
                {
                    //summon already on bf
                    return null;
                }
            }

            if (nc.PlayerOwnerID == -1)
            {
                List<int> l = new List<int>(bf.GetFreeEnemyRangedSlots(false));
                return l;
            }
            else
            {
                List<int> l = new List<int>(bf.GetFreeFriendlyRangedSlots(false));
                return l;
            }
        }
        static public List<int> Trg_OwnFreeMeleePositionsSummonNotOnBF(NetBattlefield bf, NetSkill ns, int bfPosition)
        {
            if (bf == null || ns == null) return null;

            NetCard nc = ns.GetOwner(bf);
            var list = bf.GetCardsOnBattelfield(nc.PlayerOwnerID > 0);
            foreach (NetCard v in list)
            {
                if(v.SummonedBySkillID == ns.SkillInBattleID)
                {
                    //summon already on bf
                    return null;
                }
            }

            if (nc.PlayerOwnerID == -1)
            {
                List<int> l = new List<int>(bf.GetFreeEnemyMeleeSlots(false));
                return l;
            }
            else
            {
                List<int> l = new List<int>(bf.GetFreeFriendlyMeleeSlots(false));
                return l;
            }
        }
        #endregion

        #region STRG - Splash / Secondary Targets Scripts
        static public List<int> STrg_MeleeHorizontalSplash(NetBattlefield bf, NetSkill ns, int bfPosition, List<int> primaryTargets)
        {
            if (bf == null || ns == null) return null;

            if (primaryTargets == null || primaryTargets.Count != 1)
            {
                return null;
            }

            //this skill should work only when activated from melee range
            if (!bf.IsBattleSlot(bfPosition) || !bf.IsMeleeSlot(bfPosition)) return null;

            int mainTargetPosition = primaryTargets[0];

            int lSlot = bf.GetSlotOnLeftHandSide(mainTargetPosition);
            int rSlot = bf.GetSlotOnRightHandSide(mainTargetPosition);
            List<int> secondaryTargets = null;

            if (bf.IsBattleSlot(lSlot))
            {
                secondaryTargets = new List<int>();
                secondaryTargets.Add(lSlot);
            }

            if (bf.IsBattleSlot(rSlot))
            {
                if (secondaryTargets == null) secondaryTargets = new List<int>();
                secondaryTargets.Add(rSlot);
            }
            return secondaryTargets;
        }
        static public List<int> STrg_HorizontalSplash(NetBattlefield bf, NetSkill ns, int bfPosition, List<int> primaryTargets)
        {
            if (bf == null || ns == null) return null;

            if (primaryTargets == null || primaryTargets.Count != 1)
            {
                return null;
            }
            
            int mainTargetPosition = primaryTargets[0];

            int lSlot = bf.GetSlotOnLeftHandSide(mainTargetPosition);
            int rSlot = bf.GetSlotOnRightHandSide(mainTargetPosition);
            List<int> secondaryTargets = null;

            if (bf.IsBattleSlot(lSlot))
            {
                secondaryTargets = new List<int>();
                secondaryTargets.Add(lSlot);
            }

            if (bf.IsBattleSlot(rSlot))
            {
                if (secondaryTargets == null) secondaryTargets = new List<int>();
                secondaryTargets.Add(rSlot);
            }
            return secondaryTargets;
        }
        static public List<int> STrg_MeleeVerticalSplash(NetBattlefield bf, NetSkill ns, int bfPosition, List<int> primaryTargets)
        {
            if (bf == null || ns == null) return null;

            if (primaryTargets == null || primaryTargets.Count != 1)
            {
                return null;
            }

            //this skill should work only when activated from melee range
            if (!bf.IsBattleSlot(bfPosition) || !bf.IsMeleeSlot(bfPosition)) return null;

            int mainTargetPosition = primaryTargets[0];

            int back;
            if (bf.IsMeleeSlot(mainTargetPosition))
            {
                back = bf.GetOwnSlotRangedForMelee(mainTargetPosition);
            }
            else
            {
                back = bf.GetOwnSlotMeleeForRanged(mainTargetPosition);
            }
            List<int> secondaryTargets = null;

            if (bf.IsBattleSlot(back))
            {
                secondaryTargets = new List<int>();
                secondaryTargets.Add(back);
            }

            return secondaryTargets;
        }
        static public List<int> STrg_MeleeVerticalSplashWholeColumn(NetBattlefield bf, NetSkill ns, int bfPosition, List<int> primaryTargets)
        {
            if (bf == null || ns == null) return null;

            if (primaryTargets == null || primaryTargets.Count != 1)
            {
                return null;
            }

            //this skill should work only when activated from Battlefield
            if (!bf.IsBattleSlot(bfPosition)) return null;

            int mainTargetPosition = primaryTargets[0];

            int back;
            if (bf.IsMeleeSlot(mainTargetPosition))
            {
                back = bf.GetOwnSlotRangedForMelee(mainTargetPosition);
            }
            else
            {
                return null;
            }
            List<int> secondaryTargets = null;

            if (bf.IsBattleSlot(back))
            {
                secondaryTargets = new List<int>();
                secondaryTargets.Add(back);
            }

            return secondaryTargets;
        }

        static public List<int> STrg_ExplosionAroundTarget(NetBattlefield bf, NetSkill ns, int bfPosition, List<int> primaryTargets)
        {
            if (bf == null || ns == null) return null;

            if (primaryTargets == null || primaryTargets.Count != 1)
            {
                return null;
            }

            int mainTargetPosition = primaryTargets[0];

            //this skill should work only when activated on battleslot
            if (!bf.IsBattleSlot(mainTargetPosition)) return null;

            int lSlot = bf.GetSlotOnLeftHandSide(mainTargetPosition);  // get slots around main target
            int rSlot = bf.GetSlotOnRightHandSide(mainTargetPosition);

            int aSlot = bf.GetSlotOnLeftHandSide(mainTargetPosition - bf.BattlefieldSize);
            int bSlot = mainTargetPosition - bf.BattlefieldSize;
            int cSlot = bf.GetSlotOnRightHandSide(mainTargetPosition - bf.BattlefieldSize);

            int dSlot = bf.GetSlotOnLeftHandSide(mainTargetPosition + bf.BattlefieldSize);
            int eSlot = mainTargetPosition + bf.BattlefieldSize;
            int fSlot = bf.GetSlotOnRightHandSide(mainTargetPosition + bf.BattlefieldSize);

            List<int> secondaryTargets = new List<int>() { lSlot, rSlot, aSlot, bSlot, cSlot, dSlot, eSlot, fSlot };
            secondaryTargets = secondaryTargets.FindAll(o => bf.IsBattleSlot(o) && o != -1);

            return secondaryTargets;
        }
        static public List<int> STrg_MeleeTSplash(NetBattlefield bf, NetSkill ns, int bfPosition, List<int> primaryTargets)
        {
            if (bf == null || ns == null) return null;

            if (primaryTargets == null || primaryTargets.Count != 1)
            {
                return null;
            }

            //this skill should work only when activated from melee range
            if (!bf.IsBattleSlot(bfPosition) || !bf.IsMeleeSlot(bfPosition)) return null;

            int mainTargetPosition = primaryTargets[0];
            
            int back;
            if(bf.IsMeleeSlot(mainTargetPosition))
            {
                back = bf.GetOwnSlotRangedForMelee(mainTargetPosition);
            }
            else
            {
                back = bf.GetOwnSlotMeleeForRanged(mainTargetPosition);
            }
            
            int lSlot = bf.GetSlotOnLeftHandSide(mainTargetPosition);
            int rSlot = bf.GetSlotOnRightHandSide(mainTargetPosition);

            List<int> secondaryTargets = null;

            if (bf.IsBattleSlot(back))
            {
                secondaryTargets = new List<int>();
                secondaryTargets.Add(back);
            }            
            if (bf.IsBattleSlot(lSlot))
            {
                if (secondaryTargets == null) secondaryTargets = new List<int>();
                secondaryTargets.Add(lSlot);
            }
            if (bf.IsBattleSlot(rSlot))
            {
                if (secondaryTargets == null) secondaryTargets = new List<int>();
                secondaryTargets.Add(rSlot);
            }

            return secondaryTargets;
        }
        static public List<int> STrg_TSplash(NetBattlefield bf, NetSkill ns, int bfPosition, List<int> primaryTargets)
        {
            if (bf == null || ns == null) return null;

            if (primaryTargets == null || primaryTargets.Count != 1)
            {
                return null;
            }

            //this skill should work only when activated from melee range
            if (!bf.IsBattleSlot(bfPosition)) return null;

            int mainTargetPosition = primaryTargets[0];

            int back;
            if (bf.IsMeleeSlot(mainTargetPosition))
            {
                back = bf.GetOwnSlotRangedForMelee(mainTargetPosition);
            }
            else
            {
                back = bf.GetOwnSlotMeleeForRanged(mainTargetPosition);
            }

            int lSlot = bf.GetSlotOnLeftHandSide(mainTargetPosition);
            int rSlot = bf.GetSlotOnRightHandSide(mainTargetPosition);

            List<int> secondaryTargets = null;

            if (bf.IsBattleSlot(back))
            {
                secondaryTargets = new List<int>();
                secondaryTargets.Add(back);
            }
            if (bf.IsBattleSlot(lSlot))
            {
                if (secondaryTargets == null) secondaryTargets = new List<int>();
                secondaryTargets.Add(lSlot);
            }
            if (bf.IsBattleSlot(rSlot))
            {
                if (secondaryTargets == null) secondaryTargets = new List<int>();
                secondaryTargets.Add(rSlot);
            }

            return secondaryTargets;
        }
        static public List<int> STrg_ShieldsUpSplash(NetBattlefield bf, NetSkill ns, int bfPosition, List<int> primaryTargets)
        {
            if (bf == null || ns == null) return null;

            if (primaryTargets == null || primaryTargets.Count != 1)
            {
                return null;
            }

            int mainTargetPosition = primaryTargets[0];

            //this skill should work only when activated on battleslot
            if (!bf.IsBattleSlot(mainTargetPosition) ) return null;

            int mainCard = bf.BattlefieldPositions.value[mainTargetPosition];

            List<int> secondaryTargets = null;
           // if (mainCard == ns.OwnerCardID)
            //{
                int lSlot = bf.GetSlotOnLeftHandSide(mainTargetPosition);
                int rSlot = bf.GetSlotOnRightHandSide(mainTargetPosition);

                if (bf.IsBattleSlot(lSlot))
                {
                    if (secondaryTargets == null) secondaryTargets = new List<int>();
                    secondaryTargets.Add(lSlot);
                }
                if (bf.IsBattleSlot(rSlot))
                {
                    if (secondaryTargets == null) secondaryTargets = new List<int>();
                    secondaryTargets.Add(rSlot);
                }
           // }

            return secondaryTargets;
        }
        static public List<int> STrg_FriendlyTargetsExcludingSelf(NetBattlefield bf, NetSkill ns, int bfPosition, List<int> primaryTargets)
        {
            if (bf == null || ns == null) return null;

            NetCard nc = bf.GetCardByID(ns.OwnerCardID);

            if (nc == null) return null;            
            return bf.GetOwnLivingCardsBFPositions(nc.PlayerOwnerID, nc.CardID);
        }
        #endregion

        #region ACT - Activation Scripts        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bf">general battlefield information</param>
        /// <param name="q">queue element which is already executed which trigers this skill</param>
        /// <param name="stack">Items which are still in stack including "q", which most likely is first element</param>
        /// <param name="previousItems">Queue items which were already executed in order they were executed</param>
        /// <param name="random">random generator specific to this thread</param>
        /// <returns></returns>   
        /// <summary>
        /// Basic damage
        /// </summary>
        static public object Act_Damage(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {
            NetSkill ns = q.GetNetSkill(bf);
            NetCard owner = bf.GetCardByID(ns.OwnerCardID);
            
            NetCard target = null;
            FInt damage = owner.GetSkillCastingStrength(ns);
            if (NetTypeAtomic.IsNullOrEmpty(q.Targets)) return null;            

            foreach (var v in q.Targets.value)
            {
                target = bf.ConvertPositionIDToCard(v);
                if (target != null)
                {
                    target.ReciveNormalDamage(damage, bf, q, v);
                }
            }

            //do splash if needed
            if (NetTypeAtomic.IsNullOrEmpty(q.SecondaryTargets)) return null;
            
            damage = damage / 2;

            foreach(var v in q.SecondaryTargets.value)
            {
                target = bf.ConvertPositionIDToCard(v);
                if (target != null)
                {
                    target.ReciveNormalDamage(damage, bf, q, v);
                }
            }
            return null;
        }
        /// <summary>
        /// Basic damage and Increase of Delay
        /// </summary>
        static public object Act_DamageAndStun(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {
            NetSkill ns = q.GetNetSkill(bf);
            NetCard owner = bf.GetCardByID(ns.OwnerCardID);
            
            NetCard target = null;
            FInt damage = owner.GetSkillCastingStrength(ns);
            FInt stun = ns.GetFloatAttribute("Stun");

            if (NetTypeAtomic.IsNullOrEmpty(q.Targets)) return null;            

            foreach (var v in q.Targets.value)
            {
                target = bf.ConvertPositionIDToCard(v);
                if (target != null)
                {
                    target.ReciveNormalDamage(damage, bf, q, v);
                    NetQueueItem opQ = bf.GetNetQItemFromBFPosition(v);
                    if (opQ != null) opQ.Delay += stun;
                }
            }

            //do splash if needed
            if (NetTypeAtomic.IsNullOrEmpty(q.SecondaryTargets)) return null;
            
            damage = damage / 2;

            foreach (var v in q.SecondaryTargets.value)
            {
                target = bf.ConvertPositionIDToCard(v);
                if (target != null)
                {
                    target.ReciveNormalDamage(damage, bf, q, v);
                    NetQueueItem opQ = bf.GetNetQItemFromBFPosition(v);
                    if (opQ != null) opQ.Delay += stun;
                }
            }

            return null;
        }
        /// <summary>
        /// Increase of Delay
        /// </summary>
        static public object Act_Stun(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {
            NetSkill ns = q.GetNetSkill(bf);
            NetCard owner = bf.GetCardByID(ns.OwnerCardID);
            
            NetCard target = null;            
            FInt stun = ns.GetFloatAttribute("Stun");

            if (NetTypeAtomic.IsNullOrEmpty(q.Targets)) return null;

            foreach (var v in q.Targets.value)
            {
                target = bf.ConvertPositionIDToCard(v);
                if (target != null)
                {                    
                    NetQueueItem opQ = bf.GetNetQItemFromBFPosition(v);
                    if (opQ != null) opQ.Delay += stun;
                }
            }

            //do splash if needed
            if (NetTypeAtomic.IsNullOrEmpty(q.SecondaryTargets)) return null;
            
            foreach (var v in q.SecondaryTargets.value)
            {
                target = bf.ConvertPositionIDToCard(v);
                if (target != null)
                {
                    NetQueueItem opQ = bf.GetNetQItemFromBFPosition(v);
                    if (opQ != null) opQ.Delay += stun;
                }
            }

            return null;
        }
        /// <summary>
        /// Damage bypassing shield
        /// </summary>
        static public object Act_TrueDamageAncient(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {
            NetSkill ns = q.GetNetSkill(bf);
            NetCard owner = bf.GetCardByID(ns.OwnerCardID);
            
            NetCard target = null;
            
            if (NetTypeAtomic.IsNullOrEmpty(q.Targets)) return null;
            FInt damage = owner.GetSkillCastingStrength(ns);

            foreach (var v in q.Targets.value)
            {
                target = bf.ConvertPositionIDToCard(v);
                if (target != null)
                {
                    if (target.GetCA_SHIELD() > 0)
                    {
                        target.ReciveTrueDamageAncient(damage, bf, q, v);
                    } else
                    {
                        target.ReciveNormalDamage(damage, bf, q, v);
                    }
                }
            }
            
            if (NetTypeAtomic.IsNullOrEmpty(q.SecondaryTargets)) return null;
            
            damage = damage / 2;

            foreach (var v in q.SecondaryTargets.value)
            {
                target = bf.ConvertPositionIDToCard(v);
                if (target != null)
                {
                    if (target.GetCA_SHIELD() > 0)
                    {
                        target.ReciveTrueDamageAncient(damage, bf, q, v);
                    }
                    else
                    {
                        target.ReciveNormalDamage(damage, bf, q, v);
                    }
                }
            }
            

            return null;
        }
        /// <summary>
        /// Basic damage, 1.5x damage if target has shield
        /// </summary>
        static public object Act_TrueDamageEssence(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {
            NetSkill ns = q.GetNetSkill(bf);
            NetCard owner = bf.GetCardByID(ns.OwnerCardID);

            NetCard target = null;
            
            if (NetTypeAtomic.IsNullOrEmpty(q.Targets)) return null;
            FInt damage = owner.GetSkillCastingStrength(ns);

            foreach (var v in q.Targets.value)
            {
                target = bf.ConvertPositionIDToCard(v);
                if (target != null)
                {
                    target.ReciveTrueDamageEssence(damage, bf, q, v);
                }
            }

            if (NetTypeAtomic.IsNullOrEmpty(q.SecondaryTargets)) return null;

            damage = damage / 2;

            foreach (var v in q.SecondaryTargets.value)
            {
                target = bf.ConvertPositionIDToCard(v);
                if (target != null)
                {
                    target.ReciveTrueDamageEssence(damage, bf, q, v);
                }
            }

            return null;
        }
        /// <summary>
        /// Basic damage, 0.5x damage if target has shield
        /// </summary>
        static public object Act_DamageDragonBreath(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {
            NetSkill ns = q.GetNetSkill(bf);
            NetCard owner = bf.GetCardByID(ns.OwnerCardID);

            NetCard target = null;
            FInt damage = owner.GetSkillCastingStrength(ns);
            if (NetTypeAtomic.IsNullOrEmpty(q.Targets)) return null;

            foreach (var v in q.Targets.value)
            {
                target = bf.ConvertPositionIDToCard(v);
                if (target != null)
                {
                    if (target.GetCA_SHIELD() > 0)
                    {
                        target.ReciveNormalDamage(damage / 2, bf, q, v);
                    }
                    else
                    {
                        target.ReciveNormalDamage(damage, bf, q, v);
                    }
                }
            }

            //do splash if needed
            if (NetTypeAtomic.IsNullOrEmpty(q.SecondaryTargets)) return null;

            damage = damage / 2;

            foreach (var v in q.SecondaryTargets.value)
            {
                target = bf.ConvertPositionIDToCard(v);
                if (target != null)
                {
                    if (target.GetCA_SHIELD() > 0)
                    {
                        target.ReciveNormalDamage(damage / 2, bf, q, v);
                    }
                    else
                    {
                        target.ReciveNormalDamage(damage, bf, q, v);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Basic damage, 1.35x damage if target is wounded
        /// </summary>
        static public object Act_PoisonEssenceDamage(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {
            NetSkill ns = q.GetNetSkill(bf);
            NetCard owner = bf.GetCardByID(ns.OwnerCardID);
            
            NetCard target = null;
            FInt damage = owner.GetSkillCastingStrength(ns);

            if (NetTypeAtomic.IsNullOrEmpty(q.Targets)) return null;

            foreach (int t in q.Targets.value)
            {
                target = bf.ConvertPositionIDToCard(t);
                if (target == null) continue;

                if (target.GetCA_MAX_HEALTH() > target.GetCA_HEALTH())
                {
                    target.ReciveNormalDamage(damage *1.35f, bf, q, t);
                }
                else
                {
                    target.ReciveNormalDamage(damage, bf, q, t);
                }
            }

            if (NetTypeAtomic.IsNullOrEmpty(q.SecondaryTargets)) return null;
            
            damage = damage / 2;

            foreach (var t in q.SecondaryTargets.value)
            {
                target = bf.ConvertPositionIDToCard(t);
                if (target == null) continue;
                
                if (target.GetCA_MAX_HEALTH() > target.GetCA_HEALTH())
                {
                    target.ReciveNormalDamage(damage * 1.35f, bf, q, t);
                }
                else
                {
                    target.ReciveNormalDamage(damage, bf, q, t);
                }                
            }
            

            return null;
        }
        /// <summary>
        /// Basic damage, 1.6x damage if target is wounded
        /// </summary>
        static public object Act_PoisonAncientDamage(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {
            NetSkill ns = q.GetNetSkill(bf);
            NetCard owner = bf.GetCardByID(ns.OwnerCardID);

            NetCard target = null;
            FInt damage = owner.GetSkillCastingStrength(ns);

            if (NetTypeAtomic.IsNullOrEmpty(q.Targets)) return null;

            foreach (int t in q.Targets.value)
            {
                target = bf.ConvertPositionIDToCard(t);
                if (target == null) continue;

                if (target.GetCA_MAX_HEALTH() > target.GetCA_HEALTH())
                {
                    target.ReciveNormalDamage(damage * 1.6f, bf, q, t);
                }
                else
                {
                    target.ReciveNormalDamage(damage, bf, q, t);
                }
            }

            if (NetTypeAtomic.IsNullOrEmpty(q.SecondaryTargets)) return null;

            damage = damage / 2;

            foreach (var t in q.SecondaryTargets.value)
            {
                target = bf.ConvertPositionIDToCard(t);
                if (target == null) continue;

                if (target.GetCA_MAX_HEALTH() > target.GetCA_HEALTH())
                {
                    target.ReciveNormalDamage(damage * 1.6f, bf, q, t);
                }
                else
                {
                    target.ReciveNormalDamage(damage, bf, q, t);
                }
            }


            return null;
        }
        /// <summary>
        /// Decrease Delay and increase CA1 of the target
        /// Note! script does not check if CA_SHIELD is set correctly as main attribute, but uses its strength!
        /// </summary>
        static public object Act_InspireEffect(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {
            NetSkill ns = q.GetNetSkill(bf);
            NetCard owner = bf.GetCardByID(ns.OwnerCardID);

            FInt inspireSpeed = ns.GetFloatAttribute("InspireSpeed");
            FInt inspirePower = ns.GetFloatAttribute("InspirePower");

            //if any attributes are used for the power calculation they are scaled down, so that 10 main attribute produces +1 boost
            if (ns.MainAtt != null)
            {
                //pow is main attribtue strength * its local multiplier if applicable
                //attribute is applicable in scale 0.1 so eg. 25 Strength  provides multiplier 2.5 to the InspirePower
                FInt pow = owner.GetSkillCastingStrength(ns);
                inspirePower = inspirePower * pow;
                inspirePower *= 0.1f;
                inspirePower.CutToInt();
            }

            NetCard target = null;

            if (NetTypeAtomic.IsNullOrEmpty(q.Targets)) return null;
            foreach (int t in q.Targets.value)
            {
                target = bf.ConvertPositionIDToCard(t);
                if (target == null) continue;

                NetQueueItem opQ = bf.GetNetQItemFromBFPosition(t);

                target.CAAdd += inspirePower;

                if (opQ != null)
                {
                    opQ.Delay -= inspireSpeed;
                }
            }

            if (NetTypeAtomic.IsNullOrEmpty(q.SecondaryTargets)) return null;
            foreach (int t in q.SecondaryTargets.value)
            {
                target = bf.ConvertPositionIDToCard(t);
                if (target == null) continue;

                NetQueueItem opQ = bf.GetNetQItemFromBFPosition(t);

                target.CAAdd += inspirePower;

                if (opQ != null)
                {
                    opQ.Delay -= inspireSpeed;
                }
            }            

            return null;
        }
        /// <summary>
        /// Increase Delay and Decrease CA1 & CA2 of the target
        /// Note! script does not check if CA_SHIELD is set correctly as main attribute, but uses its strength!
        /// </summary>
        static public object Act_UnInspireEffect(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {
            NetSkill ns = q.GetNetSkill(bf);
            NetCard owner = bf.GetCardByID(ns.OwnerCardID);

            FInt inspireSpeed = ns.GetFloatAttribute("UnInspireSpeed");
            FInt inspirePower = ns.GetFloatAttribute("UnInspirePower");

            //if any attributes are used for the power calculation they are scaled down, so that 10 main attribute produces +1 boost
            if (ns.MainAtt != null)
            {
                //pow is main attribtue strength * its local multiplier if applicable
                //attribute is applicable in scale 0.1 so eg. 25 Strength  provides multiplier 2.5 to the InspirePower
                FInt pow = owner.GetSkillCastingStrength(ns);
                inspirePower = inspirePower * pow;
                inspirePower *= 0.1f;
                inspirePower.CutToInt();
            }

            NetCard target = null;

            if (NetTypeAtomic.IsNullOrEmpty(q.Targets)) return null;
            foreach (int t in q.Targets.value)
            {
                target = bf.ConvertPositionIDToCard(t);
                if (target != null)
                {
                    NetQueueItem opQ = bf.GetNetQItemFromBFPosition(t);

                    target.CAAdd -= inspirePower;

                    if (opQ != null)
                    {
                        opQ.Delay += inspireSpeed;
                    }
                }
            }

            if (NetTypeAtomic.IsNullOrEmpty(q.SecondaryTargets)) return null;
            foreach (int t in q.SecondaryTargets.value)
            {
                target = bf.ConvertPositionIDToCard(t);
                if (target != null)
                {
                    NetQueueItem opQ = bf.GetNetQItemFromBFPosition(t);

                    target.CAAdd -= inspirePower;

                    if (opQ != null)
                    {
                        opQ.Delay += inspireSpeed;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Add shielding to target based on attributes coming only from skill
        /// </summary>
        static public object Act_AddShielding(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {
            NetSkill ns = q.GetNetSkill(bf);

            //skill needs target(-s)
            if (!NetTypeAtomic.IsValid(q.Targets)) return null;

            FInt value = ns.GetFloatAttribute("TAG-CA_SHIELD");
            string sign = ns.GetStringAttribute("TAG-CA_SHIELD");
            if (value == FInt.ZERO)
            {
                if (bf.ChallengeType == EChallengeType.TypePhysical)
                {
                    value = ns.GetFloatAttribute("TAG-SHIELDING_PHYSICAL");
                }
                else if (bf.ChallengeType == EChallengeType.TypeMental)
                {
                    value = ns.GetFloatAttribute("TAG-SHIELDING_MENTAL");
                }
                else if (bf.ChallengeType == EChallengeType.TypeSpirit)
                {
                    value = ns.GetFloatAttribute("TAG-SHIELDING_SPIRIT");
                }
            }
            if(string.IsNullOrEmpty(sign))
            {
                if (bf.ChallengeType == EChallengeType.TypePhysical)
                {
                    sign = ns.GetStringAttribute("TAG-SHIELDING_PHYSICAL");
                }
                else if (bf.ChallengeType == EChallengeType.TypeMental)
                {
                    sign = ns.GetStringAttribute("TAG-SHIELDING_MENTAL");
                }
                else if (bf.ChallengeType == EChallengeType.TypeSpirit)
                {
                    sign = ns.GetStringAttribute("TAG-SHIELDING_SPIRIT");
                }
            }

            value.CutToInt();
            if(sign == "*")
            {
                value = value * 0.01f;
            }

            foreach (var v in q.Targets.value)
            {
                NetCard target = bf.ConvertPositionIDToCard(v);
                if (target == null) continue;
                FInt prev = target.GetCA_SHIELD();

                if (sign == "+")
                {
                    target.SetCA_SHIELD(value + prev);
                }
                else if (sign == "*")
                {
                    target.SetCA_SHIELD(value * prev);
                }
            }

            if (NetType.IsNullOrEmpty(q.SecondaryTargets)) return null;
            
            foreach (var v in q.SecondaryTargets.value)
            {
                NetCard target = bf.ConvertPositionIDToCard(v);
                if (target == null) continue;
                FInt prev = target.GetCA_SHIELD();

                if (sign == "+")
                {
                    target.SetCA_SHIELD(value + prev);
                }
                else if (sign == "*")
                {
                    target.SetCA_SHIELD(value * prev);
                }
            }
            
            return null;
        }
        /// <summary>
        /// Reduce HP by 0.8 and add shielding to target based on attributes coming only from skill
        /// </summary>
        static public object Act_HpToShielding(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {
            NetSkill ns = q.GetNetSkill(bf);

            //skill needs target(-s)
            if (!NetTypeAtomic.IsValid(q.Targets)) return null;

            NetCard owner = bf.GetCardByID(ns.OwnerCardID);
            FInt newHp = owner.GetCA_HEALTH() * 0.8f;
            newHp.CutToInt();
            owner.SetCA_HEALTH(newHp);

            Act_AddShielding(bf, q, stack, previousItems, random);

            return null;
        }
        /// <summary>
        /// Decrease Delay and increase CA1 of the target
        /// Note! script does not check if CA_SHIELD is set correctly as main attribute, but uses its strength!
        /// Add shielding to target based on attributes coming only from skill
        /// </summary>
        static public object Act_InspireAndShielding(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {
            Act_InspireEffect(bf, q, stack, previousItems, random);
            Act_AddShielding(bf, q, stack, previousItems, random);

            return null;
        }
        /// <summary>
        /// Shift targets' Delay and add shielding. Note! It have to be only 2 targets
        /// </summary>
        static public object Act_ShiftDelayAndAddShield(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {
            //skill needs target(-s)
            if (NetType.IsNullOrEmpty(q.Targets)) return null;

            List<NetQueueItem> opQs = new List<NetQueueItem>(); ;
            foreach (var v in q.Targets.value)
            {
                NetQueueItem opQ = bf.GetNetQItemFromBFPosition(v);
                if (opQ == null) continue;
                opQs.Add(opQ);
            }

            //shift delays
            if (opQs.Count != 2) return null;
            var del0 = opQs[0].Delay;
            var del1 = opQs[1].Delay;
            opQs[0].Delay = del1;
            opQs[1].Delay = del0;

            Act_AddShielding(bf, q, stack, previousItems, random);

            return null;
        }
        /// <summary>
        /// Regenerates target health
        /// </summary>
        static public object Act_Heal(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {

            NetSkill ns = q.GetNetSkill(bf);
            NetCard owner = bf.GetCardByID(ns.OwnerCardID);
            
            //skill needs target(-s)
            if (NetType.IsNullOrEmpty(q.Targets)) return null;

            FInt healPower = owner.GetSkillCastingStrength(ns);

            foreach (var v in q.Targets.value)
            {
                NetCard target = bf.ConvertPositionIDToCard(v);
                if (target == null) continue;
                target.ReciveHealthNormal(healPower, bf, q, v);
            }
            if (NetType.IsNullOrEmpty(q.SecondaryTargets)) return null;

            foreach (var v in q.SecondaryTargets.value)
            {
                NetCard target = bf.ConvertPositionIDToCard(v);
                if (target == null) continue;
                target.ReciveHealthNormal(healPower, bf, q, v);
            }

            return null;
        }
        /// <summary>
        /// Regenerates target health by percentage value from skill
        /// </summary>
        static public object Act_HealPercentage(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {

            NetSkill ns = q.GetNetSkill(bf);
            NetCard owner = bf.GetCardByID(ns.OwnerCardID);

            //skill needs target(-s)
            if (NetType.IsNullOrEmpty(q.Targets)) return null;

            FInt healPower = FInt.ZERO;
            FInt maxHp = FInt.ZERO;
            foreach (var v in q.Targets.value)
            {
                NetCard target = bf.ConvertPositionIDToCard(v);
                if (target == null) continue;
                maxHp = target.GetCA_MAX_HEALTH();
                healPower = ns.MainAtt.value * maxHp;
                
                target.ReciveHealthNormal(healPower, bf, q, v);
            }
            if (NetType.IsNullOrEmpty(q.SecondaryTargets)) return null;

            foreach (var v in q.SecondaryTargets.value)
            {
                NetCard target = bf.ConvertPositionIDToCard(v);
                if (target == null) continue;
                maxHp = target.GetCA_MAX_HEALTH();
                healPower = ns.MainAtt.value * maxHp;
                
                target.ReciveHealthNormal(healPower, bf, q, v);
            }

            return null;
        }
        static public object Act_LoseHealPercentage(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {

            NetSkill ns = q.GetNetSkill(bf);
            NetCard owner = bf.GetCardByID(ns.OwnerCardID);

            //skill needs target(-s)
            if (NetType.IsNullOrEmpty(q.Targets)) return null;

            FInt losehealPower = FInt.ZERO;
            FInt maxHp = FInt.ZERO;
            foreach (var v in q.Targets.value)
            {
                NetCard target = bf.ConvertPositionIDToCard(v);
                if (target == null) continue;
                maxHp = target.GetCA_MAX_HEALTH();
                losehealPower = ns.MainAtt.value * maxHp;
                
                target.ReciveNormalDamage(losehealPower, bf, q, v);
            }
            if (NetType.IsNullOrEmpty(q.SecondaryTargets)) return null;

            foreach (var v in q.SecondaryTargets.value)
            {
                NetCard target = bf.ConvertPositionIDToCard(v);
                if (target == null) continue;
                maxHp = target.GetCA_MAX_HEALTH();
                losehealPower = ns.MainAtt.value * maxHp;
                
                target.ReciveNormalDamage(losehealPower, bf, q, v);
            }

            return null;
        }
        /// <summary>
        /// Heals amount of health delivered in NetQueueItem Data
        /// </summary>
        static public object Act_DamageHeal(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {
            NetSkill ns = q.GetNetSkill(bf);
            NetCard owner = bf.GetCardByID(ns.OwnerCardID);
            
            //skill needs target(-s)
            if (NetType.IsNullOrEmpty(q.Targets)) return null;

            FInt healPower = q.Data; 

            foreach (var v in q.Targets.value)
            {
                NetCard target = bf.ConvertPositionIDToCard(v);
                if (target == null) continue;
                target.ReciveHealthNormal(healPower, bf, q, v);
            }

            if (NetType.IsNullOrEmpty(q.SecondaryTargets)) return null;

            foreach (var v in q.SecondaryTargets.value)
            {
                NetCard target = bf.ConvertPositionIDToCard(v);
                if (target == null) continue;
                target.ReciveHealthNormal(healPower, bf, q, v);
            }

            return null;
        }
        /// <summary>
        /// Instantiate creature on target position, summon need to be predefined which is the only way to have it ready before fight
        /// </summary>
        static public object Act_Summon(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {
            if (!NetTypeAtomic.IsValid(q.Targets)) return null;
            foreach (var v in q.Targets.value)
            {
                if (bf.IsBattleSlot(v))
                {
                    NetSkill ns = q.GetNetSkill(bf);
                    NetCard nc = bf.InstantiateSummonBySkill(ns);
                    if(nc.GetCA_HEALTH() <= 0)
                    {
                        nc.SetCA_HEALTH(FInt.ONE);
                    }
                    NetSkill summonSkill = null;

                    List<NetSkill> skills = nc.GetBothRangesSkills();
                    foreach (var k in skills)
                    {
                        if (bf.IsMeleeSlot(v) && k.IsMeleeType())
                        {
                            summonSkill = k;
                            break;
                        }
                        else if (!bf.IsMeleeSlot(v) && k.IsRangedType())
                        {
                            summonSkill = k;
                            break;
                        }
                    }

                    if (summonSkill != null)
                    {
                        NetQueueItem newQ = new NetQueueItem(bf, summonSkill, null, null, FInt.ZERO, v);
                        bf.PlayCard(newQ, random);
                    }
                    else
                    {
                        bf.PlaceCardNoSkill(v, nc, random);
                    }
                }
            }

            //does not support secondary targets.... should it?

            return null;
        }
        /// <summary>
        /// Instantiate creature on target position and sacrifice 20% health of the caster; summon need to be predefined which is the only way to have it ready before fight
        /// </summary>
        static public object Act_LowBloodySummon(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {
            if (!NetTypeAtomic.IsValid(q.Targets)) return null;

            NetSkill ns = q.GetNetSkill(bf);
            NetCard owner = bf.GetCardByID(ns.OwnerCardID);

            foreach (var v in q.Targets.value)
            {
                if (bf.IsBattleSlot(v))
                {
                    NetCard nc = bf.InstantiateSummonBySkill(ns);
                    if(nc.GetCA_HEALTH() <= 0)
                    {
                        nc.SetCA_HEALTH(FInt.ONE);
                    }
                    NetSkill summonSkill = null;

                    List<NetSkill> skills = nc.GetBothRangesSkills();
                    foreach (var k in skills)
                    {
                        if (bf.IsMeleeSlot(v) && k.IsMeleeType())
                        {
                            summonSkill = k;
                            break;
                        }
                        else if (!bf.IsMeleeSlot(v) && k.IsRangedType())
                        {
                            summonSkill = k;
                            break;
                        }
                    }

                    if (summonSkill != null)
                    {
                        NetQueueItem newQ = new NetQueueItem(bf, summonSkill, null, null, FInt.ZERO, v);
                        bf.PlayCard(newQ, random);
                    }
                    else
                    {
                        bf.PlaceCardNoSkill(v, nc, random);
                    }
                }
            }
            // health sacrifice 
            FInt damage = owner.GetCA_HEALTH() / 5;
            damage.CutToInt();
            owner.ReciveDamageBypassShield(damage, bf, q, -1);

            return null;
        }
        /// <summary>
        /// Instantiate creature on target position and sacrifice half health of the caster; summon need to be predefined which is the only way to have it ready before fight
        /// </summary>
        static public object Act_BloodySummon(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {
            if (!NetTypeAtomic.IsValid(q.Targets)) return null;

            NetSkill ns = q.GetNetSkill(bf);
            NetCard owner = bf.GetCardByID(ns.OwnerCardID);

            foreach (var v in q.Targets.value)
            {
                if (bf.IsBattleSlot(v))
                {
                    NetCard nc = bf.InstantiateSummonBySkill(ns);
                    if(nc.GetCA_HEALTH() <= 0)
                    {
                        nc.SetCA_HEALTH(FInt.ONE);
                    }
                    NetSkill summonSkill = null;

                    List<NetSkill> skills = nc.GetBothRangesSkills();
                    foreach (var k in skills)
                    {
                        if (bf.IsMeleeSlot(v) && k.IsMeleeType())
                        {
                            summonSkill = k;
                            break;
                        }
                        else if (!bf.IsMeleeSlot(v) && k.IsRangedType())
                        {
                            summonSkill = k;
                            break;
                        }
                    }

                    if (summonSkill != null)
                    {
                        NetQueueItem newQ = new NetQueueItem(bf, summonSkill, null, null, FInt.ZERO, v);
                        bf.PlayCard(newQ, random);
                    }
                    else
                    {
                        bf.PlaceCardNoSkill(v, nc, random);
                    }
                }
            }
            // health sacrifice 
            FInt damage = owner.GetCA_HEALTH() / 2;
            damage.CutToInt();
            owner.ReciveDamageBypassShield(damage, bf, q, -1);

            return null;
        }
        /// <summary>
        /// Heals TARGET using health from any number secondary targets (draining them in provided order)
        /// </summary>
        static public object Act_DrainHealthFromFriends(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {
            if (!NetTypeAtomic.IsValid(q.Targets)) return null;
            if (!NetTypeAtomic.IsValid(q.SecondaryTargets)) return null;
            NetCard target = null;
            NetSkill ns = q.GetNetSkill(bf);
            int targetPos = 0;
            if (q.Targets.value.Count == 1)
            {
                target = bf.ConvertPositionIDToCard(q.Targets.value[0]);
                targetPos = q.Targets.value[0];
            }

            if (target == null) return null;

            FInt drainPower = ns.GetFloatAttribute("DrainPower");

            FInt wound = target.GetCA_MAX_HEALTH() - target.GetCA_HEALTH();
            if (wound <= 0) return null;
            FInt healStrength = FInt.ZERO;
            foreach (var v in q.SecondaryTargets.value)
            {
                if (wound <= 0) break;

                NetCard healSource = bf.ConvertPositionIDToCard(v);
                if (healSource == null) continue;

                FInt health = healSource.GetCA_HEALTH();
                if (health <= wound)
                {
                    healStrength += health;
                    healSource.ReciveDamageBypassShield(health, bf, q, v);
                    wound -= health;
                }
                else
                {
                    healStrength += wound;
                    healSource.ReciveDamageBypassShield(wound, bf, q, v);
                    wound = FInt.ZERO;
                }
            }

            if (healStrength > 0)
            {
                target.ReciveHealthNormal(healStrength * drainPower, bf, q, targetPos);
            }

            return null;
        }

        static public object Act_DrainHealthEssence(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {
            NetSkill ns = q.GetNetSkill(bf);
            NetCard owner = bf.GetCardByID(ns.OwnerCardID);

            NetCard target = null;
            FInt damage = owner.GetSkillCastingStrength(ns);

            if (NetTypeAtomic.IsNullOrEmpty(q.Targets)) return null;

            foreach (int t in q.Targets.value)
            {
                target = bf.ConvertPositionIDToCard(t);
                if (target == null) continue;

                
                target.ReciveNormalDamage(damage, bf, q, t);
                owner.ReciveHealthNormal(damage *0.4f , bf, q, t);
                
            }

            if (NetTypeAtomic.IsNullOrEmpty(q.SecondaryTargets)) return null;

            damage = damage / 2;

            foreach (var t in q.SecondaryTargets.value)
            {
                target = bf.ConvertPositionIDToCard(t);
                if (target == null) continue;

                target.ReciveNormalDamage(damage, bf, q, t);
                owner.ReciveHealthNormal(damage * 0.4f, bf, q, t);                
            }


            return null;
        }

        static public object Act_DrainHealthAncient(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {
            NetSkill ns = q.GetNetSkill(bf);
            NetCard owner = bf.GetCardByID(ns.OwnerCardID);

            NetCard target = null;
            FInt damage = owner.GetSkillCastingStrength(ns);

            if (NetTypeAtomic.IsNullOrEmpty(q.Targets)) return null;

            foreach (int t in q.Targets.value)
            {
                target = bf.ConvertPositionIDToCard(t);
                if (target == null) continue;

                if (owner.GetCA_MAX_HEALTH() > target.GetCA_HEALTH())
                {
                    target.ReciveNormalDamage(damage, bf, q, t);
                    owner.ReciveHealthNormal(damage * 3 / 10, bf, q, t);
                }
                else
                {
                    target.ReciveNormalDamage(damage, bf, q, t);
                }
            }

            if (NetTypeAtomic.IsNullOrEmpty(q.SecondaryTargets)) return null;

            damage = damage / 2;

            foreach (var t in q.SecondaryTargets.value)
            {
                target = bf.ConvertPositionIDToCard(t);
                if (target == null) continue;

                if (owner.GetCA_MAX_HEALTH() > target.GetCA_HEALTH())
                {
                    target.ReciveNormalDamage(damage, bf, q, t);
                    owner.ReciveHealthNormal(damage * 3 / 10, bf, q, t);
                }
                else
                {
                    target.ReciveNormalDamage(damage, bf, q, t);
                }
            }


            return null;
        }

        static public object Act_CopyHealth(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {
            NetSkill ns = q.GetNetSkill(bf);
            NetCard owner = bf.GetCardByID(ns.OwnerCardID);

            NetCard target = null;

            if (NetTypeAtomic.IsNullOrEmpty(q.Targets)) return null;

            foreach (int t in q.Targets.value)
            {
                target = bf.ConvertPositionIDToCard(t);
                if (target == null) continue;

                owner.SetCA_HEALTH(target.GetCA_HEALTH());

            }
            
            return null;
        }

        static public object Act_DrainCA(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {
            NetSkill ns = q.GetNetSkill(bf);
            NetCard owner = bf.GetCardByID(ns.OwnerCardID);

            FInt inspireSpeed = ns.GetFloatAttribute("UnInspireSpeed");
            FInt inspirePower = ns.GetFloatAttribute("UnInspirePower");

            //if any attributes are used for the power calculation they are scaled down, so that 10 main attribute produces +1 boost
            if (ns.MainAtt != null)
            {
                //pow is main attribtue strength * its local multiplier if applicable
                //attribute is applicable in scale 0.1 so eg. 25 Strength  provides multiplier 2.5 to the InspirePower
                FInt pow = owner.GetSkillCastingStrength(ns);
                inspirePower = inspirePower * pow;
                inspirePower *= 0.1f;
                inspirePower.CutToInt();
            }

            NetCard target = null;

            if (!NetTypeAtomic.IsNullOrEmpty(q.Targets))
            {
                foreach (int t in q.Targets.value)
                {
                    target = bf.ConvertPositionIDToCard(t);
                    if (target != null)
                    {
                        NetQueueItem opQ = bf.GetNetQItemFromBFPosition(t);

                        target.CAAdd -= inspirePower;

                        if (opQ != null)
                        {
                            opQ.Delay += inspireSpeed;
                        }
                    }
                }
            }

            if (!NetTypeAtomic.IsNullOrEmpty(q.SecondaryTargets))
            {
                foreach (int t in q.SecondaryTargets.value)
                {
                    target = bf.ConvertPositionIDToCard(t);
                    if (target != null)
                    {
                        NetQueueItem opQ = bf.GetNetQItemFromBFPosition(t);

                        target.CAAdd -= inspirePower;

                        if (opQ != null)
                        {
                            opQ.Delay += inspireSpeed;
                        }
                    }
                }
            }

            owner.CAAdd += inspirePower;
            q.Delay -= inspireSpeed;

            return null;
        }
        static public object Act_DestroyShielding(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {
            NetSkill ns = q.GetNetSkill(bf);
            NetCard target = null;
            FInt shieldDestroy = ns.GetFloatAttribute("ShieldDestroy");
            shieldDestroy.CutToInt();
            FInt shield = FInt.ZERO;

            if (NetTypeAtomic.IsNullOrEmpty(q.Targets) || shieldDestroy == FInt.ZERO) return null;

            foreach (var v in q.Targets.value)
            {
                target = bf.ConvertPositionIDToCard(v);
                if (target != null)
                {
                    shield = target.GetCA_SHIELD();
                    if(shield <= shieldDestroy)
                    {
                        target.SetCA_SHIELD(FInt.ZERO);
                    }
                    else
                    {
                        target.SetCA_SHIELD(shield - shieldDestroy);
                    }
                }
            }

            //do splash if needed
            if (NetTypeAtomic.IsNullOrEmpty(q.SecondaryTargets)) return null;

            foreach (var v in q.SecondaryTargets.value)
            {
                target = bf.ConvertPositionIDToCard(v);
                if (target != null)
                {
                    shield = target.GetCA_SHIELD();
                    if (shield <= shieldDestroy)
                    {
                        target.SetCA_SHIELD(FInt.ZERO);
                    }
                    else
                    {
                        target.SetCA_SHIELD(shield - shieldDestroy);
                    }
                }
            }
            return null;
        }
        static public object Act_BloodySacrifice(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {
            NetSkill ns = q.GetNetSkill(bf);
            NetCard owner = bf.GetCardByID(ns.OwnerCardID);

            NetCard target = null;

            if (NetTypeAtomic.IsNullOrEmpty(q.Targets)) return null;

            FInt sacrifice = ns.GetFloatAttribute("HealthSacrifice");
            FInt multipier = ns.GetFloatAttribute("DmgMulti");
            FInt sacrificeHp = (owner.GetCA_HEALTH() + owner.GetCA_SHIELD()) * sacrifice;
            sacrificeHp.CutToInt();
            FInt damage = sacrificeHp * multipier;

            // sacrifice health
            owner.ReciveTrueDamageEssence(sacrificeHp, bf, q, -1);

            // do damage
            foreach (var v in q.Targets.value)
            {
                target = bf.ConvertPositionIDToCard(v);
                if (target != null)
                {
                    target.ReciveTrueDamageEssence(damage, bf, q, v);
                }
            }

            if (NetTypeAtomic.IsNullOrEmpty(q.SecondaryTargets)) return null;

            damage = damage / 2;

            foreach (var v in q.SecondaryTargets.value)
            {
                target = bf.ConvertPositionIDToCard(v);
                if (target != null)
                {
                    target.ReciveTrueDamageEssence(damage, bf, q, v);
                }
            }

            return null;
        }
        /// <summary>
        /// Works with Tri_PainCommision. Transfer damage from secondary target to main target. Main target gets damage and secondary target get healed
        /// </summary>
        static public object Act_PainCommission(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        {
            NetSkill ns = q.GetNetSkill(bf);
            NetCard owner = bf.GetCardByID(ns.OwnerCardID);
            FInt damage = q.Data; 
            NetCard target = null;

            if (NetTypeAtomic.IsNullOrEmpty(q.Targets) || NetTypeAtomic.IsNullOrEmpty(q.SecondaryTargets)) return null;

            var friendsOnBf = bf.GetCardsOnBattelfield(owner.PlayerOwnerID);
            List<NetCard> skillOwners = new List<NetCard>();
            foreach (var creature in friendsOnBf)
            {
                if (!NetType.IsNullOrEmpty(creature.ActiveEffects))
                {
                    foreach (NetSkill ae in creature.ActiveEffects.value)
                    {
                        if (ae.SubSkillDbName == ns.SubSkillDbName)
                        {
                            skillOwners.Add(creature);
                            continue;
                        }
                    }
                }
            }
            if (skillOwners.Count > 0)
                damage = (damage / skillOwners.Count).ReturnRounded();

            // do damage for skill owner and heal attacked friendly creature
            foreach (var v in q.Targets.value)
            {
                target = bf.ConvertPositionIDToCard(v);
                if (target != null)
                {
                    var hp = target.GetCA_HEALTH();
                    //damage = new FInt(UnityEngine.Mathf.Min(hp.ToFloat(), damage.ToFloat()));
                    target.ReciveNormalDamage(damage, bf, q, v);
                }
            }

            foreach (var v in q.SecondaryTargets.value)
            {
                target = bf.ConvertPositionIDToCard(v);
                if (target != null)
                {
                    target.ReciveHealthNormal(damage, bf, q, v);
                }
            }

            return null;
        }
        #endregion


        #region UTILS
        static public List<int> TrgU_FallbackTargets(NetBattlefield bf, NetSkill ns, NetCard skillSourceCard)
        {
            List<int> set = bf.GetOpposingLivingCardsBFPositions(skillSourceCard.PlayerOwnerID);
            if (set.Count > 0)
            {
                return set;
            }
            else
            {
                NetCard caster = bf.GetCardByID(ns.OwnerCardID);
                if (caster != null)
                {
                    return BattleFieldUtils.CardListToIDs(bf.GetOpposingSideLivingCards(caster.PlayerOwnerID));
                }
            }

            return null;
        }

        static public List<int> TrgU_FallbackFromMeleeTargets(NetBattlefield bf, NetQueueItem q, NetSkill ns, NetCard skillSourceCard)
        {
            int ranged = bf.GetOpposingRangedSlotInFront(q.CastingPosition);
            if (bf.IsBattleSlot(ranged))
            {
                NetCard target = bf.GetCardFromBattleSlot(ranged);
                if (target != null) return new List<int>() { ranged };
            }


            List<int> set = bf.GetOpposingLivingCardsBFPositions(skillSourceCard.PlayerOwnerID);
            if (set.Count > 0)
            {
                return set;
            }
            else
            {
                NetCard caster = bf.GetCardByID(ns.OwnerCardID);
                if (caster != null)
                {
                    return BattleFieldUtils.CardListToIDs(bf.GetOpposingSideLivingCards(caster.PlayerOwnerID));
                }
            }

            return null;
        }
        #endregion
    }
}
#endif