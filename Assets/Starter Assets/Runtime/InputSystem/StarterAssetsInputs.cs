using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
		private PlayerInput _playerInput;
		private InputAction _moveAction;
		private InputAction _lookAction;
		private InputAction _jumpAction;
		private InputAction _sprintAction;

		private void Awake()
		{
			_playerInput = GetComponent<PlayerInput>();
			var actions = _playerInput.actions;
			_moveAction = actions["Move"];
			_lookAction = actions["Look"];
			_jumpAction = actions["Jump"];
			_sprintAction = actions["Sprint"];
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
