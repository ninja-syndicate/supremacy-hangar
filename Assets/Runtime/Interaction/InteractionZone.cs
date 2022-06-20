using System;
using UnityEngine;

namespace SupremacyHangar.Runtime.Interaction
{
    public class InteractionZone : MonoBehaviour
    {
        private bool currentInteraction;
        private GameObject currentPlayer;
        private FirstPersonController currentPlayerController;
        
        public event Action<GameObject, FirstPersonController> OnPlayerEntered;
        public event Action OnPlayerExited;
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out currentPlayerController)) return;
            currentPlayer = other.gameObject;
            currentInteraction = true;
            OnPlayerEntered?.Invoke(currentPlayer, currentPlayerController);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!currentInteraction) return;
            if (other.gameObject != currentPlayer) return;
            currentPlayer = null;
            currentPlayerController = null;
            currentInteraction = false;
            OnPlayerExited?.Invoke();
        }        
    }
}