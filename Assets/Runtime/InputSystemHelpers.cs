using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SupremacyHangar.Runtime
{
	public static class InputSystemHelpers
	{
		public static bool BindActionToFunction(PlayerInput playerInput, string actionName, Action<InputAction.CallbackContext> callback)
		{
			var action = playerInput.actions[actionName];
			if (action == null)
			{
				Debug.LogError($"Action player input does not have a '{actionName}' action", playerInput);
				return false;
			}

			action.performed += callback;
			action.canceled += callback;
			return true;
		}

		public static void UnbindFromFunction(PlayerInput playerInput, string actionName, Action<InputAction.CallbackContext> callback)
		{
			var action = playerInput.actions[actionName];
			if (action == null) return;
			action.performed -= callback;
			action.canceled -= callback;
		}        
    }
}