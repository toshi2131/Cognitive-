using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace CognitiveTestEngine.Core
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Drag : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public static event Action PuzzleDone = delegate { };

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Canvas canvas;

        private Vector2 initialPosition;
        private bool dropSuccessful = false;
        private bool locked = false;

        public DropZone targetDropZone;

        public DropZone[] DropList;

        public int dragId;
        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            canvas = GetComponentInParent<Canvas>();

            if (canvas == null)
            {
                Debug.LogError("Drag Script: Cannot find parent Canvas component.", this);
            }
        }

        void Start()
        {
            initialPosition = rectTransform.anchoredPosition;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (locked || canvas == null)
            {
                eventData.pointerDrag = null;
                return;
            }

            initialPosition = rectTransform.anchoredPosition;
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;
            dropSuccessful = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (locked || canvas == null) return;

            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (canvas == null) return;

            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            if (!dropSuccessful && !locked)
            {
                rectTransform.anchoredPosition = initialPosition;
            }
        }

        public void NotifyDropSuccess(bool success)
        {
            dropSuccessful = success;
            if (success)
            {
                locked = true;
                //Debug.Log($"Drag Item {dragId}: Locked = true");
            }
            else
            {
                
                locked = false;
                //Debug.Log($"Drag Item {dragId}: Locked = false (Drop Success = {success})");
            }
        }
    }
}