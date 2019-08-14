#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR
using DBDef;
using System.Collections.Generic;
using Thea2;
using Thea2.Common;
using Thea2.General;
using TheHoney;

namespace GameScript
{
    public class ProceduralEquipmentScripts : ScriptBase
    {
        #region SI - Short Info
        static public string SI_ShieldUp(object info)
        {
            Multitype<NetCard, NetSkill, NetBattlefield> data = info as Multitype<NetCard, NetSkill, NetBattlefield>;
            if (data != null)
            {
                var ns = data.t1;
                var bf = data.t2;
                FInt value = ns.GetFloatAttribute("TAG-CA_SHIELD");
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
                    else if (bf.ChallengeType == EChallengeType.TypeMental)
                    {
                        value = ns.GetFloatAttribute("TAG-SHIELDING_SPIRIT");
                    }
                }

                return value.ToInt().ToString();
            }

            Multitype<SkillInstance, Subskill, ClientEntityCharacter> dInfo = info as Multitype<SkillInstance, Subskill, ClientEntityCharacter>;
            SkillInstance si = dInfo.t0;
            Subskill ss = dInfo.t1;
            ClientEntityCharacter character = dInfo.t2;
            var dataInWorld = si.GetCurrentSkillAttributes()[ss];

            if (dataInWorld.attributes != null && dataInWorld.attributes.Keys.Count > 0)
            {
                string st = null;
                FInt value = dataInWorld.GetFInt("TAG-CA_SHIELD");
                if (value == FInt.ZERO)
                {
                    return value.ToString();
                }

                foreach (var v in ss.challengeTypes)
                {
                    switch (v)
                    {
                        case EChallengeType.TypePhysical:
                            value = dataInWorld.GetFInt("TAG-SHIELDING_PHYSICAL");
                            break;
                        case EChallengeType.TypeMental:
                            value = dataInWorld.GetFInt("TAG-SHIELDING_MENTAL");
                            break;
                        case EChallengeType.TypeSpirit:
                            value = dataInWorld.GetFInt("TAG-SHIELDING_SPIRIT");
                            break;
                    }
                    
                    if (st == null) st = "";
                    else st += "|";
                   
                    st += value;
                }
                return st != null ?  st : "";
            }

            return "";
        }
        static public string SI_ShieldUpImproved(object info)
        {
            Multitype<NetCard, NetSkill, NetBattlefield> data = info as Multitype<NetCard, NetSkill, NetBattlefield>;
            if (data != null)
            {
                var ns = data.t1;
                var bf = data.t2;
                FInt value = ns.GetFloatAttribute("TAG-CA_SHIELD");
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
                    else if (bf.ChallengeType == EChallengeType.TypeMental)
                    {
                        value = ns.GetFloatAttribute("TAG-SHIELDING_SPIRIT");
                    }
                }

                FInt f = GameplayUtils.GetDamageFor(data.t1, data.t0);
                return (value + f).ToInt().ToString();
            }

            Multitype<SkillInstance, Subskill, ClientEntityCharacter> dInfo = info as Multitype<SkillInstance, Subskill, ClientEntityCharacter>;
            SkillInstance si = dInfo.t0;
            Subskill ss = dInfo.t1;
            ClientEntityCharacter character = dInfo.t2;
            var dataInWorld = si.GetCurrentSkillAttributes()[ss];

            if (dataInWorld.attributes != null && dataInWorld.attributes.Keys.Count > 0)
            {
                FInt extra = FInt.ZERO;                

                string st = null;
                foreach (var v in ss.challengeTypes)
                {
                    if (character != null)
                    {
                        extra = GameplayUtils.GetDamageFor(si, ss, character, v);
                    }

                    FInt value = dataInWorld.GetFInt("TAG-CA_SHIELD");                    
                    string color = "";

                    switch (v)
                    {
                        case EChallengeType.TypePhysical:
                            if(value == FInt.ZERO) value = dataInWorld.GetFInt("TAG-SHIELDING_PHYSICAL");
                            color = "XML_COLOR-PHYSICAL";
                            break;
                        case EChallengeType.TypeMental:
                            if (value == FInt.ZERO) value = dataInWorld.GetFInt("TAG-SHIELDING_MENTAL");
                            color = "XML_COLOR-MENTAL";
                            break;
                        case EChallengeType.TypeSpirit:
                            if (value == FInt.ZERO) value = dataInWorld.GetFInt("TAG-SHIELDING_SPIRIT");
                            color = "XML_COLOR-SPIRITUAL";
                            break;
                    }
                    
                    if (st == null) st = "";
                    else st += "|";

                    if(extra != FInt.ZERO)
                    {
                        st +=   ColorUtils.GetColorAsTextTag(color) +
                                extra +
                                ColorUtils.GetColorAsTextTag("XML_COLOR-NORMAL_FONT") +
                                " [+" + value + "]";
                    }
                    else
                    {
                        st += " [+" + value + "]";
                    }
                }
                return st != null ? st : "";
            }

            return "";
        }
        static public string SI_ProceduralDamage(object info)
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
            float addBase = (flags & (int)EActivatorBlocks.Additive) > 0 ?  GameplayUtils.DamageNormalToAdditive(dmgBase) : 0f;

            var a = new FInt(addBase);
            
            if (ss.challengeTypes == null || character == null)
            {
                if (addBase > 0)
                {                    
                    st = "x" + dmgBase.ToString(true) + " [+" + a.ToString(true) + "]";
                }
                else
                {
                    st = "x" + dmgBase.ToString(true);
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

                    if (addBase > 0)
                    {
                        st +=   "x" + dmgBase.ToString(true) +"[+" + a.ToString(true) + "]"+
                                " ( " + 
                                ColorUtils.GetColorAsTextTag(color) +
                                GameplayUtils.GetDamageFor(si, ss, character, v, true).ToString(false) +
                                ColorUtils.GetColorAsTextTag("XML_COLOR-NORMAL_FONT") +
                                " )";
                    }
                    else
                    {
                        st += "x" + dmgBase.ToString(true) + 
                                " ( " +
                                ColorUtils.GetColorAsTextTag(color) +
                                GameplayUtils.GetDamageFor(si, ss, character, v).ToString(false) +
                                ColorUtils.GetColorAsTextTag("XML_COLOR-NORMAL_FONT") + 
                                " )";
                    }
                }
            }
            return st;
        }
        #endregion

        #region TRG - Targetting Scripts
        /// <summary>
        /// Target any enemy character
        /// </summary>
        /// <param name="bf"></param>
        /// <param name="ns"></param>
        /// <param name="bfPosition"></param>
        /// <returns></returns>
        static public List<int> Trg_RangeTargeting_Unblocked(NetBattlefield bf, NetSkill ns, int bfPosition)
        {
            if (bf == null || ns == null) return null;

            NetCard nc = bf.GetCardByID(ns.OwnerCardID);

            if (nc == null) return null;

            //this skill should work only when activated from battlefield
            if (!bf.IsBattleSlot(bfPosition)) return null;

            return SubskillScript.TrgU_FallbackTargets(bf, ns, nc);
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
        static public object Act_Damage_Procedural(
                                NetBattlefield bf, 
                                NetQueueItem q, 
                                List<NetQueueItem> stack, 
                                List<NetQueueItem> previousItems, 
                                MHRandom random)
        {
            NetSkill ns = q.GetNetSkill(bf);
            NetCard owner = bf.GetCardByID(ns.OwnerCardID);

            NetCard target = null;            
            if (NetType.IsNullOrEmpty(q.Targets)) return null;

            int flags = ns.Flags;
            bool essence = (flags & (int)EActivatorBlocks.Essence) > 0;
            bool ancient = (flags & (int)EActivatorBlocks.Ancient) > 0;
            bool gray = !ancient && !essence;

            FInt damage = GameplayUtils.GetDamageFor(ns, owner);
//             if ((flags & (int)SkillGenerator.EActivatorBlocks.Additive) > 0)
//             {
//                 
//                 if (essence)
//                 {
//                     //essence is estimated to be equal between additive and multiplicative at
//                     // value of 10  (A x 0.2 + 8) ~ (A = 10)
//                     damage = owner.GetSkillCastingAdditiveStrength(ns, SkillGenerator.ESSENCE_ADDITIVE_BASE);
//                 }
//                 else if(ancient)
//                 {
//                     //essence is estimated to be equal between additive and multiplicative at
//                     // value of 15  (A x 0.2 + 12) ~ (A = 15)
//                     damage = owner.GetSkillCastingAdditiveStrength(ns, SkillGenerator.ANCIENT_ADDITIVE_BASE);
//                 }
//                 else
//                 {
//                     //essence is estimated to be equal between additive and multiplicative at
//                     // value of 6  (A x 0.2 + 5) ~ (A = 6)
//                     damage = owner.GetSkillCastingAdditiveStrength(ns, SkillGenerator.GRAY_ADDITIVE_BASE);
//                 }
//             }
//             else
//             {
//                 damage = owner.GetSkillCastingStrength(ns);
//             }            

            bool trueDamage     = (flags & (int)EActivatorBlocks.TrueDamage) > 0;
            bool poisonDamage   = (flags & (int)EActivatorBlocks.PoisonDamage) > 0;
            bool lifeLeech      = (flags & (int)EActivatorBlocks.LifeLeech) > 0;
            bool shieldLeech    = (flags & (int)EActivatorBlocks.ShieldLeech) > 0;
            bool additive       = (flags & (int)EActivatorBlocks.Additive) > 0;

            int lifeLeeched     = 0;
            int shieldLeeched   = 0;
            #region Primary targets
            foreach (var v in q.Targets.value)
            {
                target = bf.ConvertPositionIDToCard(v);
                if (target != null)
                {

                    FInt dmg = damage;
                    if(poisonDamage && target.IsWounded())
                    {
                        if (essence) dmg *= 1.35f;
                        else dmg *= 1.6f;
                    }

                    if (trueDamage)
                    {
                        if (essence)
                        {
                            target.ReciveTrueDamageEssence(dmg, bf, q, v);
                        }
                        else //no test for ancient
                        {
                            target.ReciveTrueDamageAncient(dmg, bf, q, v);
                        }
                    }
                    else
                    {
                        target.ReciveNormalDamage(dmg, bf, q, v);
                    }

                    if(lifeLeech)
                    {
                        if (essence) lifeLeeched += (dmg * 0.4f).ToInt();
                        else lifeLeeched += (dmg * 0.8f).ToInt();
                    }
                    if (shieldLeech)
                    {
                        if (essence) shieldLeeched += (dmg * 0.5f).ToInt();
                        else shieldLeeched += (dmg * 1.0f).ToInt();
                    }
                }
            }
            #endregion
            #region Secondary targets
            ////do splash if needed
            if (!NetType.IsNullOrEmpty(q.SecondaryTargets))
            {
                bool splashBonus = (flags & (int)EActivatorBlocks.AOE) > 0;

                if (gray || !splashBonus) damage *= 0.5f;
                else
                {
                    //this is !gray && AOE
                    if (essence) damage *= 0.75f;
                    if (ancient) damage *= 1f;
                }
                
                //ancient damage is 100% splash

                foreach (var v in q.SecondaryTargets.value)
                {
                    target = bf.ConvertPositionIDToCard(v);
                    if (target != null)
                    {
                        FInt dmg = damage;
                        if (poisonDamage && target.IsWounded())
                        {
                            if (essence) dmg *= 1.35f;
                            else dmg *= 1.6f;
                        }

                        if (trueDamage)
                        {
                            if (essence)
                            {
                                target.ReciveTrueDamageEssence(dmg, bf, q, v);
                            }
                            else //no test for ancient
                            {
                                target.ReciveTrueDamageAncient(dmg, bf, q, v);
                            }
                        }
                        else
                        {
                            target.ReciveNormalDamage(dmg, bf, q, v);
                        }

                        if (lifeLeech)
                        {
                            if (essence) lifeLeeched += (dmg * 0.4f).ToInt();
                            else lifeLeeched += (dmg * 0.8f).ToInt();
                        }
                        if (shieldLeech)
                        {
                            if (essence) shieldLeeched += (dmg * 0.5f).ToInt();
                            else shieldLeeched += (dmg * 1.0f).ToInt();
                        }
                    }
                }
            }
            #endregion

            #region Leech
            if(lifeLeeched > 0)
            {
                FInt max = owner.GetCA_MAX_HEALTH();
                FInt cur = owner.GetCA_HEALTH();
                if (cur < max)
                {
                    if(lifeLeeched > max - cur)
                    {
                        owner.SetCA_HEALTH(max);
                    }
                    else
                    {
                        owner.SetCA_HEALTH(cur + lifeLeeched);
                    }
                }
            }
            if (shieldLeeched > 0)
            {                
                FInt cur = owner.GetCA_SHIELD();
                owner.SetCA_SHIELD(cur + shieldLeeched);
            }
            #endregion


            return null;
        }

        static public object Act_AddShieldingImproved(
                                NetBattlefield bf, 
                                NetQueueItem q, 
                                List<NetQueueItem> stack, 
                                List<NetQueueItem> previousItems, 
                                MHRandom random)
        {
            NetSkill ns = q.GetNetSkill(bf);
            NetCard owner = bf.GetCardByID(ns.OwnerCardID);

            //skill needs target(-s)
            if (!NetTypeAtomic.IsValid(q.Targets)) return null;
            FInt extra = FInt.ZERO;

            if (ns.MainAtt != null)
            {
                extra = owner.GetSkillCastingStrength(ns);
            }

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
            if (string.IsNullOrEmpty(sign))
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
            value += extra;

            value.CutToInt();
            if (sign == "*")
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
        //         static public object Act_ProceduralAttributeChange(NetBattlefield bf, NetQueueItem q, List<NetQueueItem> stack, List<NetQueueItem> previousItems, MHRandom random)
        //         {
        //             NetSkill ns = q.GetNetSkill(bf);
        // 
        //             //skill needs target(-s)
        //             if (!NetTypeAtomic.IsValid(q.Targets)) return null;
        //                         
        //             bool secTargets = !NetType.IsNullOrEmpty(q.SecondaryTargets);
        // 
        //             int count = ns.GetAttributeCount();
        //             int flags = ns.GetFloatAttribute("ProceduralFlags").ToInt();
        //             bool essence = (flags & (int)SkillGenerator.EActivatorBlocks.Essence) > 0;
        //             bool ancient = (flags & (int)SkillGenerator.EActivatorBlocks.Ancient) > 0;
        //             bool splashBonus = (flags & (int)SkillGenerator.EActivatorBlocks.AOE) > 0;
        //             float mp = 1f;
        //             if (essence) mp *= 0.75f;
        //             else if (!ancient) mp *= 0.5f;
        // 
        //             for (int i = 0; i < count; i++)
        //             {
        //                 NetDoubleStringFloat val = ns.GetAttributeAt(i);
        //                 if (val.key == "ProceduralFlags" || !val.key.StartsWith("TAG-"))
        //                 {
        //                     continue;
        //                 }
        //                 bool multiply = val.sValue == "*";
        // 
        //                 foreach (var v in q.Targets.value)
        //                 {
        //                     NetCard target = bf.ConvertPositionIDToCard(v);
        //                     if (target == null) continue;
        //                     FInt prev = target.GetAttribute(val.key);
        //                     if(multiply)
        //                         target.SetAttribute(val.key, val.fValue * prev * 0.01f);
        //                     else
        //                         target.SetAttribute(val.key, val.fValue + prev);
        //                 }
        //                 foreach (var v in q.SecondaryTargets.value)
        //                 {
        //                     NetCard target = bf.ConvertPositionIDToCard(v);
        //                     if (target == null) continue;
        //                     FInt prev = target.GetAttribute(val.key);
        //                     if (multiply)
        //                         target.SetAttribute(val.key, val.fValue * prev * 0.01f);
        //                     else
        //                         target.SetAttribute(val.key, val.fValue + prev);
        //                 }
        //             }
        //             return null;
        //         }

        #endregion
    }
}
#endif