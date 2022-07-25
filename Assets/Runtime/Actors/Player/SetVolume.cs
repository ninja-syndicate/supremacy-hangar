using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SupremacyHangar.Runtime.Actors.Player
{
    enum VolumeTypes
    {
        Music,
        Master,
        Effects,
    }

    [RequireComponent(typeof(Slider))]
    public class SetVolume : MonoBehaviour, IUpdateSelectedHandler
    {
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private VolumeTypes volumeTypes;
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI textSliderValue;

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

            slider.onValueChanged.AddListener(delegate {
                SetTextValue();
            });
        }

        private void SetTextValue()
        {
            var sliderValue = slider.value * 100;
            textSliderValue.text = "(" + sliderValue.ToString("F0") + ")";
        }

        public void SetLevel(string volumeName, string group)
        {
            float sliderValue = slider.value;
            mixer.SetFloat(volumeName, Mathf.Log10(sliderValue) * 20);
            PlayerPrefs.SetFloat(group, sliderValue);
            PlayerPrefs.Save();
        }

        public void OnUpdateSelected(BaseEventData eventData)
        {
            switch (volumeTypes)
            {
                case VolumeTypes.Music:
                    SetLevel("MusicVol", "MusicVolume");
                    break;
                case VolumeTypes.Master:
                    SetLevel("MasterVol", "MasterVolume");
                    break;
                case VolumeTypes.Effects:
                    SetLevel("EffectsVol", "EffectsVolume");
                    break;
                default:
                    break;
            }
        }
    }
}