using RoR2;
using RoR2.Items;

namespace NoMoreMath.EffectiveCost
{
    public static class EffectiveCostUtils
    {
        public static int GetEffectiveCost(CostTypeDef costType, int cost, CharacterMaster master)
        {
            if (costType == CostTypeCatalog.GetCostTypeDef(CostTypeIndex.Money))
            {
                if (master)
                {
                    int equipmentStocks = 0;
                    CharacterBody body = master.GetBody();
                    if (body)
                    {
                        EquipmentSlot equipmentSlot = body.equipmentSlot;
                        if (equipmentSlot)
                        {
                            equipmentStocks = equipmentSlot.stock;
                        }
                    }

                    Inventory inventory = master.inventory;
                    if (inventory && inventory.currentEquipmentIndex == DLC1Content.Equipment.MultiShopCard.equipmentIndex)
                    {
                        if (equipmentStocks > 0 && cost > 0)
                        {
                            int refundAmount = (int)(MultiShopCardUtils.refundPercentage * cost);

                            if (refundAmount > 0)
                            {
                                cost -= refundAmount;
                            }
                        }
                    }
                }
            }

            return cost;
        }

        public static int GetTotalEffectiveCost(CostTypeDef costType, int[] costs, CharacterMaster master)
        {
            if (costs.Length == 0)
                return 0;

            if (costs.Length == 1)
                return GetEffectiveCost(costType, costs[0], master);

            int maxCost = int.MinValue;
            int maxCostIndex = -1;

            for (int i = 0; i < costs.Length; i++)
            {
                if (costs[i] > maxCost)
                {
                    maxCost = costs[i];
                    maxCostIndex = i;
                }
            }

            int totalEffectiveCost = 0;

            if (maxCostIndex >= 0)
            {
                totalEffectiveCost += maxCost;
            }

            for (int i = 0; i < costs.Length; i++)
            {
                if (i != maxCostIndex)
                {
                    totalEffectiveCost += GetEffectiveCost(costType, costs[i], master);
                }
            }

            return totalEffectiveCost;
        }
    }
}
