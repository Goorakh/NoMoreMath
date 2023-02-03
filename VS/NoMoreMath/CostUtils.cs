using RoR2;
using RoR2.Items;
using System.Linq;
using System.Text;

namespace NoMoreMath
{
    public static class CostUtils
    {
        static EquipmentIndex _cardEquipmentIndex;

        [SystemInitializer(typeof(EquipmentCatalog))]
        static void InitEquipments()
        {
            static EquipmentIndex findEquipmentIndex(string equipmentName)
            {
                EquipmentIndex result = EquipmentCatalog.FindEquipmentIndex(equipmentName);

                if (result == EquipmentIndex.None)
                {
                    Log.Warning($"Unable to find equipment index \"{equipmentName}\"");
                }

                return result;
            }

            _cardEquipmentIndex = findEquipmentIndex("MultiShopCard");
        }

        public static int GetEffectiveCost(int cost, CharacterMaster master = null)
        {
            master ??= PlayerUtils.GetLocalUserMaster();
            if (master)
            {
                Inventory inventory = master.inventory;
                if (inventory)
                {
                    if (_cardEquipmentIndex != EquipmentIndex.None && inventory.GetEquipmentIndex() == _cardEquipmentIndex)
                    {
#pragma warning disable Publicizer001 // Accessing a member that was not originally public
                        return cost - (int)(cost * MultiShopCardUtils.refundPercentage);
#pragma warning restore Publicizer001 // Accessing a member that was not originally public
                    }
                }
            }

            return cost;
        }

        public static int GetTotalEffectiveCost(CharacterMaster master, params int[] costs)
        {
            int maxCostIndex = -1;
            int maxCost = 0;

            for (int i = 0; i < costs.Length; i++)
            {
                if (costs[i] > maxCost)
                {
                    maxCost = costs[i];
                    maxCostIndex = i;
                }
            }

            return costs.Select((c, i) => i == maxCostIndex ? c : GetEffectiveCost(c, master)).Sum();
        }

        public static void FormatEffectiveCost(CostTypeDef costType, int effectiveCost, StringBuilder stringBuilder, bool forWorldDisplay, bool includeColor = true)
        {
            stringBuilder.Append(" {Eff. ");

            EffectivePurchaseCost.EffectivePurchaseCostPatchController.DisableBuildCostStringPatch = true;
            costType.BuildCostStringStyled(effectiveCost, stringBuilder, forWorldDisplay, includeColor);
            EffectivePurchaseCost.EffectivePurchaseCostPatchController.DisableBuildCostStringPatch = false;

            stringBuilder.Append("}");
        }
    }
}
