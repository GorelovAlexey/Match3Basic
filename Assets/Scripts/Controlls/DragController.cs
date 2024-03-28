using System;
using Assets.Scripts.Engine;
using Microsoft.Unity.VisualStudio.Editor;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Controlls
{
    public class DragController : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        private RectTransform rect;
        public ReactiveProperty<bool> IsDragging { get; } = new ReactiveProperty<bool>(false);
        public ReactiveProperty<Vector2> DragDistance { get; } = new ReactiveProperty<Vector2>();
        public ReactiveProperty<Vector2> DragDistanceRelative { get; } = new ReactiveProperty<Vector2>();

        // Start is called before the first frame update
        void Start()
        {
            rect = GetComponent<RectTransform>();
        }

        void SetDragDistance(PointerEventData eventData)
        {
            if (eventData == null)
            {
                DragDistance.Value = Vector2.zero;
                DragDistanceRelative.Value = Vector2.zero;
            }
            else
            {
                DragDistance.Value = eventData.position - eventData.pressPosition;

                var size = new Vector2(rect.sizeDelta.x * rect.transform.lossyScale.x,
                    rect.sizeDelta.y * rect.transform.lossyScale.y); 
                var rel = new Vector2(DragDistance.Value.x / size.x, DragDistance.Value.y / size.y);
                DragDistanceRelative.Value = rel;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            IsDragging.Value = true;
            SetDragDistance(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            IsDragging.Value = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            SetDragDistance(eventData);
        }
    }
}
