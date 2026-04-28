using UnityEngine;

namespace Character
{
    public class CameraRig : MonoBehaviour
    {
        [SerializeField] private InputProvider input;
        [SerializeField] private GameObject cinemachineCameraTarget;
        [SerializeField] private float topClamp = 70.0f;
        [SerializeField] private float bottomClamp = -30.0f;
        [SerializeField] private float cameraAngleOverride = 0.0f;
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

        public void SetInput(InputProvider value)
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
