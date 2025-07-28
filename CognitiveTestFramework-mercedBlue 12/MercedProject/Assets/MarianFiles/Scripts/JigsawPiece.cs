using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

namespace CognitiveTestEngine.Core
{
    [RequireComponent(typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(CanvasGroup))]
    public class JigsawPiece : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Vector2 correctAnchoredPos;
        public RectTransform rect;
        public CanvasGroup canvasGroup;
        private Canvas canvas;
        private JigsawGame parentGame;
        private Vector2 startAnchoredPos;

        [SerializeField, Tooltip("Max distance (pixels) to snap into place")] // still changing this
        private float snapThreshold = 15f;

        // count each piece once
        private bool isPlaced = false;

        void Awake()
        {
            rect = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            canvas = GetComponentInParent<Canvas>();
            parentGame = GetComponentInParent<JigsawGame>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (isPlaced) return;
            startAnchoredPos = rect.anchoredPosition;
            rect.SetAsLastSibling();
            canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isPlaced) return;
            rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (isPlaced)
            {
                canvasGroup.blocksRaycasts = true;
                return;
            }

            canvasGroup.blocksRaycasts = true;

            if (Vector2.Distance(rect.anchoredPosition, correctAnchoredPos) <= snapThreshold)
            {
                rect.anchoredPosition = correctAnchoredPos; // snap
                rect.SetSiblingIndex(0);
                isPlaced = true;
                this.enabled = false; // locked in place
                parentGame.OnPiecePlacedCorrectly();
            }
        }
    }
}
