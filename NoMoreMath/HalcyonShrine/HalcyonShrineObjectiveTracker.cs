using NoMoreMath.Config;
using RoR2.UI;
using System.Text;
using UnityEngine;

namespace NoMoreMath.HalcyonShrine
{
    public class HalcyonShrineObjectiveTracker : ObjectivePanelController.ObjectiveTracker
    {
        HalcyonShrineObjectiveProvider halcyoniteShrine => (HalcyonShrineObjectiveProvider)sourceDescriptor.source;

        bool _displayFormatDirty;
        int _displayedChargePercent;

        public HalcyonShrineObjectiveTracker() : base()
        {
            Configs.HalcyonShrine.ChargeProgressDisplayFormat.OnValueChanged += onDisplayFormatChanged;
        }

        public override void OnRetired()
        {
            base.OnRetired();
            Configs.HalcyonShrine.ChargeProgressDisplayFormat.OnValueChanged -= onDisplayFormatChanged;
        }

        void onDisplayFormatChanged()
        {
            _displayFormatDirty = true;
        }

        int getChargeDisplayPercent()
        {
            float completeFraction = (float)halcyoniteShrine.TotalGoldDrained / halcyoniteShrine.RequiredGoldDrain;
            return Mathf.Clamp(Mathf.FloorToInt(completeFraction * 100f), 0, 100);
        }

        public override bool IsDirty()
        {
            return base.IsDirty() || (halcyoniteShrine && (_displayFormatDirty || _displayedChargePercent != getChargeDisplayPercent()));
        }

        public override string GenerateString()
        {
            _displayedChargePercent = getChargeDisplayPercent();
            _displayFormatDirty = false;

            string chargePercentString = _displayedChargePercent.ToString();

            HalcyonShrineConfig halcyonShrineConfig = Configs.HalcyonShrine;

            StringBuilder stringBuilder = HG.StringBuilderPool.RentStringBuilder();

            stringBuilder.EnsureCapacity(halcyonShrineConfig.ChargeProgressDisplayFormat.StrippedLength + chargePercentString.Length);

            halcyonShrineConfig.ChargeProgressDisplayFormat.AppendToStringBuilder(stringBuilder, new TagReplacementStringConfig.ReplacementInfo(HalcyonShrineConfig.CHARGE_PROGRESS_DISPLAY_PROGRESS_TAG, chargePercentString));

            string result = stringBuilder.ToString();
            HG.StringBuilderPool.ReturnStringBuilder(stringBuilder);
            return result;
        }
    }
}
