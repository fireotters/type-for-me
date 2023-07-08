using System;
using Signals;
using TMPro;
using UnityEngine;

namespace GameLogic.Keyboard
{
    public class KeyboardReader : MonoBehaviour
    {
        [SerializeField] private string[] phrases;
        [SerializeField] private TMP_Text textPreview;
        [SerializeField] private TMP_Text inputtedText;
        private readonly CompositeDisposable disposables = new();
        
        private void Start()
        {
            SignalBus<SignalKeyboardKeyPress>.Subscribe(ReadFromKeyboard).AddTo(disposables);
            SignalBus<SignalKeyboardBackspacePress>.Subscribe(BackspaceAction).AddTo(disposables);

            if (phrases.Length == 0)
            {
                Debug.LogError("NO PHRASES SET IN INSPECTOR! how tf u gonna play binch");
            }
            else
            {
                textPreview.text = phrases[0];
            }
        }

        private void ReadFromKeyboard(SignalKeyboardKeyPress keyPress)
        {
            Debug.Log($"<KeyboardReader> {keyPress.Letter} was pressed!");
            try
            {
                var expectedCharacter = textPreview.text[inputtedText.text.Length];
                if (expectedCharacter.ToString() != keyPress.Letter)
                {
                    inputtedText.text += $"<color=#FF0000>{keyPress.Letter}</color>";
                }
                else
                {
                    inputtedText.text += keyPress.Letter;
                }
            }
            catch (IndexOutOfRangeException ioore)
            {
                // TODO: perhaps a "bzzt wrong" sound could play here?
                // should we let the player keep typing???
            }
            
        }

        private void BackspaceAction(SignalKeyboardBackspacePress backspacePress)
        {
            Debug.Log("<KeyboardReader> Backspace pressed!");
            var currentText = inputtedText.text;
            inputtedText.text = currentText[..^1];
        }
    }
}
