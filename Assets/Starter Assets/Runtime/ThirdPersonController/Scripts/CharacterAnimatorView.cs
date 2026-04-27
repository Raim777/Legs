using UnityEngine;

namespace StarterAssets
{
    [RequireComponent(typeof(Animator))]
    public class CharacterAnimatorView : MonoBehaviour
    {
        [Header("Motor")]
        [SerializeField] private CharacterMotor motor;

        [Header("Footstep Audio")]
        [SerializeField] private AudioClip landingAudioClip;
        [SerializeField] private AudioClip[] footstepAudioClips;
        [Range(0, 1)] [SerializeField] private float footstepAudioVolume = 0.5f;
        [SerializeField] private AudioSource[] audioPool;

        private Animator _animator;
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDCrouch;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

        // cached animator params to avoid redundant native SetBool every frame
        private bool _animGrounded;
        private bool _animJump;
        private bool _animFreeFall;
        private bool _animCrouch;

        private int _audioPoolIndex;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            AssignAnimationIDs();

            _animGrounded = _animator.GetBool(_animIDGrounded);
            _animJump = _animator.GetBool(_animIDJump);
            _animFreeFall = _animator.GetBool(_animIDFreeFall);
            _animCrouch = _animator.GetBool(_animIDCrouch);
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
            _animator.SetFloat(_animIDSpeed, motor.AnimationBlend);
            _animator.SetFloat(_animIDMotionSpeed, motor.InputMagnitude);
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDCrouch = Animator.StringToHash("Crouch");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void OnGroundedChanged(bool isGrounded)
        {
            SetAnimBool(_animIDGrounded, ref _animGrounded, isGrounded);
            if (isGrounded)
            {
                SetAnimBool(_animIDJump, ref _animJump, false);
                SetAnimBool(_animIDFreeFall, ref _animFreeFall, false);
            }
        }

        private void OnJumped()
        {
            SetAnimBool(_animIDJump, ref _animJump, true);
        }

        private void OnFreeFallStarted()
        {
            SetAnimBool(_animIDFreeFall, ref _animFreeFall, true);
        }

        private void OnCrouchChanged(bool isCrouching)
        {
            SetAnimBool(_animIDCrouch, ref _animCrouch, isCrouching);
        }

        private void SetAnimBool(int id, ref bool current, bool value)
        {
            if (current == value) return;
            current = value;
            _animator.SetBool(id, value);
        }

        private void PlayPooledClip(AudioClip clip, float volume)
        {
            if (clip == null) return;
            AudioSource src = audioPool[_audioPoolIndex];
            _audioPoolIndex = (_audioPoolIndex + 1) % audioPool.Length;
            src.clip = clip;
            src.volume = volume;
            src.Play();
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (!(animationEvent.animatorClipInfo.weight > 0.5f)) return;
            if (footstepAudioClips.Length <= 0) return;

            int index = Random.Range(0, footstepAudioClips.Length);
            PlayPooledClip(footstepAudioClips[index], footstepAudioVolume);
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                PlayPooledClip(landingAudioClip, footstepAudioVolume);
            }
        }
    }
}
