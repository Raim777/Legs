using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class TouchscreenInput : MonoBehaviour
{
    [Header("Settings")] 
    [Tooltip("Move joystick magnitude is in [-1;1] range, this multiply it before sending it to move event")]
    [SerializeField] private float MoveMagnitudeMultiplier = 1.0f;
    [Tooltip("Look joystick magnitude is in [-1;1] range, this multiply it before sending it to move event")]
    [SerializeField] private float LookMagnitudeMultiplier = 1.0f;
    [SerializeField] private bool InvertLookY;
    
    [Header("Events")]
    [SerializeField] private UnityEvent<Vector2> MoveEvent;
    [SerializeField] private UnityEvent<Vector2> LookEvent;
    [SerializeField] private UnityEvent<bool> JumpEvent;
    [SerializeField] private UnityEvent<bool> SprintEvent;
    
    private UIDocument _mDocument;

    private VirtualJoystick _mMoveJoystick;
    private VirtualJoystick _mLookJoystick;

    private void Awake()
    {
        _mDocument = GetComponent<UIDocument>();

        var safeArea = Screen.safeArea;

        var root = _mDocument.rootVisualElement;

        root.style.position = Position.Absolute;
        root.style.left = safeArea.xMin;
        root.style.right = Screen.width - safeArea.xMax;
        root.style.top = Screen.height - safeArea.yMax;
        root.style.bottom = safeArea.yMin;
    }

    private void Start()
    {
        var joystickMove = _mDocument.rootVisualElement.Q<VisualElement>("JoystickMove");
        var joystickLook = _mDocument.rootVisualElement.Q<VisualElement>("JoystickLook");
        
        _mMoveJoystick = new VirtualJoystick(joystickMove);
        _mMoveJoystick.JoystickEvent.AddListener(mov =>
        {
            MoveEvent.Invoke(mov * MoveMagnitudeMultiplier);
        });;
        
        _mLookJoystick = new VirtualJoystick(joystickLook);
        _mLookJoystick.JoystickEvent.AddListener(mov =>
        {
            if (InvertLookY)
                mov.y *= -1;

            LookEvent.Invoke(mov * LookMagnitudeMultiplier);
        });

        var jumpButton = _mDocument.rootVisualElement.Q<VisualElement>("ButtonJump");
        jumpButton.RegisterCallback<PointerEnterEvent>(evt => { JumpEvent.Invoke(true); });
        jumpButton.RegisterCallback<PointerLeaveEvent>(evt => { JumpEvent.Invoke(false); });
        
        var sprintButton = _mDocument.rootVisualElement.Q<VisualElement>("ButtonSprint");
        sprintButton.RegisterCallback<PointerEnterEvent>(evt => { SprintEvent.Invoke(true); });
        sprintButton.RegisterCallback<PointerLeaveEvent>(evt => { SprintEvent.Invoke(false); });
    }
}
public class VirtualJoystick
{
    private readonly VisualElement _baseElement;
    private readonly VisualElement _thumbstick;

    public readonly UnityEvent<Vector2> JoystickEvent = new();

    public VirtualJoystick(VisualElement root)
    {
        _baseElement = root;
        _thumbstick = root.Q<VisualElement>("JoystickHandle");
            
        _baseElement.RegisterCallback<PointerDownEvent>(HandlePress);
        _baseElement.RegisterCallback<PointerMoveEvent>(HandleDrag);
        _baseElement.RegisterCallback<PointerUpEvent>(HandleRelease);
    }

    private void HandlePress(PointerDownEvent evt)
    {
        _baseElement.CapturePointer(evt.pointerId);
    }

    private void HandleRelease(PointerUpEvent evt)
    {
        _baseElement.ReleasePointer(evt.pointerId);
            
        _thumbstick.style.left = Length.Percent(50);
        _thumbstick.style.top = Length.Percent(50);
        
        JoystickEvent.Invoke(Vector2.zero);
    }

    private void HandleDrag(PointerMoveEvent evt)
    {
        if (!_baseElement.HasPointerCapture(evt.pointerId)) return;
            
        var width = _baseElement.contentRect.width;
        var center = new Vector3(width / 2, width / 2);
        var centerToPosition = evt.localPosition - center;

        if (centerToPosition.magnitude > width/2)
        {
            centerToPosition = centerToPosition.normalized * width / 2;
        }

        var newPos = center + centerToPosition;

        _thumbstick.style.left = newPos.x;
        _thumbstick.style.top = newPos.y;

        centerToPosition /= (width / 2);
        //we invert y as the y of UI goes down, but pushing the joystick up is expected to give a positive y value
        centerToPosition.y *= -1;

        JoystickEvent.Invoke(centerToPosition);
    }
}