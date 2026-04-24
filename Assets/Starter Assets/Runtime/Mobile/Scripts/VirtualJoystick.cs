using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    enum LockState { Unlocked, Locking, Locked }
    
    [SerializeField] private RectTransform padTransform, stickTransform;
    [SerializeField] private RectTransform lockTransform;
    [SerializeField] private Image padImage;
    [SerializeField] private float stickRange = 60.0f;
    [SerializeField] private float lockRange = 24.0f;
    [SerializeField] private float lockDelay = 0.2f;
    
    // optimization
    [SerializeField, HideInInspector] private float stickRangeSqr;
    [SerializeField, HideInInspector] private float lockRangeSqr;

    public event Action<Vector2> OnInput;
    
    private int? _pointerId;
    private float _lockDistance;
    private LockState _lockState;

    private void OnEnable()
    {
        padImage.enabled = false;
        stickTransform.gameObject.SetActive(false);
        lockTransform.gameObject.SetActive(false);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_pointerId != null)
            return;
        
        _lockState = LockState.Unlocked;
        
        padTransform.position = eventData.position;
        stickTransform.anchoredPosition = Vector2.zero;
        
        padImage.enabled = true;
        stickTransform.gameObject.SetActive(true);
        lockTransform.gameObject.SetActive(true);
        
        _pointerId = eventData.pointerId;
        
        OnInput?.Invoke(Vector2.zero);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_pointerId.HasValue)
            return;
        
        if (eventData.pointerId != _pointerId)
            return;

        _lockDistance = (eventData.position - (Vector2)lockTransform.position).sqrMagnitude;
        
        switch (_lockState)
        {
            case LockState.Unlocked:
                if (_lockDistance <= lockRangeSqr)
                {
                    _lockState = LockState.Locking;
                    Invoke(nameof(Lock), lockDelay);
                }
                break;
            
            case LockState.Locking:
                if (_lockDistance > lockRangeSqr)
                {
                    _lockState = LockState.Unlocked;
                    CancelInvoke(nameof(Lock));
                }
                break;
            
            case LockState.Locked:
                return;
        }
        
        // Don't let stick to go off it's range
        Vector2 stickOffset = eventData.position - (Vector2)padTransform.position;
        float stickOffsetSqrMagn = stickOffset.sqrMagnitude;

        if (stickOffsetSqrMagn > stickRangeSqr)
        {
            stickOffset = stickOffset / Mathf.Sqrt(stickOffsetSqrMagn) * stickRange;
        }

        if (_lockState == LockState.Locking)
        {
            stickTransform.localPosition = Vector3.up * stickRange;
        }
        else
        {
            stickTransform.localPosition = stickOffset;
        }
        
        OnInput?.Invoke(stickOffset / stickRange);
    }

    void Lock()
    {
        _lockState = LockState.Locked;
        
        // Move forward constantly until new command is given
        OnInput?.Invoke(new Vector2(0, 1));
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        padImage.enabled = false;
        stickTransform.gameObject.SetActive(false);
        lockTransform.gameObject.SetActive(_lockState == LockState.Locked);
        
        _pointerId = null;
        eventData.Reset();
        
        if (_lockState != LockState.Locked)
            OnInput?.Invoke(Vector2.zero);
    }

    private void OnValidate()
    {
        stickRangeSqr = stickRange * stickRange;
        lockRangeSqr  = lockRange * lockRange;
    }
}
