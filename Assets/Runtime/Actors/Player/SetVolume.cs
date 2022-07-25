using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

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
        [SerializeField] private VolumeTypes volumeType;
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI textSliderValue;
        
        private UserPreferencesService userPreferences;
        private string groupName = "";

        [Inject]
        public void InjectDependencies(UserPreferencesService userPreferences)
        {
            this.userPreferences = userPreferences;
        }
        
        void Start()
        {
            groupName = volumeType.ToString() + "Volume";


            slider.onValueChanged.AddListener(SetTextValue);
        }

        private void SetTextValue()
        {
            var sliderValue = slider.value * 100;
            textSliderValue.text = "(" + sliderValue.ToString("F0") + ")";
        }

        public void SetLevel(string volumeName)
        {
            float sliderValue = slider.value;
            mixer.SetFloat(volumeName, Mathf.Log10(sliderValue) * 20);
            playerPrefState.UpdateFloatPlayerPref(groupName, sliderValue);
        }

        public void OnUpdateSelected(BaseEventData eventData)
        {
            switch (volumeType)
            {
                case VolumeTypes.Music:
                    SetLevel("MusicVol");
                    break;
                case VolumeTypes.Master:
                    SetLevel("MasterVol");
                    break;
                case VolumeTypes.Effects:
                    SetLevel("EffectsVol");
                    break;
                default:
                    break;
            }
        }
    }
}