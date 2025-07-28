using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace CognitiveTestEngine.Core
{
    public class DropZone : MonoBehaviour, IDropHandler
    {
        public int maxSlots = 5;
        public float yOffsetPerBlock = 10f;
        public float lockYOffset = 10f;

        [SerializeField] private RectTransform baseSlotPositionMarker;
        public PyramidStand p;
        public BubbleAddition b;
        public CrossWord c;
        public int dropZoneId;

        public class Slots
        {
            public static int slotsOccupied = 0;
        }

        
        public void OnDrop(PointerEventData eventData)
        {
            GameObject draggedObject = eventData.pointerDrag;
            if (draggedObject == null) return;

            Drag draggableItem = draggedObject.GetComponent<Drag>();
            if (draggableItem == null) return;


            if (CrossWord.crossBool)
            {
                if (c != null)
                {
                    int zone = this.dropZoneId;
                    int dragged = draggableItem.dragId;
                    int puzzle = CrossWord.PuzzleNum;

                    //Debug.Log($"Crossword Drop: Item ID {dragged} onto Zone ID {zone}. Will return to start."); // Optional log

                    c.Loader(zone, dragged, puzzle);

                    draggableItem.NotifyDropSuccess(false);
                }
                else
                {
                    //Debug.LogError("CrossWord reference (c) is not set in DropZone!", this);
                    draggableItem.NotifyDropSuccess(false);
                }
                return;
            }

            if (draggableItem.targetDropZone != null && draggableItem.targetDropZone != this)
            {
                Debug.Log("Dropped on wrong zone (non-crossword)");
                draggableItem.NotifyDropSuccess(false);
                return;
            }

            if (Slots.slotsOccupied >= maxSlots)
            {
                Debug.Log("DropZone full (non-crossword)");
                draggableItem.NotifyDropSuccess(false);
                return;
            }

            Debug.Log($"Non-Crossword Drop: Item ID {draggableItem.dragId} accepted onto Zone ID {this.dropZoneId}. Locking.");

            RectTransform draggedRect = draggedObject.GetComponent<RectTransform>();
            Vector2 basePosition = (baseSlotPositionMarker != null)
                ? baseSlotPositionMarker.anchoredPosition
                : GetComponent<RectTransform>().anchoredPosition;
            Vector2 targetPosition = new Vector2(
                basePosition.x,
                basePosition.y + (Slots.slotsOccupied * yOffsetPerBlock) + lockYOffset
            );
            draggedRect.anchoredPosition = targetPosition;

            Slots.slotsOccupied++;

            draggableItem.NotifyDropSuccess(true);

            if (Slots.slotsOccupied >= maxSlots)
            {
                Debug.Log("DropZone: Puzzle Done! Stack is full (non-crossword).");
                if (PyramidStand.pyramidBool && p != null)
                {
                    p.CheckResults();
                }
                if (BubbleAddition.bubbleBool && b != null)
                {
                    b.CheckResults(BubbleAddition.WinBubble.Winner);
                }
            }
        }
    }
}