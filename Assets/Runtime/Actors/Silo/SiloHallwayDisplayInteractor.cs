using SupremacyHangar.Runtime.Interaction;
using SupremacyHangar.Runtime.Types;
using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.Actors.Silo
{
    public class SiloHallwayDisplayInteractor : InteractionZoneResponder
    {
        private SiloState siloState;

        private bool firstPersonControllerSet;
        private FirstPersonController firstPersonController;
        private bool interactionEnabled;
        
        [Inject]
        public void Construct(SiloState siloState)
        {
            this.siloState = siloState;
            siloState.OnStateChanged += OnSiloStateChanged;
        }

        private void OnSiloStateChanged(SiloState.StateName newState)
        {
            if (siloState.Contents is EmptySilo) return;

            switch (newState)
            {
                case SiloState.StateName.Loaded:
                case SiloState.StateName.Loading:
                    if (!interactionEnabled) return;
                    interactionEnabled = false;
                    if (!firstPersonControllerSet) return;
                    firstPersonController.OnInteractionTriggered -= SendInteractionMessage;
                    break;
                case SiloState.StateName.NotLoaded:
                case SiloState.StateName.LoadedWithCrate:
                    interactionEnabled = true;
                    if (!firstPersonControllerSet) return;
                    firstPersonController.IncrementInteractionPromptRequests();
                    firstPersonController.OnInteractionTriggered += SendInteractionMessage;
                    break;
            }
        }

        public override void OnPlayerExited()
        {
            if (firstPersonControllerSet)
            {
                if (interactionEnabled) firstPersonController.DecrementInteractionPromptRequests();
                firstPersonController.OnInteractionTriggered -= SendInteractionMessage;
            }
        }

        public override void OnPlayerEntered(GameObject go, FirstPersonController controller)
        {
            if (siloState.Contents is EmptySilo) return;

            firstPersonControllerSet = true;
            firstPersonController = controller;
            OnSiloStateChanged(siloState.CurrentState);
        }

        private void SendInteractionMessage()
        {
            siloState.UserInteraction();
        }
    }
}