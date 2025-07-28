namespace CognitiveTestEngine.Prototype
{
    using NUnit.Framework;
    using UnityEngine.UI;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using System.Collections.Generic;
    public class Slot : MonoBehaviour, IDropHandler
    {
        private ItemFalling itemfalling;
        public int slotIndex;
        public void OnDrop(PointerEventData eventData) // handle event when something is dropped onto this object
        {
            itemfalling = FindFirstObjectByType<ItemFalling>();
            List<Sprite> fruits = itemfalling.GetFruitNames();
            if (transform.childCount == 0) //snap to slot only if no child object exist
            {
                GameObject dropped = eventData.pointerDrag;
                DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();
                draggableItem.parentAfterDrag = transform;   
                Image droppedImage = dropped.GetComponent<Image>(); // Get sprite from dragged object

                if (droppedImage != null && droppedImage.sprite != null)
                {
                    Sprite slotImage = droppedImage.sprite; 
                    if (slotImage == fruits[slotIndex])
                    {
                        Debug.Log("Correct match at slot " + slotIndex);
                    }
                    else
                    {
                        Debug.Log("Incorrect match at slot " + slotIndex);
                        Debug.Log("Correct: " + fruits[slotIndex].name);
                        Debug.Log("Dropped : " + slotImage.name);
                    }
                }
                else
                {
                    Debug.LogWarning("Dropped object has no sprite");
                }
            }

        }
    }
}

