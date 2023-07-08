using Signals;
using TMPro;
using UnityEngine;

namespace GameLogic.Keyboard
{
    public class Key : MonoBehaviour
    {
        [SerializeField] private string letter;
        [SerializeField] private TMP_Text keyLabel;
        [SerializeField] private bool isBackspace;

        private void Start()
        {
            if (isBackspace)
            {
                keyLabel.text = "Backspace";
            }
            else
            {
                keyLabel.text = letter.ToUpper();    
            }
        }

        private void OnMouseDown()
        {
            if (isBackspace)
            {
                Debug.Log("<Key> Pressed Backspace!");
                SignalBus<SignalKeyboardBackspacePress>.Fire();
            }
            else
            {
                Debug.Log($"<Key> Pressed {letter}!");
                SignalBus<SignalKeyboardKeyPress>.Fire(new SignalKeyboardKeyPress { Letter = letter });
            }
        }
    }
}