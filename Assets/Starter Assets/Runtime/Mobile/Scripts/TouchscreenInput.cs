using System;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public class TouchscreenInput : MonoBehaviour
{
    [Header("Settings")] 
    [Tooltip("Move joystick magnitude is in [-1;1] range, this multiply it before sending it to move event")]
    [SerializeField] private float MoveMagnitudeMultiplier = 1.0f;
    [Tooltip("Look joystick magnitude is in [-1;1] range, this multiply it before sending it to move event")]
    [SerializeField] private float LookMagnitudeMultiplier = 1.0f;
    [SerializeField] private bool InvertLookY;
    [SerializeField] private VirtualJoystick moveJoystick;
    [SerializeField] private TouchPad lookPad;
    
    [Header("Events")]
    [SerializeField] private UnityEvent<Vector2> MoveEvent;
    [SerializeField] private UnityEvent<Vector2> LookEvent;
    [SerializeField] private UnityEvent<bool> JumpEvent;
    [SerializeField] private UnityEvent<bool> SprintEvent;

    [Header("Movement")] 
    [SerializeField] private float threshold = 0.06f;
    [SerializeField] private float sprintThreshold = 0.6f;
    [SerializeField] private float sprintAmplification = 0.5f;
    
    private bool _sprinting = false;

    private void Start()
    {
        moveJoystick.OnInput += (mov =>
        {
            float moveMagnitude = mov.magnitude;

            if (moveMagnitude < threshold)
            {
                MoveEvent?.Invoke(Vector2.zero);
                return;
            }
            
            SetSprinting(moveMagnitude > sprintThreshold);
            
            // Normalize and modify input when sprinting
            mov /= moveMagnitude;
            
            if (_sprinting)
            {
                mov *= Mathf.Lerp(sprintAmplification, 1.0f, (moveMagnitude - sprintThreshold) / (1.0f - sprintThreshold));
            }
            else
            {
                mov *= moveMagnitude / sprintThreshold;
            }
            
            MoveEvent.Invoke(mov * MoveMagnitudeMultiplier);
        });;
        
        lookPad.OnInput += (mov =>
        {
            if (InvertLookY)
                mov.y *= -1;

            LookEvent.Invoke(mov * LookMagnitudeMultiplier);
        });
    }

    private void SetSprinting(bool value)
    {
        if (_sprinting == value)
            return;
        
        _sprinting = value;
        
        SprintEvent.Invoke(_sprinting);
    }

    public void Jump(bool val)
    {
        JumpEvent.Invoke(val);
    }
}