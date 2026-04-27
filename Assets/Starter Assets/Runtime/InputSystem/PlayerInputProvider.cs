using UnityEngine;

namespace Character
{
	public class PlayerInputProvider : InputProvider
	{
		[SerializeField] private bool analogMovement;
		
		public override Vector2 Move => _move;
		public override Vector2 Look => _look;
		public override bool Jump => _jump;
		public override bool Sprint => _sprint;
		public override bool Crouch => _crouch;
		public override bool AnalogMovement => analogMovement;
		
		private Vector2 _move;
		private Vector2 _look;
		private bool _jump;
		private bool _sprint;
		private bool _crouch;
		
		public void MoveInput(Vector2 newMoveDirection)
		{
			_move = newMoveDirection;
		}

		public void LookInput(Vector2 newLookDirection)
		{
			_look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			_jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			_sprint = newSprintState;
		}

		public void CrouchInput(bool newCrouchState)
		{
			_crouch = newCrouchState;
		}

		public override void ConsumeJump()
		{
			_jump = false;
		}
	}
}
