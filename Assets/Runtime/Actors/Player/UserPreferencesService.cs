using System;
using UnityEngine;
using UnityEngine.Audio;
using Zenject;

namespace SupremacyHangar.Runtime.Actors.Player
{
    [CreateAssetMenu(fileName = "UserPreferencesService", menuName = "Supremacy/Installers/User Preferences Service")]
    public class UserPreferencesService : ScriptableObjectInstaller<UserPreferencesService>
    {
        [SerializeField] private float mouseSensitivityDefault = 1.0f;
        [SerializeField] private float masterVolumeDefault = 0.75f;
        [SerializeField] private float effectsVolumeDefault = 0.75f;
        [SerializeField] private float musicVolumeDefault = 0.75f;
        
        private const string LookSensitivityKey = "LookSensitivity";
        private const string MasterVolumeKey = "MasterVolume";
        private const string EffectsVolumeKey = "EffectsVolume";
        private const string MusicVolumeKey = "MusicVolume";

        public float MouseSensitivity => mouseSensitivity;
        public float MasterVolume => masterVolume;
        public float EffectsVolume => effectsVolume;
        public float MusicVolume => musicVolume;

        public event Action<float> OnMouseSensitivityChange;
        public event Action<float> OnMasterVolumeChange;
        public event Action<float> OnEffectsVolumeChange;
        public event Action<float> OnMusicVolumeChange;

        private float mouseSensitivity, masterVolume, effectsVolume, musicVolume;

        public void OnEnable()
        {
            mouseSensitivity = PlayerPrefs.GetFloat(LookSensitivityKey, mouseSensitivityDefault);
            masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, masterVolumeDefault);
            effectsVolume = PlayerPrefs.GetFloat(EffectsVolumeKey, effectsVolumeDefault);
            musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, musicVolumeDefault);
        }
        
        public override void InstallBindings()
        {
            Container.Bind<UserPreferencesService>().FromInstance(this).AsSingle();
        }

        public void SetMouseSensitivity(float value)
        {
            if (Mathf.Approximately(value, mouseSensitivity)) return;
            mouseSensitivity = value;
            PlayerPrefs.SetFloat(LookSensitivityKey, value);
            PlayerPrefs.Save();
            OnMouseSensitivityChange?.Invoke(value);
        }

        public void ResetMouseSensitivity() => SetMouseSensitivity(mouseSensitivityDefault);

        public void SetMasterVolume(float value)
        {
            if (Mathf.Approximately(value, masterVolume)) return;
            masterVolume = value;
            PlayerPrefs.SetFloat(MasterVolumeKey, value);
            PlayerPrefs.Save();
            OnMasterVolumeChange?.Invoke(value);
        }
        
        public void ResetMasterVolume() => SetMasterVolume(masterVolumeDefault);
        
        public void SetEffectsVolume(float value)
        {
            if (Mathf.Approximately(value, effectsVolume)) return;
            effectsVolume = value;
            PlayerPrefs.SetFloat(EffectsVolumeKey, value);
            PlayerPrefs.Save();
            OnEffectsVolumeChange?.Invoke(value);
        }
        
        public void ResetEffectsVolume() => SetEffectsVolume(effectsVolumeDefault);

        public void SetMusicVolume(float value)
        {
            if (Mathf.Approximately(value, musicVolume)) return;
            musicVolume = value;
            PlayerPrefs.SetFloat(MusicVolumeKey, value);
            PlayerPrefs.Save();
            OnMusicVolumeChange?.Invoke(value);
        }
        
        public void ResetMusicVolume() => SetMusicVolume(musicVolumeDefault);

    }
}