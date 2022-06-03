using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SupremacyHangar.Runtime 
{
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerDirectController : MonoBehaviour
    {
        [SerializeField] private Transform bodyTransform;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Vector2 cameraVerticalRotationRange = new Vector2(-89, 89);
        [SerializeField] private float walkSpeed = 1.0f;
        [SerializeField] private float strafeSpeed = 1.0f;
        [SerializeField] private float turnSpeed = 1.0f;
        
        private PlayerInput _playerInput;
        private CharacterController _characterController;
        private float _playerCameraAngle;

        private bool _movementDeltaEnabled;
        private Vector2 _movementDelta;

        private bool _turnDeltaEnabled;
        private Vector2 _turnDelta;

        // Start is called before the first frame update
        public void Awake()
        {
            if (!ValidateAndSetupComponentReferences()) return;
        }

        public void OnEnable()
        {
            if (!BindToInputs()) return;
        }

        public void OnDisable()
        {
            UnbindInputs();
        }

        public void Update()
        {
            if (_movementDeltaEnabled)
            {
                Vector3 move = (bodyTransform.forward * (_movementDelta.y * walkSpeed)) + (bodyTransform.right * (_movementDelta.x * turnSpeed));
                move *= Time.deltaTime;
                _characterController.Move(move);
            }

            if (_turnDeltaEnabled)
            {
                bodyTransform.localRotation *= Quaternion.AngleAxis(_turnDelta.x * turnSpeed, bodyTransform.up);
                _playerCameraAngle -= _turnDelta.y * turnSpeed;
                _playerCameraAngle = Mathf.Clamp(
                    _playerCameraAngle, cameraVerticalRotationRange.x, cameraVerticalRotationRange.y);
                cameraTransform.localRotation = Quaternion.Euler(_playerCameraAngle, 0, 0);
            }
        }

        public void OnMovementChange(InputAction.CallbackContext context)
        {
            _movementDeltaEnabled = !context.canceled;
            _movementDelta = context.ReadValue<Vector2>();
        }

        public void OnTurnChange(InputAction.CallbackContext context)
        {
            _turnDeltaEnabled = !context.canceled;
            _turnDelta = context.ReadValue<Vector2>();
        }

        private bool ValidateAndSetupComponentReferences()
        {
            bool valid = true;

            bool temp = TryGetComponent(out _playerInput);
            if (!temp)
            {
                Debug.LogError("Could not find player input!", this);
            }

            valid &= temp;

            temp = TryGetComponent(out _characterController);
            if (!temp)
            {
                Debug.LogError("Could not find character controller", this);
            }

            valid &= temp;
            
            if (bodyTransform == null)
            {
                Debug.LogError("Body Transform not set!", this);
                valid = false;
            }

            if (cameraTransform == null)
            {
                Debug.LogError("Camera Transform not set!", this);
                valid = false;
            }            
            
            if (valid) return true;

            Debug.LogError("Component disabled due to invalid setup", this);
            enabled = false;
            return false;
        }

        private bool BindToInputs()
        {
            bool valid = true;
            valid &= BindActionToFunction("Movement", OnMovementChange);
            valid &= BindActionToFunction("Turn", OnTurnChange);

            enabled = valid;
            return valid;
        }

        private void UnbindInputs()
        {
            UnbindFromFunction("Movement", OnMovementChange);
            UnbindFromFunction("Turn", OnTurnChange);
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
    }
}

