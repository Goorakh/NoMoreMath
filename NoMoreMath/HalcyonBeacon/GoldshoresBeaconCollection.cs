using HG;
using NoMoreMath.Utils;
using System;
using UnityEngine;

namespace NoMoreMath.HalcyonBeacon
{
    internal struct GoldshoresBeaconCollection : IEquatable<GoldshoresBeaconCollection>
    {
        public GameObject[] BeaconObjects;

        public GoldshoresBeaconCollection(GameObject[] beaconObjects)
        {
            BeaconObjects = beaconObjects;
        }

        public readonly bool Equals(GoldshoresBeaconCollection other)
        {
            return ArrayUtils.SequenceEquals(BeaconObjects, other.BeaconObjects, UnityObjectComparer.Instance);
        }
    }
}
