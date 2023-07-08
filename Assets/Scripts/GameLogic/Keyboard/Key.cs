using FMODUnity;
using Signals;
using TMPro;
using UnityEngine;

namespace GameLogic.Keyboard
{
    public class Key : MonoBehaviour
    {
        [SerializeField] private string letter;
        [SerializeField] private TMP_Text keyLabel;
        [SerializeField] private SpecialKey specialKeyStatus;
        [SerializeField] private bool isUnusable; // For aesthetic reasons, keys are on the keyboard which functionally do nothing.
        private SpriteRenderer _sprite;
        private Color _colorUnusable = new(0.7f, 0.7f, 0.7f, 1f);
        private StudioEventEmitter keySound;
        private Animator anim;

        public enum SpecialKey
        {
            None, Backspace, Enter, Pause
        }

        private void Start()
        {
            _sprite = GetComponent<SpriteRenderer>();
            keySound = GetComponent<StudioEventEmitter>();
            anim = GetComponent<Animator>();

            if (isUnusable)
            {
                keyLabel.text = "";
                _sprite.color = _colorUnusable;
            }
            else if (specialKeyStatus == SpecialKey.None)
                keyLabel.text = letter.ToUpper();
            // Add else-ifs for specialKeyStatus if necessary
        }

        private void DoKeyPress()
        {
            anim.SetBool("Pressed", true);
            keySound.Play();
            switch (specialKeyStatus)
            {
                case SpecialKey.Backspace:
                    Debug.Log("<Key> Pressed Backspace!");
                    SignalBus<SignalKeyboardBackspacePress>.Fire();
                    break;
                case SpecialKey.Enter:
                    Debug.Log("<Key> Pressed Enter!");
                    SignalBus<SignalKeyboardEnterPress>.Fire(); // TODO Implement pressing 'Enter' to finish a prompt
                    break;
                case SpecialKey.Pause:
                    Debug.Log("<Key> Pressed Pause!");
                    SignalBus<SignalKeyboardPausePress>.Fire();
                    break;
                default:
                {
                    if (isUnusable)
                    {
                        Debug.Log("<Key> Pressed Unusable! Play a 'useless thocking' sfx...");
                    }
                    else
                    {
                        Debug.Log($"<Key> Pressed {letter}!");
                        SignalBus<SignalKeyboardKeyPress>.Fire(new SignalKeyboardKeyPress { Letter = letter });
                    }

                    break;
                }
            }
        }

        public void KeyPress()
        {
            DoKeyPress();
        }

        public string Letter
        {
            get {
                // 'Letter' is only got upon a successful Key Press
                DoKeyPress();
                return letter;
            }
        }
    }
}