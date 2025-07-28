namespace CognitiveTestEngine.Prototype
{
    using UnityEngine;
    using UnityEngine.EventSystems;// give access to player action (mouse click for this project)
    using UnityEngine.InputSystem;
    using UnityEngine.UI;
    public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Image image;
        [HideInInspector] public Transform parentAfterDrag;
        public void OnBeginDrag(PointerEventData eventData) //initial click on object
        {
            Debug.Log("Begin drag");
            parentAfterDrag = transform.parent;
            transform.SetParent(transform.root);
            transform.SetAsLastSibling(); // put dragged object in lowest order so it stay up top of scene
            image.raycastTarget = false;
        }
        
        public void OnDrag(PointerEventData eventData) // moving object around while clicked
        {
            Debug.Log("Dragging");
            transform.position = Mouse.current.position.ReadValue()
    ; // transform.position is the position of the object, changing its value change its position. Input.mousePosition get position of pointer when clicked and change the object's position
        }

        public void OnEndDrag(PointerEventData eventData) // not clicking object
        {
            Debug.Log("End drag");
            transform.SetParent(parentAfterDrag);
            image.raycastTarget = true;
        }
    }
}
