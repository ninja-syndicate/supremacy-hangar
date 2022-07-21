using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace SupremacyHangar.Runtime.Silo
{
    [RequireComponent(typeof(AudioSource))]

    public class CrateMusicHandler : MonoBehaviour
    {
        //private CrateDoorController crateDoorController;
        
        [SerializeField] private AudioClip acInherited, acCollosal, acRare, acLegendary, acExotic, acMythic;
        private AudioSource myAudioSource;
        
        // Start is called before the first frame update
        void Start()
        {
            myAudioSource = GetComponent<AudioSource>();
            if (myAudioSource == null) { Debug.LogError("Audio Source Broken...", this); }
        }

        public void PlayCrateMusic(int crateRarity)
        {
            Debug.Log("TestT");
            
            myAudioSource.Stop();
            
            if (crateRarity < 0 || crateRarity > 5) { Debug.LogError("Crate Rarity Invalid. Make sure value is an integer between 0 and 5!", this); }

            switch (crateRarity)
            {
                case 0:
                    myAudioSource.clip = acInherited;
                    break;
                case 1:
                    myAudioSource.clip = acCollosal;
                    break;
                case 2:
                    myAudioSource.clip = acRare;
                    break;
                case 3:
                    myAudioSource.clip = acLegendary;
                    break;
                case 4:
                    myAudioSource.clip = acExotic;
                    break;
                case 5:
                    myAudioSource.clip = acMythic;
                    break;
            }

            myAudioSource.Play();
        }
    }
}
