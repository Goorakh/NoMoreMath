using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

namespace NoMoreMath.EffectiveHealth
{
    [DisallowMultipleComponent]
    public class EffectiveHealthProvider : NetworkBehaviour
    {
        static BuffIndex[] _invincibilityBuffs = [];

        [SystemInitializer(typeof(BuffCatalog))]
        static void Init()
        {
            List<BuffIndex> invincibilityBuffIndices = [];

            void addInvincibilityBuffName(string name)
            {
                BuffIndex buffIndex = BuffCatalog.FindBuffIndex(name);
                if (buffIndex != BuffIndex.None)
                {
                    invincibilityBuffIndices.Add(buffIndex);
                }
                else
                {
                    Log.Warning($"Failed to find buff '{name}'");
                }
            }

            void addInvincibilityBuffDef(BuffDef buffDef, [CallerLineNumber] int lineNumber = -1)
            {
                if (buffDef)
                {
                    invincibilityBuffIndices.Add(buffDef.buffIndex);
                }
                else
                {
                    Log.Warning($"Null BuffDef at call {lineNumber}");
                }
            }

            addInvincibilityBuffDef(DLC1Content.Buffs.BearVoidReady);
            addInvincibilityBuffDef(RoR2Content.Buffs.HiddenInvincibility);
            addInvincibilityBuffDef(DLC2Content.Buffs.SojournVehicle);
            addInvincibilityBuffDef(RoR2Content.Buffs.Immune);
            addInvincibilityBuffDef(JunkContent.Buffs.BodyArmor);
            addInvincibilityBuffDef(DLC2Content.Buffs.HiddenRejectAllDamage);

            _invincibilityBuffs = invincibilityBuffIndices.ToArray();
            Array.Sort(_invincibilityBuffs);
        }

        public HealthComponent HealthComponent { get; private set; }

        [SyncVar]
        float _adaptiveArmorValue;
        public float AdaptiveArmorValue => _adaptiveArmorValue;

        public float EffectiveHealth => GetEffectiveHealth(HealthComponent.combinedHealth);

        public float EffectiveMaxHealth => GetEffectiveHealth(HealthComponent.fullHealth);

        void Awake()
        {
            HealthComponent = GetComponent<HealthComponent>();
        }

        void Update()
        {
            if (NetworkServer.active)
            {
                updateAdaptiveArmorServer();
            }
        }

        [Server]
        void updateAdaptiveArmorServer()
        {
            _adaptiveArmorValue = HealthComponent ? HealthComponent.adaptiveArmorValue : 0f;
        }

        public float GetEffectiveHealth(float health)
        {
            if (health < 0 || !HealthComponent || !HealthComponent.body)
                return health;

            if (HealthComponent.godMode)
                return float.PositiveInfinity;

            CharacterBody body = HealthComponent.body;

            foreach (BuffIndex invincibilityBuff in _invincibilityBuffs)
            {
                if (body.HasBuff(invincibilityBuff))
                {
                    return float.PositiveInfinity;
                }
            }

            float damageMultiplier = 1f;

            if (body.HasBuff(DLC2Content.Buffs.lunarruin))
            {
                damageMultiplier *= 1f + (0.1f * body.GetBuffCount(DLC2Content.Buffs.lunarruin));
            }

            if (body.HasBuff(RoR2Content.Buffs.DeathMark))
            {
                damageMultiplier *= 1.5f;
            }

            float armor = body.armor + _adaptiveArmorValue;

            float armorDamageMultiplier = armor >= 0f ? (1f - (armor / (armor + 100f)))
                                                      : (2f - (100f / (100f - armor)));

            damageMultiplier *= armorDamageMultiplier;

            return health / damageMultiplier;
        }
    }
}
