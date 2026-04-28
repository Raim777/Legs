using Character;
using Input;
using UnityEngine;
using UnityEngine.UI;

namespace Character
{
    public class TouchscreenInput : MonoBehaviour
    {
        [Header("Settings")] 
        [SerializeField, Range(-1, 1)] private float moveMagnitudeMultiplier = 1.0f;
        [SerializeField, Range(-1, 1)] private float lookMagnitudeMultiplier = 1.0f;
        [SerializeField] private bool invertLookY;
        [SerializeField] private VirtualJoystick moveJoystick;
        [SerializeField] private TouchPad lookPad;
        [SerializeField] private Button jumpButton;
        [SerializeField] private PlayerInputProvider inputProvider;

        [Header("Movement")] 
        [SerializeField] private float threshold = 0.06f;
        [SerializeField] private float sprintThreshold = 0.6f;
        [SerializeField] private float sprintAmplification = 0.5f;
        
        private bool _sprinting = false;

        private void Start()
        {
            moveJoystick.OnInput += Moved;
            lookPad.OnInput += Looked;
            jumpButton.onClick.AddListener(Jump);
        }

        private void OnDestroy()
        {
            if (moveJoystick)
                moveJoystick.OnInput -= Moved;
            
            if (lookPad)
                lookPad.OnInput -= Looked;
            
            if (jumpButton)
                jumpButton.onClick.RemoveListener(Jump);
        }

        private void Moved(Vector2 newDirection)
        {
            float moveMagnitude = newDirection.magnitude;

            if (moveMagnitude < threshold)
            {
                inputProvider.MoveInput(Vector2.zero);
                return;
            }
                
            SetSprinting(moveMagnitude > sprintThreshold);
                
            // Normalize and modify input when sprinting
            newDirection /= moveMagnitude;
                
            if (_sprinting)
            {
                newDirection *= Mathf.Lerp(sprintAmplification, 1.0f, (moveMagnitude - sprintThreshold) / (1.0f - sprintThreshold));
            }
            else
            {
                newDirection *= moveMagnitude / sprintThreshold;
            }
                
            inputProvider.MoveInput(newDirection * moveMagnitudeMultiplier);
        }

        private void Looked(Vector2 deltaLook)
        {
            if (invertLookY)
                deltaLook.y *= -1;

            inputProvider.LookInput(deltaLook * lookMagnitudeMultiplier);
        }
        
        private void SetSprinting(bool value)
        {
            if (_sprinting == value)
                return;
            
            _sprinting = value;
            
            inputProvider.SprintInput(_sprinting);
        }

        private void Jump()
        {
            inputProvider.JumpInput(true);
        }
    }
}
