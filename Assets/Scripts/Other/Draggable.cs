using Signals;
using UnityEngine;

namespace Other
{
    public class Draggable : MonoBehaviour
    {
        private bool _dragging;
        private bool _disableDragging = false, _isInLevelTransition = false;
        private readonly CompositeDisposable _disposables = new();
        private bool _onlyJustClicked = false;

        [Header("Mouse Users")]
        public float mouseSensitivity;

        [Header("Touch Users")]
        private Vector3 _touchOffset;
        private Camera _mainCamera;

        [Header("Bounds of where Draggable can be dragged")]
        [SerializeField] private DraggableType _draggableType;
        private Vector2 _bounds, _boundsNumPad = new(9, 7), _boundsKeyboard = new(15, 7);

        public enum DraggableType { Numpad, Keyboard }

        // --------------------------------------------------------------------------------------------------------------
        // Start & End
        // --------------------------------------------------------------------------------------------------------------
        private void Start()
        {
            _mainCamera = Camera.main;

            SignalBus<SignalGameEnded>.Subscribe(DisableDragEnd).AddTo(_disposables);
            SignalBus<SignalGamePaused>.Subscribe(DisableDragPause).AddTo(_disposables);
            SignalBus<SignalGameRetryFromCheckpoint>.Subscribe(RetryLevelFromCheckpoint).AddTo(_disposables);

            if (_draggableType == DraggableType.Numpad)
                _bounds = _boundsNumPad;
            else if (_draggableType == DraggableType.Keyboard)
                _bounds = _boundsKeyboard;
        }
        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        // --------------------------------------------------------------------------------------------------------------
        // Per-Frame Updates
        // --------------------------------------------------------------------------------------------------------------
        private void Update()
        {
            // Never allow updates while dragging is disabled
            if (_disableDragging || _isInLevelTransition)
            {
                _dragging = false;
                return;
            }

            if (Input.touchCount > 0)
                HandleMovementTouch();
            else
                HandleMovementMouse();

            _onlyJustClicked = false;
        }

        private void HandleMovementMouse()
        {
            // If using mouse/trackpad, the cursor is locked in the center while moving around. The click may not activate OnMouseDown, so check here too.
            // If mouse is clicked or Esc is pressed, during dragging, then unlock the cursor. Ignore the frame where OnMouseDown is activated.
            bool draggingInterrupted = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Escape);
            if (draggingInterrupted && _dragging && !_onlyJustClicked)
            {
                _dragging = false;
                Cursor.lockState = CursorLockMode.None;
            }

            if (_dragging)
            {
                var mouseDelta = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0);
                var mouseMovement = mouseSensitivity * mouseDelta;
                var desiredPos = transform.position + mouseMovement;
                transform.position = ValidPosition(desiredPos);
            }
        }

        private void HandleMovementTouch()
        {
            // Ensure the cursor is never locked
            Cursor.lockState = CursorLockMode.None;

            // Touchscreen has just been let go of
            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                _dragging = false;
                return;
            }

            if (_dragging)
            {
                var desiredPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition) + _touchOffset;
                transform.position = ValidPosition(desiredPos);
            }
        }

        private void OnMouseDown()
        {
            if (!_disableDragging && !_isInLevelTransition)
            {
                // Set pointerOffset for touch users
                _touchOffset = transform.position - _mainCamera.ScreenToWorldPoint(Input.mousePosition);

                if (_dragging)
                    Cursor.lockState = CursorLockMode.None;
                else
                    Cursor.lockState = CursorLockMode.Locked;
                _dragging = !_dragging;
                _onlyJustClicked = true;
            }
        }

        // --------------------------------------------------------------------------------------------------------------
        // Disabling input
        // --------------------------------------------------------------------------------------------------------------
        private void DisableDragEnd(SignalGameEnded signal)
        {
            _disableDragging = true;
            Cursor.lockState = CursorLockMode.None;
        }
        private void RetryLevelFromCheckpoint(SignalGameRetryFromCheckpoint s)
        {
            _disableDragging = false;
        }

        private void DisableDragPause(SignalGamePaused signal)
        {
            if (signal.paused)
            {
                Cursor.lockState = CursorLockMode.None;
                _disableDragging = true;
            }
            else
                _disableDragging = false;
        }

        public void BeginLevelTransition()
        {
            Cursor.lockState = CursorLockMode.None;
            _isInLevelTransition = true;
        }
        public void EndLevelTransition()
        {
            Cursor.lockState = CursorLockMode.None;
            _isInLevelTransition = false;
        }

        // --------------------------------------------------------------------------------------------------------------
        // Per-Frame Updates
        // --------------------------------------------------------------------------------------------------------------
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