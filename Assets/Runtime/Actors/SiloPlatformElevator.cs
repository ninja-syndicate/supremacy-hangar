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
    public class SiloPlatformElevator : MonoBehaviour
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
        }

        private Vector3 CalcPlatformHeight(Vector3 pos)
        {
            Vector3 offset = filledTargetTransform.position - pos;
            offset.x = 0;
            offset.z = 0;

            return offset;
        }

        public void Update()
        {
            if (!stopsReady) return;
            if (UnityMath.math.distancesq(currentPos, stops[nextStop]) < Mathf.Epsilon) return;
            Move(Time.deltaTime);
        }

        public void MoveToNextStop()
        {
            if (UnityMath.math.distancesq(currentPos, stops[nextStop]) > Mathf.Epsilon) return;
            nextStop++;
            if (nextStop >= stops.Length) nextStop = 0;
        }

        public void Move(float deltaTime)
        {
            // get our current, and next move vectors;
            Vector3 desiredPos = stops[nextStop];
            // get the movement vector
            Vector3 nextMove = desiredPos - currentPos;

            // we use square distance as it's quicker to calculate
            float sqDistanceToDesired = UnityMath.math.lengthsq(nextMove);
            // max distance we can move in this frame (squared to easily compare with above)
            float distanceThisFrame = speed * deltaTime;
            float sqDistanceThisFrame = distanceThisFrame * distanceThisFrame;

            // if we can move more than the max distance, it's easy.
            if (sqDistanceThisFrame <= sqDistanceToDesired)
            {
                Vector3 thisMove = UnityMath.math.normalize(nextMove) * distanceThisFrame;
                currentPos += thisMove;
                transform.localPosition = currentPos;
                return;
            }

            //otherwise we move straight to the stop.
            transform.localPosition = desiredPos;
            currentPos = desiredPos;
        }
    }
}
