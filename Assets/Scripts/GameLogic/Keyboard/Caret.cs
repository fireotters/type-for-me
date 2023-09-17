using Signals;
using System;
using System.Linq;
using TMPro;
using UnityEngine;


namespace GameLogic.Keyboard
{
    public class Caret : MonoBehaviour
    {
        [Header("Caret Appearance")]
        private SpriteRenderer _rend;
        private Color _colorRed = new(1, 0, 0, 0.8f), _colorGrey = new(.5f, .5f, .5f, 0.8f);

        [Header("Caret Blinking")]
        private bool state;
        private float _nextBlink;
        private readonly float _onDuration = .8f, _offDuration = .6f;

        [Header("Caret Position")]
        [SerializeField] TextMeshPro _inputTextTmp;
        private Vector3 _basePos = new(-0.34f, 0f, 0f);

        private readonly CompositeDisposable _disposables = new();


        // --------------------------------------------------------------------------------------------------------------
        // Start & End
        // --------------------------------------------------------------------------------------------------------------
        private void Start()
        {
            _rend = GetComponent<SpriteRenderer>();
            SignalBus<SignalKeyboardKeyPress>.Subscribe(MoveCaretSig1).AddTo(_disposables);
            SignalBus<SignalKeyboardBackspacePress>.Subscribe(MoveCaretSig2).AddTo(_disposables);
            SignalBus<SignalKeyboardMistakeMade>.Subscribe(ChangeColourMistake).AddTo(_disposables);
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
                _nextBlink += (state ? _onDuration : _offDuration);
                _rend.enabled = state;
            }
        }

        private void MoveCaretSig1 (SignalKeyboardKeyPress context)
        {
            Invoke(nameof(MoveCaret), 0.05f);
        }
        private void MoveCaretSig2(SignalKeyboardBackspacePress context)
        {
            _rend.color = _colorGrey;
            Invoke(nameof(MoveCaret), 0.05f);
        }

        private void MoveCaret()
        {
            //TODO figure out how to move the caret correctly when a space (' ') is input

            try //TODO consider changing to a length check - it didn't work when I tried earlier
            {
                Vector3 bottomright = _inputTextTmp.textInfo.characterInfo[_inputTextTmp.textInfo.characterCount - 1].bottomRight;
                Vector3 caretPos = new(bottomright.x + 0.03f, 0f, bottomright.z);
                transform.localPosition = caretPos;
            }
            catch (IndexOutOfRangeException)
            {
                transform.localPosition = _basePos;
            }
        }

        private void ChangeColourMistake(SignalKeyboardMistakeMade context)
        {
            _rend.color = _colorRed;
        }
    }
}