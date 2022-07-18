using System.Runtime.CompilerServices;
using SupremacyHangar.Runtime.Interaction;
using UnityEngine;

namespace SupremacyHangar.Runtime.Actors.Elevator
{
    public class ElevatorCallButton : InteractionZoneResponder
    {
        [SerializeField] private int stopNumber;
        [SerializeField] private PlayerElevator elevator;

        private FirstPersonController playerController;
        private bool playerPresent;

        public override void Awake()
        {
            base.Awake();
            SetupElevator();
        }

        public override void OnPlayerExited()
        {
            elevator.OnStopChanged -= PlayerInsideInteraction;
            if (!playerPresent) return;
            playerPresent = false;
            playerController.OnInteractionTriggered -= OnButtonInteraction;
            playerController.DecrementInteractionPromptRequests();
            playerController = null;
        }

        public override void OnPlayerEntered(GameObject go, FirstPersonController controller)
        {
            elevator.OnStopChanged += PlayerInsideInteraction;
            playerPresent = true;
            playerController = controller;
            PlayerInsideInteraction(elevator.CurrentStop);
        }

        private void PlayerInsideInteraction(int newStop)
        {
            if (stopNumber == elevator.CurrentStop || elevator.CurrentStop == -1) return;

            playerController.IncrementInteractionPromptRequests();
            playerController.OnInteractionTriggered += OnButtonInteraction;
        }

        private void OnButtonInteraction()
        {
            //TODO: make this good.
            elevator.MoveToNextStop();
        }
        
        private void SetupElevator()
        {
            if (stopNumber < 0)
            {
                Debug.LogError("Stop number must be 0 or greater than 0", this);
                enabled = false;
            }

            if (elevator == null)
            {
                Debug.LogError("Elevator not set", this);
                enabled = false;
                return;
            }

            if (stopNumber > elevator.Stops.Length)
            {
                Debug.LogError("Stop number can't be larger than linked elevator stops", this);
                enabled = false;
            }
        }        
    }
}