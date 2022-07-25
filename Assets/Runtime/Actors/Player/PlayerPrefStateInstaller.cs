using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.Actors.Player
{
    [CreateAssetMenu(fileName = "PlayerPrefStateInstaller", menuName = "Installers/PlayerPrefStateInstaller")]
    public class PlayerPrefStateInstaller : ScriptableObjectInstaller<PlayerPrefStateInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<PlayerPrefStateInstaller>().FromInstance(this).AsSingle();
        }

        public void UpdateFloatPlayerPref(string group, float level)
        {
            PlayerPrefs.SetFloat(group, level);
            PlayerPrefs.Save();
        }

        public void UpdateIntPlayerPref(string group, int level)
        {
            PlayerPrefs.SetInt(group, level);
            PlayerPrefs.Save();
        }

        public float GetFloat(string group, float defaultVal)
        {
            return PlayerPrefs.GetFloat(group, defaultVal);
        }

        public int GetInt(string group, int defaultVal)
        {
            return PlayerPrefs.GetInt(group, defaultVal);
        }
    }
}