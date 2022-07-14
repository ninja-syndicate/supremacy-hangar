using UnityEngine;
using UnityEngine.Serialization;
using UnityMath = Unity.Mathematics;

namespace SupremacyHangar.Runtime.Actors.Elevator
{
    public class PlayerElevator : ElevatorMotor
    {
        [SerializeField] protected Vector3[] stops;
        [SerializeField] private int initialStop;
        [FormerlySerializedAs("velocity"), SerializeField] protected float speed;

        protected UnityMath.float3 currentPos;

        [SerializeField, Tooltip("Optional linked elevator control")] SiloPlatformElevator linkedElevator;

        public void Start()
        {
            currentPos = stops[initialStop];
            transform.localPosition = currentPos;
            InitializeMotor(stops, initialStop, currentPos, speed);
        }

        public override void MoveToNextStop()
        {
            base.MoveToNextStop();
            linkedElevator?.MoveToNextStop();
        }
    }
}
