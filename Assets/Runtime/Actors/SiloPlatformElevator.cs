using SupremacyData.Runtime;
using SupremacyHangar.Runtime.ContentLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityMath = Unity.Mathematics;
using Zenject;
using SupremacyHangar.Runtime.Silo;

namespace SupremacyHangar.Runtime.Actors
{        
    public class SiloPlatformElevator : ElevatorMotor
    {
        private Vector3[] stops;
        private bool stopsReady = false;
        [FormerlySerializedAs("velocity"), SerializeField] protected float speed;

        private int nextStop = 0;
        private Vector3 currentPos;

        SignalBus _bus;
        private bool _subscribed;

        [SerializeField] private Transform filledTargetTransform;

        [Inject]
        public void Construct(SignalBus bus)
        {
            _bus = bus;
            SubscribeToSignal();
        }

        private void OnEnable()
        {
            SubscribeToSignal();
        }

        private void OnDisable()
        {
            if (!_subscribed) return;
            _bus.Unsubscribe<PlatformRepositionSignal>(SetupPlatform);

            _subscribed = false;
        }

        private void SubscribeToSignal()
        {
            if (_bus == null || _subscribed) return;
            _bus.Subscribe<PlatformRepositionSignal>(SetupPlatform);

            _subscribed = true;
        }

        private void SetupPlatform(PlatformRepositionSignal signal)
        {
            currentPos = transform.localPosition + CalcPlatformHeight(signal.Position);
            stops = new[] { currentPos, transform.localPosition };
            transform.localPosition = currentPos;
            stopsReady = true;
            InitializeMotor(stops, nextStop, currentPos, speed);
        }

        private Vector3 CalcPlatformHeight(Vector3 pos)
        {
            Vector3 offset = filledTargetTransform.position - pos;
            offset.x = 0;
            offset.z = 0;

            return offset;
        }

        public override void Update()
        {
            if (!stopsReady) return;
            base.Update();
        }
    }
}
