using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace SupremacyHangar.Runtime.Actors
{
    enum VolumeTypes
    {
        Music,
        Master,
        Effects,
    }

    [RequireComponent(typeof(Slider))]
    public class SetVolume : MonoBehaviour
    {
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private VolumeTypes volumeTypes;
        [SerializeField] private Slider slider;

        void Start()
        {
            switch (volumeTypes)
            {
                case VolumeTypes.Music:
                    slider.value = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
                    break;
                case VolumeTypes.Master:
                    slider.value = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
                    break;
                case VolumeTypes.Effects:
                    slider.value = PlayerPrefs.GetFloat("EffectsVolume", 0.75f);
                    break;
                default:
                    break;
            }
        }

        public void SetMusicLevel()
        {
            float sliderValue = slider.value;
            mixer.SetFloat("MusicVol", Mathf.Log10(sliderValue) * 20);
            PlayerPrefs.SetFloat("MusicVolume", sliderValue);
        }

        public void SetEffectsLevel()
        {
            float sliderValue = slider.value;
            mixer.SetFloat("EffectsVol", Mathf.Log10(sliderValue) * 20);
            PlayerPrefs.SetFloat("EffectsVolume", sliderValue);
        }

        public void SetMasterLevel()
        {
            float sliderValue = slider.value;
            mixer.SetFloat("MasterVol", Mathf.Log10(sliderValue) * 20);
            PlayerPrefs.SetFloat("MasterVolume", sliderValue);
        }
    }
}