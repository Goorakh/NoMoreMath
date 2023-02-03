using RoR2;
using UnityEngine;

namespace NoMoreMath.HoldoutZoneTimeRemaining
{
    public class HoldoutZoneChargeRateTracker : MonoBehaviour
    {
        HoldoutZoneController _holdoutZoneController;

        public float LastPositiveChargeRate { get; private set; }
        public float CurrentChargeRate { get; private set; }

        public float PositiveChargeRate
        {
            get
            {
                if (CurrentChargeRate > 0f)
                {
                    return CurrentChargeRate;
                }
                else
                {
                    return LastPositiveChargeRate;
                }
            }
        }

        void Awake()
        {
            _holdoutZoneController = GetComponent<HoldoutZoneController>();
        }

        public void RecordChargeRate(float rate)
        {
            CurrentChargeRate = rate;

            if (rate > 0f)
            {
                LastPositiveChargeRate = rate;
            }
        }
    }
}
