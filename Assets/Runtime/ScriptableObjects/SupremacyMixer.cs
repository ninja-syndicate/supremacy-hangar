using System;
using SupremacyHangar.Runtime.Actors.Player;
using UnityEngine;
using UnityEngine.Audio;
using Zenject;

namespace SupremacyHangar
{
    [CreateAssetMenu(fileName = "NewSupremacyMixerHandler", menuName = "Supremacy/Audio/Mixer Handler")]
    public class SupremacyMixer : ScriptableObjectInstaller<SupremacyMixer>
    {
        [SerializeField] private AudioMixer mixer;

        private UserPreferencesService preferencesService;

        public override void InstallBindings()
        {
            Container.Bind<SupremacyMixer>().FromInstance(this).AsSingle();
        }

        public void OnEnable()
        {
            if (preferencesService == null) return;
        }

        [Inject]
        public void InjectDependencies(UserPreferencesService prefsService)
        {
            preferencesService = prefsService;
            preferencesService.OnMasterVolumeChange += SetMasterMixer;
            SetMasterMixer(prefsService.MasterVolume);
            preferencesService.OnMusicVolumeChange += SetMusicMixer;
            SetMusicMixer(prefsService.MusicVolume);
            preferencesService.OnEffectsVolumeChange += SetMusicMixer;
            SetEffectsMixer(prefsService.EffectsVolume);
        }

        public void OnDisable()
        {
            if (preferencesService == null) return;
            preferencesService.OnMasterVolumeChange -= SetMasterMixer;
            preferencesService.OnMusicVolumeChange -= SetMusicMixer;
            preferencesService.OnEffectsVolumeChange -= SetMusicMixer;
        }

        private void SetMasterMixer(float sliderValue)
        {
            mixer.SetFloat("MasterVol", CalcVolume(sliderValue));
        }

        private void SetMusicMixer(float sliderValue)
        {
            mixer.SetFloat("MusicVol", CalcVolume(sliderValue));
        }
        
        private void SetEffectsMixer(float sliderValue)
        {
            mixer.SetFloat("EffectsVol", CalcVolume(sliderValue));
        }

        private float CalcVolume(float value)
        {
            return Mathf.Log10(value) * 20;
        }
    }
}
