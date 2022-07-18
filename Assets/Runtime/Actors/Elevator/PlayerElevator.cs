using UnityEngine;
using UnityEngine.Serialization;
using UnityMath = Unity.Mathematics;

namespace SupremacyHangar.Runtime.Actors.Elevator
{
    public class PlayerElevator : ElevatorMotor
    {
        public Vector3[] Stops => stops;

        [SerializeField] protected Vector3[] stops;
        [SerializeField] private int initialStop;

        protected UnityMath.float3 currentPos;

        [SerializeField, Tooltip("Optional linked elevator control")] SiloPlatformElevator linkedElevator;
        private bool linkedElevatorPresent;
        
        public void Start()
        {
            currentPos = stops[initialStop];
            transform.localPosition = currentPos;
            InitializeMotor(stops, initialStop);
            linkedElevatorPresent = linkedElevator != null;
        }

        public override void MoveToNextStop()
        {
            if (linkedElevatorPresent) MoveLinkedElevator();
            base.MoveToNextStop();

        }

        private void MoveLinkedElevator()
        {
            if(linkedElevator.CurrentStop != CurrentStop)
                linkedElevator.MoveToNextStop();
        }
    }
}
