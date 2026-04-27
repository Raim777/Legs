using UnityEngine;
using UnityEngine.Serialization;

namespace Character
{
    [RequireComponent(typeof(CharacterMotor), typeof(CameraRig))]
    public class ThirdPersonController : MonoBehaviour
    {
        [SerializeField] private CharacterMotor motor;
        [SerializeField] private CameraRig cameraRig;

        public float moveSpeed
        {
            get => motor.moveSpeed;
            set => motor.moveSpeed = value;
        }

        public void SetInput(InputProvider input)
        {
            motor.SetInput(input);
            cameraRig.SetInput(input);
        }

        public void SetAirControl(bool value)
        {
            motor.SetAirControl(value);
        }
    }
}
