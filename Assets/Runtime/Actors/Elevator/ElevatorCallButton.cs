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
        private bool playerSubscribed;
        private bool buttonInteractible;

        public override void Awake()
        {
            base.Awake();
            SetupElevator();
        }

        public override void OnPlayerExited()
        {
            if (!playerPresent) return;
            playerPresent = false;
            UpdateState();
            playerController = null;
        }

        public override void OnPlayerEntered(GameObject go, FirstPersonController controller)
        {
            playerPresent = true;
            playerController = controller;
            UpdateState();
        }

        private void OnButtonInteraction()
        {
            //TODO: Handle elevators with more than 2 stops better.
            elevator.MoveToNextStop();
        }

        private void OnStopChanged(int stopIndex)
        {
            buttonInteractible = stopIndex > -1 && stopIndex != stopNumber;
            UpdateState();
        }

        private void UpdateState()
        {
            if (buttonInteractible && playerPresent)
            {
                playerController.OnInteractionTriggered += OnButtonInteraction;
                playerController.IncrementInteractionPromptRequests();
                playerSubscribed = true;
            } else if (playerSubscribed)
            {
                playerController.OnInteractionTriggered -= OnButtonInteraction;
                playerController.DecrementInteractionPromptRequests();
                playerSubscribed = false;
            }
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

            if (!enabled) return;
            OnStopChanged(elevator.CurrentStop);
            elevator.OnStopChanged += OnStopChanged;
        }
    }
}