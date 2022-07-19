using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SupremacyHangar.Runtime.Audio
{
    public class CrateAudioTrigger : MonoBehaviour
    {
        [SerializeField] private AudioClip OpenCrateClip;
        [SerializeField] private AudioClip CloseCrateClip;
        private AudioSource myAudioSource;
        private Animator CrateAnimator;

        // Start is called before the first frame update
        void Start()
        {
            myAudioSource = gameObject.GetComponent(typeof(AudioSource)) as AudioSource;
            CrateAnimator = gameObject.GetComponent(typeof(Animator)) as Animator;
        }


        public void PlayOpenCrateSound()
        {
            myAudioSource.clip = OpenCrateClip;
            myAudioSource.Play();
        }


        // Update is called once per frame
        void Update()
        {
            if (CrateAnimator.GetCurrentAnimatorStateInfo(0).IsName("Opening"))
            {
                if (!myAudioSource.isPlaying)
                {
                    myAudioSource.clip = OpenCrateClip;
                    myAudioSource.Play();
                }
            }

            if (CrateAnimator.GetCurrentAnimatorStateInfo(0).IsName("Closing"))
            {
                if (!myAudioSource.isPlaying)
                {
                    myAudioSource.clip = CloseCrateClip;
                    myAudioSource.Play();
                }
            }
        }

        public void OpenCrateSound(string AnimName)
        {
            if (CrateAnimator.GetCurrentAnimatorStateInfo(0).IsName("Closing"))
            {
                if (!myAudioSource.isPlaying)
                {
                    
                    myAudioSource.PlayOneShot(OpenCrateClip, 1);
                }
            }
        }

        public void CloseCrateSound(string AnimName)
        {
            if (CrateAnimator.GetCurrentAnimatorStateInfo(0).IsName(AnimName))
            {
                if (!myAudioSource.isPlaying)
                {
                    myAudioSource.PlayOneShot(CloseCrateClip, 1);
                }
            }
        }
    }
}
