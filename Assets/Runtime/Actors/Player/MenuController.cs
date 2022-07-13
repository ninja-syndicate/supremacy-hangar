using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.Actors.Player
{
    public class MenuController : MonoBehaviour
	{
		public enum VisibilityState
		{
			None,
			SettingsMenu,
			HelpMenu,
			InteractionPrompt
		} 
		
		[SerializeField] private GameObject settingsMenu;
		[SerializeField] private GameObject helpMenu;
		[SerializeField] private GameObject interactionPrompt;

		private VisibilityState state = VisibilityState.None;
		
		private MenuSignalHandler _menuSignalHandler;
        public void Awake()
        {
	        SetupUIItems();
        }

        [Inject]
		public void Construct(MenuSignalHandler menuSignalHandler)
        {
			_menuSignalHandler = menuSignalHandler;
        }

		public void SetState(VisibilityState newState)
		{
			if (state == newState) return;
			SetVisibilityFor(state, false);
			state = newState;
			SetVisibilityFor(state, true); 
		}

		public void ToggleState(VisibilityState newState)
		{
			if (state == newState) newState = VisibilityState.None;
			SetVisibilityFor(state, false);
			state = newState;
			SetVisibilityFor(state, true); 
		}
		
		private void SetupUIItems()
		{
			if (settingsMenu == null)
			{
				Debug.LogError("Settings Menu not linked", this);
				enabled = false;
			}
			else
			{
				settingsMenu.SetActive(false);
			}

			if (helpMenu == null)
			{
				Debug.LogError("Help Menu not linked", this);
				enabled = false;
			}
			else
			{
				helpMenu.SetActive(false);
			}

			if (interactionPrompt == null)
			{
				Debug.LogError("Interaction Prompt not linked", this);
				enabled = false;
			}
			else
			{
				interactionPrompt.SetActive(false);
			}
		}

		// ReSharper disable once ParameterHidesMember
		private void SetVisibilityFor(VisibilityState state, bool value)
		{
			bool handlePause = false;
			switch (state)
			{
				case VisibilityState.None:
					break;
				case VisibilityState.HelpMenu:
					helpMenu.SetActive(value);
					handlePause = true;
					break;
				case VisibilityState.InteractionPrompt:
					interactionPrompt.SetActive(value);
					break;
				case VisibilityState.SettingsMenu:
					settingsMenu.SetActive(value);
					handlePause = true;
					break;
			}

			if (handlePause && value)
			{
				_menuSignalHandler.PauseGame();
				Cursor.lockState = CursorLockMode.None;
			}
			else if (handlePause) 
			{
				_menuSignalHandler.ResumeGame();
				Cursor.lockState = CursorLockMode.Locked;
			}
		}
	}
}
