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

        [SerializeField] private AudioClip closeSoundClip;
        [SerializeField] private AudioClip openSoundClip;
        [SerializeField] private AudioSource myAudioSource;

        private int doorPropertyHash;
        private bool animatorDoorState;

        private bool playerPresent;
        private FirstPersonController playerController;

        public override void Awake()
        {
            base.Awake();
            SetupAnimator();            
            if (myAudioSource) return;

            Debug.LogError("Audio source is not set");
            enabled = false;
        }
        
        public override void OnPlayerEntered(GameObject go, FirstPersonController controller)
        {
            playerPresent = true;
            playerController = controller;
            playerController.OnInteractionTriggered += OnDoorInteraction;
            if (automaticOpen)
            {
                animatorDoorState = true;
                animator.SetBool(doorPropertyHash, true);
                myAudioSource.clip = openSoundClip;
                myAudioSource.Play();
                
            }
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

            if (automaticClose)
            {
                animatorDoorState = false;
                animator.SetBool(doorPropertyHash, false);
                myAudioSource.clip = closeSoundClip;
                myAudioSource.Play();
            }
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