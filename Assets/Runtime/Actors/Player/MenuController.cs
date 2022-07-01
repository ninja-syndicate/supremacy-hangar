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
		private MenuSignalHandler _menuSignalHandler;
        public void Start()
        {
			if (TryGetComponent(out playerController)) return;

			Debug.LogError("Could not find and set FirstPersonController", this);
			enabled = false;
        }

        [Inject]
		public void Construct(MenuSignalHandler menuSignalHandler)
        {
			_menuSignalHandler = menuSignalHandler;
        }

		public void ToggleHelpMenu()
		{
			if (!playerController.showHelpMenu && !playerController.showSettings)
			{
				interactionPrompt.SetActive(false);

				helpMenu.SetActive(true);
				SetCursorState(false);
				_menuSignalHandler.PauseGame();
				playerController.showHelpMenu = true;
			}
			else if(!playerController.showSettings)
			{
				interactionPrompt.SetActive(true);

				helpMenu.SetActive(false);
				SetCursorState(true);
				playerController.showHelpMenu = false;
				_menuSignalHandler.ResumeGame();
			}
		}

		public void ToggelSettingsMenu()
		{
			if (!playerController.showSettings && !playerController.showHelpMenu)
			{
				interactionPrompt.SetActive(false);

				settingsMenu.SetActive(true);

				_menuSignalHandler.PauseGame();
				SetCursorState(false);
				playerController.showSettings = true;
			}
			else if(!playerController.showHelpMenu)
			{
				interactionPrompt.SetActive(true);

				settingsMenu.SetActive(false);
				SetCursorState(true);
				playerController.showSettings = false;
				_menuSignalHandler.ResumeGame();
			}
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
}
