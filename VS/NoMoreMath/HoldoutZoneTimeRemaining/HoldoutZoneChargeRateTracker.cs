using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace NoMoreMath.HoldoutZoneTimeRemaining
{
    [RequireComponent(typeof(HoldoutZoneController))]
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

        struct ClientsideVariables
        {
            readonly HoldoutZoneChargeRateTracker _owner;

            float _lastChargeAmount;
            float _lastChargeReceiveTime;

            float _targetChargeRate;

            public ClientsideVariables(HoldoutZoneChargeRateTracker owner)
            {
                _owner = owner;
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
        ClientsideVariables _client;

        void Awake()
        {
            _holdoutZoneController = GetComponent<HoldoutZoneController>();

            if (!NetworkServer.active)
            {
                _client = new ClientsideVariables(this);
            }
        }

        public void OnHoldoutZoneStarted()
        {
            if (NetworkServer.active)
                return;

            _client.RecordCurrentCharge();
        }

        public void OnChargeReceived(float newCharge)
        {
            if (NetworkServer.active)
                return;

            _client.OnChargeReceived(newCharge);
        }

        void Update()
        {
            if (NetworkServer.active)
                return;

            _client.Update(Time.deltaTime);
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
