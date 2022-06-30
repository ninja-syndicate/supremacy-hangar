using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.Actors.Player
{
    public class MenuController : MonoBehaviour
	{
		[SerializeField] private GameObject settingsMenu;
		[SerializeField] private GameObject helpMenu;
		[SerializeField] private GameObject interactionPrompt;

		private FirstPersonController playerController;

        public void Start()
        {
			if (TryGetComponent(out playerController)) return;

			Debug.LogError("Could not find and set FirstPersonController", this);
			enabled = false;
        }

        [Inject]
		public void Construct()
        {

        }

		public void ToggleHelpMenu()
		{
			if (!playerController.showHelpMenu && !playerController.showSettings)
			{
				interactionPrompt.SetActive(false);

				helpMenu.SetActive(true);
				SetCursorState(false);
				Time.timeScale = 0;
				playerController.showHelpMenu = true;
			}
			else
			{
				interactionPrompt.SetActive(true);

				helpMenu.SetActive(false);
				SetCursorState(true);
				playerController.showHelpMenu = false;
				Time.timeScale = 1;
			}
		}

		public void ToggelSettingsMenu()
		{
			if (!playerController.showSettings && !playerController.showHelpMenu)
			{
				interactionPrompt.SetActive(false);

				settingsMenu.SetActive(true);
				Time.timeScale = 0;
				SetCursorState(false);
				playerController.showSettings = true;
			}
			else
			{
				interactionPrompt.SetActive(true);

				settingsMenu.SetActive(false);
				SetCursorState(true);
				playerController.showSettings = false;
				Time.timeScale = 1;
			}
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
}
