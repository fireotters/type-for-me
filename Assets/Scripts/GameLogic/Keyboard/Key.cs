using Signals;
using TMPro;
using UnityEngine;

namespace GameLogic.Keyboard
{
    public class Key : MonoBehaviour
    {
        [SerializeField] private string letter;
        [SerializeField] private TMP_Text keyLabel;
        [SerializeField] private SpecialKeyStatus specialKeyStatus;
        [SerializeField] private bool isUnusable; // For aesthetic reasons, keys are on the keyboard which functionally do nothing.
        private SpriteRenderer _sprite;
        private Color _colorUnusable = new(0.7f, 0.7f, 0.7f, 1f);

        public enum SpecialKeyStatus
        {
            None, Backspace, Enter
        }

        private void Start()
        {
            _sprite = GetComponent<SpriteRenderer>();

            if (specialKeyStatus == SpecialKeyStatus.Backspace)
                keyLabel.text = "Backspace";
            else if (specialKeyStatus == SpecialKeyStatus.Enter)
                keyLabel.text = "Enter";
            else if (isUnusable)
            {
                keyLabel.text = "";
                _sprite.color = _colorUnusable;
            }
            else
                keyLabel.text = letter.ToUpper();
        }

        private void OnMouseDown()
        {
            if (specialKeyStatus == SpecialKeyStatus.Backspace)
            {
                Debug.Log("<Key> Pressed Backspace!");
                SignalBus<SignalKeyboardBackspacePress>.Fire();
            }
            else if (specialKeyStatus == SpecialKeyStatus.Enter)
            {
                Debug.Log("<Key> Pressed Enter!");
                // SignalBus<SignalKeyboardEnterPress>.Fire(); // TODO Implement pressing 'Enter' to finish a level
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