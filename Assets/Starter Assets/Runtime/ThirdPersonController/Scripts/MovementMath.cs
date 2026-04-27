using System.Runtime.CompilerServices;
using UnityEngine;

namespace Character
{
    public static class MovementMath
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float StepGravity(float verticalVelocity, float gravity, float terminalVelocity, float deltaTime)
        {
            if (verticalVelocity < terminalVelocity)
            {
                return verticalVelocity + gravity * deltaTime;
            }
            return verticalVelocity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float JumpImpulse(float jumpHeight, float gravity)
        {
            return Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ComputeTargetYaw(Vector2 moveInput, float cameraYawDegrees)
        {
            Vector3 dir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            return Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + cameraYawDegrees;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360f) angle += 360f;
            if (angle > 360f) angle -= 360f;
            return Mathf.Clamp(angle, min, max);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float StepHorizontalSpeed(
            float currentSpeed,
            float targetSpeed,
            float inputMagnitude,
            float speedOffset,
            float speedChangeRate,
            float deltaTime)
        {
            if (currentSpeed < targetSpeed - speedOffset || currentSpeed > targetSpeed + speedOffset)
            {
                return Mathf.Lerp(currentSpeed, targetSpeed * inputMagnitude, deltaTime * speedChangeRate);
            }
            return targetSpeed;
        }
    }
}
