using UnityEngine;

namespace Character
{
    [RequireComponent(typeof(CharacterMotor), typeof(CameraRig))]
    public class ThirdPersonController : MonoBehaviour
    {
        private CharacterMotor _motor;
        private CameraRig _cameraRig;

        public float moveSpeed
        {
            get => _motor.moveSpeed;
            set => _motor.moveSpeed = value;
        }

        private void Awake()
        {
            _motor = GetComponent<CharacterMotor>();
            _cameraRig = GetComponent<CameraRig>();
        }

        public void SetInput(InputProvider input)
        {
            _motor.SetInput(input);
            _cameraRig.SetInput(input);
        }

        public void SetAirControl(bool value)
        {
            _motor.SetAirControl(value);
        }
    }
}
