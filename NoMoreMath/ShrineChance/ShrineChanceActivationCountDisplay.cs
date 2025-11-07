using NoMoreMath.Config;
using NoMoreMath.EffectiveCost;
using NoMoreMath.MiscPatches;
using RoR2;
using System;
using System.Text;

namespace NoMoreMath.ShrineChance
{
    static class ShrineChanceActivationCountDisplay
    {
        static bool _eventListenerActive = false;
        static bool eventListenerActive
        {
            get
            {
                return _eventListenerActive;
            }
            set
            {
                if (_eventListenerActive == value)
                    return;

                _eventListenerActive = value;

                if (_eventListenerActive)
                {
                    PurchaseInteractionHooks.ModifyName += PurchaseInteractionHooks_ModifyName;

                    Log.Debug("Added name modifier");
                }
                else
                {
                    PurchaseInteractionHooks.ModifyName -= PurchaseInteractionHooks_ModifyName;

                    Log.Debug("Removed name modifier");
                }
            }
        }

        static void refreshEventListenerActive()
        {
            eventListenerActive = Configs.ShrineChance.EnableActivationCountDisplay.Value;
        }

        [SystemInitializer]
        static void Init()
        {
            Configs.ShrineChance.EnableActivationCountDisplay.SettingChanged += DisplayActivationCountEnabled_SettingChanged;
            refreshEventListenerActive();
        }

        static void DisplayActivationCountEnabled_SettingChanged(object sender, EventArgs e)
        {
            refreshEventListenerActive();
        }

        static void PurchaseInteractionHooks_ModifyName(PurchaseInteraction purchaseInteraction, CharacterMaster viewer, StringBuilder nameBuilder)
        {
            if (!purchaseInteraction || !viewer)
                return;

            if (purchaseInteraction.cost <= 0 || purchaseInteraction.costType != CostTypeIndex.Money)
                return;

            if (!purchaseInteraction.TryGetComponent(out ShrineChanceBehavior shrineChanceBehavior))
                return;

            ShrineChanceConfig shrineChanceConfigs = Configs.ShrineChance;

            CostTypeDef costTypeDef = CostTypeCatalog.GetCostTypeDef(purchaseInteraction.costType);

            int currentCost = purchaseInteraction.cost;

            int solituteItemCount = TeamManager.LongstandingSolitudesInParty();
            if (solituteItemCount > 0)
            {
                currentCost *= 1 + solituteItemCount;
            }

            // Limit so we don't spend too much time in the loop.
            // Any value over this should be sufficiently large
            // enough to not really matter anymore.
            const byte INTERACTIONS_CHECK_LIMIT = 10;
            const string INTERACTIONS_CHECK_LIMIT_STRING = "10"; // kinda ugly, but number.ToString is not a constant expression

            byte affordableInteractions = 0;

            uint remainingMoney = viewer.money;

            // If the viewer can't afford a single instance of the cost,
            // then it doesn't matter what the effective cost is,
            // they won't be able to buy it at all
            if (remainingMoney >= currentCost)
            {
                for (affordableInteractions = 0; affordableInteractions <= INTERACTIONS_CHECK_LIMIT; affordableInteractions++)
                {
                    uint effectiveCost = (uint)EffectiveCostUtils.GetEffectiveCost(costTypeDef, currentCost, viewer);
                    if (remainingMoney < effectiveCost)
                        break;

                    remainingMoney -= effectiveCost;
                    currentCost = (int)(currentCost * shrineChanceBehavior.costMultiplierPerPurchase);
                }
            }

            string numActivationsString;
            if (affordableInteractions > INTERACTIONS_CHECK_LIMIT)
            {
                numActivationsString = $"{INTERACTIONS_CHECK_LIMIT_STRING}+";
            }
            else
            {
                numActivationsString = affordableInteractions.ToString();
            }

            nameBuilder.EnsureCapacity(nameBuilder.Length + shrineChanceConfigs.ActivationCountDisplayFormatter.StrippedLength + numActivationsString.Length + 1);

            nameBuilder.Append(' ');

            shrineChanceConfigs.ActivationCountDisplayFormatter.AppendToStringBuilder(nameBuilder, new TagReplacementStringConfig.ReplacementInfo(ShrineChanceConfig.ACTIVATION_COUNT_DISPLAY_COUNT_TAG, numActivationsString));
        }
    }
}
