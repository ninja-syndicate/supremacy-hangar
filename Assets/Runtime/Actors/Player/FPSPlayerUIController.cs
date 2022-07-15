using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace SupremacyHangar.Runtime.Actors.Player
{
	public class FPSPlayerUIController : MonoBehaviour
	{
		public enum VisibilityState
		{
			None,
			SettingsMenu,
			HelpMenu
		} 
		
		[SerializeField] private PlayerInput playerInput;
		[SerializeField] private GameObject settingsMenu;
		[SerializeField] private GameObject helpMenu;
		[SerializeField] private GameObject interactionPrompt;
		
		private VisibilityState state = VisibilityState.None;
		
		private MenuSignalHandler _menuSignalHandler;
        public void Awake()
        {
	        SetupUIItems();
	        SetupPlayerInput();
        }

        public void Start()
        {
	        DisplayHelp();
        }

        public void OnEnable()
        {
	        InputSystemHelpers.BindActionToFunction(playerInput, "Settings", OnSettingsChange);
	        InputSystemHelpers.BindActionToFunction(playerInput, "Help", OnHelpChange);
        }

        public void OnDisable()
        {
	        InputSystemHelpers.UnbindFromFunction(playerInput, "Settings", OnSettingsChange);
	        InputSystemHelpers.UnbindFromFunction(playerInput, "Help", OnHelpChange);
	        
#if UNITY_EDITOR
	        PlayerPrefs.DeleteKey("HelpShown");
#endif
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

		private void DisplayHelp()
		{
			if (PlayerPrefs.GetInt("HelpShown", 0) != 0) return;

			SetState(VisibilityState.HelpMenu);
			PlayerPrefs.SetInt("HelpShown", 1);
			PlayerPrefs.Save();
		}		
		
		private void OnSettingsChange(InputAction.CallbackContext context)
		{
			if (context.phase == InputActionPhase.Canceled) return;
			ToggleState(VisibilityState.SettingsMenu);
		}

		private void OnHelpChange(InputAction.CallbackContext context)
		{
			if (context.phase == InputActionPhase.Canceled) return;
			ToggleState(VisibilityState.HelpMenu);
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
				interactionPrompt.SetActive(true);
			}
		}
		
		private void SetupPlayerInput()
		{
			if (playerInput != null) return;
			if (TryGetComponent(out playerInput)) return;
			
			Debug.LogError("Player Input not present or set", this);
			enabled = false;
		}
		

		// ReSharper disable once ParameterHidesMember
		private void SetVisibilityFor(VisibilityState state, bool value)
		{
			bool handlePause = false;
			switch (state)
			{
				case VisibilityState.None:
					interactionPrompt.SetActive(value);
					break;
				case VisibilityState.HelpMenu:
					helpMenu.SetActive(value);
					handlePause = true;
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
