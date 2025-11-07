using NoMoreMath.Utils;
using RoR2;
using UnityEngine.Networking;

namespace NoMoreMath.HoldoutZone
{
    public sealed class HoldoutZoneChargeRateProvider : NetworkBehaviour, ICachableComponent
    {
        public static readonly ObjectComponentCache<HoldoutZoneController, HoldoutZoneChargeRateProvider> ComponentCache = new ObjectComponentCache<HoldoutZoneController, HoldoutZoneChargeRateProvider>();

        HoldoutZoneController _holdoutZoneController;

        int? ICachableComponent.DictionaryKey { get; set; }

        [SyncVar(hook = nameof(syncChargeRate))]
        public float ChargeRate;

        public float LastPositiveChargeRate { get; private set; }

        void Awake()
        {
            _holdoutZoneController = GetComponent<HoldoutZoneController>();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            syncChargeRate(ChargeRate);
        }

        void OnDestroy()
        {
            ComponentCache.OnDestroyed(this);
        }

        void syncChargeRate(float chargeRate)
        {
            ChargeRate = chargeRate;

            if (chargeRate > 0f)
            {
                LastPositiveChargeRate = chargeRate;
            }
        }
    }
}
