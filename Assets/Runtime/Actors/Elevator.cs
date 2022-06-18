using System;
using SupremacyHangar.Runtime.Interaction;
using UnityMath = Unity.Mathematics;
using UnityEngine;

namespace SupremacyHangar.Runtime.Actors
{
    public class Elevator : MonoBehaviour
    {
        [SerializeField] private InteractionZone interactionZone;
        [SerializeField] private UnityMath.float3[] stops;
        [SerializeField] private int initialStop;
        [SerializeField] private float velocity;
        [SerializeField] private bool testMove;
        
        private bool playerPresent;
        private GameObject player;
        private FirstPersonController playerController;
        private int nextStop;
        private UnityMath.float3 currentPos;
        
        public void Awake()
        {
            SetupInteractionZone();
        }

        public void Start()
        {
            currentPos = stops[initialStop];
            transform.localPosition = currentPos;
            nextStop = initialStop;
        }

        public void OnEnable()
        {
            interactionZone.OnPlayerEntered += OnPlayerEntered;
            interactionZone.OnPlayerExited += OnOnPlayerExited;
        }

        private void OnOnPlayerExited()
        {
            playerPresent = false;
            player = null;
            playerController = null;
        }

        public void Update()
        {
            if (testMove)
            {
                if (MoveToNextStop())
                {
                    testMove = false;    
                }
            }
            
            if (UnityMath.math.distancesq(currentPos, stops[nextStop]) < Mathf.Epsilon) return;
            Move(Time.deltaTime);
        }

        public bool MoveToNextStop()
        {
            if (UnityMath.math.distancesq(currentPos, stops[nextStop]) > Mathf.Epsilon) return false;
            nextStop++;
            if (nextStop >= stops.Length) nextStop = 0;
            return true;
        }

        private void OnPlayerEntered(GameObject go, FirstPersonController controller)
        {
            playerPresent = true;
            player = go;
            playerController = controller;
        }

        private void SetupInteractionZone()
        {
            if (interactionZone != null) return;
            if (TryGetComponent(out interactionZone)) return;
            {
                Debug.LogError("No interaction Zone on this gameobject and none set!", this);
                enabled = false;
            }
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
                currentPos += UnityMath.math.normalize(nextMove) * distanceThisFrame;
                transform.localPosition = currentPos;
                return;
            }
            
            //otherwise we move straight to the stop.
            transform.localPosition = desiredPos;
            currentPos = desiredPos;
        }
    }
}