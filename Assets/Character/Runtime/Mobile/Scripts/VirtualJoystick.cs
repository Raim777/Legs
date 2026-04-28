using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    enum LockState { Unlocked, Locking, Locked }
    
    [SerializeField] private RectTransform padTransform, stickTransform;
    [SerializeField] private RectTransform lockTransform;
    [SerializeField] private Image padImage;
    [SerializeField] private CanvasGroup lockPadCanvas;
    [SerializeField] private float stickRange = 60.0f;
    [SerializeField] private float lockRange = 24.0f;
    [SerializeField] private float lockDelay = 0.2f;
    
    // optimization
    [SerializeField, HideInInspector] private float stickRangeSqr;
    [SerializeField, HideInInspector] private float lockRangeSqr;

    public event Action<Vector2> OnInput;
    
    private int? _pointerId;
    private LockState _lockState;
    private const float MinLockPadAlpha = 0.33f;

    private void OnEnable()
    {
        padImage.enabled = false;
        lockPadCanvas.alpha = 0.0f;
        stickTransform.gameObject.SetActive(false);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_pointerId != null)
            return;
        
        _lockState = LockState.Unlocked;
        
        padTransform.position = eventData.position;
        stickTransform.anchoredPosition = Vector2.zero;
        
        padImage.enabled = true;
        lockPadCanvas.alpha = MinLockPadAlpha;
        stickTransform.gameObject.SetActive(true);
        
        _pointerId = eventData.pointerId;
        
        OnInput?.Invoke(Vector2.zero);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_pointerId.HasValue)
            return;

        if (eventData.pointerId != _pointerId)
            return;
                
        CheckLockState((eventData.position - (Vector2)lockTransform.position).sqrMagnitude);

        if (_lockState == LockState.Locked)
            return;
        
        DragInput(eventData.position - (Vector2)padTransform.position);
    }

    void CheckLockState(float lockDistance)
    {
        switch (_lockState)
        {
            case LockState.Unlocked:
                if (lockDistance <= lockRangeSqr)
                {
                    lockPadCanvas.alpha = 1.0f;
                    _lockState = LockState.Locking;
                    Invoke(nameof(Lock), lockDelay);
                }
                break;
            
            case LockState.Locked:
            case LockState.Locking:
                if (lockDistance > lockRangeSqr)
                {
                    lockPadCanvas.alpha = MinLockPadAlpha;
                    _lockState = LockState.Unlocked;
                    CancelInvoke(nameof(Lock));
                }
                break;
        }

    }
    
    void Lock()
    {
        _lockState = LockState.Locked;
        
        // Move forward constantly until new command is given
        OnInput?.Invoke(new Vector2(0, 1));
    }
    
    void DragInput(Vector2 stickOffset)
    {
        float stickOffsetSqrMagn = stickOffset.sqrMagnitude;

        // Don't let stick to go off it's range
        if (stickOffsetSqrMagn > stickRangeSqr)
        {
            stickOffset = stickOffset / Mathf.Sqrt(stickOffsetSqrMagn) * stickRange;
        }

        // Show input direction when locking it
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
    
    public void OnPointerUp(PointerEventData eventData)
    {
        CancelInvoke(nameof(Lock));
        
        padImage.enabled  = false;
        lockPadCanvas.alpha = _lockState == LockState.Locked ? 1.0f : 0.0f;
        stickTransform.gameObject.SetActive(false);
        
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
