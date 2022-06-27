using SupremacyHangar.Runtime.Interaction;
using UnityMath = Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace SupremacyHangar.Runtime.Actors
{
    public class ElevatorInteraction : InteractionZoneResponder
    {
        [SerializeField] ElevatorMovement playerElevator;

        private FirstPersonController playerController;

        public override void OnPlayerExited()
        {
            playerController.OnInteractionTriggered -= playerElevator.MoveToNextStop;
            playerController.DecrementInteractionPromptRequests();
            playerController = null;            
        }

        public override void OnPlayerEntered(GameObject go, FirstPersonController controller)
        {
            playerElevator.playerController = playerController = controller;
            playerController.OnInteractionTriggered += playerElevator.MoveToNextStop;
            playerController.IncrementInteractionPromptRequests();
        }
    }
}