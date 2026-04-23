using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarterAssets
{
    public enum ScriptedInputEndMode
    {
        Stop,
        Loop,
        PingPong,
    }

    [Serializable]
    public struct InputCommand
    {
        public Vector2 move;
        public bool sprint;
        public bool jump;
        [Min(0f)] public float duration;

        public static InputCommand Forward(float duration, bool sprint = false) =>
            new InputCommand { move = Vector2.up, duration = duration, sprint = sprint };

        public static InputCommand Back(float duration, bool sprint = false) =>
            new InputCommand { move = Vector2.down, duration = duration, sprint = sprint };

        public static InputCommand Left(float duration, bool sprint = false) =>
            new InputCommand { move = Vector2.left, duration = duration, sprint = sprint };

        public static InputCommand Right(float duration, bool sprint = false) =>
            new InputCommand { move = Vector2.right, duration = duration, sprint = sprint };

        public static InputCommand Idle(float duration) =>
            new InputCommand { move = Vector2.zero, duration = duration };

        public static InputCommand Jump(float duration) =>
            new InputCommand { move = Vector2.zero, duration = duration, jump = true };
    }

    public class ScriptedCharacterInput : CharacterInputProvider
    {
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private ScriptedInputEndMode endMode = ScriptedInputEndMode.Stop;
        [SerializeField] private bool analogMovement;
        [SerializeField] private List<InputCommand> commands = new List<InputCommand>();

        private Vector2 _move;
        private bool _jump;
        private bool _sprint;
        private int _index;
        private float _elapsed;
        private int _direction = 1;
        private bool _isPlaying;

        public override Vector2 Move => _move;
        public override Vector2 Look => Vector2.zero;
        public override bool Sprint => _sprint;
        public override bool Jump => _jump;
        public override bool AnalogMovement => analogMovement;

        public bool IsPlaying => _isPlaying;
        public int CurrentIndex => _index;
        public float CurrentElapsed => _elapsed;
        public IReadOnlyList<InputCommand> Commands => commands;

        public event Action Finished;

        public override void ConsumeJump()
        {
            _jump = false;
        }

        private void Start()
        {
            if (playOnStart && commands.Count > 0)
            {
                Play();
            }
        }

        public void Play()
        {
            _index = 0;
            _elapsed = 0f;
            _direction = 1;
            _isPlaying = commands.Count > 0;
            ApplyCurrent();
        }

        public void Play(IList<InputCommand> sequence, ScriptedInputEndMode mode = ScriptedInputEndMode.Stop)
        {
            commands.Clear();
            for (int i = 0; i < sequence.Count; i++)
            {
                commands.Add(sequence[i]);
            }
            endMode = mode;
            Play();
        }

        public void Stop()
        {
            _isPlaying = false;
            _move = Vector2.zero;
            _jump = false;
            _sprint = false;
        }

        private void Update()
        {
            if (!_isPlaying) return;

            _elapsed += Time.deltaTime;

            InputCommand current = commands[_index];
            if (_elapsed < current.duration) return;

            AdvanceIndex();
        }

        private void AdvanceIndex()
        {
            _elapsed = 0f;
            int next = _index + _direction;

            if (next >= 0 && next < commands.Count)
            {
                _index = next;
                ApplyCurrent();
                return;
            }

            switch (endMode)
            {
                case ScriptedInputEndMode.Loop:
                    _index = 0;
                    ApplyCurrent();
                    break;
                case ScriptedInputEndMode.PingPong:
                    _direction = -_direction;
                    _index = Mathf.Clamp(_index + _direction, 0, commands.Count - 1);
                    ApplyCurrent();
                    break;
                default:
                    Stop();
                    Finished?.Invoke();
                    break;
            }
        }

        private void ApplyCurrent()
        {
            if (_index < 0 || _index >= commands.Count)
            {
                Stop();
                return;
            }

            InputCommand c = commands[_index];
            _move = c.move;
            _sprint = c.sprint;
            _jump = c.jump;
        }
    }
}
