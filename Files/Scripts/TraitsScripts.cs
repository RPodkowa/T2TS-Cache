#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR
using DBDef;
using System.Collections.Generic;
using Thea2.Common;
using Thea2.Server;

namespace GameScript
{
    public class TraitScript : ScriptBase
    {
        static public void Tra_LongTravel(SPlayer player)
        {
            player.attributes.AddToBase((Tag)TAG.SEA_MOVEMENT_RANGE, new FInt(2));
        }
        static public void Tra_BeastOnYourSide(SPlayer player)
        {
            var sPlayerGroups = GameInstance.Get().GetPlayerGroups().FindAll(o => o.ownerID == player.ID);
            foreach (var v in sPlayerGroups)
            {
                if (v.characters != null)
                {
                    foreach (var c in v.characters)
                    {
                        if (c.Get().attributes.GetBase(TAG.CHOSEN) > FInt.ZERO)
                        {
                            var co = (Character)c;
                            co.LearnSkill(Character.SkillCategory.Effect, (Skill)"SKILL-HORZ_BLOODY_SUMMON_WEREWOLF", FInt.ONE);
                            return;
                        }
                    }
                }
            }
        }
        static public void Tra_GodsCommander(SPlayer player)
        {
            var sPlayerGroups = GameInstance.Get().GetPlayerGroups().FindAll(o => o.ownerID == player.ID);
            foreach (var v in sPlayerGroups)
            {
                if (v.characters != null)
                {
                    foreach (var c in v.characters)
                    {
                        if (c.Get().attributes.GetBase(TAG.CHOSEN) > FInt.ZERO)
                        {
                            var co = (Character)c;
                            co.LearnSkill(Character.SkillCategory.Effect, (Skill)"SKILL-TRIGLAV_GODS_COMMANDER_CHOSEN", FInt.ONE);
                            return;
                        }
                    }
                }
            }
        }
        static public void Tra_InnerPeace(SPlayer player)
        {
            var sPlayerGroups = GameInstance.Get().GetPlayerGroups().FindAll(o => o.ownerID == player.ID);
            foreach (var v in sPlayerGroups)
            {
                if (v.characters != null)
                {
                    foreach (var c in v.characters)
                    {
                        if (c.Get().attributes.GetBase(TAG.CHOSEN) > FInt.ZERO)
                        {
                            var co = (Character)c;
                            co.LearnSkill(Character.SkillCategory.Effect, (Skill)"SKILL-INNER_PEACE", FInt.ONE);
                            return;
                        }
                    }
                }
            }
        }
        static public void Tra_LifeTouchSkill(SPlayer player)
        {
            var sPlayerGroups = GameInstance.Get().GetPlayerGroups().FindAll(o => o.ownerID == player.ID);
            foreach (var v in sPlayerGroups)
            {
                if (v.characters != null)
                {
                    foreach (var c in v.characters)
                    {
                        if (c.Get().attributes.GetBase(TAG.CHOSEN) > FInt.ZERO)
                        {
                            var co = (Character)c;
                            co.LearnSkill(Character.SkillCategory.Effect, (Skill)"SKILL-HEALING_TOUCH", FInt.ONE);
                            return;
                        }
                    }
                }
            }
        }
        static public void Tra_DemonicPossession(SPlayer player)
        {
            var sPlayerGroups = GameInstance.Get().GetPlayerGroups().FindAll(o => o.ownerID == player.ID);
            foreach (var v in sPlayerGroups)
            {
                if (v.characters != null)
                {
                    foreach (var c in v.characters)
                    {
                        if (c.Get().attributes.GetBase(TAG.CHOSEN) > FInt.ZERO)
                        {
                            var co = (Character)c;
                            co.LearnSkill(Character.SkillCategory.Effect, (Skill)"SKILL-DEMONIC_POSSESSION", FInt.ONE);
                            return;
                        }
                    }
                }
            }
        }
        static public void Tra_ILoveYou(SPlayer player)
        {
            player.attributes.AddToBase((Tag)TAG.LADA_LOVE, FInt.ONE);
        }
        static public void Tra_ExtraMoraleForFood(SPlayer player)
        {
            player.attributes.AddToBase((Tag)TAG.MORALE_FOOD_BONUS, FInt.ONE);
        }
        static public void Tra_CampGatheringRange(SPlayer player)
        {
            player.attributes.AddToBase((Tag)TAG.CAMP_GATHERING_RANGE, FInt.ONE);
        }
        static public void Tra_AllEnemiesAreMine(SPlayer player)
        {
            player.attributes.AddToBase((Tag)TAG.ALL_ENEMIES_ARE_MINE, FInt.ONE);
        }
        static public void Tra_ILikeGreen(SPlayer player)
        {
            GameInstance.Get().sharedAttributes.SetBaseTo(TAG.LOYALTY_SPIRIT_TALKERS, 10);
        }
        static public void Tra_ILikeFight(SPlayer player)
        {
            GameInstance.Get().sharedAttributes.SetBaseTo(TAG.LOYALTY_ALPHA_CLANS, 10);
        }
        static public void Tra_StartingLoyaltyDemons(SPlayer player)
        {
            Attributes attributes = GameInstance.Get().sharedAttributes;
            attributes.SetBaseTo(TAG.LOYALTY_FOREST_DEMONS, 10);
            attributes.SetBaseTo(TAG.LOYALTY_ICE_DEMONS, 10);
            attributes.SetBaseTo(TAG.LOYALTY_NIGHT_DEMONS, 10);
            attributes.SetBaseTo(TAG.LOYALTY_WATER_DEMONS, 10);
        }
        static public void Tra_AditionalResearchAdvancementPoints(SPlayer player)
        {
            player.unlockPoints += 4;
        }
        static public void Tra_JumpingFish(SPlayer player)
        {
            var sPlayerGroups = GameInstance.Get().GetPlayerGroups().FindAll(o => o.ownerID == player.ID);
            foreach (var v in sPlayerGroups)
            {
                if (v.characters != null)
                {
                    foreach (var c in v.characters)
                    {
                        if (c.Get().attributes.GetBase(TAG.CHOSEN) > FInt.ZERO)
                        {
                            var co = (Character)c;
                            co.LearnSkill(Character.SkillCategory.Effect, (Skill)"SKILL-JUMPING_FISH", FInt.ONE);
                            return;
                        }
                    }
                }
            }
        }
        static public void Tra_MentalWall(SPlayer player)
        {
            var sPlayerGroups = GameInstance.Get().GetPlayerGroups().FindAll(o => o.ownerID == player.ID);
            FInt mentalSgielding = (FInt)20;
            FInt charactersInGroups = FInt.ZERO;

            foreach (var v in sPlayerGroups)
            {
                charactersInGroups += v.characters.Count;
            }

            FInt bonus = (mentalSgielding / charactersInGroups).ReturnRounded();

            foreach (var v in sPlayerGroups)
            {
                if (v.characters != null)
                {
                    foreach (var c in v.characters)
                    {
                        c.Get().attributes.AddToBase(TAG.SHIELDING_MENTAL, bonus);
                    }
                }
            }
        }
        static public void Tra_CrafterFlock(SPlayer player)
        {
            var sPlayerGroups = GameInstance.Get().GetPlayerGroups().FindAll(o => o.ownerID == player.ID);
            FInt extraCrafting = (FInt)5 / 2;

            foreach (var v in sPlayerGroups)
            {
                if (v.characters != null)
                {
                    foreach (var c in v.characters)
                    {
                        c.Get().attributes.AddToBase(TAG.CRAFTING, extraCrafting);
                    }
                }
            }
        }
        static public void Tra_SeasonedFollowers(SPlayer player)
        {
            var sPlayerGroups = GameInstance.Get().GetPlayerGroups().FindAll(o => o.ownerID == player.ID);
            FInt extraXP = (FInt)20;
            FInt charactersInGroups = FInt.ZERO;

            foreach (var v in sPlayerGroups)
            {
                charactersInGroups += v.characters.Count;
            }

            FInt bonus = extraXP / charactersInGroups;

            foreach (var v in sPlayerGroups)
            {
                if (v.characters != null)
                {
                    foreach (var c in v.characters)
                    {
                        c.Get().Xp = c.Get().Xp + bonus;
                    }
                }
            }
        }
        static public void Tra_UnlockAllFoodRecipes(SPlayer player)
        {
            List<ItemTech> its = Globals.GetTypeFromDB<ItemTech>();
            its = its.FindAll(o => o.dbName.StartsWith("ITEM_TECH-COOKING") && !player.unlockedTechs.Contains(o.dbName));

            foreach (var v in its)
            {
                foreach (var y in v.unlockRecipes)
                {
                    CraftingRecipe cr = player.unlockedRecipes.Find(o => o.recipeSource == y.itemRecipe.dbName);
                    if (cr == null)
                    {
                        cr = CraftingRecipe.FactoryFrom(y.itemRecipe);
                        cr.SetUnlockLevel(2);
                        player.unlockedRecipes.Add(cr);
                    }
                }
                player.unlockedTechs.Add(v.dbName);
            }
        }
        static public void Tra_MoonBridge(SPlayer player)
        {
            List<BuildingTech> its = Globals.GetTypeFromDB<BuildingTech>();
            its = its.FindAll(o => o.dbName.Equals("BUILDING_TECH-MOON_BRIDGE") && !player.unlockedTechs.Contains(o.dbName));

            foreach (var v in its)
            {
                foreach (var y in v.unlockRecipes)
                {
                    CraftingRecipe cr = player.unlockedRecipes.Find(o => o.recipeSource == y.ritualRecipe.dbName);
                    if (cr == null)
                    {
                        cr = CraftingRecipe.FactoryFrom(y.ritualRecipe);
                        cr.SetUnlockLevel(5);
                        player.unlockedRecipes.Add(cr);
                    }
                }
                player.unlockedTechs.Add(v.dbName);
            }
        }
    }
}
#endif