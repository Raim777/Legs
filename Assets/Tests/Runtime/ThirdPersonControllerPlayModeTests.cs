using System.Collections;
using System.Reflection;
using NUnit.Framework;
using StarterAssets;
using UnityEngine;
using UnityEngine.TestTools;

namespace StarterAssets.Tests
{
    public class ThirdPersonControllerPlayModeTests
    {
        private GameObject _floor;
        private GameObject _cameraGO;
        private GameObject _characterGO;

        [TearDown]
        public void TearDown()
        {
            if (_floor != null) Object.Destroy(_floor);
            if (_cameraGO != null) Object.Destroy(_cameraGO);
            if (_characterGO != null) Object.Destroy(_characterGO);
        }

        [UnityTest]
        public IEnumerator Character_MovesForwardThenLeft_WithScriptedInput()
        {
            BuildScene();
            var input = _characterGO.GetComponent<ScriptedCharacterInput>();

            // let Start() run on all components
            yield return null;

            Vector3 start = _characterGO.transform.position;

            input.Play(new[]
            {
                InputCommand.Forward(5f),
                InputCommand.Left(2f),
            });

            yield return RunFor(7.2f);

            Vector3 delta = _characterGO.transform.position - start;

            // MoveSpeed = 2 m/s with short ramp-up; forward 5s should push +Z well past 6m
            Assert.Greater(delta.z, 9.7f, $"Expected forward (+Z) travel > 9.7m, got {delta}");
            // left 2s should push -X past -2m
            Assert.Less(delta.x, -2f, $"Expected left (-X) travel < -2m, got {delta}");
        }

        [UnityTest]
        public IEnumerator Character_StaysStill_WhenInputIsIdle()
        {
            BuildScene();
            var input = _characterGO.GetComponent<ScriptedCharacterInput>();

            yield return null;

            Vector3 start = _characterGO.transform.position;

            input.Play(new[] { InputCommand.Idle(1.5f) });

            yield return RunFor(1.7f);

            Vector3 delta = _characterGO.transform.position - start;
            Assert.Less(Mathf.Abs(delta.x), 0.1f, $"No X drift expected, got {delta.x}");
            Assert.Less(Mathf.Abs(delta.z), 0.1f, $"No Z drift expected, got {delta.z}");
        }

        [UnityTest]
        public IEnumerator ScriptedInput_FiresFinished_AndStopsPlaying()
        {
            BuildScene();
            var input = _characterGO.GetComponent<ScriptedCharacterInput>();

            yield return null;

            bool finished = false;
            input.Finished += () => finished = true;
            input.Play(new[] { InputCommand.Forward(0.5f) });

            yield return RunFor(0.9f);

            Assert.IsTrue(finished, "Finished event should have fired");
            Assert.IsFalse(input.IsPlaying, "IsPlaying should be false after Stop mode finished");
        }

        [UnityTest]
        public IEnumerator ScriptedInput_LoopsWhenConfigured()
        {
            BuildScene();
            var input = _characterGO.GetComponent<ScriptedCharacterInput>();

            yield return null;

            input.Play(new[] { InputCommand.Forward(0.3f) }, ScriptedInputEndMode.Loop);

            yield return RunFor(1.2f);

            Assert.IsTrue(input.IsPlaying, "Loop mode should keep IsPlaying true");
        }

        private static IEnumerator RunFor(float seconds)
        {
            float t = 0f;
            while (t < seconds)
            {
                yield return null;
                t += Time.deltaTime;
            }
        }

        private void BuildScene()
        {
            _floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _floor.name = "TestFloor";
            _floor.transform.position = new Vector3(0f, -0.5f, 0f);
            _floor.transform.localScale = new Vector3(100f, 1f, 100f);

            _cameraGO = new GameObject("TestCamera");
            _cameraGO.transform.position = new Vector3(0f, 5f, -5f);
            var cam = _cameraGO.AddComponent<Camera>();

            _characterGO = new GameObject("TestCharacter");
            _characterGO.transform.position = new Vector3(0f, 1f, 0f);

            var cmTarget = new GameObject("CinemachineCameraTarget");
            cmTarget.transform.SetParent(_characterGO.transform);
            cmTarget.transform.localPosition = new Vector3(0f, 1.6f, 0f);

            var controller = _characterGO.AddComponent<CharacterController>();
            controller.radius = 0.28f;
            controller.height = 1.8f;
            controller.center = new Vector3(0f, 0.93f, 0f);

            var motor = _characterGO.AddComponent<CharacterMotor>();
            var cameraRig = _characterGO.AddComponent<CameraRig>();
            var tpc = _characterGO.AddComponent<ThirdPersonController>();
            var input = _characterGO.AddComponent<ScriptedCharacterInput>();

            tpc.moveSpeed = 2f;

            SetPrivateField(motor, "mainCamera", cam);
            SetPrivateField(motor, "groundLayers", (LayerMask)~0);
            SetPrivateField(cameraRig, "cinemachineCameraTarget", cmTarget);

            tpc.SetInput(input);
        }

        private static void SetPrivateField(object target, string name, object value)
        {
            var field = target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field, $"Field '{name}' not found on {target.GetType().Name}");
            field.SetValue(target, value);
        }
    }
}
