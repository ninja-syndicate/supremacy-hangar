using SupremacyHangar.Runtime;
using SupremacyHangar.Runtime.Actors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityMath = Unity.Mathematics;

namespace SupremacyHangar
{
    public class ElevatorMotor : MonoBehaviour
    {
        private Vector3[] myStops;
        private float mySpeed;

        private bool playerPresent;
        private FirstPersonController playerController;
        private int myNextStop;
        private UnityMath.float3 myCurrentPos;

        public void InitializeMotor(Vector3[] newStops, int newNextStop, UnityMath.float3 newCurrentPos, float newSpeed)
        {
            myStops = newStops;
            myNextStop = newNextStop;
            myCurrentPos = newCurrentPos;
            mySpeed = newSpeed;
        }

        public virtual void Update()
        {
            if (UnityMath.math.distancesq(myCurrentPos, myStops[myNextStop]) < Mathf.Epsilon) return;
            Move(Time.deltaTime);
        }

        public virtual void MoveToNextStop()
        {
            if (UnityMath.math.distancesq(myCurrentPos, myStops[myNextStop]) > Mathf.Epsilon) return;
            myNextStop++;
            if (myNextStop >= myStops.Length) myNextStop = 0;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out playerController)) return;
            playerPresent = true;
        }

        public void OnTriggerExit(Collider other)
        {
            playerController.PlatformVelocity = UnityMath.float3.zero;
            playerPresent = false;
            playerController = null;
        }

        void Move(float deltaTime)
        {
            // get our current, and next move vectors;
            UnityMath.float3 desiredPos = myStops[myNextStop];
            // get the movement vector
            UnityMath.float3 nextMove = desiredPos - myCurrentPos;

            // we use square distance as it's quicker to calculate
            float sqDistanceToDesired = UnityMath.math.lengthsq(nextMove);
            // max distance we can move in this frame (squared to easily compare with above)
            float distanceThisFrame = mySpeed * deltaTime;
            float sqDistanceThisFrame = distanceThisFrame * distanceThisFrame;

            // if we can move more than the max distance, it's easy.
            if (sqDistanceThisFrame <= sqDistanceToDesired)
            {
                UnityMath.float3 thisMove = UnityMath.math.normalize(nextMove) * distanceThisFrame;
                myCurrentPos += thisMove;
                if (playerPresent) playerController.PlatformVelocity = thisMove;
                transform.localPosition = myCurrentPos;
                return;
            }

            //otherwise we move straight to the stop.
            if (playerPresent) playerController.PlatformVelocity = desiredPos - myCurrentPos;
            transform.localPosition = desiredPos;
            myCurrentPos = desiredPos;
        }
    }
}