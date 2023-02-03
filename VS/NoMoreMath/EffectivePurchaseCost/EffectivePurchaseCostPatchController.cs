using RoR2;
using System.Text;
using UnityEngine;

namespace NoMoreMath.EffectivePurchaseCost
{
    public static class EffectivePurchaseCostPatchController
    {
        public static bool DisableBuildCostStringPatch;

        public static void Apply()
        {
            On.RoR2.CostTypeDef.BuildCostStringStyled += CostTypeDef_BuildCostStringStyled;
        }

        public static void Cleanup()
        {
            On.RoR2.CostTypeDef.BuildCostStringStyled -= CostTypeDef_BuildCostStringStyled;
        }

        static void CostTypeDef_BuildCostStringStyled(On.RoR2.CostTypeDef.orig_BuildCostStringStyled orig, CostTypeDef self, int cost, StringBuilder stringBuilder, bool forWorldDisplay, bool includeColor)
        {
            orig(self, cost, stringBuilder, forWorldDisplay, includeColor);

            if (self == CostTypeCatalog.GetCostTypeDef(CostTypeIndex.Money) && !DisableBuildCostStringPatch)
            {
                int effectiveCost = CostUtils.GetEffectiveCost(cost);
                if (effectiveCost != cost)
                {
                    CostUtils.FormatEffectiveCost(self, effectiveCost, stringBuilder, forWorldDisplay, includeColor);
                }
            }
        }
    }
}
