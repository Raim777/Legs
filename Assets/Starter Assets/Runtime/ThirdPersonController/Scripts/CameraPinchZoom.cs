using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace StarterAssets
{
    [RequireComponent(typeof(CinemachineThirdPersonFollow))]
    public class CameraPinchZoom : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private CinemachineThirdPersonFollow thirdPersonFollow;

        [Header("Whitelist UI")]
        [Tooltip("UIDocument owning the VisualElements that allow pinch to pass through. " +
                 "If no whitelist is configured, pinch works anywhere on screen.")]
        [SerializeField] private UIDocument uiDocument;

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

        private VisualElement[] _whitelist;
        private Rect[] _cachedRects;
        private EventCallback<GeometryChangedEvent>[] _geometryCallbacks;
        private IPanel _panel;

        private float _target;
        private float _velocity;
        private float _prevPinchDist = -1f;

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

        private void OnDisable()
        {
            ReleaseWhitelist();
        }

        private void Update()
        {
            if (thirdPersonFollow == null) return;

            EnsureWhitelist();

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
                int count = touches.Count;
                for (int i = 0; i < count && found < 2; i++)
                {
                    var t = touches[i];
                    if (!t.press.isPressed) continue;
                    Vector2 pos = t.position.ReadValue();
                    if (!IsWhitelisted(pos)) continue;
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

        private void EnsureWhitelist()
        {
            if (_whitelist != null) return;
            if (uiDocument == null || whitelistElementNames == null || whitelistElementNames.Length == 0) return;

            var root = uiDocument.rootVisualElement;
            if (root == null) return; // UIDocument not yet initialized; retry next frame.

            _panel = root.panel;
            int n = whitelistElementNames.Length;
            _whitelist = new VisualElement[n];
            _cachedRects = new Rect[n];
            _geometryCallbacks = new EventCallback<GeometryChangedEvent>[n];

            for (int i = 0; i < n; i++)
            {
                var el = root.Q<VisualElement>(whitelistElementNames[i]);
                _whitelist[i] = el;
                if (el == null) continue;

                _cachedRects[i] = el.worldBound;
                int captured = i;
                EventCallback<GeometryChangedEvent> cb = evt => _cachedRects[captured] = evt.newRect;
                _geometryCallbacks[i] = cb;
                el.RegisterCallback(cb);
            }
        }

        private void ReleaseWhitelist()
        {
            if (_whitelist == null) return;
            for (int i = 0; i < _whitelist.Length; i++)
            {
                if (_whitelist[i] != null && _geometryCallbacks[i] != null)
                    _whitelist[i].UnregisterCallback(_geometryCallbacks[i]);
            }
            _whitelist = null;
            _cachedRects = null;
            _geometryCallbacks = null;
            _panel = null;
        }

        private bool IsWhitelisted(Vector2 screenPos)
        {
            if (_whitelist == null || _whitelist.Length == 0) return true;
            if (_panel == null) return false;

            Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(
                _panel, new Vector2(screenPos.x, Screen.height - screenPos.y));

            var rects = _cachedRects;
            for (int i = 0; i < rects.Length; i++)
            {
                if (_whitelist[i] == null) continue;
                if (rects[i].Contains(panelPos)) return true;
            }
            return false;
        }
    }
}
