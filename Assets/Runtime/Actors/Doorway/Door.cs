using SupremacyHangar.Runtime.Interaction;
using UnityEngine;

namespace SupremacyHangar.Runtime.Actors.Doorway
{
    public class Door : InteractionZoneResponder
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string doorProperty = "IsOpen";
        [SerializeField] private bool automaticOpen;
        [SerializeField] private bool automaticClose;
        
        private int doorPropertyHash;
        private bool animatorDoorState;

        private bool playerPresent;
        private FirstPersonController playerController;

        public override void Awake()
        {
            base.Awake();
            SetupAnimator();            
        }
        
        public override void OnPlayerEntered(GameObject go, FirstPersonController controller)
        {
            playerPresent = true;
            playerController = controller;
            playerController.OnInteractionTriggered += OnDoorInteraction;
            if (!automaticOpen) return;

            animatorDoorState = true;
            animator.SetBool(doorPropertyHash, true);
        }

        public void OnDoorInteraction()
        {
            if (!automaticOpen && !animatorDoorState)
            {
                animatorDoorState = true;
                animator.SetBool(doorPropertyHash, animatorDoorState);
            } else if (!automaticClose && animatorDoorState)
            {
                animatorDoorState = false;
                animator.SetBool(doorPropertyHash, animatorDoorState);
            }
        }

        public override void OnPlayerExited()
        {
            if (playerPresent)
            {
                playerPresent = false;
                playerController.OnInteractionTriggered -= OnDoorInteraction;
                playerController = null;
            }

            if (!automaticClose) return;

            animatorDoorState = false;
            animator.SetBool(doorPropertyHash, false);
        }
        
        private void SetupAnimator()
        {
            if (animator == null)
            {
                if (TryGetComponent(out animator)) return;
                {
                    Debug.LogError("No Animator Zone on this gameobject and none set!", this);
                    enabled = false;
                    return;
                }
            }

            doorPropertyHash = Animator.StringToHash(doorProperty);
            animatorDoorState = animator.GetBool(doorProperty);
        }          
    }
}