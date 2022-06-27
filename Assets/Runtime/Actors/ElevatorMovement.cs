using SupremacyHangar.Runtime.Interaction;
using UnityMath = Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace SupremacyHangar.Runtime.Actors
{
    //todo Change to static elevator (predetermined stops)
    public class ElevatorMovement : MonoBehaviour
    {
        [SerializeField] protected UnityMath.float3[] stops;
        [SerializeField] private int initialStop;
        [FormerlySerializedAs("velocity"), SerializeField] protected float speed;

        private bool playerPresent;
        public FirstPersonController playerController { get; set; }
        protected int nextStop;
        protected UnityMath.float3 currentPos;

        [SerializeField] MechElevator mechElevator;

        public void Start()
        {
            currentPos = stops[initialStop];
            transform.localPosition = currentPos;
            nextStop = initialStop;
        }

        public void Update()
        {
            if (UnityMath.math.distancesq(currentPos, stops[nextStop]) < Mathf.Epsilon) return;
            Move(Time.deltaTime);
            mechElevator?.Move(Time.deltaTime);
        }

        public void MoveToNextStop()
        {
            if (UnityMath.math.distancesq(currentPos, stops[nextStop]) > Mathf.Epsilon) return;
            nextStop++;
            mechElevator?.MoveToNextStop();
            if (nextStop >= stops.Length) nextStop = 0;
        }

        private void OnTriggerEnter(Collider other)
        {
            playerPresent = true;
        }

        private void OnTriggerExit(Collider other)
        {
            playerController.PlatformVelocity = UnityMath.float3.zero;
            playerPresent = false;
            playerController = null;
        }

        public virtual void Move(float deltaTime)
        {
            // get our current, and next move vectors;
            UnityMath.float3 desiredPos = stops[nextStop];
            // get the movement vector
            UnityMath.float3 nextMove = desiredPos - currentPos;

            // we use square distance as it's quicker to calculate
            float sqDistanceToDesired = UnityMath.math.lengthsq(nextMove);
            // max distance we can move in this frame (squared to easily compare with above)
            float distanceThisFrame = speed * deltaTime;
            float sqDistanceThisFrame = distanceThisFrame * distanceThisFrame;

            // if we can move more than the max distance, it's easy.
            if (sqDistanceThisFrame <= sqDistanceToDesired)
            {
                UnityMath.float3 thisMove = UnityMath.math.normalize(nextMove) * distanceThisFrame;
                currentPos += thisMove;
                if (playerPresent) playerController.PlatformVelocity = thisMove;
                transform.localPosition = currentPos;
                return;
            }

            //otherwise we move straight to the stop.
            if (playerPresent) playerController.PlatformVelocity = desiredPos - currentPos;
            transform.localPosition = desiredPos;
            currentPos = desiredPos;
        }
    }
}
