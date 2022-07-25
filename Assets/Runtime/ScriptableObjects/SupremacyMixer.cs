using UnityEngine;
using UnityEngine.Audio;

namespace SupremacyHangar
{
    [CreateAssetMenu(fileName = "NewSupremacyMixerHandler", menuName = "Supremacy/Audio/Mixer Handler")]
    public class SupremacyMixer : ScriptableObject
    {
        [SerializeField] private AudioMixer mixer;
        
        public void SetMasterMixer(float sliderValue)
        {
            mixer.SetFloat("MasterVol", CalcVolume(sliderValue));
        }

        public void SetMusicMixer(float sliderValue)
        {
            mixer.SetFloat("MusicVol", CalcVolume(sliderValue));
        }
        
        public void SetEffectsMixer(float sliderValue)
        {
            mixer.SetFloat("EffectsVol", CalcVolume(sliderValue));
        }

        private float CalcVolume(float value)
        {
            return Mathf.Log10(value) * 20;
        }
    }
}
