using Signals;
using UnityEngine;

namespace Other
{
    public class Draggable : MonoBehaviour
    {
        private Vector3 _pointerOffset;
        private bool _dragging;
        private Camera _mainCamera;
        private bool _hasGameEnded;

        private readonly CompositeDisposable _disposables = new();
        
        private void Start()
        {
            _mainCamera = Camera.main;
            SignalBus<SignalGameEnded>.Subscribe(DisableDrag).AddTo(_disposables);
        }
        
        private void Update()
        {
            if (_hasGameEnded)
                _dragging = false;
            
            if (_dragging && Time.timeScale != 0)
            {
                transform.position = _mainCamera.ScreenToWorldPoint(Input.mousePosition) + _pointerOffset;
            }
        }

        private void DisableDrag(SignalGameEnded signal)
        {
            _hasGameEnded = true;
        }
        
        private void OnMouseDown()
        {
            _pointerOffset = transform.position - _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            _dragging = true;
        }

        private void OnMouseUp()
        {
            _dragging = false;
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }    
}