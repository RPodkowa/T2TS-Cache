#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR
using DBDef;
using Thea2.Common;

namespace GameScript
{
    public class TraderScripts : ScriptBase
    {
        #region TCL - Trader Confirm Lock
        static public bool Tcl_TradeDefaultConfirmAllowed(int curentBalance)
        {
            //curentBalance >= 0 is default fall back in case no script is provided
            return curentBalance >= 0;
        }
        #endregion

        #region TBV - Trader Buying Value

        const float defaultBuyMultiplier = 1.0f;
        const float friendlyBuyMultiplier = 1.5f;
        const float lovedBuyMultiplier = 2.0f;
        const float cookedFoodDivisor = 4.0f;
        const float petBuyMultiplier = 3.0f;
        const float petSellMultiplier = 5.0f;


        static public int Tbv_TradeDefaultBuying(ClientEntity ce)
        {

            if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) >0)
            {
                return BuingCookedFoodAndPet(ce);
            }
            else
            {
                return (int)(ce.Value * defaultBuyMultiplier);
            }
        }
        static public int Tbv_TradeFriendlyBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                return BuingCookedFoodAndPetFriendly(ce);
            }
            else
            {
                return (int)(ce.Value * friendlyBuyMultiplier);
            }
        }
        static public int Tbv_TradeBuyingFor1Value(ClientEntity ce)
        {
            return 1;
        }
        static public int Tbv_TradeWoodBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.WOOD) > 0 ||
                ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPet(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else return (int)(ce.Value * defaultBuyMultiplier);
        }

        static public int Tbv_TradeRoamersBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.MELEE_WEAPON) > 0 ||
                ce.GetAttribute(TAG.RANGE_WEAPON) > 0 ||
                ce.GetAttribute(TAG.ARMOUR) > 0 ||
                ce.GetAttribute(TAG.SHIELD) > 0 || 
                ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPet(ce);
                }
                else
                {
                    return (int)(ce.Value * friendlyBuyMultiplier);
                }
            }
            else
            {
                return (int)(ce.Value * defaultBuyMultiplier);
            }
        }
        static public int Tbv_TradeRoamersFriendlyBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.MELEE_WEAPON) > 0 ||
                ce.GetAttribute(TAG.RANGE_WEAPON) > 0 ||
                ce.GetAttribute(TAG.ARMOUR) > 0 ||
                ce.GetAttribute(TAG.SHIELD) > 0 ||
                ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPetFriendly(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else
            {
                return (int)(ce.Value * friendlyBuyMultiplier);
            }
        }
        static public int Tbv_TradeScavengersBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.FOOD) > 0 ||
                ce.GetAttribute(TAG.TOOLS) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPet(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else
            {
                return (int)(ce.Value * defaultBuyMultiplier);
            }
        }
        static public int Tbv_TradeScavengersFriendlyBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.FOOD) > 0 ||
                ce.GetAttribute(TAG.TOOLS) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPetFriendly(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else
            {
                return (int)(ce.Value * friendlyBuyMultiplier);
            }
        }
        static public int Tbv_TradeElderKinBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.GOLD) > 0 ||
                ce.GetAttribute(TAG.SILVER) > 0 ||
                ce.GetAttribute(TAG.STEEL) > 0 ||
                ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPetFriendly(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else
            {
                return (int)(ce.Value * defaultBuyMultiplier);
            }
        }
        static public int Tbv_TradeElderKinFriendlyBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.GOLD) > 0 ||
                ce.GetAttribute(TAG.SILVER) > 0 ||
                ce.GetAttribute(TAG.STEEL) > 0 ||
                ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPetFriendly(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else
            {
                return (int)(ce.Value * friendlyBuyMultiplier);
            }
        }
        static public int Tbv_TradeShadowKinBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.SWORD_1H) > 0 ||
                ce.GetAttribute(TAG.SWORD_2H) > 0 ||
                ce.GetAttribute(TAG.POLEARM_1H) > 0 ||
                ce.GetAttribute(TAG.POLEARM_2H) > 0 ||
                ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPet(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else
            {
                return (int)(ce.Value * defaultBuyMultiplier);
            }
        }
        static public int Tbv_TradeShadowKinFriendlyBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.SWORD_1H) > 0 ||
                ce.GetAttribute(TAG.SWORD_2H) > 0 ||
                ce.GetAttribute(TAG.POLEARM_1H) > 0 ||
                ce.GetAttribute(TAG.POLEARM_2H) > 0 ||
                ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPetFriendly(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else
            {
                return (int)(ce.Value * friendlyBuyMultiplier);
            }
        }
        static public int Tbv_TradeEarthboundBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.DRYAD_WOOD) > 0 ||
                ce.GetAttribute(TAG.DARK_WOOD) > 0 ||
                ce.GetAttribute(TAG.ELVEN_WOOD) > 0 ||
                ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPet(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else
            {
                return (int)(ce.Value * defaultBuyMultiplier);
            }
        }
        static public int Tbv_TradeEarthboundFriendlyBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.DRYAD_WOOD) > 0 ||
                ce.GetAttribute(TAG.DARK_WOOD) > 0 ||
                ce.GetAttribute(TAG.ELVEN_WOOD) > 0 ||
                ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPetFriendly(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else
            {
                return (int)(ce.Value * friendlyBuyMultiplier);
            }
        }
        static public int Tbv_TradeAlphaClansBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.DRYAD_WOOD) > 0 ||
                ce.GetAttribute(TAG.DARK_WOOD) > 0 ||
                ce.GetAttribute(TAG.ELVEN_WOOD) > 0 ||
                ce.GetAttribute(TAG.GOLD) > 0 ||
                ce.GetAttribute(TAG.SILVER) > 0 ||
                ce.GetAttribute(TAG.STEEL) > 0 ||
                ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPet(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else
            {
                return (int)(ce.Value * defaultBuyMultiplier);
            }
        }
        static public int Tbv_TradeAlphaClansFriandlyBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.DRYAD_WOOD) > 0 ||
                ce.GetAttribute(TAG.DARK_WOOD) > 0 ||
                ce.GetAttribute(TAG.ELVEN_WOOD) > 0 ||
                ce.GetAttribute(TAG.GOLD) > 0 ||
                ce.GetAttribute(TAG.SILVER) > 0 ||
                ce.GetAttribute(TAG.STEEL) > 0 ||
                ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPetFriendly(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else
            {
                return (int)(ce.Value * friendlyBuyMultiplier);
            }
        }
        static public int Tbv_TradeClanlessBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.DRYAD_WOOD) > 0 ||
                ce.GetAttribute(TAG.DARK_WOOD) > 0 ||
                ce.GetAttribute(TAG.ELVEN_WOOD) > 0 ||
                ce.GetAttribute(TAG.FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPet(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else
            {
                return (int)(ce.Value * defaultBuyMultiplier);
            }
        }
        static public int Tbv_TradeClanlessFriendlyBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.DRYAD_WOOD) > 0 ||
                ce.GetAttribute(TAG.DARK_WOOD) > 0 ||
                ce.GetAttribute(TAG.ELVEN_WOOD) > 0 ||
                ce.GetAttribute(TAG.FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPetFriendly(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else
            {
                return (int)(ce.Value * friendlyBuyMultiplier);
            }
        }
        static public int Tbv_TradeTinkerersBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.OBSIDIAN) > 0 ||
                ce.GetAttribute(TAG.BOAR) > 0 ||
                ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPet(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else
            {
                return (int)(ce.Value * defaultBuyMultiplier);
            }
        }
        static public int Tbv_TradeTinkerersFriendlyBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.OBSIDIAN) > 0 ||
                ce.GetAttribute(TAG.BOAR) > 0 ||
                ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPetFriendly(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else
            {
                return (int)(ce.Value * defaultBuyMultiplier);
            }
        }
        static public int Tbv_TradeSpiritTalkersBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.GHOST) > 0 ||
                ce.GetAttribute(TAG.WRAITH) > 0 ||
                ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPet(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else
            {
                return (int)(ce.Value * defaultBuyMultiplier);
            }
        }
        static public int Tbv_TradeSpiritTalkersFriendlyBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.GHOST) > 0 ||
                ce.GetAttribute(TAG.WRAITH) > 0 ||
                ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPetFriendly(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else
            {
                return (int)(ce.Value * friendlyBuyMultiplier);
            }
        }
        static public int Tbv_TradeForestDemonsBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.MALACHITE) > 0 ||
                ce.GetAttribute(TAG.TOPAZ) > 0 ||
                ce.GetAttribute(TAG.RUBY) > 0 ||
                ce.GetAttribute(TAG.BEAST) > 0 ||
                ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPet(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else
            {
                return (int)(ce.Value * defaultBuyMultiplier);
            }
        }
        static public int Tbv_TradeForestDemonsFriendlyBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.MALACHITE) > 0 ||
                ce.GetAttribute(TAG.TOPAZ) > 0 ||
                ce.GetAttribute(TAG.RUBY) > 0 ||
                ce.GetAttribute(TAG.BEAST) > 0 ||
                ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPetFriendly(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else
            {
                return (int)(ce.Value * friendlyBuyMultiplier);
            }
        }
        static public int Tbv_TradeNightDemonsBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.MELEE_WEAPON) > 0 ||
                ce.GetAttribute(TAG.RANGE_WEAPON) > 0 ||
                ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPet(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else
            {
                return (int)(ce.Value * defaultBuyMultiplier);
            }
        }
        static public int Tbv_TradeNightDemonsFriendlyBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.MELEE_WEAPON) > 0 ||
                ce.GetAttribute(TAG.RANGE_WEAPON) > 0 ||
                ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPetFriendly(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else
            {
                return (int)(ce.Value * friendlyBuyMultiplier);
            }
        }
        static public int Tbv_TradeWaterDemonsBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.CHILD) > 0 ||
                ce.GetAttribute(TAG.CLAY) > 0 ||
                ce.GetAttribute(TAG.QUARTZ) > 0 ||
                ce.GetAttribute(TAG.GRANITE) > 0 ||
                ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPet(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else
            {
                return (int)(ce.Value * defaultBuyMultiplier);
            }
        }
        static public int Tbv_TradeWaterDemonsFriendlyBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.CHILD) > 0 ||
                ce.GetAttribute(TAG.CLAY) > 0 ||
                ce.GetAttribute(TAG.QUARTZ) > 0 ||
                ce.GetAttribute(TAG.GRANITE) > 0 ||
                ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPetFriendly(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else
            {
                return (int)(ce.Value * friendlyBuyMultiplier);
            }
        }
        static public int Tbv_TradeIceDemonsBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.HUMANOID) > 0 ||
                ce.GetAttribute(TAG.BEAST) > 0 ||
                ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPet(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else
            {
                return (int)(ce.Value * defaultBuyMultiplier);
            }
        }
        static public int Tbv_TradeIceDemonsFriendlyBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.HUMANOID) > 0 ||
                ce.GetAttribute(TAG.BEAST) > 0 ||
                ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPetFriendly(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else
            {
                return (int)(ce.Value * friendlyBuyMultiplier);
            }
        }
        static public int Tbv_TradeBeastBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPet(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else return (int)(ce.Value * defaultBuyMultiplier);
        }
        static public int Tbv_TradeBeastFriendlyBuying(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    return BuingCookedFoodAndPetFriendly(ce);
                }
                else
                {
                    return (int)(ce.Value * lovedBuyMultiplier);
                }
            }
            else return (int)(ce.Value * friendlyBuyMultiplier);
        }
        #endregion

        #region TSV - Trader Selling Value
        static public int Tsv_TradeDefaultSelling(ClientEntity ce)
        {
            return ce.Value;
        }
        static public int Tsv_Trade2XSelling(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
            {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0)
                {
                    return (int)(ce.Value * 2.0f / cookedFoodDivisor);
                }
                else
                {
                    return (int)(ce.Value * petSellMultiplier);
                }
                
            }
            else return (int)(ce.Value * 2.0f);
        }

        static public int Tsv_TradeAncientWoodSelling(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.ANCIENT_WOOD) > 0 ||
                ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
               {
                if (ce.GetAttribute(TAG.COOKED_FOOD) > 0 ||
                ce.GetAttribute(TAG.PET) > 0)
                {
                    if (ce.GetAttribute(TAG.COOKED_FOOD) > 0)
                    {
                        return (int)(ce.Value * 2.5f / cookedFoodDivisor);
                    }
                    else
                    {
                        return (int)(ce.Value * petSellMultiplier);
                    }
                }
               else return (int)(ce.Value * 3.0f);

            }
            else return (int)(ce.Value * 2.5f);
        }
        static public int Tbv_TradeFor1ValueSelling(ClientEntity ce)
        {
            return 1;
        }
        #endregion

        #region Utils

        static int BuingCookedFoodAndPet(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.COOKED_FOOD) > 0)
            {
                return (int)(ce.Value / cookedFoodDivisor);
            }
            else
            {
                return (int)(ce.Value * petBuyMultiplier);
            }
        }

        static int BuingCookedFoodAndPetFriendly(ClientEntity ce)
        {
            if (ce.GetAttribute(TAG.COOKED_FOOD) > 0)
            {
                return (int)(ce.Value * friendlyBuyMultiplier / cookedFoodDivisor);
            }
            else
            {
                return (int)(ce.Value * petBuyMultiplier);
            }
        }
        #endregion
    }
}
#endif