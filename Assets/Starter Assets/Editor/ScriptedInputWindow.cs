using StarterAssets;
using UnityEditor;
using UnityEngine;

namespace StarterAssetsEditor
{
    public class ScriptedInputWindow : EditorWindow
    {
        private enum Direction
        {
            Forward,
            Back,
            Left,
            Right,
            Idle,
            Custom,
        }

        [SerializeField] private ScriptedCharacterInput target;
        private SerializedObject _targetSO;
        private SerializedProperty _commandsProp;
        private SerializedProperty _endModeProp;
        private SerializedProperty _playOnStartProp;
        private Vector2 _scroll;

        [MenuItem("Tools/Starter Assets/Scripted Input")]
        public static void Open()
        {
            GetWindow<ScriptedInputWindow>("Scripted Input").Show();
        }

        private void OnEnable()
        {
            TryBindTarget(target);
        }

        private void OnSelectionChange()
        {
            if (Selection.activeGameObject == null) return;
            var found = Selection.activeGameObject.GetComponent<ScriptedCharacterInput>();
            if (found != null)
            {
                target = found;
                TryBindTarget(target);
                Repaint();
            }
        }

        private void TryBindTarget(ScriptedCharacterInput newTarget)
        {
            target = newTarget;
            _targetSO = null;
            _commandsProp = null;
            _endModeProp = null;
            _playOnStartProp = null;
            if (target == null) return;

            _targetSO = new SerializedObject(target);
            _commandsProp = _targetSO.FindProperty("commands");
            _endModeProp = _targetSO.FindProperty("endMode");
            _playOnStartProp = _targetSO.FindProperty("playOnStart");
        }

        private void OnGUI()
        {
            DrawTargetField();

            if (target == null)
            {
                EditorGUILayout.HelpBox(
                    "Select a GameObject with a ScriptedCharacterInput component, or drag one into the field above.",
                    MessageType.Info);
                return;
            }

            _targetSO.Update();

            EditorGUILayout.PropertyField(_playOnStartProp);
            EditorGUILayout.PropertyField(_endModeProp);

            EditorGUILayout.Space();
            DrawControls();

            EditorGUILayout.Space();
            DrawCommandList();

            EditorGUILayout.Space();
            DrawPlaybackStatus();

            _targetSO.ApplyModifiedProperties();
        }

        private void DrawTargetField()
        {
            EditorGUI.BeginChangeCheck();
            var picked = (ScriptedCharacterInput)EditorGUILayout.ObjectField(
                "Target", target, typeof(ScriptedCharacterInput), allowSceneObjects: true);
            if (EditorGUI.EndChangeCheck())
            {
                TryBindTarget(picked);
            }
        }

        private void DrawControls()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Forward 5s")) AddPreset(Direction.Forward, 5f);
                if (GUILayout.Button("Add Left 2s")) AddPreset(Direction.Left, 2f);
                if (GUILayout.Button("Add Right 2s")) AddPreset(Direction.Right, 2f);
                if (GUILayout.Button("Add Back 2s")) AddPreset(Direction.Back, 2f);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Command")) AddPreset(Direction.Custom, 1f);
                if (GUILayout.Button("Clear All"))
                {
                    Undo.RecordObject(target, "Clear Scripted Input");
                    _commandsProp.ClearArray();
                }

                using (new EditorGUI.DisabledScope(!Application.isPlaying))
                {
                    if (GUILayout.Button("▶ Play"))
                    {
                        target.Play();
                    }
                    if (GUILayout.Button("■ Stop"))
                    {
                        target.Stop();
                    }
                }
            }
        }

        private void AddPreset(Direction dir, float duration)
        {
            _commandsProp.arraySize++;
            var element = _commandsProp.GetArrayElementAtIndex(_commandsProp.arraySize - 1);
            element.FindPropertyRelative("move").vector2Value = DirectionToVector(dir);
            element.FindPropertyRelative("duration").floatValue = duration;
            element.FindPropertyRelative("sprint").boolValue = false;
            element.FindPropertyRelative("jump").boolValue = false;
        }

        private static Vector2 DirectionToVector(Direction dir) => dir switch
        {
            Direction.Forward => Vector2.up,
            Direction.Back => Vector2.down,
            Direction.Left => Vector2.left,
            Direction.Right => Vector2.right,
            Direction.Idle => Vector2.zero,
            _ => Vector2.up,
        };

        private void DrawCommandList()
        {
            EditorGUILayout.LabelField($"Commands ({_commandsProp.arraySize})", EditorStyles.boldLabel);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            for (int i = 0; i < _commandsProp.arraySize; i++)
            {
                DrawCommandRow(i);
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawCommandRow(int i)
        {
            var element = _commandsProp.GetArrayElementAtIndex(i);
            var moveProp = element.FindPropertyRelative("move");
            var durProp = element.FindPropertyRelative("duration");
            var sprintProp = element.FindPropertyRelative("sprint");
            var jumpProp = element.FindPropertyRelative("jump");

            using (new EditorGUILayout.VerticalScope("box"))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"#{i}", GUILayout.Width(28));

                    Direction dir = VectorToDirection(moveProp.vector2Value);
                    Direction newDir = (Direction)EditorGUILayout.EnumPopup(dir, GUILayout.Width(90));
                    if (newDir != dir && newDir != Direction.Custom)
                    {
                        moveProp.vector2Value = DirectionToVector(newDir);
                    }

                    EditorGUILayout.LabelField("dur", GUILayout.Width(28));
                    durProp.floatValue = Mathf.Max(0f, EditorGUILayout.FloatField(durProp.floatValue, GUILayout.Width(60)));

                    sprintProp.boolValue = GUILayout.Toggle(sprintProp.boolValue, "sprint", GUILayout.Width(60));
                    jumpProp.boolValue = GUILayout.Toggle(jumpProp.boolValue, "jump", GUILayout.Width(50));

                    GUILayout.FlexibleSpace();

                    using (new EditorGUI.DisabledScope(i == 0))
                    {
                        if (GUILayout.Button("▲", GUILayout.Width(24))) _commandsProp.MoveArrayElement(i, i - 1);
                    }
                    using (new EditorGUI.DisabledScope(i == _commandsProp.arraySize - 1))
                    {
                        if (GUILayout.Button("▼", GUILayout.Width(24))) _commandsProp.MoveArrayElement(i, i + 1);
                    }
                    if (GUILayout.Button("✕", GUILayout.Width(24)))
                    {
                        _commandsProp.DeleteArrayElementAtIndex(i);
                        return;
                    }
                }

                if (VectorToDirection(moveProp.vector2Value) == Direction.Custom)
                {
                    moveProp.vector2Value = EditorGUILayout.Vector2Field("move", moveProp.vector2Value);
                }
            }
        }

        private static Direction VectorToDirection(Vector2 v)
        {
            if (v == Vector2.up) return Direction.Forward;
            if (v == Vector2.down) return Direction.Back;
            if (v == Vector2.left) return Direction.Left;
            if (v == Vector2.right) return Direction.Right;
            if (v == Vector2.zero) return Direction.Idle;
            return Direction.Custom;
        }

        private void DrawPlaybackStatus()
        {
            if (!Application.isPlaying) return;

            EditorGUILayout.LabelField("Playback", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Playing", target.IsPlaying.ToString());
            if (target.IsPlaying && target.Commands.Count > 0)
            {
                EditorGUILayout.LabelField("Index", $"{target.CurrentIndex} / {target.Commands.Count - 1}");
                var cur = target.Commands[target.CurrentIndex];
                float t = cur.duration > 0 ? target.CurrentElapsed / cur.duration : 1f;
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(false, 18), t,
                    $"{target.CurrentElapsed:F2} / {cur.duration:F2}");
                Repaint();
            }
        }
    }
}
