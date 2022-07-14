using SupremacyHangar.Runtime.Silo;
using UnityEngine;
using UnityEngine.Serialization;

namespace SupremacyHangar.Runtime.Actors.Elevator
{        
    public class SiloPlatformElevator : ElevatorMotor
    {
        private Vector3[] stops;
        private bool stopsReady = false;
        [FormerlySerializedAs("velocity"), SerializeField] protected float speed;

        private int nextStop = 0;
        private Vector3 currentPos;

        [SerializeField] private Transform filledTargetTransform;

        public override void OnDisable()
        {
            base.OnDisable();
            _bus.Unsubscribe<PlatformRepositionSignal>(SetupPlatform);
        }

        protected override void SubscribeToSignal()
        {
            base.SubscribeToSignal();
            _bus.Subscribe<PlatformRepositionSignal>(SetupPlatform);
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
