using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform padTransform, stickTransform;
    [SerializeField] private float stickRange = 60.0f;
    [SerializeField, HideInInspector] private float stickRangeSqr;

    public event Action<Vector2> OnInput;
    
    private int? _pointerId;

    private void OnEnable()
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_pointerId != null)
            return;
        
        padTransform.position = eventData.position;
        stickTransform.anchoredPosition = Vector2.zero;
        
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1.0f;
        
        _pointerId = eventData.pointerId;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_pointerId.HasValue)
            return;
        
        if (eventData.pointerId != _pointerId)
            return;
        
        // Don't let stick to go off it's range
        Vector2 stickOffset = eventData.position - (Vector2)padTransform.position;
        float stickOffsetSqrMagn = stickOffset.sqrMagnitude;

        if (stickOffsetSqrMagn > stickRangeSqr)
        {
            stickOffset = stickOffset / Mathf.Sqrt(stickOffsetSqrMagn) * stickRange;
        }
        
        stickTransform.position = (Vector3)stickOffset + padTransform.position;
        
        OnInput?.Invoke(stickOffset / stickRange);
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.0f;
        
        _pointerId = null;
        eventData.Reset();
        
        OnInput?.Invoke(Vector2.zero);
    }

    private void OnValidate()
    {
        stickRangeSqr = stickRange * stickRange;
    }
}
