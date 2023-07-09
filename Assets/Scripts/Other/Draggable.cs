using System;
using Signals;
using UnityEngine;

namespace Other
{
    public class Draggable : MonoBehaviour
    {
        private Vector3 pointerOffset;
        private bool dragging;
        private Camera mainCamera;
        private bool hasGameEnded;

        private readonly CompositeDisposable disposables = new();
        
        private void Start()
        {
            mainCamera = Camera.main;
            SignalBus<SignalGameEnded>.Subscribe(DisableDrag).AddTo(disposables);
        }
        
        private void Update()
        {
            if (hasGameEnded)
                dragging = false;
            
            if (dragging && Time.timeScale != 0)
            {
                transform.position = mainCamera.ScreenToWorldPoint(Input.mousePosition) + pointerOffset;
            }
        }

        private void DisableDrag(SignalGameEnded signal)
        {
            hasGameEnded = true;
        }
        
        private void OnMouseDown()
        {
            pointerOffset = transform.position - mainCamera.ScreenToWorldPoint(Input.mousePosition);
            dragging = true;
        }

        private void OnMouseUp()
        {
            dragging = false;
        }

        private void OnDestroy()
        {
            disposables.Dispose();
        }
    }    
}

