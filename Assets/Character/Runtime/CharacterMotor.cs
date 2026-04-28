using System;
using UnityEngine;

namespace Character
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterMotor : MonoBehaviour
    {
        [SerializeField] private CharacterData data;
        [SerializeField] private InputProvider input;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask groundLayers; // определение поверхности земли для прыжков

        [Tooltip("Useful for rough ground")]
        [SerializeField] private float groundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        [SerializeField] private float groundedRadius = 0.28f;

        public event Action<bool> GroundedChanged;
        public event Action Jumped;
        public event Action FreeFallStarted;
        public event Action<bool> CrouchChanged;

        public bool IsGrounded => _grounded;
        public float AnimationBlend => _animationBlend;
        public float InputMagnitude => _inputMagnitude;

        private const float SpeedOffset = 0.1f;
        private const float TerminalVelocity = 53.0f;

        private CharacterController _controller;
        private Transform _transform;
        private Transform _mainCameraTransform;

        private float _speed;
        private float _animationBlend;
        private float _inputMagnitude;
        private float _targetRotation;
        private float _rotationVelocity;
        private float _verticalVelocity;

        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        private bool _freeFallTriggered;
        private bool _crouchState;
        private bool _grounded = true;
        private bool _airControlBlocked = false;

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _transform = transform;
            _mainCameraTransform = mainCamera.transform;

            _jumpTimeoutDelta = data.JumpTimeout;
            _fallTimeoutDelta = data.FallTimeout;
        }

        public void SetInput(InputProvider value)
        {
            input = value;
        }

        public void SetAirControl(bool value)
        {
            _airControlBlocked = !value;
        }

        private void Update()
        {
            JumpAndGravity();
            GroundedCheck();
            Move();
            Crouch();
        }

        private void GroundedCheck()
        {
            Vector3 pos = _transform.position;
            Vector3 spherePosition = new Vector3(pos.x, pos.y - groundedOffset, pos.z);
            bool wasGrounded = _grounded;
            _grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers,
                QueryTriggerInteraction.Ignore);

            if (wasGrounded != _grounded)
            {
                GroundedChanged?.Invoke(_grounded);
            }
        }

        private void Move()
        {
            bool inputEnabled = _grounded || (!_airControlBlocked && data.AirControl);

            float targetSpeed;

            if (inputEnabled)
            {
                targetSpeed = input.Sprint ? data.SprintSpeed : data.MoveSpeed;

                // note: Vector2's == uses approximation, cheaper than magnitude
                if (input.Move == Vector2.zero) targetSpeed = 0.0f;

                Vector3 vel = _controller.velocity;
                float currentHorizontalSpeed = Mathf.Sqrt(vel.x * vel.x + vel.z * vel.z);

                _inputMagnitude = input.AnalogMovement ? input.Move.magnitude : 1f;

                _speed = MovementMath.StepHorizontalSpeed(
                    currentHorizontalSpeed, targetSpeed, _inputMagnitude,
                    SpeedOffset, data.AccelerationRate, Time.deltaTime);

                _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * data.AccelerationRate);
                if (_animationBlend < 0.01f) _animationBlend = 0f;

                if (input.Move != Vector2.zero)
                {
                    _targetRotation = MovementMath.ComputeTargetYaw(input.Move, _mainCameraTransform.eulerAngles.y);
                    float rotation = Mathf.SmoothDampAngle(_transform.eulerAngles.y, _targetRotation,
                        ref _rotationVelocity, data.TurnTime);

                    _transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                }
            }
            else
            {
                // airborne with AirControl disabled: preserve pre-jump speed/heading, ignore input
                targetSpeed = _speed;
                _inputMagnitude = 1f;
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            _controller.Move(targetDirection * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }

        private void JumpAndGravity()
        {
            if (_grounded)
            {
                _fallTimeoutDelta = data.FallTimeout;
                _freeFallTriggered = false;

                // stop vertical velocity from dropping infinitely while grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                if (input.Jump && _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity = MovementMath.JumpImpulse(data.JumpHeight, data.Gravity);
                    Jumped?.Invoke();
                }

                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                _jumpTimeoutDelta = data.JumpTimeout;

                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else if (!_freeFallTriggered)
                {
                    _freeFallTriggered = true;
                    FreeFallStarted?.Invoke();
                }

                input.ConsumeJump();
            }

            _verticalVelocity = MovementMath.StepGravity(_verticalVelocity, data.Gravity, TerminalVelocity, Time.deltaTime);
        }

        private void Crouch()
        {
            bool newCrouch = input.Crouch;
            if (newCrouch == _crouchState) return;
            _crouchState = newCrouch;
            CrouchChanged?.Invoke(_crouchState);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            Gizmos.color = _grounded ? transparentGreen : transparentRed;

            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z),
                groundedRadius);
        }
    }
}
