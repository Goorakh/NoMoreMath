using NoMoreMath.Utils;
using RoR2;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;

namespace NoMoreMath.HalcyonBeacon
{
    [DisallowMultipleComponent]
    public class GoldshoresMissionControllerBeaconsProvider : NetworkBehaviour
    {
        public static GoldshoresMissionControllerBeaconsProvider Instance { get; private set; }

        GoldshoresMissionController _missionController;

        [SyncVar(hook = nameof(syncBeacons))]
        GoldshoresBeaconCollection _beaconCollection;

        readonly List<GameObject> _beaconsList = [];
        public ReadOnlyCollection<GameObject> BeaconObjects { get; private set; }

        readonly List<PurchaseInteraction> _beaconPurchaseInteractions = [];
        public ReadOnlyCollection<PurchaseInteraction> BeaconPurchaseInteractions { get; private set; }

        void Awake()
        {
            _missionController = GetComponent<GoldshoresMissionController>();

            BeaconObjects = new ReadOnlyCollection<GameObject>(_beaconsList);
            BeaconPurchaseInteractions = new ReadOnlyCollection<PurchaseInteraction>(_beaconPurchaseInteractions);
        }

        void OnEnable()
        {
            Instance = SingletonHelper.Assign(Instance, this);

            if (NetworkServer.active)
            {
                updateServerVars();
            }
        }

        void OnDisable()
        {
            Instance = SingletonHelper.Unassign(Instance, this);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            syncBeacons(_beaconCollection);
        }

        void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                updateServerVars();
            }
        }

        [Server]
        void updateServerVars()
        {
            GameObject[] beaconObjects = _missionController ? _missionController.beaconInstanceList.ToArray() : [];
            _beaconCollection = new GoldshoresBeaconCollection(beaconObjects);
        }

        void syncBeacons(GoldshoresBeaconCollection beaconCollection)
        {
            _beaconCollection = beaconCollection;

            _beaconsList.Clear();
            _beaconsList.AddRange(beaconCollection.BeaconObjects);

            _beaconPurchaseInteractions.Clear();
            foreach (GameObject beacon in BeaconObjects)
            {
                if (beacon && beacon.TryGetComponent(out PurchaseInteraction purchaseInteraction))
                {
                    _beaconPurchaseInteractions.Add(purchaseInteraction);
                }
            }
        }

        public static void TryAddComponent(GoldshoresMissionController missionController)
        {
            if (!missionController)
            {
                Log.Error("Null mission controller instance");
                return;
            }

            if (!missionController.GetComponent<NetworkIdentity>())
            {
                Log.Error("Mission controller is missing network identity");
                return;
            }

            missionController.gameObject.AddComponent<GoldshoresMissionControllerBeaconsProvider>();
        }
    }
}
