using SupremacyHangar.Runtime.Interaction;
using UnityMath = Unity.Mathematics;
using UnityEngine;

namespace SupremacyHangar.Runtime.Actors
{
    public class Elevator : InteractionZoneResponder
    {
        [SerializeField] private UnityMath.float3[] stops;
        [SerializeField] private int initialStop;
        [SerializeField] private float velocity;

        private bool playerPresent;
        private GameObject player;
        private FirstPersonController playerController;
        private int nextStop;
        private UnityMath.float3 currentPos;
        
        public void Start()
        {
            currentPos = stops[initialStop];
            transform.localPosition = currentPos;
            nextStop = initialStop;
        }
        
        public override void OnPlayerExited()
        {
            playerController.OnInteractionTriggered -= MoveToNextStop;
            playerController.PlatformVelocity = UnityMath.float3.zero;
            playerPresent = false;
            player = null;
            playerController = null;
        }
        
        public void Update()
        {
            if (UnityMath.math.distancesq(currentPos, stops[nextStop]) < Mathf.Epsilon) return;
            Move(Time.deltaTime);
        }

        public void MoveToNextStop()
        {
            if (UnityMath.math.distancesq(currentPos, stops[nextStop]) > Mathf.Epsilon) return;
            nextStop++;
            if (nextStop >= stops.Length) nextStop = 0;
        }

        public override void OnPlayerEntered(GameObject go, FirstPersonController controller)
        {
            playerPresent = true;
            player = go;
            playerController = controller;
            controller.OnInteractionTriggered += MoveToNextStop;
        }
        
        private void Move(float deltaTime)
        {
            // get our current, and next move vectors;
            UnityMath.float3 desiredPos = stops[nextStop];
            // get the movement vector
            UnityMath.float3 nextMove = desiredPos - currentPos;

            // we use square distance as it's quicker to calculate
            float sqDistanceToDesired = UnityMath.math.lengthsq(nextMove); 
            // max distance we can move in this frame (squared to easily compare with above)
            float distanceThisFrame = velocity * deltaTime;
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