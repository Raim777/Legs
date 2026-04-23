using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
#if ENABLE_INPUT_SYSTEM
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class StarterAssetsInputs : CharacterInputProvider
	{
		[Header("Character Input Values")]
		[SerializeField] private Vector2 move;
		[SerializeField] private Vector2 look;
		[SerializeField] private bool jump;
		[SerializeField] private bool sprint;
		[SerializeField] private bool crouch;

		[Header("Movement Settings")]
		[SerializeField] private bool analogMovement;

		[Header("Mouse Cursor Settings")]
		[SerializeField] private bool cursorLocked = true;
		[SerializeField] private bool cursorInputForLook = true;


		public override Vector2 Move => move;
		public override Vector2 Look => look;
		public override bool Jump => jump;
		public override bool Sprint => sprint;
		public override bool Crouch => crouch;

		public override bool AnalogMovement => analogMovement;

#if ENABLE_INPUT_SYSTEM
		private PlayerInput _playerInput;
		private InputAction _moveAction;
		private InputAction _lookAction;
		private InputAction _jumpAction;
		private InputAction _sprintAction;
		private InputAction _crouchAction;

		private void Awake()
		{
			_playerInput = GetComponent<PlayerInput>();
			var actions = _playerInput.actions;
			_moveAction = actions["Move"];
			_lookAction = actions["Look"];
			_jumpAction = actions["Jump"];
			_sprintAction = actions["Sprint"];
			_crouchAction = actions["Crouch"];
		}

		private void OnEnable()
		{
			_moveAction.performed += OnMove;
			_moveAction.canceled += OnMove;
			_lookAction.performed += OnLook;
			_lookAction.canceled += OnLook;
			_jumpAction.performed += OnJump;
			_jumpAction.canceled += OnJump;
			_sprintAction.performed += OnSprint;
			_sprintAction.canceled += OnSprint;
			_crouchAction.performed += OnCrouch;
		}

		private void OnDisable()
		{
			_moveAction.performed -= OnMove;
			_moveAction.canceled -= OnMove;
			_lookAction.performed -= OnLook;
			_lookAction.canceled -= OnLook;
			_jumpAction.performed -= OnJump;
			_jumpAction.canceled -= OnJump;
			_sprintAction.performed -= OnSprint;
			_sprintAction.canceled -= OnSprint;
			_crouchAction.performed -= OnCrouch;
		}

		private void OnMove(InputAction.CallbackContext ctx)
		{
			move = ctx.ReadValue<Vector2>();
		}

		private void OnLook(InputAction.CallbackContext ctx)
		{
			if (cursorInputForLook)
			{
				look = ctx.ReadValue<Vector2>();
			}
		}

		private void OnJump(InputAction.CallbackContext ctx)
		{
			jump = ctx.ReadValueAsButton();
		}

		private void OnSprint(InputAction.CallbackContext ctx)
		{
			sprint = ctx.ReadValueAsButton();
		}

		private void OnCrouch(InputAction.CallbackContext ctx)
		{
			if (ctx.performed)
			{
				crouch = !crouch;
			}
		}
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		}

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		public override void ConsumeJump()
		{
			jump = false;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}

}
