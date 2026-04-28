using UnityEngine;

namespace Character
{
    [CreateAssetMenu(fileName = "Character", menuName = "Character/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 2.0f;
        [SerializeField] private float sprintSpeed = 5.335f;
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -15.0f;
        [Tooltip("Время требующееся персонажу чтобы повернуться")]
        [SerializeField] private float turnTime = 0.12f;
        [SerializeField] private float jumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        [SerializeField] private float fallTimeout = 0.15f;
        [SerializeField] private float accelerationRate = 10.0f;

        [Tooltip("If true, player can change direction and speed while airborne")]
        [SerializeField] private bool airControl = true;
        
        public float MoveSpeed => moveSpeed;
        public float SprintSpeed => sprintSpeed;
        public float JumpHeight => jumpHeight;
        public float Gravity => gravity;
        public float TurnTime => turnTime;
        public float JumpTimeout => jumpTimeout;
        public float FallTimeout => fallTimeout;
        public float AccelerationRate => accelerationRate;
        public bool AirControl => airControl;
    }
}