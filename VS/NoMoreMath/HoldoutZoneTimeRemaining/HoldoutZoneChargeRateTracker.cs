using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace NoMoreMath.HoldoutZoneTimeRemaining
{
    [RequireComponent(typeof(HoldoutZoneController))]
    public class HoldoutZoneChargeRateTracker : NetworkBehaviour
    {
        HoldoutZoneController _holdoutZoneController;

        bool _hasReceivedNetworkChargeRate;

        const int NETWORK_CHARGE_RATE_DIRTY_BIT = 0;
        void setNetworkChargeRateDirty()
        {
#if DEBUG
            Log.Debug("Network Charge Rate set dirty");
#endif
            SetDirtyBit(1 << NETWORK_CHARGE_RATE_DIRTY_BIT);
        }

        public string FormatRemainingTime(float remainingTime)
        {
            if (NetworkServer.active || _hasReceivedNetworkChargeRate)
            {
                return remainingTime.ToString("F1");
            }
            else
            {
                return Mathf.RoundToInt(remainingTime).ToString();
            }
        }

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

        struct ChargeRateEstimator
        {
            readonly HoldoutZoneChargeRateTracker _owner;

            float _lastChargeAmount;
            float _lastChargeReceiveTime;

            float _targetChargeRate;

            public ChargeRateEstimator(HoldoutZoneChargeRateTracker owner)
            {
                _owner = owner;
                _lastChargeAmount = -1;
                _lastChargeReceiveTime = -1;
                _targetChargeRate = -1;
            }

            public void RecordCurrentCharge()
            {
                _lastChargeAmount = _owner._holdoutZoneController.charge;
                _lastChargeReceiveTime = Time.unscaledTime;
            }

            public void Update(float deltaTime)
            {
                if (_targetChargeRate == 0f || Mathf.Abs(_targetChargeRate - _owner.CurrentChargeRate) >= 0.005f)
                {
                    _owner.RecordChargeRate(_targetChargeRate);
                }
                else
                {
                    _owner.RecordChargeRate(Mathf.MoveTowards(_owner.CurrentChargeRate, _targetChargeRate, 0.007f * deltaTime));
                }
            }

            public void OnChargeReceived(float newCharge)
            {
                float currentTime = Time.unscaledTime;
                float estimatedChargeRate = (newCharge - _lastChargeAmount) / (currentTime - _lastChargeReceiveTime);

                if (Mathf.Abs(_targetChargeRate) <= float.Epsilon || Mathf.Abs(_targetChargeRate - estimatedChargeRate) > 0.007f)
                {
#if DEBUG
                    Log.Debug($"Setting target directly, {nameof(_targetChargeRate)}={_targetChargeRate}, {nameof(estimatedChargeRate)}={estimatedChargeRate}");
#endif

                    _targetChargeRate = estimatedChargeRate;
                }
                else
                {
                    _targetChargeRate += (estimatedChargeRate - _targetChargeRate) * 0.02f;
                }

                _lastChargeAmount = newCharge;
                _lastChargeReceiveTime = currentTime;
            }
        }
        ChargeRateEstimator _clientEstimator;

        void Awake()
        {
            _holdoutZoneController = GetComponent<HoldoutZoneController>();

            if (!NetworkServer.active)
            {
                _clientEstimator = new ChargeRateEstimator(this);
            }
        }

        public void OnHoldoutZoneStarted()
        {
            if (NetworkServer.active || _hasReceivedNetworkChargeRate)
                return;

            _clientEstimator.RecordCurrentCharge();
        }

        public void OnServerChargeReceived(float newCharge)
        {
            if (NetworkServer.active || _hasReceivedNetworkChargeRate)
                return;

            _clientEstimator.OnChargeReceived(newCharge);
        }

        void Update()
        {
            if (NetworkServer.active || _hasReceivedNetworkChargeRate)
                return;
            
            _clientEstimator.Update(Time.deltaTime);
        }

        public void RecordChargeRate(float rate)
        {
            if (NetworkServer.active && rate != CurrentChargeRate)
            {
                setNetworkChargeRateDirty();
            }

            CurrentChargeRate = rate;

            if (rate > 0f)
            {
                LastPositiveChargeRate = rate;
            }
        }

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            if (initialState)
            {
                writer.Write(CurrentChargeRate);
                return true;
            }

            uint dirtyBits = syncVarDirtyBits;
            writer.Write(dirtyBits);

            bool writtenAnything = false;

            if ((dirtyBits & (1 << NETWORK_CHARGE_RATE_DIRTY_BIT)) != 0)
            {
                writer.Write(CurrentChargeRate);
                writtenAnything |= true;
            }

            return writtenAnything;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (reader.Position == reader.Length)
            {
#if DEBUG
                Log.Debug($"empty reader received, server most likely doesn't have the mod");
#endif
                return;
            }

            if (initialState)
            {
                float chargeRate = reader.ReadSingle();

#if DEBUG
                Log.Debug($"Network Charge Rate received, {nameof(chargeRate)}={chargeRate} ({nameof(initialState)})");
#endif

                RecordChargeRate(chargeRate);
                _hasReceivedNetworkChargeRate = true;
                return;
            }

            uint dirtyBits = reader.ReadUInt32();

            if ((dirtyBits & (1 << NETWORK_CHARGE_RATE_DIRTY_BIT)) != 0)
            {
                _hasReceivedNetworkChargeRate = true;

                float chargeRate = reader.ReadSingle();

#if DEBUG
                Log.Debug($"Network Charge Rate received, {nameof(chargeRate)}={chargeRate}");
#endif

                RecordChargeRate(chargeRate);
            }
        }
    }
}
