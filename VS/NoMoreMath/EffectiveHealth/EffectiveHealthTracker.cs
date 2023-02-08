using NoMoreMath.Utility;
using RoR2;
using RoR2.UI;
using UnityEngine;

namespace NoMoreMath.EffectiveHealth
{
    [RequireComponent(typeof(HealthBar))]
    public class EffectiveHealthTracker : MonoBehaviour
    {
        public class Tracker
        {
            public float LastValue { get; private set; }
            public float CurrentValue { get; private set; }

            public bool IsDirty => LastValue != CurrentValue;

            public void OnRefreshed()
            {
                CurrentValue = LastValue;
            }

            public void RecordEffectiveHealth(CharacterBody body, float health)
            {
                CurrentValue = HealthUtils.CalculateEffectiveHealth(body, health);
            }
        }

        public readonly Tracker CurrentHealth = new Tracker();

        public readonly Tracker FullHealth = new Tracker();

        HealthBar _healthBar;

        void Awake()
        {
            _healthBar = GetComponent<HealthBar>();
        }

        void Update()
        {
            if (!_healthBar)
                return;

            HealthComponent healthComponent = _healthBar.source;
            if (!healthComponent)
                return;

            CharacterBody body = healthComponent.body;
            if (!body)
                return;

            CurrentHealth.RecordEffectiveHealth(body, healthComponent.combinedHealth);
            FullHealth.RecordEffectiveHealth(body, healthComponent.fullCombinedHealth + healthComponent.barrier);
        }
    }
}
