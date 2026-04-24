using UnityEngine;

namespace StarterAssets
{
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
		
		public override Vector2 Move => move;
		public override Vector2 Look => look;
		public override bool Jump => jump;
		public override bool Sprint => sprint;
		public override bool Crouch => crouch;
		public override bool AnalogMovement => analogMovement;
		
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

		public void CrouchInput(bool newCrouchState)
		{
			crouch = newCrouchState;
		}

		public override void ConsumeJump()
		{
			jump = false;
		}
	}
}
