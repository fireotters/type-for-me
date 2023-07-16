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

        [Header("Bounds of where Draggable can be dragged")]
        [SerializeField] private DraggableType _draggableType;
        private Vector2 _bounds, _boundsNumPad = new(8, 5), _boundsKeyboard = new(15, 7);

        private readonly CompositeDisposable _disposables = new();
        
        public enum DraggableType
        {
            Numpad, Keyboard
        }

        private void Start()
        {
            _mainCamera = Camera.main;
            SignalBus<SignalGameEnded>.Subscribe(DisableDrag).AddTo(_disposables);

            if (_draggableType == DraggableType.Numpad)
                _bounds = _boundsNumPad;
            else if (_draggableType == DraggableType.Keyboard)
                _bounds = _boundsKeyboard;
        }
        
        private void Update()
        {
            if (_hasGameEnded)
                _dragging = false;

            // If using touch, check if the touch has ended.
            // If this isn't done, the keyboard will have to be tapped twice to pick up again.
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
                _dragging = false;

            if (_dragging && Time.timeScale != 0)
            {
                var desiredPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition) + _pointerOffset;
                transform.position = ValidPosition(desiredPos);
            }
        }

        private void DisableDrag(SignalGameEnded signal)
        {
            _hasGameEnded = true;
        }

        private void OnMouseDown()
        {
            _pointerOffset = transform.position - _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            _dragging = !_dragging;
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        private Vector3 ValidPosition(Vector3 desiredPos)
        {
            Vector3 origPos = transform.position;
            if (desiredPos.x < -_bounds.x || _bounds.x < desiredPos.x)
                desiredPos.x = origPos.x;
            if (desiredPos.y < -_bounds.y || _bounds.y < desiredPos.y)
                desiredPos.y = origPos.y;
            return desiredPos;
        }
    }    
}