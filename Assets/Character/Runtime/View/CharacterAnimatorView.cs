using UnityEngine;

namespace Character
{
    [RequireComponent(typeof(Animator))]
    public class CharacterAnimatorView : MonoBehaviour
    {
        [SerializeField] private CharacterMotor motor;

        private Animator _animator;

        // cached animator params to avoid redundant native SetBool every frame
        private bool _animGrounded;
        private bool _animJump;
        private bool _animFreeFall;
        private bool _animCrouch;

        private static readonly int AnimIdSpeed = Animator.StringToHash("Speed");
        private static readonly int AnimIDGrounded = Animator.StringToHash("Grounded");
        private static readonly int AnimIDJump = Animator.StringToHash("Jump");
        private static readonly int AnimIDCrouch = Animator.StringToHash("Crouch");
        private static readonly int AnimIDFreeFall = Animator.StringToHash("FreeFall");
        private static readonly int AnimIDMotionSpeed = Animator.StringToHash("MotionSpeed");

        private void Awake()
        {
            _animator = GetComponent<Animator>();

            _animGrounded = _animator.GetBool(AnimIDGrounded);
            _animJump = _animator.GetBool(AnimIDJump);
            _animFreeFall = _animator.GetBool(AnimIDFreeFall);
            _animCrouch = _animator.GetBool(AnimIDCrouch);
        }

        private void Start()
        {
            motor.GroundedChanged += OnGroundedChanged;
            motor.Jumped += OnJumped;
            motor.FreeFallStarted += OnFreeFallStarted;
            motor.CrouchChanged += OnCrouchChanged;

            // initial sync — motor.IsGrounded reflects the inspector default at this point
            OnGroundedChanged(motor.IsGrounded);
        }

        private void OnDestroy()
        {
            if (motor == null) return;
            motor.GroundedChanged -= OnGroundedChanged;
            motor.Jumped -= OnJumped;
            motor.FreeFallStarted -= OnFreeFallStarted;
            motor.CrouchChanged -= OnCrouchChanged;
        }

        private void LateUpdate()
        {
            _animator.SetFloat(AnimIdSpeed, motor.AnimationBlend);
            _animator.SetFloat(AnimIDMotionSpeed, motor.InputMagnitude);
        }

        private void OnGroundedChanged(bool isGrounded)
        {
            SetAnimBool(AnimIDGrounded, ref _animGrounded, isGrounded);
            if (isGrounded)
            {
                SetAnimBool(AnimIDJump, ref _animJump, false);
                SetAnimBool(AnimIDFreeFall, ref _animFreeFall, false);
            }
        }

        private void OnJumped()
        {
            SetAnimBool(AnimIDJump, ref _animJump, true);
        }

        private void OnFreeFallStarted()
        {
            SetAnimBool(AnimIDFreeFall, ref _animFreeFall, true);
        }

        private void OnCrouchChanged(bool isCrouching)
        {
            SetAnimBool(AnimIDCrouch, ref _animCrouch, isCrouching);
        }

        private void SetAnimBool(int id, ref bool current, bool value)
        {
            if (current == value) return;
            current = value;
            _animator.SetBool(id, value);
        }
    }
}
