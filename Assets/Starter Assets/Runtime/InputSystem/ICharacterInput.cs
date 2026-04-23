using UnityEngine;

namespace StarterAssets
{
    public interface ICharacterInput
    {
        Vector2 Move { get; }
        Vector2 Look { get; }
        bool Sprint { get; }
        bool Crouch { get; }
        bool Jump { get; }
        bool AnalogMovement { get; }
        void ConsumeJump();
    }
}
