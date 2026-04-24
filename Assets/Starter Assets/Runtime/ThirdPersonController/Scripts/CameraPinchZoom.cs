using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UIElements;

namespace StarterAssets
{
    public class CameraPinchZoom : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Target")]
        [SerializeField] private CinemachineThirdPersonFollow thirdPersonFollow;

        [Tooltip("Names of VisualElements (resolved via Q<VisualElement>) through which pinch is allowed.")]
        [SerializeField] private string[] whitelistElementNames;

        [Header("Zoom Range")]
        [SerializeField] private float minDistance = 2f;
        [SerializeField] private float maxDistance = 8f;

        [Header("Input Sensitivity")]
        [Tooltip("World-units of zoom per pixel of pinch-distance change.")]
        [SerializeField] private float pinchSensitivity = 0.01f;

        [Tooltip("Pinch deltas below this threshold (pixels per frame) are ignored.")]
        [SerializeField] private float pinchDeadzone = 2f;

        [Tooltip("World-units of zoom per mouse scroll unit. Desktop fallback only.")]
        [SerializeField] private float scrollSensitivity = 0.5f;

        [Header("Smoothing")]
        [SerializeField] private float smoothTime = 0.12f;

        private Rect[] _cachedRects;
        private EventCallback<GeometryChangedEvent>[] _geometryCallbacks;
        private IPanel _panel;

        private float _target;
        private float _velocity;
        private float _prevPinchDist = -1f;
        private HashSet<int> _validTouches = new();

        private void Reset()
        {
            thirdPersonFollow = GetComponent<CinemachineThirdPersonFollow>();
        }

        private void Awake()
        {
            if (thirdPersonFollow == null)
                thirdPersonFollow = GetComponent<CinemachineThirdPersonFollow>();
            if (thirdPersonFollow != null)
                _target = Mathf.Clamp(thirdPersonFollow.CameraDistance, minDistance, maxDistance);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData is ExtendedPointerEventData extendedEventData
                && extendedEventData.touchId != 0)
            {
                _validTouches.Add(extendedEventData.touchId);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData is ExtendedPointerEventData extendedEventData
                && extendedEventData.touchId != 0)
            {
                _validTouches.Remove(extendedEventData.touchId);
            }
        }

        private void Update()
        {
            if (thirdPersonFollow == null) return;

            var mouse = Mouse.current;
            if (mouse != null)
            {
                float scrollY = mouse.scroll.ReadValue().y;
                if (scrollY != 0f)
                    _target -= scrollY * scrollSensitivity;
            }

            var ts = Touchscreen.current;
            if (ts != null)
            {
                Vector2 a = default, b = default;
                int found = 0;
                var touches = ts.touches;
                for(int i = 0;  i < touches.Count; i++)
                {
                    var t = touches[i];

                    if (!_validTouches.Contains(t.touchId.value))
                        continue;
                    
                    if (!t.press.isPressed) continue;
                    Vector2 pos = t.position.ReadValue();
                    if (found == 0) a = pos; else b = pos;
                    found++;
                }

                if (found == 2)
                {
                    float dist = Vector2.Distance(a, b);
                    if (_prevPinchDist >= 0f)
                    {
                        float delta = dist - _prevPinchDist;
                        if (delta > pinchDeadzone || delta < -pinchDeadzone)
                            _target -= delta * pinchSensitivity;
                    }
                    _prevPinchDist = dist;
                }
                else
                {
                    _prevPinchDist = -1f;
                }
            }

            _target = Mathf.Clamp(_target, minDistance, maxDistance);
            thirdPersonFollow.CameraDistance = Mathf.SmoothDamp(
                thirdPersonFollow.CameraDistance, _target, ref _velocity, smoothTime);
        }
    }
}
