using SupremacyHangar.Runtime.Scriptable;
using SupremacyHangar.Runtime.Silo;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

namespace SupremacyHangar.Runtime.Interaction
{
    public class InteractionZone : MonoBehaviour
    {
        private bool hasCollided = false;

        [SerializeField]
        private Text labelText;

        [SerializeField]
        private SiloPositioner spawner;

        private PlayerInput _playerInput;

        [Inject]
        private AddressablesManager _addressablesManager;
        
        [Inject]
        private SiloContent[] _siloContent;

        [Inject]
        private SupremacyDictionary _supremacyDictionary;

        [SerializeField]
        private int siloIndex = 0;

        private void Awake()
        {
            _playerInput = FindObjectOfType<PlayerInput>();
        }

        public void OnEnable()
        {
            if (!BindToInputs()) return;
        }

        public void OnDisable()
        {
            UnbindInputs();
        }

        private bool BindToInputs()
        {
            bool valid = true;
            valid &= BindActionToFunction("Interaction", OnInteractionChange);

            enabled = valid;
            return valid;
        }

        private void UnbindInputs()
        {
            UnbindFromFunction("Interaction", OnInteractionChange);
        }

        private void OnInteractionChange(InputAction.CallbackContext obj)
        {
            if (hasCollided)
            {
                //Todo: accept loot boxes
                //if (_siloContent[siloIndex].type.Contains("mech"))
                //{
                    _addressablesManager.targetMech = _supremacyDictionary.mechDictionary[_siloContent[siloIndex].chassisId];
                    _addressablesManager.targetSkin = _supremacyDictionary.AllSkinsDictionary[_siloContent[siloIndex].chassisId][_siloContent[siloIndex].skinId];
                //}
                //else
                //{
                //    _addressablesManager.targetMech = _supremacyDictionary.lootBoxDictionary[_siloContent[siloIndex].id];
                //}
                    spawner.spawnSilo();
            }
        }

        private bool BindActionToFunction(string actionName, Action<InputAction.CallbackContext> callback)
        {
            var action = _playerInput.actions[actionName];
            if (action == null)
            {
                Debug.LogError($"Action player input does not have a '{actionName}' action", this);
                return false;
            }

            action.performed += callback;
            action.canceled += callback;
            return true;
        }

        private void UnbindFromFunction(string actionName, Action<InputAction.CallbackContext> callback)
        {
            var action = _playerInput.actions[actionName];
            if (action == null) return;
            action.performed -= callback;
            action.canceled -= callback;
        }

        void OnGUI()
        {
            if (hasCollided == true)
            {
                //GUI.Box(new Rect(140, Screen.height - 50, Screen.width - 300, 120), (labelText));
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            labelText.enabled = true;
            hasCollided = true;
            //labelText = "Hit E to pick up the key!";
        }

        private void OnTriggerExit(Collider other)
        {
            labelText.enabled = false;
            hasCollided = false;
        }
    }
}