#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR

using DBDef;
using System;
using System.Collections.Generic;
using Thea2.Common;
using Thea2.Server;
using UnityEngine;

namespace GameScript
{
    //public const int IDMarkerLength = 3 + 3 + 3 + 3 + 2;
    //public const int IDSubSkillMarkerLength = 1 + 3 + 3 + 3 + 3 + 2;

    //         public enum ESkillRank
    //         {
    //             Gray = 9,
    //             Essence = 24,
    //             Ancient = 44
    //         }
    //         public enum ESkillWeight
    //         {
    //             // ONE HANDED requires 100% of the essences per level
    //             Light = 100,
    //             // TWO HAND requires 160% of the essences per level- whey will be heavier and use more resources
    //             Heavy = 160, 
    //             Shield = 135,
    //             Robes = 110,
    //             MediumArmours = 175,
    //             HeavyArmours = 250,
    //         }
    //         public enum EActivatorBlocks
    //         {
    //             Gray = 1 << 0,
    //             Essence = 1 << 1,
    //             Ancient = 1 << 2,
    // 
    //             TrueDamage = 1 << 3, //Increases damage toward shielded characters or apply damage to shield and to the person underneath
    //             PoisonDamage = 1 << 4, //Poison increases damage to wounded characters by 35% or 60%        
    //             AOE = 1 << 5, //Secondary targets get 75% or 100%. (bow gets splash area)
    //             LifeLeech = 1 << 6, //Leech damage done as health
    //             ShieldLeech = 1 << 7, //Leech damage done as shield
    //             //Speed = 1 << 8, //item have improved speed from essence
    // 
    //             Additive = 1 << 9, //BONE- converts damages from multiplicative to additive
    //         }

    public class ProceduralEquipment : ScriptBase
    {
        #region Variables and Constants        
        static Dictionary<Int64, Skill> markerToSkillDictionary = new Dictionary<Int64, Skill>();
        static Dictionary<Int64, Subskill> markerToSubSkillDictionary = new Dictionary<Int64, Subskill>();

        static float[,] grayDmg = new float[,]
        {
            /*LVL1*/{1.1f, 1.2f, 1.4f, 1.5f, 1.6f },
            /*LVL5*/{2.0f, 2.4f, 2.6f, 2.8f, 3.0f }
        };
        static float[,] essenceDmg = new float[,]
        {
            /*LVL1*/{1.15f, 1.25f, 1.45f, 1.55f, 1.65f },
            /*LVL5*/{2.50f, 3.00f, 3.25f, 3.50f, 3.75f }
        };
        static float[,] ancientDmg = new float[,]
        {
            /*LVL1*/{1.20f, 1.35f, 1.55f, 1.65f, 1.75f },
            /*LVL5*/{3.00f, 3.75f, 4.00f, 4.30f, 4.60f }
        };

        #endregion

        #region External Access
        static public string GetProceduralIDMarkerString(ItemRecipe ir, ESkillRank sr, Tag essence1, Tag essence2, short additionalInfo)
        {
            Int64 longID = GetProceduralIDMarker(ir, sr, essence1, essence2, additionalInfo);
            string sb = longID.ToString().PadLeft(SkillGenerator.IDMarkerLength, '0');
            return sb;
        }
        static public Int64 GetProceduralIDMarker(ItemRecipe ir, ESkillRank sr, Tag essence1, Tag essence2, short additionalInfo)
        {
            Int64 longID = 0;

            // 3 digits 0-999
            int v = Globals.GetTypeFromDB<ItemRecipe>().IndexOf(ir);
            if (v > 999) Debug.LogError("[ERROR]Serializing ID marker with the item of wrong length! ItemRecipe: " + v);
            //sb.Append(v.ToString("000"));
            longID = v;

            // 3 digits (int)sr
            v = (int)sr;
            if (v > 999) Debug.LogError("[ERROR]Serializing ID marker with the item of wrong length! ESkillRank: " + v);
            //sb.Append(v.ToString("000"));
            longID *= 1000;
            longID += v;

            // 3 digits 0-999
            v = Globals.GetTypeFromDB<Tag>().IndexOf(essence1);
            if (v > 999) Debug.LogError("[ERROR]Serializing ID marker with the item of wrong length! Tag 1: " + v);
            //sb.Append(v.ToString("000"));
            longID *= 1000;
            longID += v;

            // 3 digits 0-999
            v = Globals.GetTypeFromDB<Tag>().IndexOf(essence2);
            if (v > 999) Debug.LogError("[ERROR]Serializing ID marker with the item of wrong length! Tag 2: " + v);
            //sb.Append(v.ToString("000"));
            longID *= 1000;
            longID += v;

            // 3 digits 0-999
            longID *= 1000;
            longID += additionalInfo;
            if (additionalInfo < 0 || additionalInfo > 999) Debug.LogError("[ERROR]Serializing ID marker with the item of wrong length! additionalInfo: " + additionalInfo);

            return longID;
        }
        static public Skill GetSkillByProceduralIDMarker(Int64 idMarker)
        {
            if (markerToSkillDictionary.ContainsKey(idMarker))
            {
                return markerToSkillDictionary[idMarker];
            }

            //reverse construction

            // 3 digits 0-99
            int additionalInfo = (int)(idMarker % 1000);
            idMarker /= 1000;

            // 3 digits 0-999
            //id = Convert.ToInt32(idMarker.Substring(12, 3));
            int id = (int)(idMarker % 1000);
            Tag essence2 = Globals.GetTypeFromDB<Tag>()[id];
            idMarker /= 1000;

            // 3 digits 0-999
            //id = Convert.ToInt32(idMarker.Substring(9, 3));
            id = (int)(idMarker % 1000);
            Tag essence1 = Globals.GetTypeFromDB<Tag>()[id];
            idMarker /= 1000;

            // 3 digits (int)sr
            //id = Convert.ToInt32(idMarker.Substring(3, 3));
            id = (int)(idMarker % 1000);
            ESkillRank sr = (ESkillRank)id;
            idMarker /= 1000;

            // 3 digits 0-999
            //int id = Convert.ToInt32(idMarker.Substring(0, 3));
            id = (int)(idMarker % 1000);
            ItemRecipe ir = Globals.GetTypeFromDB<ItemRecipe>()[id];

            return GenerateSkill(ir, sr, essence1, essence2, (short)additionalInfo);
        }
        static public List<Skill> GenerateSkills(ItemRecipe ir, Tag e1, Tag e2, short additionalInfo)
        {
            List<Skill> skills = new List<Skill>();

            foreach (ESkillRank v in Enum.GetValues(typeof(ESkillRank)))
            {
                Skill s = GenerateSkill(ir, v, e1, e2, additionalInfo);
                if (s != null)
                {
                    skills.Add(s);
                }
            }

            return skills;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ir">Recipe identifying relevaint item</param>
        /// <param name="sr">Skill Rank Enum which determines its class rank, eg Gray or Ancient </param>
        /// <param name="sw">Skill weight differencin single and two handed types if applicable </param>
        /// <param name="essence1">Main Essence, from resources</param>
        /// <param name="essence2">Secondary essence, IF ANY, second most popular essence in the item </param>
        /// <param name="material">Master material which influences additional effects of the item </param>
        /// <returns></returns>    
        static public Skill GenerateSkill(
            ItemRecipe ir, ESkillRank sr,
            Tag essence1, Tag essence2, short additionalInfo)
        {

            //Passive attribute change uses 1 space
            //Active and reactive subskills uses 2 spaces
            int skillSpace = 11;

            Int64 markerINT = GetProceduralIDMarker(ir, sr, essence1, essence2, additionalInfo);
            if (markerToSkillDictionary.ContainsKey(markerINT))
            {
                return markerToSkillDictionary[markerINT];
            }

            string marker = markerINT.ToString().PadLeft(SkillGenerator.IDMarkerLength, '0');

            RecipeAttributes ra = Globals.GetTypeFromDB<RecipeAttributes>().Find(o => o.relatedRecipe == ir);
            if (ra == null) return null;

            Skill s = new Skill();
            s.dbName = marker;
            markerToSkillDictionary[markerINT] = s;
            s.rarity = (int)sr; //reuse essence base cost as rarity

            s.descriptionInfo = GenerateSkillDIBlock(ir, sr, ra.iconPrefix, s.dbName);
            List<Subskill> sss = new List<Subskill>();

            #region RECIPE SPECIFICS

            int flags = 0;
            bool blockAOE = ir == (ItemRecipe)ITEM_REC.AXE_1H || ir == (ItemRecipe)ITEM_REC.AXE_2H;
            flags = GenerateAttackFlags(sr, essence1, essence2, blockAOE);

            ESkillWeight sw = ESkillWeight.Light;
            Subskill ss = null;

            if (ir == (ItemRecipe)ITEM_REC.SWORD_1H)
            {
                sw = ESkillWeight.Light;
                ss = GenerateSubskill_Sword1H(sr, ra.damageStars, marker, flags, sss.Count + 1);
                ss.descriptionInfo = GenerateSubSkillDIBlock(sr, ra.iconPrefix, ss.dbName);
                FinishOffenseSkills(s, ss, sss, sr, ra, essence1, essence2, ir, flags);

                ss = GenerateSubskill_Parry(sr, 1, marker, flags, sss.Count + 1);
                ss.descriptionInfo = GenerateSubSkillShieldsDIBlock(sr, ra.iconPrefix, ss.dbName);
                FinishPassiveShieldSkills(s, ss, sss, sr, ra, essence1, essence2, ir);
            }
            else if (ir == (ItemRecipe)ITEM_REC.SWORD_2H)
            {
                sw = ESkillWeight.Heavy;
                ss = GenerateSubskill_Sword2H(sr, ra.damageStars, marker, flags, sss.Count + 1);
                ss.descriptionInfo = GenerateSubSkillDIBlock(sr, ra.iconPrefix, ss.dbName);
                FinishOffenseSkills(s, ss, sss, sr, ra, essence1, essence2, ir, flags);

                ss = GenerateSubskill_Parry(sr, 1.2f, marker, flags, sss.Count + 1);
                ss.descriptionInfo = GenerateSubSkillShieldsDIBlock(sr, ra.iconPrefix, ss.dbName);
                FinishPassiveShieldSkills(s, ss, sss, sr, ra, essence1, essence2, ir);
            }
            else if (ir == (ItemRecipe)ITEM_REC.AXE_1H)
            {
                sw = ESkillWeight.Light;
                ss = GenerateSubskill_Axe1H(sr, ra.damageStars, marker, flags, sss.Count + 1);
                ss.descriptionInfo = GenerateSubSkillDIBlock(sr, ra.iconPrefix, ss.dbName);
                FinishOffenseSkills(s, ss, sss, sr, ra, essence1, essence2, ir, flags);
            }
            else if (ir == (ItemRecipe)ITEM_REC.AXE_2H)
            {
                sw = ESkillWeight.Heavy;
                ss = GenerateSubskill_Axe2H(sr, ra.damageStars, marker, flags, sss.Count + 1);
                ss.descriptionInfo = GenerateSubSkillDIBlock(sr, ra.iconPrefix, ss.dbName);
                FinishOffenseSkills(s, ss, sss, sr, ra, essence1, essence2, ir, flags);
            }
            else if (ir == (ItemRecipe)ITEM_REC.CLUB_1H)
            {
                sw = ESkillWeight.Light;
                ss = GenerateSubskill_Club1H(sr, ra.damageStars, marker, flags, sss.Count + 1);
                ss.descriptionInfo = GenerateSubSkillDIBlock(sr, ra.iconPrefix, ss.dbName);
                FinishOffenseSkills(s, ss, sss, sr, ra, essence1, essence2, ir, flags);
            }
            else if (ir == (ItemRecipe)ITEM_REC.CLUB_2H)
            {
                sw = ESkillWeight.Heavy;
                ss = GenerateSubskill_Club2H(sr, ra.damageStars, marker, flags, sss.Count + 1);
                ss.descriptionInfo = GenerateSubSkillDIBlock(sr, ra.iconPrefix, ss.dbName);
                FinishOffenseSkills(s, ss, sss, sr, ra, essence1, essence2, ir, flags);
            }
            else if (ir == (ItemRecipe)ITEM_REC.POLEARM_1H)
            {
                sw = ESkillWeight.Light;
                ss = GenerateSubskill_Polarm1H(sr, ra.damageStars, marker, flags, sss.Count + 1);
                ss.descriptionInfo = GenerateSubSkillDIBlock(sr, ra.iconPrefix, ss.dbName);
                FinishOffenseSkills(s, ss, sss, sr, ra, essence1, essence2, ir, flags);

                ss = GenerateSubskill_Polarm1H_PreStrike(sr, ra.damageStars, marker, flags, sss.Count + 1, (Tag)TAG.CA_1);
                ss.descriptionInfo = GenerateSubSkillDIBlockPreAttack(sr, ra.iconPrefix, ss.dbName);
                FinishOffenseSkills(s, ss, sss, sr, ra, essence1, essence2, ir, flags);
                ss.delay = 0f;
            }
            else if (ir == (ItemRecipe)ITEM_REC.POLEARM_2H)
            {
                sw = ESkillWeight.Heavy;
                ss = GenerateSubskill_Polarm2H(sr, ra.damageStars, marker, flags, sss.Count + 1);
                ss.descriptionInfo = GenerateSubSkillDIBlock(sr, ra.iconPrefix, ss.dbName);
                FinishOffenseSkills(s, ss, sss, sr, ra, essence1, essence2, ir, flags);

                ss = GenerateSubskill_Polarm2H_PreStrike(sr, ra.damageStars, marker, flags, sss.Count + 1, (Tag)TAG.CA_1);
                ss.descriptionInfo = GenerateSubSkillDIBlockPreAttack(sr, ra.iconPrefix, ss.dbName);
                FinishOffenseSkills(s, ss, sss, sr, ra, essence1, essence2, ir, flags);
                ss.delay = 0f;
            }
            else if (ir == (ItemRecipe)ITEM_REC.ARTIFACT_1H)
            {
                sw = ESkillWeight.Light;
                ss = GenerateSubskill_Artifact1H(sr, ra.damageStars, marker, flags, sss.Count + 1);
                ss.descriptionInfo = GenerateSubSkillDIBlock(sr, ra.iconPrefix, ss.dbName);
                FinishOffenseSkills(s, ss, sss, sr, ra, essence1, essence2, ir, flags);
            }
            else if (ir == (ItemRecipe)ITEM_REC.ARTIFACT_2H)
            {
                sw = ESkillWeight.Heavy;
                ss = GenerateSubskill_Artifact2H(sr, ra.damageStars, marker, flags, sss.Count + 1);
                ss.descriptionInfo = GenerateSubSkillDIBlock(sr, ra.iconPrefix, ss.dbName);
                FinishOffenseSkills(s, ss, sss, sr, ra, essence1, essence2, ir, flags);
            }
            else if (ir == (ItemRecipe)ITEM_REC.SCROLL_1H)
            {
                sw = ESkillWeight.Light;
                //             ss = GenerateSubskill_Axe1H(sr, ra.damageStars, marker, flags);
            }
            else if (ir == (ItemRecipe)ITEM_REC.SCROLL_2H)
            {
                sw = ESkillWeight.Heavy;
                //             ss = GenerateSubskill_Axe1H(sr, ra.damageStars, marker, flags);
            }
            else if (ir == (ItemRecipe)ITEM_REC.BOW)
            {
                sw = ESkillWeight.Light;
                ss = GenerateSubskill_Bow(sr, ra.damageStars, marker, flags, sss.Count + 1);
                ss.descriptionInfo = GenerateSubSkillDIBlock(sr, ra.iconPrefix, ss.dbName);
                FinishOffenseSkills(s, ss, sss, sr, ra, essence1, essence2, ir, flags, true);
            }
            else if (ir == (ItemRecipe)ITEM_REC.THROWN_SPEARS)
            {
                sw = ESkillWeight.Heavy;
                ss = GenerateSubskill_TSpear(sr, ra.damageStars, marker, flags, sss.Count + 1);
                ss.descriptionInfo = GenerateSubSkillDIBlock(sr, ra.iconPrefix, ss.dbName);
                FinishOffenseSkills(s, ss, sss, sr, ra, essence1, essence2, ir, flags, true);

                ss = GenerateSubskill_Polarm1H_PreStrike(sr, ra.damageStars, marker, flags, sss.Count + 1, (Tag)TAG.CA_2);
                ss.descriptionInfo = GenerateSubSkillDIBlockPreAttack(sr, ra.iconPrefix, ss.dbName);
                FinishOffenseSkills(s, ss, sss, sr, ra, essence1, essence2, ir, flags);
                ss.delay = 0f;
            }
            else if (ir == (ItemRecipe)ITEM_REC.WAND)
            {
                sw = ESkillWeight.Light;
                ss = GenerateSubskill_Wand(sr, ra.damageStars, marker, flags, sss.Count + 1);
                ss.descriptionInfo = GenerateSubSkillDIBlock(sr, ra.iconPrefix, ss.dbName);
                FinishOffenseSkills(s, ss, sss, sr, ra, essence1, essence2, ir, flags, true);
            }
            else if (ir == (ItemRecipe)ITEM_REC.SHIELD)
            {
                sw = ESkillWeight.Shield;
                ss = GenerateSubskill_ShieldsUp(sr, ra.damageStars, marker, flags, sss.Count + 1);
                ss.descriptionInfo = GenerateSubSkillShieldsUpDIBlock(sr, ra.iconPrefix, ss.dbName);
                FinishActiveShieldSkills(s, ss, sss, sr, ra, essence1, essence2, ir);

                ss = GenerateSubskill_Shield(sr, ra.damageStars, marker, flags, sss.Count + 1);
                ss.descriptionInfo = GenerateSubSkillShieldsDIBlock(sr, ra.iconPrefix, ss.dbName);
                FinishPassiveShieldSkills(s, ss, sss, sr, ra, essence1, essence2, ir);
            }
            else if (ir == (ItemRecipe)ITEM_REC.ROBES)
            {
                sw = ESkillWeight.Robes;
                ss = GenerateSubskill_Shield(sr, ra.damageStars * 1.5f, marker, flags, sss.Count + 1);
                ss.descriptionInfo = GenerateSubSkillShieldsDIBlock(sr, ra.iconPrefix, ss.dbName);
                FinishPassiveShieldSkills(s, ss, sss, sr, ra, essence1, essence2, ir);
            }
            else if (ir == (ItemRecipe)ITEM_REC.MEDIUM_ARMOUR)
            {
                sw = ESkillWeight.MediumArmours;
                ss = GenerateSubskill_Shield(sr, ra.damageStars, marker, flags, sss.Count + 1);
                ss.descriptionInfo = GenerateSubSkillShieldsDIBlock(sr, ra.iconPrefix, ss.dbName);
                FinishPassiveShieldSkills(s, ss, sss, sr, ra, essence1, essence2, ir);
            }
            else if (ir == (ItemRecipe)ITEM_REC.HEAVY_ARMOUR)
            {
                sw = ESkillWeight.HeavyArmours;
                ss = GenerateSubskill_Shield(sr, ra.damageStars, marker, flags, sss.Count + 1);
                ss.descriptionInfo = GenerateSubSkillShieldsDIBlock(sr, ra.iconPrefix, ss.dbName);
                FinishPassiveShieldSkills(s, ss, sss, sr, ra, essence1, essence2, ir);
            }
            else
            {
                Debug.Log("Unsupported skill generation! for " + ir.dbName);
            }

            s.baseEssence = GetEssenceBlock(essence1, essence2, sr, sw);

            #endregion

            foreach (var v in sss)
            {
                if (v.trigger.triggerGroup == ETriggerGroupType.Passive)
                {
                    skillSpace -= 1;
                }
                else
                {
                    skillSpace -= 2;
                }
            }

            if (sr != ESkillRank.Gray)
            {
                bool armour = ir == (ItemRecipe)ITEM_REC.ROBES ||
                              ir == (ItemRecipe)ITEM_REC.MEDIUM_ARMOUR ||
                              ir == (ItemRecipe)ITEM_REC.HEAVY_ARMOUR;
                bool shield = ir == (ItemRecipe)ITEM_REC.SHIELD;

                if (armour)
                {
                    float attScale = sr == ESkillRank.Essence ? 1f : 1.3f;
                    if (essence1 != essence2)
                    {
                        attScale *= 0.6f;
                    }

                    float fromAdd = 5f * attScale;
                    float toAdd = 20f * attScale;
                    float fromMp = 15f * attScale;
                    float toMp = 35f * attScale;

                    if (HaveFlag(flags, EActivatorBlocks.TrueDamage))
                    {
                        ss = GenerateSubskill_AttAddon(sr, fromAdd, toAdd, marker, flags, sss.Count, (Tag)TAG.MAX_HEALTH_PHYSICAL);
                        ss.descriptionInfo = GenerateSubSkillAttributeDIBlock(sr, ra.iconPrefix, ss.dbName);
                        FinishAttributeSkills(s, ss, sss, sr, ra, essence1, essence2, ir);
                    }
                    if (HaveFlag(flags, EActivatorBlocks.Additive))
                    {
                        ss = GenerateSubskill_AttMultiplier(sr, fromMp, toMp, marker, flags, sss.Count, (Tag)TAG.MAX_HEALTH_PHYSICAL);
                        ss.descriptionInfo = GenerateSubSkillAttributeDIBlock(sr, ra.iconPrefix, ss.dbName);
                        FinishAttributeSkills(s, ss, sss, sr, ra, essence1, essence2, ir);
                    }
                    if (HaveFlag(flags, EActivatorBlocks.PoisonDamage))
                    {
                        ss = GenerateSubskill_AttAddon(sr, fromAdd, toAdd, marker, flags, sss.Count, (Tag)TAG.MAX_HEALTH_MENTAL);
                        ss.descriptionInfo = GenerateSubSkillAttributeDIBlock(sr, ra.iconPrefix, ss.dbName);
                        FinishAttributeSkills(s, ss, sss, sr, ra, essence1, essence2, ir);
                    }
                    if (HaveFlag(flags, EActivatorBlocks.ShieldLeech))
                    {
                        ss = GenerateSubskill_AttMultiplier(sr, fromMp, toMp, marker, flags, sss.Count, (Tag)TAG.MAX_HEALTH_MENTAL);
                        ss.descriptionInfo = GenerateSubSkillAttributeDIBlock(sr, ra.iconPrefix, ss.dbName);
                        FinishAttributeSkills(s, ss, sss, sr, ra, essence1, essence2, ir);
                    }
                    if (HaveFlag(flags, EActivatorBlocks.AOE))
                    {
                        ss = GenerateSubskill_AttAddon(sr, fromAdd, toAdd, marker, flags, sss.Count, (Tag)TAG.MAX_HEALTH_SPIRIT);
                        ss.descriptionInfo = GenerateSubSkillAttributeDIBlock(sr, ra.iconPrefix, ss.dbName);
                        FinishAttributeSkills(s, ss, sss, sr, ra, essence1, essence2, ir);
                    }
                    if (HaveFlag(flags, EActivatorBlocks.LifeLeech))
                    {
                        ss = GenerateSubskill_AttMultiplier(sr, fromMp, toMp, marker, flags, sss.Count, (Tag)TAG.MAX_HEALTH_SPIRIT);
                        ss.descriptionInfo = GenerateSubSkillAttributeDIBlock(sr, ra.iconPrefix, ss.dbName);
                        FinishAttributeSkills(s, ss, sss, sr, ra, essence1, essence2, ir);
                    }
                }
                if (sr == ESkillRank.Ancient && (armour || shield))
                {
                    ss = GenerateSubskill_MPAddon(sr, 1, marker, flags, sss.Count, (Tag)TAG.MOVEMENT_RANGE);
                    ss.descriptionInfo = GenerateSubSkillAttributeDIBlock(sr, ra.iconPrefix, ss.dbName);
                    FinishAttributeSkills(s, ss, sss, sr, ra, essence1, essence2, ir);
                }
            }

            //KHASH: TODO smarter way of applying predesigned skills 
            if (ra.additionalSubskillsCount > 0)
            {
                if (ra.subskillCollection != null && ra.subskillCollection.Length > 0)
                {
                    foreach (var v in ra.subskillCollection)
                    {
                        if ((sr == ESkillRank.Gray && v.essenceQuality == EEssenceQuality.Gray) ||
                            (sr == ESkillRank.Essence && v.essenceQuality == EEssenceQuality.Essence) ||
                            (sr == ESkillRank.Ancient && v.essenceQuality == EEssenceQuality.Ancient))
                        {
                            if (v.subskillEssenceType != null && v.subskillEssenceType.Length > 0)
                            {
                                foreach (var k in v.subskillEssenceType)
                                {
                                    if (k.essenceTypes != null && Array.Find(k.essenceTypes, o => o == essence1) != null)
                                    {
                                        foreach (var sskill in k.subskills)
                                        {
                                            sss.Add(sskill);
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            s.skillSubskills = sss.ToArray();

            return s;
        }
        #endregion
        #region Generator Blocks
        static CountedTag[] GetEssenceBlock(Tag essence1, Tag essence2, ESkillRank sr, ESkillWeight sw)
        {
            if (essence1 == null || essence2 == null)
            {
                Debug.LogError("[ERROR]Invalid essence sources: " + essence1 + ", " + essence2);
            }

            float essenceCount = (int)sr * (int)sw * 0.01f;

            if (essence2 == essence1)
            {
                return new CountedTag[]
                    {
                    GenerateCTBlock(essence1, essenceCount)
                    };
            }
            else
            {
                return new CountedTag[]
                    {
                    GenerateCTBlock(essence1, essenceCount * 0.5f),
                    GenerateCTBlock(essence2, essenceCount * 0.5f)
                    };
            }
        }
        static DescriptionInfo GenerateSkillDIBlock(ItemRecipe ir, ESkillRank sr, string name, string ownerDbName)
        {
            DescriptionInfo di = new DescriptionInfo();

            if (sr == ESkillRank.Essence)
            {
                di.name = "DES_ELEMENTAL ";
            }
            else if (sr == ESkillRank.Ancient)
            {
                di.name = "DES_LEGENDARY ";
            }
            else
            {
                di.name = "";
            }

            if (ir == (ItemRecipe)ITEM_REC.ROBES || ir == (ItemRecipe)ITEM_REC.MEDIUM_ARMOUR || ir == (ItemRecipe)ITEM_REC.HEAVY_ARMOUR)
            {
                di.name += "DES_SKILL_ARMOUR_SKILL";
                di.description = "DES_SKILL_ARMOUR_SKILL_DES";
            }

            else if (ir == (ItemRecipe)ITEM_REC.SHIELD)
            {
                di.name += "DES_SKILL_SHIELD_SKILL";
                di.description = "DES_SKILL_SHIELD_SKILL_DES";
            }

            else
            {
                di.name += "DES_SKILL_WEAPON_SKILL";
                di.description = "DES_SKILL_WEAPON_SKILL_DES";
            }

            di.iconName = "Skill" + name + "_" + sr.ToString();
            di.dbName = ownerDbName;

            CustomDescriptionInfo v = new CustomDescriptionInfo(di, ownerDbName);
            return v.GetDI();
        }

        static DescriptionInfo GenerateSubSkillDIBlock(ESkillRank sr, string name, string ownerDbName)
        {
            DescriptionInfo di = new DescriptionInfo();

            di.name = "DES_SUBSKILL_WEAPON_ATTACK";
            di.description = "DES_SUBSKILL_WEAPON_ATTACK_DES";

            di.iconName = "Subskill" + name;// + "_" + sr.ToString();
            di.dbName = ownerDbName;

            CustomDescriptionInfo v = new CustomDescriptionInfo(di, ownerDbName);
            return v.GetDI();
        }
        static DescriptionInfo GenerateSubSkillDIBlockPreAttack(ESkillRank sr, string name, string ownerDbName)
        {
            DescriptionInfo di = new DescriptionInfo();

            di.name = "DES_SUBSKILL_FIRST_STRIKE";
            di.description = "DES_SUBSKILL_FIRST_STRIKE_DES";

            di.iconName = "Subskill" + name;// + "_" + sr.ToString();
            di.dbName = ownerDbName;

            CustomDescriptionInfo v = new CustomDescriptionInfo(di, ownerDbName);
            return v.GetDI();
        }
        static DescriptionInfo GenerateSubSkillReflectDIBlock(ESkillRank sr, string name, string ownerDbName)
        {
            DescriptionInfo di = new DescriptionInfo();

            di.name = "DES_SUBSKILL_RETALIATION_DMG";
            di.description = "DES_SUBSKILL_RETALIATION_DMG_DES";

            di.iconName = "SubskillRetaliationCA1";// + "_" + sr.ToString();
            di.dbName = ownerDbName;
            CustomDescriptionInfo v = new CustomDescriptionInfo(di, ownerDbName);
            return v.GetDI();
        }
        static DescriptionInfo GenerateSubSkillShieldsUpDIBlock(ESkillRank sr, string name, string ownerDbName)
        {
            DescriptionInfo di = new DescriptionInfo();

            di.name = "DES_SUBSKILL_SHIELDS_UP";
            di.description = "DES_SUBSKILL_SHIELDS_UP_DES";

            di.iconName = "SubskillShieldsUp";// + "_" + sr.ToString();
            di.dbName = ownerDbName;
            CustomDescriptionInfo v = new CustomDescriptionInfo(di, ownerDbName);
            return v.GetDI();
        }
        static DescriptionInfo GenerateSubSkillShieldsDIBlock(ESkillRank sr, string name, string ownerDbName)
        {
            DescriptionInfo di = new DescriptionInfo();

            di.name = "DES_SUBSKILL_SHIELDS";
            di.description = "DES_SUBSKILL_SHIELDS_DES";

            di.iconName = "SubskillAddAttribute";// + "_" + sr.ToString();
            di.dbName = ownerDbName;
            CustomDescriptionInfo v = new CustomDescriptionInfo(di, ownerDbName);
            return v.GetDI();
        }


        static DescriptionInfo GenerateSubSkillAttributeDIBlock(ESkillRank sr, string name, string ownerDbName)
        {
            DescriptionInfo di = new DescriptionInfo();

            di.name = "DES_SUBSKILL_INCREASE_ATTRIBUTE";
            di.description = "DES_SUBSKILL_INCREASE_ATTRIBUTE_DES";

            di.iconName = "SubskillAddAttribute";// + "_" + sr.ToString();
            di.dbName = ownerDbName;
            CustomDescriptionInfo v = new CustomDescriptionInfo(di, ownerDbName);
            return v.GetDI();
        }

        static SubSkillAttribute[] GenerateAttributeArrayBlock(float flags)
        {
            var subskillAttributes = new SubSkillAttribute[1];
            subskillAttributes[0] = new SubSkillAttribute();
            subskillAttributes[0].identifier = "ProceduralFlags";
            subskillAttributes[0].attributeFloatValue = flags;
            return subskillAttributes;
        }
        static SubSkillAttribute[] GenerateAttributeArrayBlock(Tag tag, bool plus, int value)
        {
            var subskillAttributes = new SubSkillAttribute[1];
            subskillAttributes[0] = new SubSkillAttribute();
            subskillAttributes[0].identifier = tag.dbName;
            subskillAttributes[0].attributeStringValue = plus ? "+" : "*";
            subskillAttributes[0].attributeFloatValue = plus ? value : value + 100;
            return subskillAttributes;
        }

        static SkillAttributesOnLevel[] GenerateLeveAttributesBlock(ESkillRank skillRank, int damageCategory, Tag mainAttrib, float flags, float dmgScale = 1f)
        {
            SkillAttributesOnLevel[] attrib = new SkillAttributesOnLevel[6];

            float lvl1 = 0f;
            float lvl5 = 0f;

            switch (skillRank)
            {
                case ESkillRank.Gray:
                    lvl1 = grayDmg[0, damageCategory - 1];
                    lvl5 = grayDmg[1, damageCategory - 1];
                    break;
                case ESkillRank.Essence:
                    lvl1 = essenceDmg[0, damageCategory - 1];
                    lvl5 = essenceDmg[1, damageCategory - 1];
                    break;
                case ESkillRank.Ancient:
                    lvl1 = ancientDmg[0, damageCategory - 1];
                    lvl5 = ancientDmg[1, damageCategory - 1];
                    break;
            }

            attrib[0] = new SkillAttributesOnLevel();
            attrib[0].level = 0;
            attrib[0].mainAtt = GenerateCTBlock(mainAttrib, 0);
            attrib[0].subskillAttributes = GenerateAttributeArrayBlock(flags);

            for (int i = 1; i <= 5; i++)
            {
                attrib[i] = new SkillAttributesOnLevel();
                attrib[i].level = i;
                attrib[i].mainAtt =
                    GenerateCTBlock(mainAttrib, Mathf.Lerp(lvl1, lvl5, (i - 1) / 4f) * dmgScale);
                attrib[i].subskillAttributes =
                    GenerateAttributeArrayBlock(flags);
            }

            return attrib;
        }
        static SkillAttributesOnLevel[] GenerateFlatAttributesBlock(ESkillRank skillRank, int damageRank, Tag mainAttrib, float flags)
        {
            SkillAttributesOnLevel[] attrib = new SkillAttributesOnLevel[6];

            attrib[0] = new SkillAttributesOnLevel();
            attrib[0].level = 0;
            attrib[0].subskillAttributes = GenerateAttributeArrayBlock(mainAttrib, true, 0);

            for (int i = 1; i <= 5; i++)
            {
                attrib[i] = new SkillAttributesOnLevel();
                attrib[i].level = i;
                attrib[i].subskillAttributes =
                    GenerateAttributeArrayBlock(mainAttrib, true, damageRank);
            }

            return attrib;
        }

        static SkillAttributesOnLevel[] GenerateLeveLinearAttributesBlock(ESkillRank skillRank, bool plus, float from, float to, Tag attribChanged)
        {
            SkillAttributesOnLevel[] attrib = new SkillAttributesOnLevel[6];

            float baseValue = 0f;
            switch (skillRank)
            {
                case ESkillRank.Gray:
                    baseValue = 0.5f;
                    break;
                case ESkillRank.Essence:
                    baseValue = 1f;
                    break;
                case ESkillRank.Ancient:
                    baseValue = 1.5f;
                    break;
            }

            attrib[0] = new SkillAttributesOnLevel();
            attrib[0].level = 0;
            attrib[0].subskillAttributes =
                GenerateAttributeArrayBlock(attribChanged, plus, 0);

            for (int i = 1; i <= 5; i++)
            {
                attrib[i] = new SkillAttributesOnLevel();
                attrib[i].level = i;
                attrib[i].subskillAttributes =
                    GenerateAttributeArrayBlock(attribChanged, plus, Mathf.RoundToInt(Mathf.Lerp(from, to, (i - 1) / 4f) * baseValue));
            }

            return attrib;
        }
        static SkillAttributesOnLevel[] GenerateLeveNewAttributesBlock(ESkillRank skillRank, bool plus, float from, float to, Tag attribChanged)
        {
            SkillAttributesOnLevel[] attrib = new SkillAttributesOnLevel[6];

            attrib[0] = new SkillAttributesOnLevel();
            attrib[0].level = 0;
            attrib[0].subskillAttributes =
                GenerateAttributeArrayBlock(attribChanged, plus, 0);

            for (int i = 1; i <= 5; i++)
            {
                attrib[i] = new SkillAttributesOnLevel();
                attrib[i].level = i;
                attrib[i].subskillAttributes =
                    GenerateAttributeArrayBlock(attribChanged, plus, Mathf.RoundToInt(Mathf.Lerp(from, to, (i - 1) / 4f)));
            }

            return attrib;
        }
        static SkillAttributesOnLevel[] GenerateShieldUpLeveAttributesBlock(ESkillRank skillRank, bool plus, float scale, Tag attribChanged)
        {
            SkillAttributesOnLevel[] attrib = new SkillAttributesOnLevel[6];

            float baseValue = 0f;
            switch (skillRank)
            {
                case ESkillRank.Gray:
                    baseValue = 1f;
                    break;
                case ESkillRank.Essence:
                    baseValue = 2f;
                    break;
                case ESkillRank.Ancient:
                    baseValue = 5f;
                    break;
            }

            attrib[0] = new SkillAttributesOnLevel();
            attrib[0].level = 0;
            attrib[0].mainAtt = TagUtils.FactoryCountedTag((Tag)TAG.SHIELDING, 0.2f);
            attrib[0].subskillAttributes =
                GenerateAttributeArrayBlock(attribChanged, plus, Mathf.RoundToInt(baseValue * scale * 0.5f));

            for (int i = 1; i <= 5; i++)
            {
                attrib[i] = new SkillAttributesOnLevel();
                attrib[i].level = i;
                attrib[i].mainAtt = TagUtils.FactoryCountedTag((Tag)TAG.SHIELDING, 0.2f);
                attrib[i].subskillAttributes =
                    GenerateAttributeArrayBlock(attribChanged, plus, Mathf.RoundToInt(baseValue * scale * i));
            }

            return attrib;
        }
        static Script GenerateScriptBlock(string scriptName)
        {
            Script sc = new Script();
            sc.scriptName = scriptName;
            return sc;
        }
        static TargetsCount GenerateTargetsCountBlock(int min, int max, ETargetIndicationType selectionMode)
        {
            TargetsCount tc = new TargetsCount();
            tc.minimumCount = min;
            tc.maximumCount = max;
            tc.targetIndication = selectionMode;
            return tc;
        }
        static CountedTag GenerateCTBlock(TAG t, float value)
        {
            return GenerateCTBlock((Tag)t, value);
        }
        static CountedTag GenerateCTBlock(Tag t, float value)
        {
            CountedTag ct = new CountedTag();
            ct.tag = t;
            ct.amount = value;

            return ct;
        }
        static SubSkillShortInfo GenerateSSSIBlock(Subskill ss, int flags, Tag targettingIcon, Tag trigger = null)
        {
            SubSkillShortInfo shortInfo = new SubSkillShortInfo();

            shortInfo.attackInfo = "{SCRIPT:SI_ProceduralDamage}";

            string dmgIconMod = "";


            if ((flags & (int)EActivatorBlocks.TrueDamage) > 0)
            {
                dmgIconMod += "_TRUE";
            }

            if ((flags & (int)EActivatorBlocks.PoisonDamage) > 0)
            {
                dmgIconMod += "_POISON";
            }

            if ((flags & (int)EActivatorBlocks.AOE) > 0)
            {
                dmgIconMod += "_AOE";
            }

            if ((flags & (int)EActivatorBlocks.LifeLeech) > 0)
            {
                dmgIconMod += "_LIFE_LEECH";
            }

            if ((flags & (int)EActivatorBlocks.ShieldLeech) > 0)
            {
                dmgIconMod += "_SHIELD_LEECH";
            }


            if (dmgIconMod.Length > 0)
            {
                string tName = "TAG-DAMAGE" + dmgIconMod;
                if ((flags & (int)EActivatorBlocks.Essence) > 0)
                {
                    tName += "_ESSENCE";
                }
                else if ((flags & (int)EActivatorBlocks.Ancient) > 0)
                {
                    tName += "_ANCIENT";
                }

                if ((flags & (int)EActivatorBlocks.Additive) > 0)
                {
                    tName += "_ADD";
                }

                shortInfo.damageIcon = Globals.GetInstanceFromDB<Tag>(tName);
            }
            else
            {
                shortInfo.damageIcon = (Tag)TAG.DAMAGE_NORMAL;
            }

            if (trigger == null)
            {
                shortInfo.triggerTypeIcon = (Tag)TAG.TRIGGER_DO_ATTACK;
            }
            else
            {
                shortInfo.triggerTypeIcon = trigger;
            }

            shortInfo.targetIcon = targettingIcon;

            return shortInfo;
        }
        static SubSkillShortInfo GenerateSIBlock(Subskill ss, int flags, Tag damageIcon, Tag targettingIcon, Tag triggerIcon)
        {
            SubSkillShortInfo shortInfo = new SubSkillShortInfo();

            shortInfo.attackInfo = "{SCRIPT:SI_DefaultAttributeChange}";

            shortInfo.damageIcon = damageIcon;

            shortInfo.targetIcon = targettingIcon;
            shortInfo.triggerTypeIcon = triggerIcon;

            return shortInfo;
        }
        static SubSkillShortInfo GenerateShieldSIBlock(Subskill ss, int flags, Tag damageIcon, Tag targettingIcon, Tag triggerIcon)
        {
            SubSkillShortInfo shortInfo = new SubSkillShortInfo();

            shortInfo.attackInfo = "{SCRIPT:SI_ShieldUpImproved}";

            shortInfo.damageIcon = damageIcon;

            shortInfo.targetIcon = targettingIcon;
            shortInfo.triggerTypeIcon = triggerIcon;

            return shortInfo;
        }

        static Trigger GenerateTriggerDoAttackBlock(bool requiredInFront, bool requiredInBack)
        {
            Trigger trigger = new Trigger();
            trigger.priority = 1;
            trigger.requiredToBeInFrontline = requiredInFront;
            trigger.requiredToBeInBackline = requiredInBack;
            trigger.script = null;
            trigger.triggerGroup = ETriggerGroupType.DoAttack;
            return trigger;
        }
        static Trigger GenerateTriggerDoAlternativeAttackBlock(bool requiredInFront, bool requiredInBack)
        {
            Trigger trigger = new Trigger();
            trigger.priority = 1;
            trigger.requiredToBeInFrontline = requiredInFront;
            trigger.requiredToBeInBackline = requiredInBack;
            trigger.script = null;
            trigger.triggerGroup = ETriggerGroupType.DoAlternateAttack;
            return trigger;
        }
        static Trigger GenerateTriggerDoCastInstantBlock(bool requiredInFront, bool requiredInBack)
        {
            Trigger trigger = new Trigger();
            trigger.priority = 1;
            trigger.requiredToBeInFrontline = requiredInFront;
            trigger.requiredToBeInBackline = requiredInBack;
            trigger.script = null;
            trigger.triggerGroup = ETriggerGroupType.DoCastInstant;
            return trigger;
        }
        static Trigger GenerateTriggerPassiveBlock()
        {
            Trigger trigger = new Trigger();
            trigger.priority = 1;
            trigger.triggerGroup = ETriggerGroupType.Passive;
            return trigger;
        }
        static Trigger GenerateTriggerOnDamageBlock()
        {
            Trigger trigger = new Trigger();
            trigger.priority = 1;
            trigger.triggerGroup = ETriggerGroupType.OnDamage;
            trigger.script = GenerateScriptBlock("Tri_OnAttackDamageRetaliation");
            return trigger;
        }
        static Trigger GenerateTriggerOnSelfCardPlay()
        {
            Trigger trigger = new Trigger();
            trigger.priority = 1;
            trigger.triggerGroup = ETriggerGroupType.OnCardPlay;
            trigger.script = GenerateScriptBlock("Tri_OnOwnEffectDefault");
            return trigger;
        }
        static TargetsSelector GenerateTargetSelectorBlock(bool manualTargetting, string mainTargetScript, string secondaryTargetScript = null)
        {
            TargetsSelector targets = new TargetsSelector();
            if (!string.IsNullOrEmpty(mainTargetScript))
            {
                targets.script = GenerateScriptBlock(mainTargetScript);
            }
            if (!string.IsNullOrEmpty(secondaryTargetScript))
            {
                targets.script2 = GenerateScriptBlock(secondaryTargetScript);
            }

            if (manualTargetting)
            {
                targets.targetCountRange = GenerateTargetsCountBlock(1, 1, ETargetIndicationType.Chosen);
            }
            else
            {
                targets.targetCountRange = GenerateTargetsCountBlock(1, 1, ETargetIndicationType.Random);
            }
            return targets;
        }
        static TargetsSelector GenerateTargetSelectorShieldUpBlock(bool manualTargetting, string mainTargetScript, string secondaryTargetScript = null)
        {
            TargetsSelector targets = new TargetsSelector();
            targets.script = GenerateScriptBlock(mainTargetScript);
            targets.targetCountRange = GenerateTargetsCountBlock(1, 2, ETargetIndicationType.Chosen);

            return targets;
        }


        #endregion
        #region Subskill generator
        #region FLAG GENERATOR
        static int GenerateAttackFlags(ESkillRank sr, Tag essence1, Tag essence2, bool blockAOE = false)
        {
            int flags = 0;

            switch (sr)
            {
                case ESkillRank.Gray:
                    flags |= (int)EActivatorBlocks.Gray;
                    break;
                case ESkillRank.Essence:
                    flags |= (int)EActivatorBlocks.Essence;
                    break;
                case ESkillRank.Ancient:
                    flags |= (int)EActivatorBlocks.Ancient;
                    break;
            }

            if (sr != ESkillRank.Gray)
            {
                ProduceFlagsByMany(ref flags, essence1, blockAOE);
                ProduceFlagsByMany(ref flags, essence2, blockAOE);
            }

            return flags;
        }

        #endregion
        #region OFFENCES
        static Subskill GenerateSubskill_Sword1H(ESkillRank sr, int damageRank, string skillMarker, int flags, int subskilIndex)
        {
            Subskill ss = new Subskill();
            string ssMarker = subskilIndex + skillMarker;
            ss.dbName = ssMarker;

            ss.challengeTypes = new EChallengeType[] { EChallengeType.TypePhysical };
            ss.childEffect = null;
            ss.trigger = GenerateTriggerDoAttackBlock(true, false);

            //in case of swords which would receive AOE bonus
            //decrease damage further to 90% and add AOE of large swords.
            if (HaveFlag(flags, EActivatorBlocks.AOE))
            {
                ss.targets = GenerateTargetSelectorBlock(false, "Trg_MeleeTargeting", "STrg_MeleeHorizontalSplash");
                ss.subskillAttributesOnLevels = GenerateLeveAttributesBlock(sr, damageRank, (Tag)TAG.CA_1, flags, 0.9f);
                ss.shortInfo = GenerateSSSIBlock(ss, flags, (Tag)TAG.TARGET_HORIZONTAL_MELEE_ENEMY, (Tag)TAG.TRIGGER_DO_ATTACK_MELEE);
            }
            else
            {
                ss.targets = GenerateTargetSelectorBlock(false, "Trg_MeleeTargeting");
                ss.subskillAttributesOnLevels = GenerateLeveAttributesBlock(sr, damageRank, (Tag)TAG.CA_1, flags);
                ss.shortInfo = GenerateSSSIBlock(ss, flags, (Tag)TAG.TARGET_SINGLE_MELEE_ENEMY, (Tag)TAG.TRIGGER_DO_ATTACK_MELEE);
            }

            return ss;
        }
        static Subskill GenerateSubskill_Sword2H(ESkillRank sr, int damageRank, string skillMarker, int flags, int subskilIndex)
        {
            Subskill ss = new Subskill();
            string ssMarker = subskilIndex + skillMarker;
            ss.dbName = ssMarker;

            ss.challengeTypes = new EChallengeType[] { EChallengeType.TypePhysical };
            ss.childEffect = null;

            ss.trigger = GenerateTriggerDoAttackBlock(true, false);
            ss.targets = GenerateTargetSelectorBlock(false, "Trg_MeleeTargeting", "STrg_MeleeHorizontalSplash");
            ss.subskillAttributesOnLevels = GenerateLeveAttributesBlock(sr, damageRank, (Tag)TAG.CA_1, flags);
            ss.shortInfo = GenerateSSSIBlock(ss, flags, (Tag)TAG.TARGET_HORIZONTAL_MELEE_ENEMY, (Tag)TAG.TRIGGER_DO_ATTACK_MELEE);
            return ss;
        }
        static Subskill GenerateSubskill_Axe1H(ESkillRank sr, int damageRank, string skillMarker, int flags, int subskilIndex)
        {
            Subskill ss = new Subskill();
            string ssMarker = subskilIndex + skillMarker;
            ss.dbName = ssMarker;
            ss.challengeTypes = new EChallengeType[] { EChallengeType.TypePhysical };
            ss.childEffect = null;

            ss.trigger = GenerateTriggerDoAttackBlock(true, false);
            ss.targets = GenerateTargetSelectorBlock(false, "Trg_MeleeTargeting");
            ss.subskillAttributesOnLevels = GenerateLeveAttributesBlock(sr, damageRank, (Tag)TAG.CA_1, flags, 1.2f);
            ss.shortInfo = GenerateSSSIBlock(ss, flags, (Tag)TAG.TARGET_SINGLE_MELEE_ENEMY, (Tag)TAG.TRIGGER_DO_ATTACK_MELEE);
            return ss;
        }
        static Subskill GenerateSubskill_Axe2H(ESkillRank sr, int damageRank, string skillMarker, int flags, int subskilIndex)
        {
            Subskill ss = new Subskill();
            string ssMarker = subskilIndex + skillMarker;
            ss.dbName = ssMarker;
            ss.challengeTypes = new EChallengeType[] { EChallengeType.TypePhysical };
            ss.childEffect = null;

            ss.trigger = GenerateTriggerDoAttackBlock(true, false);
            ss.targets = GenerateTargetSelectorBlock(false, "Trg_MeleeTargeting");
            ss.subskillAttributesOnLevels = GenerateLeveAttributesBlock(sr, damageRank, (Tag)TAG.CA_1, flags, 1.2f);
            ss.shortInfo = GenerateSSSIBlock(ss, flags, (Tag)TAG.TARGET_SINGLE_MELEE_ENEMY, (Tag)TAG.TRIGGER_DO_ATTACK_MELEE);
            return ss;
        }
        static Subskill GenerateSubskill_Club1H(ESkillRank sr, int damageRank, string skillMarker, int flags, int subskilIndex)
        {
            Subskill ss = new Subskill();
            string ssMarker = subskilIndex + skillMarker;
            ss.dbName = ssMarker;
            ss.challengeTypes = new EChallengeType[] { EChallengeType.TypePhysical };
            ss.childEffect = null;

            ss.trigger = GenerateTriggerDoAttackBlock(true, false);
            ss.targets = GenerateTargetSelectorBlock(false, "Trg_MeleeTargeting", "STrg_MeleeTSplash");
            ss.subskillAttributesOnLevels = GenerateLeveAttributesBlock(sr, damageRank, (Tag)TAG.CA_1, flags);
            ss.shortInfo = GenerateSSSIBlock(ss, flags, (Tag)TAG.TARGET_T_SPLASH_ENEMY, (Tag)TAG.TRIGGER_DO_ATTACK_MELEE);
            return ss;
        }
        static Subskill GenerateSubskill_Club2H(ESkillRank sr, int damageRank, string skillMarker, int flags, int subskilIndex)
        {
            Subskill ss = new Subskill();
            string ssMarker = subskilIndex + skillMarker;
            ss.dbName = ssMarker;
            ss.challengeTypes = new EChallengeType[] { EChallengeType.TypePhysical };
            ss.childEffect = null;

            ss.trigger = GenerateTriggerDoAttackBlock(true, false);
            ss.targets = GenerateTargetSelectorBlock(false, "Trg_MeleeTargeting", "STrg_MeleeTSplash");
            ss.subskillAttributesOnLevels = GenerateLeveAttributesBlock(sr, damageRank, (Tag)TAG.CA_1, flags);
            ss.shortInfo = GenerateSSSIBlock(ss, flags, (Tag)TAG.TARGET_T_SPLASH_ENEMY, (Tag)TAG.TRIGGER_DO_ATTACK_MELEE);
            return ss;
        }
        static Subskill GenerateSubskill_Polarm1H(ESkillRank sr, int damageRank, string skillMarker, int flags, int subskilIndex)
        {
            Subskill ss = new Subskill();
            string ssMarker = subskilIndex + skillMarker;
            ss.dbName = ssMarker;
            ss.challengeTypes = new EChallengeType[] { EChallengeType.TypePhysical };
            ss.childEffect = null;

            ss.trigger = GenerateTriggerDoAttackBlock(false, false);
            ss.targets = GenerateTargetSelectorBlock(false, "Trg_MeleeTargeting", "STrg_MeleeVerticalSplash");
            ss.subskillAttributesOnLevels = GenerateLeveAttributesBlock(sr, damageRank, (Tag)TAG.CA_1, flags, 0.8f);
            ss.shortInfo = GenerateSSSIBlock(ss, flags, (Tag)TAG.TARGET_VERTICAL_MELEE_ENEMY, (Tag)TAG.TRIGGER_DO_ATTACK);
            return ss;
        }
        static Subskill GenerateSubskill_Polarm1H_PreStrike(ESkillRank sr, int damageRank, string skillMarker, int flags, int subskilIndex, Tag attackAttribute)
        {
            Subskill ss = new Subskill();
            string ssMarker = subskilIndex + skillMarker;
            ss.dbName = ssMarker;
            ss.challengeTypes = new EChallengeType[] { EChallengeType.TypePhysical };
            ss.childEffect = null;

            ss.trigger = GenerateTriggerOnSelfCardPlay();
            ss.targets = GenerateTargetSelectorBlock(false, "Trg_SpearOnlyTargeting", "STrg_MeleeVerticalSplash");
            ss.subskillAttributesOnLevels = GenerateLeveAttributesBlock(sr, damageRank, attackAttribute, flags, 0.6f);
            ss.shortInfo = GenerateSSSIBlock(ss, flags, (Tag)TAG.TARGET_DIRECTLY_OPPOSITE, (Tag)TAG.TRIGGER_ON_CARD_SELF_PLAY);
            return ss;
        }
        static Subskill GenerateSubskill_Polarm2H(ESkillRank sr, int damageRank, string skillMarker, int flags, int subskilIndex)
        {
            Subskill ss = new Subskill();
            string ssMarker = subskilIndex + skillMarker;
            ss.dbName = ssMarker;
            ss.challengeTypes = new EChallengeType[] { EChallengeType.TypePhysical };
            ss.childEffect = null;

            ss.trigger = GenerateTriggerDoAttackBlock(false, false);
            ss.targets = GenerateTargetSelectorBlock(false, "Trg_MeleeTargeting", "STrg_MeleeVerticalSplash");
            ss.subskillAttributesOnLevels = GenerateLeveAttributesBlock(sr, damageRank, (Tag)TAG.CA_1, flags, 0.8f);
            ss.shortInfo = GenerateSSSIBlock(ss, flags, (Tag)TAG.TARGET_VERTICAL_MELEE_ENEMY, (Tag)TAG.TRIGGER_DO_ATTACK);
            return ss;
        }
        static Subskill GenerateSubskill_Polarm2H_PreStrike(ESkillRank sr, int damageRank, string skillMarker, int flags, int subskilIndex, Tag attackAttribute)
        {
            Subskill ss = new Subskill();
            string ssMarker = subskilIndex + skillMarker;
            ss.dbName = ssMarker;
            ss.challengeTypes = new EChallengeType[] { EChallengeType.TypePhysical };
            ss.childEffect = null;

            ss.trigger = GenerateTriggerOnSelfCardPlay();
            ss.targets = GenerateTargetSelectorBlock(false, "Trg_SpearOnlyTargeting", "STrg_MeleeVerticalSplash");
            ss.subskillAttributesOnLevels = GenerateLeveAttributesBlock(sr, damageRank, attackAttribute, flags, 0.6f);
            ss.shortInfo = GenerateSSSIBlock(ss, flags, (Tag)TAG.TARGET_DIRECTLY_OPPOSITE, (Tag)TAG.TRIGGER_ON_CARD_SELF_PLAY);
            return ss;
        }
        static Subskill GenerateSubskill_Artifact1H(ESkillRank sr, int damageRank, string skillMarker, int flags, int subskilIndex)
        {
            Subskill ss = new Subskill();
            string ssMarker = subskilIndex + skillMarker;
            ss.dbName = ssMarker;
            ss.challengeTypes = new EChallengeType[] { EChallengeType.TypeMental, EChallengeType.TypeSpirit };
            ss.childEffect = null;

            ss.trigger = GenerateTriggerDoAttackBlock(true, false);
            ss.targets = GenerateTargetSelectorBlock(false, "Trg_MeleeTargeting", "STrg_MeleeTSplash");
            ss.subskillAttributesOnLevels = GenerateLeveAttributesBlock(sr, damageRank, (Tag)TAG.CA_2, flags);
            ss.shortInfo = GenerateSSSIBlock(ss, flags, (Tag)TAG.TARGET_T_SPLASH_ENEMY, (Tag)TAG.TRIGGER_DO_ATTACK_MELEE);
            return ss;
        }
        static Subskill GenerateSubskill_Artifact2H(ESkillRank sr, int damageRank, string skillMarker, int flags, int subskilIndex)
        {
            Subskill ss = new Subskill();
            string ssMarker = subskilIndex + skillMarker;
            ss.dbName = ssMarker;
            ss.challengeTypes = new EChallengeType[] { EChallengeType.TypeMental, EChallengeType.TypeSpirit };
            ss.childEffect = null;

            ss.trigger = GenerateTriggerDoAttackBlock(true, false);
            ss.targets = GenerateTargetSelectorBlock(false, "Trg_MeleeTargeting", "STrg_MeleeTSplash");
            ss.subskillAttributesOnLevels = GenerateLeveAttributesBlock(sr, damageRank, (Tag)TAG.CA_2, flags);
            ss.shortInfo = GenerateSSSIBlock(ss, flags, (Tag)TAG.TARGET_T_SPLASH_ENEMY, (Tag)TAG.TRIGGER_DO_ATTACK_MELEE);
            return ss;
        }
        static Subskill GenerateSubskill_Bow(ESkillRank sr, int damageRank, string skillMarker, int flags, int subskilIndex)
        {
            Subskill ss = new Subskill();
            string ssMarker = subskilIndex + skillMarker;
            ss.dbName = ssMarker;
            ss.challengeTypes = new EChallengeType[] { EChallengeType.TypePhysical };
            ss.childEffect = null;

            ss.trigger = GenerateTriggerDoAlternativeAttackBlock(false, true);

            if (HaveFlag(flags, EActivatorBlocks.AOE))
            {
                ss.targets = GenerateTargetSelectorBlock(true, "Trg_RangeTargeting_Unblocked", "STrg_HorizontalSplash");
                ss.shortInfo = GenerateSSSIBlock(ss, flags, (Tag)TAG.TARGET_SELECTED_AND_HORIZONTAL_ENEMIES, (Tag)TAG.TRIGGER_DO_ATTACK_RANGED);
            }
            else
            {
                ss.targets = GenerateTargetSelectorBlock(true, "Trg_RangeTargeting_Unblocked");
                ss.shortInfo = GenerateSSSIBlock(ss, flags, (Tag)TAG.TARGET_SELECTED_ENEMY, (Tag)TAG.TRIGGER_DO_ATTACK_RANGED);

            }

            ss.subskillAttributesOnLevels = GenerateLeveAttributesBlock(sr, damageRank, (Tag)TAG.CA_2, flags);

            return ss;
        }
        static Subskill GenerateSubskill_TSpear(ESkillRank sr, int damageRank, string skillMarker, int flags, int subskilIndex)
        {
            Subskill ss = new Subskill();
            string ssMarker = subskilIndex + skillMarker;
            ss.dbName = ssMarker;
            ss.challengeTypes = new EChallengeType[] { EChallengeType.TypePhysical };
            ss.childEffect = null;

            ss.trigger = GenerateTriggerDoAlternativeAttackBlock(false, true);

            ss.targets = GenerateTargetSelectorBlock(false, "Trg_RangeTargeting");
            ss.subskillAttributesOnLevels = GenerateLeveAttributesBlock(sr, damageRank, (Tag)TAG.CA_2, flags, 0.8f);
            ss.shortInfo = GenerateSSSIBlock(ss, flags, (Tag)TAG.TARGET_RANGED_ENEMY, (Tag)TAG.TRIGGER_DO_ATTACK_RANGED);
            return ss;
        }
        static Subskill GenerateSubskill_Wand(ESkillRank sr, int damageRank, string skillMarker, int flags, int subskilIndex)
        {
            Subskill ss = new Subskill();
            string ssMarker = subskilIndex + skillMarker;
            ss.dbName = ssMarker;
            ss.challengeTypes = new EChallengeType[] { EChallengeType.TypeMental, EChallengeType.TypeSpirit };
            ss.childEffect = null;

            ss.trigger = GenerateTriggerDoAlternativeAttackBlock(false, true);
            if ((flags & (int)EActivatorBlocks.AOE) > 0)
            {
                ss.targets = GenerateTargetSelectorBlock(false, "Trg_RangeTargeting", "STrg_MeleeVerticalSplashWholeColumn");
            }
            else
            {
                ss.targets = GenerateTargetSelectorBlock(true, "Trg_RangeTargeting");
            }

            ss.subskillAttributesOnLevels = GenerateLeveAttributesBlock(sr, damageRank, (Tag)TAG.DESTINY, flags);
            ss.shortInfo = GenerateSSSIBlock(ss, flags, (Tag)TAG.TARGET_RANGED_ENEMY, (Tag)TAG.TRIGGER_DO_ATTACK_RANGED);
            return ss;
        }
        #endregion
        #region EVENT SKILLS
        static Subskill GenerateSubskill_ReflectDamage(ESkillRank sr, int damageRank, string skillMarker, int flags, int subskilIndex)
        {
            Subskill ss = new Subskill();
            string ssMarker = subskilIndex + skillMarker;
            ss.dbName = ssMarker;
            ss.animation = "Hit4";
            ss.challengeTypes = new EChallengeType[] { EChallengeType.TypePhysical, EChallengeType.TypeMental, EChallengeType.TypeSpirit };
            ss.childEffect = null;

            ss.sound = "";
            ss.trigger = GenerateTriggerOnDamageBlock();

            ss.targets = GenerateTargetSelectorBlock(false, "");
            ss.shortInfo = GenerateSSSIBlock(ss, flags, (Tag)TAG.TARGET_SELF, (Tag)TAG.TRIGGER_ON_DAMAGE);

            ss.subskillAttributesOnLevels = GenerateLeveAttributesBlock(sr, damageRank, (Tag)TAG.CA_1, flags);
            return ss;
        }
        #endregion
        #region DEFENCES
        static Subskill GenerateSubskill_ShieldsUp(ESkillRank sr, float defenseScale, string skillMarker, int flags, int subskilIndex)
        {
            Subskill ss = new Subskill();
            string ssMarker = subskilIndex + skillMarker;
            ss.dbName = ssMarker;
            ss.challengeTypes = null;
            ss.animation = "ShieldBonus";
            ss.childEffect = null;

            ss.sound = "SubskillAddShielding";

            ss.trigger = GenerateTriggerDoCastInstantBlock(false, false);
            ss.targets = GenerateTargetSelectorShieldUpBlock(true, "Trg_FriendlyTarget");

            if (sr == ESkillRank.Gray)
            {
                ss.challengeTypes = new EChallengeType[] { EChallengeType.TypePhysical };
                ss.shortInfo = GenerateShieldSIBlock(ss, flags, (Tag)TAG.SHIELDING_PHYSICAL, (Tag)TAG.TARGET_SELECTED_ALLIES, (Tag)TAG.TRIGGER_DO_CAST_INSTANT);
                ss.subskillAttributesOnLevels = GenerateShieldUpLeveAttributesBlock(sr, true, defenseScale, (Tag)TAG.SHIELDING_PHYSICAL);
            }
            else if (sr == ESkillRank.Essence)
            {
                ss.challengeTypes = new EChallengeType[] { EChallengeType.TypePhysical, EChallengeType.TypeMental, EChallengeType.TypeSpirit };
                ss.shortInfo = GenerateShieldSIBlock(ss, flags, (Tag)TAG.DAMAGE_SHIELD, (Tag)TAG.TARGET_SELECTED_ALLIES, (Tag)TAG.TRIGGER_DO_CAST_INSTANT);
                ss.subskillAttributesOnLevels = GenerateShieldUpLeveAttributesBlock(sr, true, .75f * defenseScale, (Tag)TAG.CA_SHIELD);
            }
            else if (sr == ESkillRank.Ancient)
            {
                ss.challengeTypes = new EChallengeType[] { EChallengeType.TypePhysical, EChallengeType.TypeMental, EChallengeType.TypeSpirit };
                ss.shortInfo = GenerateShieldSIBlock(ss, flags, (Tag)TAG.DAMAGE_SHIELD, (Tag)TAG.TARGET_SELECTED_ALLIES, (Tag)TAG.TRIGGER_DO_CAST_INSTANT);
                ss.subskillAttributesOnLevels = GenerateShieldUpLeveAttributesBlock(sr, true, .6f * defenseScale, (Tag)TAG.CA_SHIELD);
            }
            return ss;
        }
        static Subskill GenerateSubskill_Shield(ESkillRank sr, float defenseScale, string skillMarker, int flags, int subskilIndex)
        {
            Subskill ss = new Subskill();
            string ssMarker = subskilIndex + skillMarker;
            ss.dbName = ssMarker;
            ss.animation = null;
            ss.challengeTypes = null;
            ss.childEffect = null;

            ss.sound = "";
            ss.trigger = GenerateTriggerPassiveBlock();
            ss.targets = null;
            if (sr == ESkillRank.Gray)
            {
                float min = 2.5f * defenseScale * 2;
                float max = 15f * defenseScale * 2;
                ss.shortInfo = GenerateSIBlock(ss, flags, (Tag)TAG.SHIELDING_PHYSICAL, (Tag)TAG.TARGET_SELF, (Tag)TAG.TRIGGER_PASSIVE);
                ss.subskillAttributesOnLevels = GenerateLeveLinearAttributesBlock(sr, true, min, max, (Tag)TAG.SHIELDING_PHYSICAL);
            }
            else if (sr == ESkillRank.Essence)
            {
                float min = 3.5f * defenseScale;
                float max = 20f * defenseScale;

                ss.shortInfo = GenerateSIBlock(ss, flags, (Tag)TAG.DAMAGE_SHIELD, (Tag)TAG.TARGET_SELF, (Tag)TAG.TRIGGER_PASSIVE);
                ss.subskillAttributesOnLevels = GenerateLeveLinearAttributesBlock(sr, true, min, max, (Tag)TAG.CA_SHIELD);
            }
            else if (sr == ESkillRank.Ancient)
            {
                float min = 3.0f * defenseScale * 0.66f;
                float max = 28f * defenseScale * 0.66f;

                ss.shortInfo = GenerateSIBlock(ss, flags, (Tag)TAG.DAMAGE_SHIELD, (Tag)TAG.TARGET_SELF, (Tag)TAG.TRIGGER_PASSIVE);
                ss.subskillAttributesOnLevels = GenerateLeveLinearAttributesBlock(sr, true, min, max, (Tag)TAG.CA_SHIELD);
            }
            return ss;
        }
        static Subskill GenerateSubskill_Parry(ESkillRank sr, float defenseScale, string skillMarker, int flags, int subskilIndex)
        {
            Subskill ss = new Subskill();
            string ssMarker = subskilIndex + skillMarker;
            ss.dbName = ssMarker;
            ss.animation = null;
            ss.challengeTypes = null;
            ss.childEffect = null;

            ss.sound = "";
            ss.trigger = GenerateTriggerPassiveBlock();
            ss.targets = null;
            if (sr == ESkillRank.Gray)
            {
                float min = 15f * defenseScale;
                float max = 50f * defenseScale;

                ss.shortInfo = GenerateSIBlock(ss, flags, (Tag)TAG.SHIELDING_PHYSICAL, (Tag)TAG.TARGET_SELF, (Tag)TAG.TRIGGER_PASSIVE);
                ss.subskillAttributesOnLevels = GenerateLeveNewAttributesBlock(sr, false, min, max, (Tag)TAG.SHIELDING_PHYSICAL);
            }
            else if (sr == ESkillRank.Essence)
            {
                float min = 30f * defenseScale;
                float max = 80f * defenseScale;

                ss.shortInfo = GenerateSIBlock(ss, flags, (Tag)TAG.DAMAGE_SHIELD, (Tag)TAG.TARGET_SELF, (Tag)TAG.TRIGGER_PASSIVE);
                ss.subskillAttributesOnLevels = GenerateLeveNewAttributesBlock(sr, false, min, max, (Tag)TAG.CA_SHIELD);
            }
            else
            {
                float min = 50f * defenseScale;
                float max = 100f * defenseScale;

                ss.shortInfo = GenerateSIBlock(ss, flags, (Tag)TAG.DAMAGE_SHIELD, (Tag)TAG.TARGET_SELF, (Tag)TAG.TRIGGER_PASSIVE);
                ss.subskillAttributesOnLevels = GenerateLeveNewAttributesBlock(sr, false, min, max, (Tag)TAG.CA_SHIELD);
            }

            return ss;
        }
        //         static Subskill GenerateSubskill_SpearStrike(ESkillRank sr, float defenseScale, string skillMarker, int flags, int subskilIndex)
        //         {
        //             Subskill ss = new Subskill();
        //             string ssMarker = subskilIndex + skillMarker;
        //             ss.dbName = ssMarker;
        //             ss.animation = null;
        //             ss.challengeTypes = null;
        //             ss.childEffect = null;
        // 
        //             ss.sound = "";
        //             ss.trigger = GenerateTriggerPassiveBlock();
        //             ss.targets = null;
        //             if (sr == ESkillRank.Gray)
        //             {
        //                 ss.shortInfo = GenerateSIBlock(ss, flags, (Tag)TAG.SHIELDING_PHYSICAL, (Tag)TAG.TARGET_SELF, (Tag)TAG.TRIGGER_PASSIVE);
        //                 ss.subskillAttributesOnLevels = GenerateLeveAttributesBlock(sr, true, defenseScale, (Tag)TAG.SHIELDING_PHYSICAL);
        //             }
        //             else if (sr == ESkillRank.Essence)
        //             {
        //                 ss.shortInfo = GenerateSIBlock(ss, flags, (Tag)TAG.DAMAGE_SHIELD, (Tag)TAG.TARGET_SELF, (Tag)TAG.TRIGGER_PASSIVE);
        //                 ss.subskillAttributesOnLevels = GenerateLeveAttributesBlock(sr, true, defenseScale, (Tag)TAG.CA_SHIELD);
        //             }
        //             else
        //             {
        //                 ss.shortInfo = GenerateSIBlock(ss, flags, (Tag)TAG.DAMAGE_SHIELD, (Tag)TAG.TARGET_SELF, (Tag)TAG.TRIGGER_PASSIVE);
        //                 ss.subskillAttributesOnLevels = GenerateLeveAttributesBlock(sr, true, defenseScale * 1.5f, (Tag)TAG.CA_SHIELD);
        //             }
        // 
        //             return ss;
        //         }
        static Subskill GenerateSubskill_AttMultiplier(ESkillRank sr, float from, float to, string skillMarker, int flags, int subskilIndex, Tag attribute)
        {
            Subskill ss = new Subskill();
            string ssMarker = subskilIndex + skillMarker;
            ss.dbName = ssMarker;
            ss.animation = null;
            ss.challengeTypes = null;
            ss.childEffect = null;

            ss.sound = "";

            ss.shortInfo = GenerateSIBlock(ss, flags, attribute, (Tag)TAG.TARGET_SELF, (Tag)TAG.TRIGGER_PASSIVE);
            ss.trigger = GenerateTriggerPassiveBlock();
            ss.targets = null;

            ss.subskillAttributesOnLevels = GenerateLeveLinearAttributesBlock(sr, false, from, to, attribute);
            return ss;
        }
        static Subskill GenerateSubskill_AttAddon(ESkillRank sr, float from, float to, string skillMarker, int flags, int subskilIndex, Tag attribute)
        {
            Subskill ss = new Subskill();
            string ssMarker = subskilIndex + skillMarker;
            ss.dbName = ssMarker;
            ss.animation = null;
            ss.challengeTypes = null;
            ss.childEffect = null;

            ss.sound = "";

            ss.shortInfo = GenerateSIBlock(ss, flags, attribute, (Tag)TAG.TARGET_SELF, (Tag)TAG.TRIGGER_PASSIVE);
            ss.trigger = GenerateTriggerPassiveBlock();
            ss.targets = null;

            ss.subskillAttributesOnLevels = GenerateLeveNewAttributesBlock(sr, true, from, to, attribute);
            return ss;
        }
        static Subskill GenerateSubskill_MPAddon(ESkillRank sr, int scale, string skillMarker, int flags, int subskilIndex, Tag attribute)
        {
            Subskill ss = new Subskill();
            string ssMarker = subskilIndex + skillMarker;
            ss.dbName = ssMarker;
            ss.animation = null;
            ss.challengeTypes = null;
            ss.childEffect = null;

            ss.sound = "";

            ss.shortInfo = GenerateSIBlock(ss, flags, attribute, (Tag)TAG.TARGET_SELF, (Tag)TAG.TRIGGER_PASSIVE);
            ss.trigger = GenerateTriggerPassiveBlock();
            ss.targets = null;

            ss.subskillAttributesOnLevels = GenerateFlatAttributesBlock(sr, scale, attribute, flags);
            return ss;
        }
        #endregion
        #region FINISHING SKILLS
        static void FinishOffenseSkills(Skill s, Subskill ss, List<Subskill> sss,
                                        ESkillRank sr, RecipeAttributes ra,
                                        Tag essence1, Tag essence2, ItemRecipe ir, int flags, bool ranged = false)
        {
            ss.delay = 11.0f - ra.speedStars * 1.5f;
            //             if (HaveFlag(flags, EActivatorBlocks.Speed) && HaveFlag(flags, EActivatorBlocks.Essence)) ss.delay *= 0.7f;
            //             if (HaveFlag(flags, EActivatorBlocks.Speed) && HaveFlag(flags, EActivatorBlocks.Ancient)) ss.delay *= 0.4f;

            if (sr != ESkillRank.Gray) UpgradeSkillChallengeList(essence1, essence2, ss);

            ss.activation = GenerateScriptBlock("Act_Damage_Procedural");
            ss.animation = string.IsNullOrEmpty(ss.animation) ? ra.animation : ss.animation;
            ss.sound = string.IsNullOrEmpty(ss.sound) ? ra.sound : ss.sound;

            sss.Add(ss);
            Int64 intID = Convert.ToInt64(ss.dbName);
            markerToSubSkillDictionary[intID] = ss;
        }
        static void FinishSimpleDamageSkills(Skill s, Subskill ss, List<Subskill> sss,
                                        ESkillRank sr, RecipeAttributes ra,
                                        Tag essence1, Tag essence2, ItemRecipe ir, int flags, bool ranged = false)
        {
            ss.delay = 11.0f - ra.speedStars * 1.5f;
            //             if (HaveFlag(flags, EActivatorBlocks.Speed) && HaveFlag(flags, EActivatorBlocks.Essence)) ss.delay *= 0.7f;
            //             if (HaveFlag(flags, EActivatorBlocks.Speed) && HaveFlag(flags, EActivatorBlocks.Ancient)) ss.delay *= 0.4f;

            if (sr != ESkillRank.Gray) UpgradeSkillChallengeList(essence1, essence2, ss);

            ss.activation = GenerateScriptBlock("Act_Damage_Procedural");
            ss.animation = string.IsNullOrEmpty(ss.animation) ? ra.animation : ss.animation;
            ss.sound = string.IsNullOrEmpty(ss.sound) ? ra.sound : ss.sound;
            sss.Add(ss);
            Int64 intID = Convert.ToInt64(ss.dbName);
            markerToSubSkillDictionary[intID] = ss;
        }
        static void FinishAttributeSkills(Skill s, Subskill ss, List<Subskill> sss,
                                        ESkillRank sr, RecipeAttributes ra,
                                        Tag essence1, Tag essence2, ItemRecipe ir)
        {
            ss.delay = 0f;
            ss.activation = GenerateScriptBlock("WAct_AttributeChange");
            ss.animation = string.IsNullOrEmpty(ss.animation) ? ra.animation : ss.animation;
            ss.sound = string.IsNullOrEmpty(ss.sound) ? ra.sound : ss.sound;
            sss.Add(ss);
            Int64 intID = Convert.ToInt64(ss.dbName);
            markerToSubSkillDictionary[intID] = ss;

            float statMultiplier = 1f;
            //         if (essence1 == (Tag)TAG.ESSENCE_METAL ||
            //             essence2 == (Tag)TAG.ESSENCE_METAL)
            //         {
            //             statMultiplier *= UpgradeByFire(ir, sr, ss);
            //         }
            // 
            //         if (essence1 == (Tag)TAG.ESSENCE_BONE ||
            //             essence2 == (Tag)TAG.ESSENCE_BONE)
            //         {
            //             statMultiplier *= UpgradeBySoul(ir, sr, ss);
            //         }
            // 
            //         if (essence1 == (Tag)TAG.ESSENCE_STONE ||
            //             essence2 == (Tag)TAG.ESSENCE_STONE)
            //         {
            //             statMultiplier *= UpgradeByMind(ir, sr, ss);
            //         }

            if (statMultiplier != 1f)
            {
                UpdateAttributeMultipliers(ss, statMultiplier);
            }
        }
        static void FinishPassiveShieldSkills(Skill s, Subskill ss, List<Subskill> sss,
                                        ESkillRank sr, RecipeAttributes ra,
                                        Tag essence1, Tag essence2, ItemRecipe ir)
        {
            ss.delay = 0f;
            ss.activation = GenerateScriptBlock("WAct_ShieldChange");
            sss.Add(ss);
            Int64 intID = Convert.ToInt64(ss.dbName);
            markerToSubSkillDictionary[intID] = ss;

            // 
            //             if (DBTypeUtils<Resource>.GetParentsWithSelf(material).Contains((Resource)RES.SANDSTONE))
            //             {
            //                 statMultiplier *= UpgradeShieldByStone(ir, sr, ss);
            //             }

            //             if (statMultiplier != 1f)
            //             {
            //                 UpdateAttributeMultipliers(ss, statMultiplier);
            //             }
        }
        static void FinishActiveShieldSkills(Skill s, Subskill ss, List<Subskill> sss,
                                        ESkillRank sr, RecipeAttributes ra,
                                        Tag essence1, Tag essence2, ItemRecipe ir)
        {
            ss.delay = 0f;
            ss.activation = GenerateScriptBlock("Act_AddShieldingImproved");
            ss.animation = string.IsNullOrEmpty(ss.animation) ? ra.animation : ss.animation;
            ss.sound = string.IsNullOrEmpty(ss.sound) ? ra.sound : ss.sound;
            sss.Add(ss);
            Int64 intID = Convert.ToInt64(ss.dbName);
            markerToSubSkillDictionary[intID] = ss;

            //             float statMultiplier = 1f;
            // 
            //             if (DBTypeUtils<Resource>.GetParentsWithSelf(material).Contains((Resource)RES.SANDSTONE))
            //             {
            //                 statMultiplier *= UpgradeShieldByStone(ir, sr, ss);
            //             }
            // 
            //             if (statMultiplier != 1f)
            //             {
            //                 UpdateAttributeMultipliers(ss, statMultiplier);
            //             }
        }
        #endregion

        #endregion
        static void UpgradeSkillChallengeList(Tag essence1, Tag essence2, Subskill ss)
        {
            List<EChallengeType> s = new List<EChallengeType>();

            if ((essence1 == (Tag)TAG.ESSENCE_METAL ||
                essence1 == (Tag)TAG.ESSENCE_LEATHER) &&
               (essence2 == (Tag)TAG.ESSENCE_METAL ||
                essence2 == (Tag)TAG.ESSENCE_LEATHER))
            {
                s.Add(EChallengeType.TypePhysical);

                if (Array.FindIndex(ss.challengeTypes, o => o == EChallengeType.TypeMental) > -1)
                {
                    s.Add(EChallengeType.TypeMental);
                }

                if (Array.FindIndex(ss.challengeTypes, o => o == EChallengeType.TypeSpirit) > -1)
                {
                    s.Add(EChallengeType.TypeSpirit);
                }

                ss.challengeTypes = s.ToArray();
            }
            else if
              ((essence1 == (Tag)TAG.ESSENCE_WOOD ||
                essence1 == (Tag)TAG.ESSENCE_STONE) &&
               (essence2 == (Tag)TAG.ESSENCE_WOOD ||
                essence2 == (Tag)TAG.ESSENCE_STONE))
            {
                if (Array.FindIndex(ss.challengeTypes, o => o == EChallengeType.TypePhysical) > -1)
                {
                    s.Add(EChallengeType.TypePhysical);
                }

                s.Add(EChallengeType.TypeMental);

                if (Array.FindIndex(ss.challengeTypes, o => o == EChallengeType.TypeSpirit) > -1)
                {
                    s.Add(EChallengeType.TypeSpirit);
                }

                ss.challengeTypes = s.ToArray();
            }
            else if
              ((essence1 == (Tag)TAG.ESSENCE_GEM ||
                essence1 == (Tag)TAG.ESSENCE_BONE) &&
               (essence2 == (Tag)TAG.ESSENCE_GEM ||
                essence2 == (Tag)TAG.ESSENCE_BONE))
            {
                if (Array.FindIndex(ss.challengeTypes, o => o == EChallengeType.TypePhysical) > -1)
                {
                    s.Add(EChallengeType.TypePhysical);
                }

                if (Array.FindIndex(ss.challengeTypes, o => o == EChallengeType.TypeMental) > -1)
                {
                    s.Add(EChallengeType.TypeMental);
                }

                //                 if (Array.FindIndex(ss.challengeTypes, o => o == EChallengeType.TypeSpirit) > -1)
                //                 {
                s.Add(EChallengeType.TypeSpirit);
                //                }

                ss.challengeTypes = s.ToArray();
            }

        }
        static bool HaveFlag(int data, EActivatorBlocks flag)
        {
            return (data & (int)flag) > 0;
        }

        #region Subskill Skill Rank modifier
        /// <summary>
        /// Fire increases Damage by 
        /// Essence: 20%  
        /// Ancient: 30%
        /// </summary>
        /// <param name="skillRank"></param>
        /// <param name="ss"></param>
        /// <returns></returns>
        static float UpgradeShieldByStone(ItemRecipe ir, ESkillRank skillRank, Subskill ss)
        {
            if (skillRank == ESkillRank.Gray)
            {
                return 1.1f;
            }

            return 1.2f;
        }
        static float UpgradeWeaponByFire(ItemRecipe ir, ESkillRank skillRank, Subskill ss)
        {
            if (skillRank == ESkillRank.Gray)
            {
                ss.delay -= 0.5f;
                return 1f;
            }

            if (skillRank == ESkillRank.Essence)
            {
                ss.delay -= 1;
            }
            else if (skillRank == ESkillRank.Ancient)
            {
                ss.delay -= 2;
            }

            return 1f;
        }

        static void UpdateAttributeMultipliers(Subskill ss, float multiplier)
        {
            foreach (var v in ss.subskillAttributesOnLevels)
            {
                if (v.mainAtt != null)
                {
                    v.mainAtt.amount *= multiplier;
                }
            }
        }
        static void ProduceFlagsByMany(ref int flags, Tag essence, bool blockAOE = false)
        {
            if (essence == (Tag)TAG.ESSENCE_WOOD)
            {
                flags |= (int)EActivatorBlocks.PoisonDamage;
            }
            else if (essence == (Tag)TAG.ESSENCE_GEM && !blockAOE)
            {
                flags |= (int)EActivatorBlocks.AOE;
            }
            else if (essence == (Tag)TAG.ESSENCE_LEATHER)
            {
                flags |= (int)EActivatorBlocks.Additive;
            }
            else if (essence == (Tag)TAG.ESSENCE_METAL)
            {
                flags |= (int)EActivatorBlocks.TrueDamage;
            }
            else if (essence == (Tag)TAG.ESSENCE_BONE)
            {
                flags |= (int)EActivatorBlocks.LifeLeech;
            }
            else if (essence == (Tag)TAG.ESSENCE_STONE)
            {
                flags |= (int)EActivatorBlocks.ShieldLeech;
            }
        }

        #endregion

        static public bool HaveGenerator(DBClass source)
        {
            if (source is ItemRecipe)
            {
                ItemRecipe ir = source as ItemRecipe;
                if (ir.inventorySlot != null && ir.inventorySlot.Length > 0)
                {
                    string l = "ITEM_REC-";
                    string name = ir.dbName.Substring(l.Length);

                    for (int i = 0; i <= (int)ITEM_REC.HEAVY_ARMOUR; i++)
                    {
                        if (((ITEM_REC)i).ToString() == name)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
#endif