using UnityEngine;
using UnityEngine.Serialization;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
#if ENABLE_INPUT_SYSTEM
	public class KeyboardGamepadInput : MonoBehaviour
	{
		[Header("Output")]
		[SerializeField] private StarterAssetsInputs inputs;
		[SerializeField] private PlayerInput playerInput;
		
		[Header("Mouse Cursor Settings")]
		[SerializeField] private bool cursorLocked = true;
		[SerializeField] private bool cursorInputForLook = true;

		private InputAction _moveAction;
		private InputAction _lookAction;
		private InputAction _jumpAction;
		private InputAction _sprintAction;
		private InputAction _crouchAction;

		private void Awake()
		{
			var actions = playerInput.actions;
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
			inputs.MoveInput(ctx.ReadValue<Vector2>());
		}

		private void OnLook(InputAction.CallbackContext ctx)
		{
			if (cursorInputForLook)
			{
				inputs.LookInput(ctx.ReadValue<Vector2>());
			}
		}

		private void OnJump(InputAction.CallbackContext ctx)
		{
			inputs.JumpInput(ctx.ReadValueAsButton());
		}

		private void OnSprint(InputAction.CallbackContext ctx)
		{
			inputs.SprintInput(ctx.ReadValueAsButton());
		}

		private void OnCrouch(InputAction.CallbackContext ctx)
		{
			if (ctx.performed)
			{
				inputs.CrouchInput(!inputs.Crouch);
			}
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
#endif
}
