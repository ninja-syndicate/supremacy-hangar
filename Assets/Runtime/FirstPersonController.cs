using UnityEngine;
using System;
using UnityEngine.InputSystem;
using Zenject;
using SupremacyHangar.Runtime.Interaction;
using Unity.Mathematics;
using SupremacyHangar.Runtime.Actors.Player;

namespace SupremacyHangar.Runtime
{
	[RequireComponent(typeof(CharacterController))]
	[RequireComponent(typeof(PlayerInput))]
	[DefaultExecutionOrder(1)]
	public class FirstPersonController : MonoBehaviour
	{
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 4.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;
		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		public float UpdateRotationSpeed
        {
			get { return RotationSpeed; }
			set { RotationSpeed = value; }
        }

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		[SerializeField] private AnimatorBoolController interactionPromptController;
		private bool interactionPromptControllerSet;
		
		public event Action OnInteractionTriggered;
		
		public float3 PlatformVelocity = float3.zero;
		
		// cinemachine
		private float _cinemachineTargetPitch;

		// player
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;

		private PlayerInput _playerInput;
		private CharacterController _controller;
		private GameObject _mainCamera;

		private const float _threshold = 0.01f;

		private bool IsCurrentDeviceMouse => _playerInput.currentControlScheme == "KeyboardMouse";

		public bool jump = false;
		public bool sprint = false;

		private Vector2 move;

		private Vector2 look;

		private int interactionPrompts = 0;

		[Header("Movement Settings")]
		public bool analogMovement;

#if !UNITY_IOS || !UNITY_ANDROID
		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;
#endif

		public bool showSettings = false;
		public bool showHelpMenu = false;
		private MenuController menuController;

		public void Awake()
		{
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
			if (!ValidateAndSetupComponentReferences()) return;
			interactionPromptControllerSet = interactionPromptController != null;
		}
		
		private void Start()
		{
			_controller = GetComponent<CharacterController>();
			_playerInput = GetComponent<PlayerInput>();
			if(!TryGetComponent(out menuController))
            {
				Debug.LogError("Cannot find or set Menu Controller", this);
				enabled = false;
            }

			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
			
			SetCursorState(cursorLocked);
			
			if (PlayerPrefs.GetInt("HelpShown", 0) == 0)
			{
				menuController.ToggleHelpMenu();
				PlayerPrefs.SetInt("HelpShown", 1);
				PlayerPrefs.Save();
			}
		}

		public void OnEnable()
		{
			if (!BindToInputs()) return; 
		}

		public void OnDisable()
		{
#if UNITY_EDITOR
			PlayerPrefs.DeleteAll();
#endif
			UnbindInputs();
		}

		public void DecrementInteractionPromptRequests()
		{
			if (interactionPrompts > 0) interactionPrompts--;
		}

		public void IncrementInteractionPromptRequests()
		{
			interactionPrompts++;
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

			temp = TryGetComponent(out _controller);
			if (!temp)
			{
				Debug.LogError("Could not find character controller", this);
			}

			valid &= temp;

			if (_mainCamera == null)
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
			valid &= BindActionToFunction("Move", OnMovementChange);
			valid &= BindActionToFunction("Look", OnTurnChange);
			valid &= BindActionToFunction("Jump", OnJumpChange);
			valid &= BindActionToFunction("Sprint", OnSprintChange);
			valid &= BindActionToFunction("Interaction", OnInteractionChange);
			valid &= BindActionToFunction("Settings", OnSettingsChange);
			valid &= BindActionToFunction("Help", OnHelpChange);

			enabled = valid;
			return valid;
		}

		private void UnbindInputs()
		{
			UnbindFromFunction("Move", OnMovementChange);
			UnbindFromFunction("Look", OnTurnChange);
			UnbindFromFunction("Jump", OnJumpChange);
			UnbindFromFunction("Sprint", OnSprintChange);
			UnbindFromFunction("Interaction", OnInteractionChange);
			UnbindFromFunction("Settings", OnSettingsChange);
			UnbindFromFunction("Help", OnHelpChange);
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

		public void OnMovementChange(InputAction.CallbackContext context)
		{
			move = context.ReadValue<Vector2>();
		}

		public void OnTurnChange(InputAction.CallbackContext context)
		{
			look = context.ReadValue<Vector2>();
		}

		public void OnJumpChange(InputAction.CallbackContext context)
		{
			if (Grounded)
				jump = context.phase == InputActionPhase.Performed;
		}

		public void OnSprintChange(InputAction.CallbackContext context)
		{
			sprint = context.phase == InputActionPhase.Performed;
		}

        private void OnInteractionChange(InputAction.CallbackContext context)
		{
			if (context.phase == InputActionPhase.Canceled) return;
			if (OnInteractionTriggered == null) return;
			
			OnInteractionTriggered.Invoke();
			interactionPrompts = 0;
		}

		private void OnSettingsChange(InputAction.CallbackContext context)
		{
			if (context.phase == InputActionPhase.Canceled) return;
			menuController.ToggelSettingsMenu();
		}

		private void OnHelpChange(InputAction.CallbackContext context)
		{
			if (context.phase == InputActionPhase.Canceled) return;
			menuController.ToggleHelpMenu();
		}		

        private void Update()
		{
			if (showSettings || showHelpMenu) return;
			if (interactionPromptControllerSet)
			{
				bool showPrompt = OnInteractionTriggered != null;
				showPrompt &= interactionPrompts > 0;
				interactionPromptController.Set(showPrompt);
			}
			JumpAndGravity();
			GroundedCheck();
			Move();
		}

		private void LateUpdate()
		{
			if (showSettings || showHelpMenu) return;

			CameraRotation();
		}
		
		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			//If we're going down, to keep the grounded check working, we need to adjust the spherecheck
			if (PlatformVelocity.y < 0) spherePosition.y += PlatformVelocity.y;
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		}

		private void CameraRotation()
		{
			// if there is an input
			if (look.sqrMagnitude >= _threshold)
			{
				//Don't multiply mouse input by Time.deltaTime
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
				
				_cinemachineTargetPitch += look.y * RotationSpeed * deltaTimeMultiplier;
				_rotationVelocity = look.x * RotationSpeed * deltaTimeMultiplier;

				// clamp our pitch rotation
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

				// Update Cinemachine camera target pitch
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

				// rotate the player left and right
				transform.Rotate(Vector3.up * _rotationVelocity);
			}
		}

		private void Move()
		{
			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = sprint ? SprintSpeed : MoveSpeed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (move == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = analogMovement ? move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

			// normalise input direction
			Vector3 inputDirection = new Vector3(move.x, 0.0f, move.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (move != Vector2.zero)
			{
				// move
				inputDirection = transform.right * move.x + transform.forward * move.y;
			}

			var nextMove = inputDirection.normalized * (_speed * Time.deltaTime);
			nextMove += new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime;
			if (Grounded) nextMove += new Vector3(PlatformVelocity.x, PlatformVelocity.y, PlatformVelocity.z);
			PlatformVelocity = float3.zero;

			// move the player
			_controller.Move(nextMove);
		}

		private void JumpAndGravity()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = 0;
				}

				// Jump
				if (jump && _jumpTimeoutDelta <= 0.0f)
				{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
				}
				
				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}

				// if we are not grounded, do not jump
				jump = false;
				
				// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
				if (_verticalVelocity < _terminalVelocity)
				{
					_verticalVelocity += Gravity * Time.deltaTime;
				}
			}
		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}

#if !UNITY_IOS || !UNITY_ANDROID

		private void OnApplicationFocus(bool hasFocus)
		{
				SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}

#endif
	}
}