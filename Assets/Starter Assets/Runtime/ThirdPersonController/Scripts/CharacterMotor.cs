using System;
using UnityEngine;

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterMotor : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private CharacterInputProvider input;

        [Header("Camera")]
        [Tooltip("Reference to the main camera — used for camera-relative movement direction")]
        [SerializeField] private Camera mainCamera;

        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        [SerializeField] public float moveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        [SerializeField] private float sprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        [SerializeField] private float rotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        [SerializeField] private float speedChangeRate = 10.0f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        [SerializeField] private float jumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        [SerializeField] private float gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        [SerializeField] private float jumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        [SerializeField] private float fallTimeout = 0.15f;

        [Tooltip("If true, player can change direction and speed while airborne")]
        [SerializeField] private bool airControl = true;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        [SerializeField] private bool grounded = true;

        [Tooltip("Useful for rough ground")]
        [SerializeField] private float groundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        [SerializeField] private float groundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        [SerializeField] private LayerMask groundLayers;

        public event Action<bool> GroundedChanged;
        public event Action Jumped;
        public event Action FreeFallStarted;
        public event Action<bool> CrouchChanged;

        public bool IsGrounded => grounded;
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

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _transform = transform;
            _mainCameraTransform = mainCamera.transform;

            _jumpTimeoutDelta = jumpTimeout;
            _fallTimeoutDelta = fallTimeout;
        }

        public void SetInput(CharacterInputProvider value)
        {
            input = value;
        }

        public void SetAirControl(bool value)
        {
            airControl = value;
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
            bool wasGrounded = grounded;
            grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers,
                QueryTriggerInteraction.Ignore);

            if (wasGrounded != grounded)
            {
                GroundedChanged?.Invoke(grounded);
            }
        }

        private void Move()
        {
            bool inputEnabled = grounded || airControl;

            float targetSpeed;

            if (inputEnabled)
            {
                targetSpeed = input.Sprint ? sprintSpeed : moveSpeed;

                // note: Vector2's == uses approximation, cheaper than magnitude
                if (input.Move == Vector2.zero) targetSpeed = 0.0f;

                Vector3 vel = _controller.velocity;
                float currentHorizontalSpeed = Mathf.Sqrt(vel.x * vel.x + vel.z * vel.z);

                _inputMagnitude = input.AnalogMovement ? input.Move.magnitude : 1f;

                _speed = MovementMath.StepHorizontalSpeed(
                    currentHorizontalSpeed, targetSpeed, _inputMagnitude,
                    SpeedOffset, speedChangeRate, Time.deltaTime);

                _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);
                if (_animationBlend < 0.01f) _animationBlend = 0f;

                if (input.Move != Vector2.zero)
                {
                    _targetRotation = MovementMath.ComputeTargetYaw(input.Move, _mainCameraTransform.eulerAngles.y);
                    float rotation = Mathf.SmoothDampAngle(_transform.eulerAngles.y, _targetRotation,
                        ref _rotationVelocity, rotationSmoothTime);

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
            if (grounded)
            {
                _fallTimeoutDelta = fallTimeout;
                _freeFallTriggered = false;

                // stop vertical velocity from dropping infinitely while grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                if (input.Jump && _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity = MovementMath.JumpImpulse(jumpHeight, gravity);
                    Jumped?.Invoke();
                }

                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                _jumpTimeoutDelta = jumpTimeout;

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

            _verticalVelocity = MovementMath.StepGravity(_verticalVelocity, gravity, TerminalVelocity, Time.deltaTime);
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

            Gizmos.color = grounded ? transparentGreen : transparentRed;

            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z),
                groundedRadius);
        }
    }
}
