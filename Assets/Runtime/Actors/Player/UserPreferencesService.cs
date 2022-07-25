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
        [SerializeField] private float ambientVolumeDefault = 0.75f;
        [SerializeField] private float musicVolumeDefault = 0.75f;
        
        private const string LookSensitivityKey = "LookSensitivity";
        private const string MasterVolumeKey = "MasterVolume";
        private const string AmbientVolumeKey = "MasterVolume";
        private const string MusicVolumeKey = "MusicVolume";

        public float MouseSensitivity => mouseSensitivity;
        public float MasterVolume => masterVolume;
        public float AmbientVolume => ambientVolume;
        public float MusicVolume => musicVolume;

        public event Action<float> OnMouseSensitivityChange;
        public event Action<float> OnMasterVolumeChange;
        public event Action<float> OnAmbientVolumeChange;
        public event Action<float> OnMusicVolumeChange;

        private float mouseSensitivity, masterVolume, ambientVolume, musicVolume;

        public void OnEnable()
        {
            mouseSensitivity = PlayerPrefs.GetFloat(LookSensitivityKey, mouseSensitivityDefault);
            masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, masterVolumeDefault);
            ambientVolume = PlayerPrefs.GetFloat(AmbientVolumeKey, ambientVolumeDefault);
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
            OnMouseSensitivityChange?.Invoke(value);
        }

        public void ResetMouseSensitivity() => SetMouseSensitivity(mouseSensitivityDefault);

        public void SetMasterVolume(float value)
        {
            if (Mathf.Approximately(value, masterVolume)) return;
            masterVolume = value;
            PlayerPrefs.SetFloat(MasterVolumeKey, value);
            OnMasterVolumeChange?.Invoke(value);
        }
        
        public void ResetMasterVolume() => SetMasterVolume(masterVolumeDefault);
        
        public void SetAmbientVolume(float value)
        {
            if (Mathf.Approximately(value, ambientVolume)) return;
            ambientVolume = value;
            PlayerPrefs.SetFloat(AmbientVolumeKey, value);
            OnAmbientVolumeChange?.Invoke(value);
        }
        
        public void ResetAmbientVolume() => SetAmbientVolume(ambientVolumeDefault);

        public void SetMusicVolume(float value)
        {
            if (Mathf.Approximately(value, musicVolume)) return;
            musicVolume = value;
            PlayerPrefs.SetFloat(MusicVolumeKey, value);
            OnMusicVolumeChange?.Invoke(value);
        }
        
        public void ResetMusicVolume() => SetAmbientVolume(musicVolumeDefault);

    }
}