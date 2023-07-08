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
        [SerializeField] private bool isUnusable; // For aesthetic reasons, keys are on the keyboard which functionally do nothing.

        private void Start()
        {
            if (isBackspace)
                keyLabel.text = "Backspace";
            else if (isUnusable)
                keyLabel.text = "";
            else
                keyLabel.text = letter.ToUpper();
        }

        private void OnMouseDown()
        {
            if (isBackspace)
            {
                Debug.Log("<Key> Pressed Backspace!");
                SignalBus<SignalKeyboardBackspacePress>.Fire();
            }
            else if (isUnusable)
            {
                Debug.Log("<Key> Pressed Unusable! Play a 'useless thocking' sfx...");
            }
            else
            {
                Debug.Log($"<Key> Pressed {letter}!");
                SignalBus<SignalKeyboardKeyPress>.Fire(new SignalKeyboardKeyPress { Letter = letter });
            }
        }

        private void StartHighlight()
        {

        }
        private void EndHighlight()
        {

        }
    }
}