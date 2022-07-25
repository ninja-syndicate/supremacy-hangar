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
    enum PreferenceTypes
    {
        LookSensitivity,
        MusicVolume,
        MasterVolume,
        EffectsVolume,
    }

    [RequireComponent(typeof(Slider))]
    public class PreferenceSliderUI : MonoBehaviour
    {
        [SerializeField] private PreferenceTypes preferenceType;
        private Slider slider;
        [SerializeField] private TextMeshProUGUI textSliderValue;
        
        private UserPreferencesService userPreferences;

        [Inject]
        public void InjectDependencies(UserPreferencesService userPreferences)
        {
            slider = GetComponent<Slider>();
            this.userPreferences = userPreferences;
            slider.onValueChanged.AddListener(SetLevel);

            switch (preferenceType)
            {
                case PreferenceTypes.LookSensitivity:
                    slider.value = userPreferences.MouseSensitivity;
                    break;
                case PreferenceTypes.MasterVolume:
                    slider.value = userPreferences.MasterVolume;
                    break;
                case PreferenceTypes.MusicVolume:
                    slider.value = userPreferences.MusicVolume;
                    break;
                default:
                    break;
            }
        }

        private void SetTextValue()
        {
            float sliderValue = 0;
            switch (preferenceType)
            {
                case PreferenceTypes.LookSensitivity:
                    sliderValue = slider.value;
                    textSliderValue.text = "(" + sliderValue.ToString("F1") + ")";
                    break;
                case PreferenceTypes.MasterVolume:
                case PreferenceTypes.MusicVolume:
                case PreferenceTypes.EffectsVolume:
                    sliderValue = slider.value * 100;
                    textSliderValue.text = "(" + sliderValue.ToString("F0") + ")";
                    break;
                default:
                    break;
            }
        }

        public void SetLevel(float value)
        {
            slider.value = value;
            switch(preferenceType)
            {
                case PreferenceTypes.LookSensitivity:
                    userPreferences.SetMouseSensitivity(value);
                    break;
                case PreferenceTypes.MusicVolume:
                    userPreferences.SetMusicVolume(value);
                    break;
                case PreferenceTypes.MasterVolume:
                    userPreferences.SetMasterVolume(value);
                    break;
                case PreferenceTypes.EffectsVolume:
                    userPreferences.SetEffectsVolume(value);
                    break;
                default:
                    break;
            }
            SetTextValue();
        }
    }
}