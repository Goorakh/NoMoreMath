using NoMoreMath.Config;
using NoMoreMath.MiscPatches;
using RoR2;
using System;
using System.Text;

namespace NoMoreMath.ShrineBlood
{
    static class ShrineBloodGoldGainDisplay
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
            eventListenerActive = Configs.ShrineBlood.EnableGoldGainedDisplay.Value;
        }

        [SystemInitializer]
        static void Init()
        {
            Configs.ShrineBlood.EnableGoldGainedDisplay.SettingChanged += EnableGoldGainedDisplay_SettingChanged;
            refreshEventListenerActive();
        }

        static void EnableGoldGainedDisplay_SettingChanged(object sender, EventArgs e)
        {
            refreshEventListenerActive();
        }

        static void PurchaseInteractionHooks_ModifyName(PurchaseInteraction purchaseInteraction, CharacterMaster viewer, StringBuilder nameBuilder)
        {
            if (!purchaseInteraction || !viewer)
                return;

            CharacterBody viewerBody = viewer.GetBody();
            if (!viewerBody)
                return;

            HealthComponent viewerHealthComponent = viewerBody.healthComponent;
            if (!viewerHealthComponent)
                return;

            if (!purchaseInteraction.TryGetComponent(out ShrineBloodBehavior shrineBloodBehavior))
                return;

            ShrineBloodConfig shrineBloodConfig = Configs.ShrineBlood;

            uint goldGain = (uint)(viewerHealthComponent.fullCombinedHealth * purchaseInteraction.cost / 100f * shrineBloodBehavior.goldToPaidHpRatio);

            string goldGainString = goldGain.ToString();

            nameBuilder.EnsureCapacity(nameBuilder.Length + shrineBloodConfig.GoldGainedDisplayFormatter.StrippedLength + goldGainString.Length + 1);

            nameBuilder.Append(' ');

            shrineBloodConfig.GoldGainedDisplayFormatter.AppendToStringBuilder(nameBuilder, new TagReplacementStringConfig.ReplacementInfo(ShrineBloodConfig.GOLD_GAINED_DISPLAY_AMOUNT_TAG, goldGainString));
        }
    }
}
