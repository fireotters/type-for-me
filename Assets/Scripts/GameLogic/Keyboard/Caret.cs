using Signals;
using System;
using TMPro;
using UnityEngine;


namespace GameLogic.Keyboard
{
    public class Caret : MonoBehaviour
    {
        [Header("Caret Appearance")]
        private SpriteRenderer _rend;
        private Color _colorMistake = new(1, 0, 0, 1), _colorNormal = new(0, 0, 0, 1), _colorWhiteForDarkMode = new(1, 1, 1, 1);

        [Header("Caret Blinking")]
        private bool state;
        private float _nextBlink;
        private readonly float _onDuration = .8f, _offDuration = .6f;

        [Header("Caret Position")]
        [SerializeField] private TextMeshPro _tmpInput;
        [SerializeField] private TextMeshPro _tmpPreview;
        private Vector3 _basePos;
        private readonly float _lilOffset = 0.02f;

        private readonly CompositeDisposable _disposables = new();


        // --------------------------------------------------------------------------------------------------------------
        // Start & End
        // --------------------------------------------------------------------------------------------------------------
        private void Start()
        {
            _rend = GetComponent<SpriteRenderer>();
            _rend.color = _colorNormal;
            SignalBus<SignalKeyboardKeyPress>.Subscribe(MoveCaretSig1).AddTo(_disposables);
            SignalBus<SignalKeyboardBackspacePress>.Subscribe(MoveCaretSig2).AddTo(_disposables);
            SignalBus<SignalKeyboardMistakeMade>.Subscribe(ChangeColourMistake).AddTo(_disposables);
            Invoke(nameof(FindBasePos), 0.05f);
        }

        private void FindBasePos()
        {
            // Find basePos by asking tmpPreview for first char
            Vector3 bottomleft = _tmpPreview.textInfo.characterInfo[0].bottomLeft;
            _basePos = new(bottomleft.x - _lilOffset, -_lilOffset, bottomleft.z);
            transform.localPosition = _basePos;
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
            if (Time.time > _nextBlink)
            {
                state = !state;
                _nextBlink += state ? _onDuration : _offDuration;
                _rend.enabled = state;
            }
        }

        private void MoveCaretSig1 (SignalKeyboardKeyPress context)
        {
            Invoke(nameof(MoveCaret), 0.03f);
        }
        private void MoveCaretSig2(SignalKeyboardBackspacePress context)
        {
            _rend.color = _colorNormal;
            Invoke(nameof(MoveCaret), 0.03f);
        }

        private void MoveCaret()
        {
            try
            {
                Vector3 bottomright = _tmpInput.textInfo.characterInfo[_tmpInput.textInfo.characterCount - 1].bottomRight;
                Vector3 caretPos = new(bottomright.x + _lilOffset, -_lilOffset, bottomright.z);
                transform.localPosition = caretPos;
            }
            catch (IndexOutOfRangeException)
            {
                transform.localPosition = _basePos;
            }
        }

        public void ResetCaret()
        {
            transform.localPosition = _basePos;
            if (_rend)
                _rend.color = _colorNormal;
        }

        private void ChangeColourMistake(SignalKeyboardMistakeMade context)
        {
            _rend.color = _colorMistake;
        }

        public void ChangeColourForDarkMode()
        {
            _colorNormal = _colorWhiteForDarkMode;
        }
    }
}