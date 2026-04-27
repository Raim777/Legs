using NUnit.Framework;
using UnityEngine;

namespace StarterAssets.Tests
{
    public class MovementMathTests
    {
        private const float Epsilon = 1e-4f;

        [Test]
        public void StepGravity_BelowTerminal_AccumulatesGravityOverTime()
        {
            float result = MovementMath.StepGravity(0f, -15f, 53f, 0.1f);
            Assert.AreEqual(-1.5f, result, Epsilon);
        }

        [Test]
        public void StepGravity_AtTerminal_DoesNotAccumulate()
        {
            float result = MovementMath.StepGravity(53f, -15f, 53f, 0.1f);
            Assert.AreEqual(53f, result, Epsilon);
        }

        [Test]
        public void StepGravity_AboveTerminal_DoesNotAccumulate()
        {
            float result = MovementMath.StepGravity(60f, -15f, 53f, 0.1f);
            Assert.AreEqual(60f, result, Epsilon);
        }

        [Test]
        public void JumpImpulse_ProducesVelocityToReachTargetHeight()
        {
            // h = v² / (2 * |g|)  →  for h=1.2, g=-15: v = sqrt(36) = 6
            float impulse = MovementMath.JumpImpulse(1.2f, -15f);
            Assert.AreEqual(6f, impulse, Epsilon);
        }

        [Test]
        public void ComputeTargetYaw_ForwardInput_NoCamera_ReturnsZero()
        {
            float yaw = MovementMath.ComputeTargetYaw(new Vector2(0f, 1f), 0f);
            Assert.AreEqual(0f, yaw, Epsilon);
        }

        [Test]
        public void ComputeTargetYaw_RightInput_NoCamera_Returns90()
        {
            float yaw = MovementMath.ComputeTargetYaw(new Vector2(1f, 0f), 0f);
            Assert.AreEqual(90f, yaw, Epsilon);
        }

        [Test]
        public void ComputeTargetYaw_BackInput_NoCamera_Returns180()
        {
            float yaw = MovementMath.ComputeTargetYaw(new Vector2(0f, -1f), 0f);
            Assert.AreEqual(180f, yaw, Epsilon);
        }

        [Test]
        public void ComputeTargetYaw_AddsCameraYaw()
        {
            float yaw = MovementMath.ComputeTargetYaw(new Vector2(0f, 1f), 45f);
            Assert.AreEqual(45f, yaw, Epsilon);
        }

        [Test]
        public void ClampAngle_WithinRange_PassesThrough()
        {
            Assert.AreEqual(45f, MovementMath.ClampAngle(45f, -90f, 90f), Epsilon);
        }

        [Test]
        public void ClampAngle_BelowMinusFullCircle_WrapsBeforeClamp()
        {
            // -370 + 360 = -10  →  clamp(-10, -90, 90) = -10
            Assert.AreEqual(-10f, MovementMath.ClampAngle(-370f, -90f, 90f), Epsilon);
        }

        [Test]
        public void ClampAngle_AboveFullCircle_WrapsBeforeClamp()
        {
            // 370 - 360 = 10  →  clamp(10, -90, 90) = 10
            Assert.AreEqual(10f, MovementMath.ClampAngle(370f, -90f, 90f), Epsilon);
        }

        [Test]
        public void ClampAngle_ClampsAboveMax()
        {
            Assert.AreEqual(90f, MovementMath.ClampAngle(120f, -90f, 90f), Epsilon);
        }

        [Test]
        public void ClampAngle_ClampsBelowMin()
        {
            Assert.AreEqual(-90f, MovementMath.ClampAngle(-120f, -90f, 90f), Epsilon);
        }

        [Test]
        public void StepHorizontalSpeed_WithinDeadband_SnapsToTarget()
        {
            float result = MovementMath.StepHorizontalSpeed(
                currentSpeed: 1.95f,
                targetSpeed: 2f,
                inputMagnitude: 1f,
                speedOffset: 0.1f,
                speedChangeRate: 10f,
                deltaTime: 0.016f);
            Assert.AreEqual(2f, result, Epsilon);
        }

        [Test]
        public void StepHorizontalSpeed_OutsideDeadband_LerpsTowardTarget()
        {
            // current=0, target=2, mag=1, dt*rate = 0.16  →  Lerp(0, 2, 0.16) = 0.32
            float result = MovementMath.StepHorizontalSpeed(
                currentSpeed: 0f,
                targetSpeed: 2f,
                inputMagnitude: 1f,
                speedOffset: 0.1f,
                speedChangeRate: 10f,
                deltaTime: 0.016f);
            Assert.AreEqual(0.32f, result, Epsilon);
        }

        [Test]
        public void StepHorizontalSpeed_InputMagnitudeScalesTarget()
        {
            // analog stick at half-tilt: targetSpeed * 0.5 is the effective target inside Lerp
            float result = MovementMath.StepHorizontalSpeed(
                currentSpeed: 0f,
                targetSpeed: 2f,
                inputMagnitude: 0.5f,
                speedOffset: 0.1f,
                speedChangeRate: 10f,
                deltaTime: 0.016f);
            // Lerp(0, 2 * 0.5, 0.16) = Lerp(0, 1, 0.16) = 0.16
            Assert.AreEqual(0.16f, result, Epsilon);
        }
    }
}
