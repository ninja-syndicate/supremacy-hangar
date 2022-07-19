using UnityEngine;
using UnityEngine.Audio;

namespace SupremacyHangar.Runtime.Actors
{
    public class AudioEventPlayer : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioMixerGroup audioMixer;
        private bool audioSourceSet;
        
        public void Awake()
        {
            SetupAudioSource();          
        }

        public void PlayAudio(AudioClip clip)
        {
            if (!audioSourceSet)
            {
                Debug.LogError($"Attempted to play clip {clip.name} and no audio source set or found!", this);
                return;
            }

            audioSource.clip = clip;
            audioSource.Play();
            //Debug.Log("PLAYING SOUND");
        }
        
        private void SetupAudioSource()
        {
            if (audioSource == null)
            {
                if (TryGetComponent(out audioSource)) return;
                {
                    Debug.LogError("No Audio Source on this gameobject and none set!", this);
                    enabled = false;
                    return;
                }
            }

            audioSource.outputAudioMixerGroup = audioMixer;

            audioSourceSet = true;
        }          
    }
}