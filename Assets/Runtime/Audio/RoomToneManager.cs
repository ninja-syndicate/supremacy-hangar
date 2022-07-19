using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace SupremacyHangar.Runtime.Audio
{
    public class RoomToneManager : MonoBehaviour
    {
        public AudioClip defaultRoomTone;
        public static RoomToneManager instance;
        public AudioMixerGroup mixerGroup;

        private AudioSource hallwayAudio, siloAudio;
        private bool isPlayingHallway;
        private bool isHallwayAmbiPlaying;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        public void SwapAmbiance(AudioClip newClip)
        {
            StopAllCoroutines();

            StartCoroutine(FadeTrack(newClip));

            isPlayingHallway = !isPlayingHallway;
            Debug.Log("inPlayingHallway = " +isPlayingHallway);
        }
        
        // Start is called before the first frame update
        void Start()
        {
            hallwayAudio = gameObject.AddComponent<AudioSource>();
            siloAudio = gameObject.AddComponent<AudioSource>();
            hallwayAudio.outputAudioMixerGroup = mixerGroup;
            siloAudio.outputAudioMixerGroup = mixerGroup;

            isPlayingHallway = true;
            siloAudio.volume = 0;
            SwapAmbiance(defaultRoomTone);
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void ReturnToDefault()
        {
            SwapAmbiance(defaultRoomTone);
        }

        private IEnumerator FadeTrack(AudioClip newClip)
        {
            float timeToFade = 1.25f;
            float timeElapsed = 0;
            
            if (isPlayingHallway)
            {
                siloAudio.clip = newClip;
                siloAudio.Play();

                while (timeElapsed < timeToFade)
                {
                    siloAudio.volume = Mathf.Lerp(0, 1, timeElapsed / timeToFade);
                    hallwayAudio.volume = Mathf.Lerp(1, 0, timeElapsed / timeToFade);
                    timeElapsed += Time.deltaTime;
                    yield return null; 
                }

                hallwayAudio.Stop();
                Debug.Log("PLAYING SILO AMBI");
            }
            else
            {
                hallwayAudio.clip = newClip;
                hallwayAudio.Play();

                while (timeElapsed < timeToFade)
                {
                    hallwayAudio.volume = Mathf.Lerp(0, 1, timeElapsed / timeToFade);
                    siloAudio.volume = Mathf.Lerp(1, 0, timeElapsed / timeToFade);
                    timeElapsed += Time.deltaTime;
                    yield return null;
                }

                siloAudio.Stop();
                Debug.Log("PLAYING HALLWAY AMBI");
            }
        }
    }
}
