using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Input
{
    public class TouchPad : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        public event Action<Vector2> OnInput;

        private bool _wasDragged = false;

        public void OnDrag(PointerEventData eventData)
        {
            _wasDragged = true;
        
            OnInput?.Invoke(eventData.delta);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _wasDragged = false;
        
            OnInput?.Invoke(Vector2.zero);
        }

        private void LateUpdate()
        {
            if (_wasDragged)
            {
                OnInput?.Invoke(Vector2.zero);
            }
        
            _wasDragged = false;
        }
    }
}