using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace SupremacyHangar.Runtime.Actors.Player
{
    public class ResetUserPrefs : MonoBehaviour, ISelectHandler
    {
        private UserPreferencesService userPreferences;

        [Inject]
        public void InjectDependencies(UserPreferencesService userPreferences)
        {
            this.userPreferences = userPreferences;
        }

        public void OnSelect(BaseEventData eventData)
        {
            userPreferences.ResetMasterVolume();
            userPreferences.ResetEffectsVolume();
            userPreferences.ResetMouseSensitivity();
            userPreferences.ResetMusicVolume();
        }
    }
}
