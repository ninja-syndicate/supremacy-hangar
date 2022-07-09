using System;
using Unity.Mathematics;
using UnityEngine;

namespace SupremacyHangar.Runtime.Actors.Player
{
    public class VRController : MonoBehaviour, IPlayerController
    {
        public event Action OnInteractionTriggered;
        public float3 PlatformVelocity { get; set; }

        private int interactableCount = 0;

        public void Update()
        {
            transform.Translate(new Vector3(PlatformVelocity.x, PlatformVelocity.y, PlatformVelocity.z));
            PlatformVelocity = float3.zero;
        }
        
        public void IncrementInteractionPromptRequests()
        {
            interactableCount++;
            //Show we have interactions
        }

        public void DecrementInteractionPromptRequests()
        {
            if (interactableCount > 0)
            {
                interactableCount--;
            }
            else
            {
                //hide the interactions
                interactableCount = 0;
            }
        }
    }
}