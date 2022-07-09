using SupremacyHangar.Runtime.Actors.Player;
using UnityEngine;

namespace SupremacyHangar.Runtime.Interaction
{
    public abstract class InteractionZoneResponder : MonoBehaviour
    {
        [SerializeField] private InteractionZone interactionZone;

        public virtual void Awake()
        {
            SetupInteractionZone();
        }

        public virtual void OnEnable()
        {
            interactionZone.OnPlayerEntered += OnPlayerEntered;
            interactionZone.OnPlayerExited += OnPlayerExited;
        }
        
        public virtual void OnDisable()
        {
            interactionZone.OnPlayerEntered -= OnPlayerEntered;
            interactionZone.OnPlayerExited -= OnPlayerExited;
        }        
        
        public abstract void OnPlayerExited();
        public abstract void OnPlayerEntered(GameObject go, FirstPersonController controller);
        
        private void SetupInteractionZone()
        {
            if (interactionZone != null) return;
            if (TryGetComponent(out interactionZone)) return;

            Debug.LogError("No interaction Zone on this GameObject and none set!", this);
            enabled = false;
        }        
    }
}