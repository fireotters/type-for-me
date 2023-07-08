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
                if (keyPress.Letter == "||")
                {
                    Debug.Log("Paused"); // TODO program in pausing signal
                    return;
                }

                var expectedCharacter = textPreview.text[inputtedText.text.Length];
                if (expectedCharacter.ToString() != keyPress.Letter)
                {
                    if (keyPress.Letter == " ")
                        keyPress.Letter = "_"; // Indicate an incorrect SpaceKey usage
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
            var curText = inputtedText.text;
            string newText;
            if (curText.Length == 0)
            {
                return;
            }
            else if (curText.EndsWith(">"))
            {
                // Remove RTF tags. E.g: 123<color>4</color>
                // Everything from the last '>' to the second-last '<' will be deleted
                int posOfSecondLastRTFOpener = curText[..curText.LastIndexOf("<")].LastIndexOf("<");
                newText = curText[..posOfSecondLastRTFOpener];
            }
            else
            {
                newText = curText[..^1]; // Remove last character
            }
            inputtedText.text = newText;
        }
    }
}
