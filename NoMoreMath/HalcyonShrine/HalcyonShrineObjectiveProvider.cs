using EntityStates.ShrineHalcyonite;
using NoMoreMath.Config;
using RoR2;
using RoR2.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace NoMoreMath.HalcyonShrine
{
    [DisallowMultipleComponent]
    public sealed class HalcyonShrineObjectiveProvider : NetworkBehaviour
    {
        HalcyoniteShrineInteractable _shrine;

        [SyncVar]
        public bool IsActive;

        [SyncVar]
        public int TotalGoldDrained = -1;

        [SyncVar]
        public int RequiredGoldDrain = -1;

        void Awake()
        {
            _shrine = GetComponent<HalcyoniteShrineInteractable>();

            if (NetworkServer.active)
            {
                updateServerVars();
            }
        }

        void OnEnable()
        {
            ObjectivePanelController.collectObjectiveSources += ObjectivePanelController_collectObjectiveSources;
        }

        void OnDisable()
        {
            ObjectivePanelController.collectObjectiveSources -= ObjectivePanelController_collectObjectiveSources;
        }

        void Update()
        {
            if (NetworkServer.active)
            {
                updateServerVars();
            }
        }

        [Server]
        void updateServerVars()
        {
            bool isActive = false;
            int totalGoldDrained = -1;
            int requiredGoldDrain = -1;
            if (_shrine)
            {
                isActive = _shrine.interactions > 0 && _shrine.stateMachine && _shrine.stateMachine.state is not ShrineHalcyoniteMaxQuality and not ShrineHalcyoniteFinished;
                totalGoldDrained = _shrine.goldDrained;
                requiredGoldDrain = _shrine.maxGoldCost;
            }

            IsActive = isActive;
            TotalGoldDrained = totalGoldDrained;
            RequiredGoldDrain = requiredGoldDrain;
        }

        void ObjectivePanelController_collectObjectiveSources(CharacterMaster viewer, List<ObjectivePanelController.ObjectiveSourceDescriptor> objectiveSources)
        {
            if (IsActive && Configs.HalcyonShrine.EnableProgressObjective.Value)
            {
                objectiveSources.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
                {
                    master = viewer,
                    objectiveType = typeof(HalcyonShrineObjectiveTracker),
                    source = this
                });
            }
        }
    }
}
