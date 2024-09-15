using NoMoreMath.Config;
using NoMoreMath.Utils;
using RoR2;
using RoR2.UI;
using UnityEngine;

namespace NoMoreMath.EffectiveHealth
{
    [DisallowMultipleComponent]
    public class EffectiveHealthDisplayController : MonoBehaviour, ICachableComponent
    {
        public static readonly ObjectComponentCache<HealthBar, EffectiveHealthDisplayController> ComponentCache = new ObjectComponentCache<HealthBar, EffectiveHealthDisplayController>();

        HealthBar _healthBar;

        int? ICachableComponent.DictionaryKey { get; set; }

        EffectiveHealthProvider _healthProvider;
        public EffectiveHealthProvider HealthProvider
        {
            get
            {
                HealthComponent healthBarSource = _healthBar ? _healthBar.source : null;
                if (!_healthProvider || _healthProvider.HealthComponent != healthBarSource)
                {
                    _healthProvider = healthBarSource ? healthBarSource.GetComponent<EffectiveHealthProvider>() : null;
                }

                return _healthProvider;
            }
        }

        public readonly EffectiveHealthDisplayData HealthDisplayData = new EffectiveHealthDisplayData(Configs.EffectiveHealth.CurrentHealthDisplay);

        public readonly EffectiveHealthDisplayData MaxHealthDisplayData = new EffectiveHealthDisplayData(Configs.EffectiveHealth.MaxHealthDisplay);

        public GameObject SlashRoot { get; private set; }

        void Awake()
        {
            _healthBar = GetComponent<HealthBar>();

            if (_healthBar.spriteAsNumberManager)
            {
                Destroy(_healthBar.spriteAsNumberManager.gameObject);
            }

            Transform slashRoot = transform.Find("Slash");
            if (slashRoot)
            {
                SlashRoot = slashRoot.gameObject;
            }
            else
            {
                Log.Error("Failed to find healthbar SlashRoot");
            }
        }

        void OnDestroy()
        {
            ComponentCache.OnDestroyed(this);

            HealthDisplayData?.Dispose();
            MaxHealthDisplayData?.Dispose();
        }
    }
}
