using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Character.Tests
{
    public class ThirdPersonControllerPlayModeTests
    {
        private const string TestScenePrefabPath = "Assets/Tests/Editor/ControllerTest.prefab";

        private GameObject _root;
        private ScriptedCharacterInput _input;
        private Transform _playerTransform;

        [TearDown]
        public void TearDown()
        {
            if (_root != null) Object.Destroy(_root);
        }

        [UnityTest]
        public IEnumerator Character_MovesForwardThenLeft_WithScriptedInput()
        {
            BuildScene();

            // let Start() run on all components
            yield return null;

            Vector3 start = _playerTransform.position;

            _input.Play(new[]
            {
                InputCommand.Forward(5f),
                InputCommand.Left(2f),
            });

            yield return RunFor(7.2f);

            Vector3 delta = _playerTransform.position - start;

            // MoveSpeed = 2 m/s with short ramp-up; forward 5s should push +Z well past 6m
            Assert.Greater(delta.z, 9.7f, $"Expected forward (+Z) travel > 9.7m, got {delta}");
            // left 2s should push -X past -2m
            Assert.Less(delta.x, -2f, $"Expected left (-X) travel < -2m, got {delta}");
        }

        [UnityTest]
        public IEnumerator Character_StaysStill_WhenInputIsIdle()
        {
            BuildScene();

            yield return null;

            Vector3 start = _playerTransform.position;

            _input.Play(new[] { InputCommand.Idle(1.5f) });

            yield return RunFor(1.7f);

            Vector3 delta = _playerTransform.position - start;
            Assert.Less(Mathf.Abs(delta.x), 0.1f, $"No X drift expected, got {delta.x}");
            Assert.Less(Mathf.Abs(delta.z), 0.1f, $"No Z drift expected, got {delta.z}");
        }

        [UnityTest]
        public IEnumerator ScriptedInput_FiresFinished_AndStopsPlaying()
        {
            BuildScene();

            yield return null;

            bool finished = false;
            _input.Finished += () => finished = true;
            _input.Play(new[] { InputCommand.Forward(0.5f) });

            yield return RunFor(0.9f);

            Assert.IsTrue(finished, "Finished event should have fired");
            Assert.IsFalse(_input.IsPlaying, "IsPlaying should be false after Stop mode finished");
        }

        [UnityTest]
        public IEnumerator ScriptedInput_LoopsWhenConfigured()
        {
            BuildScene();

            yield return null;

            _input.Play(new[] { InputCommand.Forward(0.3f) }, ScriptedInputEndMode.Loop);

            yield return RunFor(1.2f);

            Assert.IsTrue(_input.IsPlaying, "Loop mode should keep IsPlaying true");
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
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TestScenePrefabPath);
            Assert.NotNull(prefab, $"Test scene prefab not found at {TestScenePrefabPath}");

            _root = Object.Instantiate(prefab);
            _input = _root.GetComponentInChildren<ScriptedCharacterInput>();
            _playerTransform = _root.GetComponentInChildren<CharacterMotor>().transform;
        }
    }
}
