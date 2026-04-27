using UnityEngine;

namespace StarterAssets
{
    public class CameraRig : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private CharacterInputProvider input;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        [SerializeField] private GameObject cinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        [SerializeField] private float topClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        [SerializeField] private float bottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        [SerializeField] private float cameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        [SerializeField] private bool lockCameraPosition = false;

        private const float Threshold = 0.01f;

        private Transform _cinemachineTargetTransform;
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        private void Start()
        {
            _cinemachineTargetTransform = cinemachineCameraTarget.transform;
            _cinemachineTargetYaw = _cinemachineTargetTransform.rotation.eulerAngles.y;
        }

        public void SetInput(CharacterInputProvider value)
        {
            input = value;
        }

        private void LateUpdate()
        {
            // mouse input is delta per event, NOT scaled by Time.deltaTime
            if (input.Look.sqrMagnitude >= Threshold && !lockCameraPosition)
            {
                _cinemachineTargetYaw += input.Look.x;
                _cinemachineTargetPitch += input.Look.y;
            }

            _cinemachineTargetYaw = MovementMath.ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = MovementMath.ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);

            _cinemachineTargetTransform.rotation = Quaternion.Euler(_cinemachineTargetPitch + cameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }
    }
}
