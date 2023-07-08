using UnityEngine;

namespace Other
{
    public class Draggable : MonoBehaviour
    {
        private Vector3 pointerOffset;
        private bool dragging;
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
        }
        
        private void Update()
        {
            if (dragging)
            {
                transform.position = mainCamera.ScreenToWorldPoint(Input.mousePosition) + pointerOffset;
            }
        }

        private void OnMouseDown()
        {
            print("got clicked!");
            pointerOffset = transform.position - mainCamera.ScreenToWorldPoint(Input.mousePosition);
            dragging = true;
        }

        private void OnMouseUp()
        {
            dragging = false;
        }
    }    
}

