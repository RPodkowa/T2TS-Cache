#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR
using System.Collections.Generic;
using DBDef;
using Thea2.Common;
using Thea2.Server;
using Thea2.General;
using TheHoney;
using Thea2;
using UnityEngine;
using System.Linq;
using System;

namespace GameScript
{
    public class RitualScripts : ScriptBase
    {
        #region RIT - scripts for Rituals
        static public object Rit_HealSHP(Thea2.Server.Group target, RitualsTask sourceTask)
        {
            if (target != null && target.characters != null)
            {
                Tag shp = (Tag)TAG.HEALTH_SPIRIT;
                foreach (var v in target.characters)
                {
                    FInt f = v.Get().attributes.GetBase(shp);
                    FInt max = v.Get().maxSHP;
                    if (max.ToInt() == f.ToInt()) continue;

                    int heal = Mathf.Max(1,  max.ToInt() / 2);
                    int newHP = Mathf.Min(max.ToInt(), (f + heal).ToInt());

                    v.Get().attributes.SetBaseTo(shp, FInt.ONE * newHP);
                }
            }
            return null;
        }
        static public object Rit_CurseRemoval(Thea2.Server.Group target, RitualsTask sourceTask)
        {
            if (target != null && target.characters != null)
            {
                SkillPack spp = (SkillPack)"SKILL_PACK-CURSE_P";
                SkillPack spm = (SkillPack)"SKILL_PACK-CURSE_M";
                SkillPack sps = (SkillPack)"SKILL_PACK-CURSE_S";
                
                foreach (var v in target.characters)
                {
                    var si = v.Get().GetSkillEffectOnCharacterMatchingSkillpack(spp);
                    if(si == null)
                    {
                        si = v.Get().GetSkillEffectOnCharacterMatchingSkillpack(spm);
                        if (si == null)
                        {
                            si = v.Get().GetSkillEffectOnCharacterMatchingSkillpack(sps);

                        }
                    }
                    if (si == null) continue;

                    v.Get().LearnSkill(Character.SkillCategory.Effect, si.source, FInt.ONE, true, -1);
                }
            }
            return null;
        }
        static public object Rit_RefreshUpgradeIdol(Thea2.Server.Group target, RitualsTask sourceTask)
        {
            if (target != null && target.items != null)
            {
                BuildingRecipe br = (BuildingRecipe)BUILD_REC.IDOL;

                var oldIdol = target.items.Find(o => o.GetItem().GetSource() == br);
                if (oldIdol == null) return null;

                var oldDemon = oldIdol.GetItem().superConnection;

                #region Create new idol
                List<ItemBase> ibs = ItemBase.items.Where(o =>
                                o.GetSource() == br &&
                                (o as ItemCrafted).recipeIngredientCount[0] == sourceTask.GetMaterial1Count() &&
                                (o as ItemCrafted).recipeIngreadients[0].Get() == sourceTask.GetMaterial1()).ToList();

                if (ibs.Count != 1)
                {
                    Debug.LogError("[ERROR]Number of options in upgrade idol is different than 1: " + ibs.Count);
                    return null;
                }

                ItemCrafted ib = ibs[0] as ItemCrafted;
                ItemBase inst = ib.Clone<ItemBase>(true);
                Skill skill = null;
                if (br.skills != null)
                {
                    if (!SkillInstance.skillPacks.ContainsKey(br.skills))
                    {
                        Debug.LogError("[ERROR]Missing skillpack " + br.skills);
                        return null;
                    }

                    List<Skill> si = SkillInstance.skillPacks[br.skills];
                    HashSet<Tag> essences = new HashSet<Tag>();
                    Dictionary<Tag, FInt> essenceCounts = new Dictionary<Tag, FInt>();
                    List<Skill> valid = new List<Skill>();

                    CountedResource material1 = null;
                    CountedResource material2 = null;
                    if(ib.recipeIngreadients.Length > 0)
                    {
                        material1 = new CountedResource();
                        material1.resourceName = ib.recipeIngreadients[0].Get();
                        material1.resourceCount = ib.recipeIngredientCount[0];
                    }
                    if (ib.recipeIngreadients.Length > 1)
                    {
                        material2 = new CountedResource();
                        material2.resourceName = ib.recipeIngreadients[1].Get();
                        material2.resourceCount = ib.recipeIngredientCount[1];
                    }

                    if (material1 != null)
                    {
                        foreach (var v in material1.resourceName.essences)
                        {
                            FInt count = new FInt(v.amount) * material1.resourceCount;
                            if (count <= 0) continue;

                            essences.Add(v.tag);
                            essenceCounts[v.tag] = count;
                        }
                    }
                    if (material2 != null)
                    {
                        foreach (var v in material2.resourceName.essences)
                        {
                            FInt count = new FInt(v.amount) * material2.resourceCount;
                            if (count <= 0) continue;

                            if (!essences.Contains(v.tag))
                                essences.Add(v.tag);

                            if (!essenceCounts.ContainsKey(v.tag))
                            {
                                essenceCounts[v.tag] = count;
                            }
                            else
                            {
                                essenceCounts[v.tag] += count;
                            }
                        }

                        if (essences.Count > 0)
                        {
                            Tag g = (Tag)TAG.ESSENCE_GRAY;
                            essences.Add(g);
                        }
                    }

                    foreach (var v in si)
                    {
                        bool b = true;
                        foreach (var e in v.baseEssence)
                        {
                            if (!essences.Contains(e.tag))
                            {
                                b = false;
                                break;
                            }
                        }

                        if (!b) continue;

                        valid.Add(v);
                    }

                    if (valid.Count < 1)
                    {
                        Debug.LogError("[ERROR]No valid skill during upgrade of the idol in skillpack " + br.skills);
                        return null;
                    }
                    skill = valid[0];
                }
                
                if (skill != null)
                {
                    inst.skill = SkillInstance.Instantiate(skill, inst);   
                }
                #endregion
                #region Destroy old idol
                //destroy old idol
                var ceb = target.TakeItem(oldIdol.GetItem(), 1);
                ceb.GetItem().Destroy();
                #endregion

                //Add new idol
                var cebNewIdol = target.AddItem(inst, 1);

                //super connect with old demon to keep it alive if one was still there, otherwise it will create new one.
                if (oldDemon != null && oldDemon.Valid())
                {
                    cebNewIdol.GetItem().SuperConnectWith(oldDemon);
                }
            }

            return null;
        }
        static public object Rit_Celebration(Thea2.Server.Group target, RitualsTask sourceTask)
        {
            if (target != null && target.characters != null)
            {
                FInt newMorale = (FInt)40;
                foreach (Character c in target.characters)
                {
                    if (c.attributes.GetFinal(TAG.MORALE) < newMorale)
                    {
                        c.attributes.SetBaseTo(TAG.MORALE, newMorale);
                    }
                }
            }
                return null;
        }
        static public object Rit_BlessPhisical(Thea2.Server.Group target, RitualsTask sourceTask)
        {
            if (target != null && target.characters != null)
            {
                Skill skill = (Skill)SKILL.BLESS_P_RIT;
                Util_LearnSkillEffect(target, skill);
            }
            return null;
        }
        static public object Rit_BlessMental(Thea2.Server.Group target, RitualsTask sourceTask)
        {
            if (target != null && target.characters != null)
            {
                Skill skill = (Skill)SKILL.BLESS_M_RIT;
                Util_LearnSkillEffect(target, skill);
            }
            return null;
        }
        static public object Rit_BlessSpiritual(Thea2.Server.Group target, RitualsTask sourceTask)
        {
            if (target != null && target.characters != null)
            {
                Skill skill = (Skill)SKILL.BLESS_S_RIT;
                Util_LearnSkillEffect(target, skill);
            }
            return null;
        }
        static public object Rit_FireGift(Thea2.Server.Group target, RitualsTask sourceTask)
        {
            if (target != null && target.characters != null)
            {
                Skill skill = (Skill)SKILL.FIRE_GIFT;
                Util_LearnSkillEffect(target, skill);
            }
            return null;
        }
        static public object Rit_ShouldersPower(Thea2.Server.Group target, RitualsTask sourceTask)
        {
            if (target != null && target.characters != null)
            {
                Skill skill = (Skill)SKILL.SHOULDERS_POWER;
                Util_LearnSkillEffect(target, skill);
            }
            return null;
        }
        static public object Rit_KnowledgeArmor(Thea2.Server.Group target, RitualsTask sourceTask)
        {
            if (target != null && target.characters != null)
            {
                Skill skill = (Skill)SKILL.KNOWLEDGE_ARMOR;
                Util_LearnSkillEffect(target, skill);
            }
            return null;
        }
        static public object Rit_EarthVision(Thea2.Server.Group target, RitualsTask sourceTask)
        {
            if (target != null && target.characters != null)
            {
                Skill skill = (Skill)SKILL.EARTH_VISION;
                Util_LearnSkillEffect(target, skill);
            }
            return null;
        }
        static public object Rit_HigherEducation(Thea2.Server.Group target, RitualsTask sourceTask)
        {
            if (target != null && target.characters != null)
            {
                FInt extraXp = (FInt)3;
                FInt charactersInGroups = (FInt)target.characters.Count;
                FInt bonus = extraXp / charactersInGroups;
                
                foreach (var c in target.characters)
                {
                    c.Get().Xp = c.Get().Xp + bonus;
                }
            }
            return null;
        }
        static public object Rit_SummonBoat(Thea2.Server.Group target, RitualsTask sourceTask)
        {
            if (target != null && target.characters != null)
            {
                DBClass t = Globals.GetInstanceFromDB("ITEM_CARGO-RAFT");
                if(t != null)
                {
                    Type tp = t.GetType();

                    if (tp == typeof(ItemCargo))
                    {
                        CountEntityBase ceb = ItemBase.InstantaiteFrom(t as ItemCargo);
                        target.AddItem(ceb);
                    }
                }
            }
            return null;
        }
        static public object Rit_SummonPet(Thea2.Server.Group target, RitualsTask sourceTask)
        {
            if (target != null && target.characters != null)
            {
                DBClass t = Globals.GetInstanceFromDB("ITEM_CARGO-WEAK_ONE_RANDOM_DOMESTIC_PET");
                if (t != null)
                {
                    Type tp = t.GetType();

                    if (tp == typeof(ItemCargo))
                    {
                        CountEntityBase ceb = ItemBase.InstantaiteFrom(t as ItemCargo);
                        target.AddItem(ceb);
                    }
                }
            }
            return null;
        }
        static public object Rit_CreateBogBies(Thea2.Server.Group target, RitualsTask sourceTask)
        {
            if (target != null && target.characters != null)
            {
                string sb = "SUBRACE-CRATE_BOG_BIES";
                Util_AddCharacter(target, sb);
            }
            return null;
        }
        static public object Rit_CreateAlkonost(Thea2.Server.Group target, RitualsTask sourceTask)
        {
            if (target != null && target.characters != null)
            {
                string sb = "SUBRACE-CRATE_ALKONOST";
                Util_AddCharacter(target, sb);
            }
            return null;
        }
        static public object Rit_CreateTrollRock(Thea2.Server.Group target, RitualsTask sourceTask)
        {
            if (target != null && target.characters != null)
            {
                string sb = "SUBRACE-CRATE_TROLL_ROCK";
                Util_AddCharacter(target, sb);
            }
            return null;
        }
        static public object Rit_MoonBridge(Thea2.Server.Group target, RitualsTask sourceTask)
        {
            List<Thea2.Server.Group> groups = GameInstance.Get().GetValidPlayerGroups();
            Vector3i v3iTarget = target.position;
            Vector3i vectorG;
            Vector3i position = new Vector3i();
            int distance = 0;

            foreach (var g in groups)
            {
                // check if testing group is different than target group
                if(g != target)
                {
                    vectorG = g.position;
                    int distanceToG = HexCoordinates.HexDistance(v3iTarget, vectorG);
                    
                    if(distance == 0 || distanceToG < distance)
                    {
                        distance = distanceToG;
                        position = vectorG;
                    }
                }
            }

            if(distance != 0)
            {
                GameInstance.Get().AddToTeleport(target.ID, position);
            }

            return null;
        }
        #endregion

        #region UTILS
        static void Util_LearnSkillEffect(Thea2.Server.Group target, Skill skill)
        {
            foreach (var v in target.characters)
            {
                Character c = v.Get();
                SkillInstance si = c.effects.Find(o => o.source.reference == skill);
                if (si != null)
                {
                    si.charges += 1;
                }
                else
                {
                    c.LearnSkill(Character.SkillCategory.Effect, skill, FInt.ONE);
                }
            }
        }
        static void Util_AddCharacter(Thea2.Server.Group target, string subrace)
        {
            DBClass t = Globals.GetInstanceFromDB(subrace);
            if (t != null)
            {
                Type tp = t.GetType();

                if (tp == typeof(Subrace))
                {
                    Character c = Character.Instantiate(target, t as Subrace, 1);
                }
            }
        }
        #endregion

    }
}
#endif