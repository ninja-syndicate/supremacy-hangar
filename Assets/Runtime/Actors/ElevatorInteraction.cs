using SupremacyHangar.Runtime.Interaction;
using UnityEngine;

namespace SupremacyHangar.Runtime.Actors
{
    public class ElevatorInteraction : InteractionZoneResponder
    {
        [SerializeField] PlayerElevator playerElevator;

        private FirstPersonController playerController;

        public override void OnPlayerExited()
        {
            SetupElevatorInteraction();
            playerController.OnInteractionTriggered -= playerElevator.MoveToNextStop;
            playerController.DecrementInteractionPromptRequests();
            playerController = null;            
        }

        public override void OnPlayerEntered(GameObject go, FirstPersonController controller)
        {
            SetupElevatorInteraction();
            playerController = controller;
            playerController.OnInteractionTriggered += playerElevator.MoveToNextStop;
            playerController.IncrementInteractionPromptRequests();
        }

        private void SetupElevatorInteraction()
        {
            if (playerElevator != null) return;

            Debug.LogError("No Player Elevate set!", this);
            enabled = false;
        }
    }
}