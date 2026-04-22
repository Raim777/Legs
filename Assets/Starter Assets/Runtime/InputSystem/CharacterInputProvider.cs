using UnityEngine;

namespace StarterAssets
{
    public abstract class CharacterInputProvider : MonoBehaviour, ICharacterInput
    {
        public abstract Vector2 Move { get; }
        public abstract Vector2 Look { get; }
        public abstract bool Sprint { get; }
        public abstract bool Jump { get; }
        public abstract bool AnalogMovement { get; }
        public abstract void ConsumeJump();
    }
}
