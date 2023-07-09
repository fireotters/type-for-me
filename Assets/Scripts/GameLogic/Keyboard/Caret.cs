using System;
using System.Linq;
using TMPro;
using UnityEngine;


// TODO: keep working on thise when you really wanna get angry
public class Caret : MonoBehaviour
{
    [SerializeField] TextMeshPro _inputTextTmp;
    private Renderer _rend;
    private bool state, isLastCharacterASpace;
    private float _nextBlink;
    private readonly float _onDuration = .8f, _offDuration = .6f;

    private readonly CompositeDisposable _disposables = new();


    // --------------------------------------------------------------------------------------------------------------
    // Start & End
    // --------------------------------------------------------------------------------------------------------------
    private void Start()
    {
        _rend = GetComponent<Renderer>();
        //SignalBus<SignalKeyboardKeyPress>.Subscribe(Press).AddTo(_disposables);
        //SignalBus<SignalKeyboardBackspacePress>.Subscribe(BackPress).AddTo(_disposables);
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

        MoveCaret();
    }

    private void MoveCaret()
    {
        var currentCharacterInfo = _inputTextTmp.textInfo.characterInfo;
        var currentCaretPos = transform.position;
        
        if (string.IsNullOrEmpty(_inputTextTmp.text))
            return;
        
        print($"<Caret> current text: {_inputTextTmp.text}");
        print($"<Caret> current buffer: {new string(currentCharacterInfo.Select(charInfo => charInfo.character).ToArray())}");
        
        if (!IsBufferEmpty(currentCharacterInfo))
        {
            if (!DoesBufferContentsMatchCurrentText(currentCharacterInfo)) // dirty ass buffer fucking tmp garbage
            {
                try
                {
                    print("<Caret> buffer and current text dont match");
                    // its likely someone used backspace now
                    var lastCharacter = _inputTextTmp.text[^1];
                    var lastMatchingCharacterInfo =
                        currentCharacterInfo.Last(characterInfo => characterInfo.character.Equals(lastCharacter));
                    var lastCharCoords = lastMatchingCharacterInfo.bottomRight;
                    transform.position = new Vector3(lastCharCoords.x, currentCaretPos.y);
                    return;    
                } catch (InvalidOperationException) { Debug.LogWarning("TextMeshPro sucks big dick");}
            }
            
            var lastCharacterInfo = currentCharacterInfo.Last(characterInfo => characterInfo.character != '\0');
            if (char.IsWhiteSpace(lastCharacterInfo.character) && !isLastCharacterASpace)
            {
                // its a space, there's no sprite data to base our moving calculation from
                transform.position = new Vector3(currentCaretPos.x + 0.4f, currentCaretPos.y);
                isLastCharacterASpace = true;
            }

            if (char.IsWhiteSpace(lastCharacterInfo.character)) return;

            var localCoord = lastCharacterInfo.bottomRight;
            transform.position = new Vector3(localCoord.x, currentCaretPos.y);
            isLastCharacterASpace = false;
        }
    }

    private bool IsBufferEmpty(TMP_CharacterInfo[] currentCharacterInfo)
    {
        return currentCharacterInfo.All(charInfo => charInfo.character == '\0');
    }

    private bool DoesBufferContentsMatchCurrentText(TMP_CharacterInfo[] currentCharacterInfo)
    {
        var bufferContent = new string(currentCharacterInfo
            .Select(charInfo => charInfo.character)
            .Where(character => !character.Equals('\0'))
            .ToArray());
        return _inputTextTmp.text.Equals(bufferContent);
    }
}