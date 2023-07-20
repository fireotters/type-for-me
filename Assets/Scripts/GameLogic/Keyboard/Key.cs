using System.Collections;
using FMODUnity;
using Signals;
using TMPro;
using UnityEngine;

public class Key : MonoBehaviour
{
    [SerializeField] private string letter;
    [SerializeField] private TMP_Text keyLabel;
    [SerializeField] private SpecialKey specialKeyStatus;

    private SpriteRenderer _sprite;
    private Color _colorUnusable = new(0.7f, 0.7f, 0.7f, 1f);
    private StudioEventEmitter keySound;
    private Animator _anim;
    private readonly CompositeDisposable disposables = new();

    public enum SpecialKey
    {
        None, Unusable, Backspace, Pause
    }

    // --------------------------------------------------------------------------------------------------------------
    // Start & End
    // --------------------------------------------------------------------------------------------------------------
    private void Start()
    {
        _sprite = GetComponent<SpriteRenderer>();
        keySound = GetComponent<StudioEventEmitter>();
        _anim = GetComponent<Animator>();

        if (specialKeyStatus == SpecialKey.Unusable)
        {
            keyLabel.text = "";
            _sprite.color = _colorUnusable;
        }
        else if (specialKeyStatus == SpecialKey.Backspace)
            SignalBus<SignalKeyboardMistakeMade>.Subscribe(StartPulsatingKey).AddTo(disposables);
        else if (specialKeyStatus == SpecialKey.None)
            keyLabel.text = letter.ToUpper();
    }

    private void OnDestroy()
    {
        disposables.Dispose();
    }

    private void DoKeyPress()
    {
        _anim.SetBool("Pressed", true);
        keySound.Play();
        switch (specialKeyStatus)
        {
            case SpecialKey.Backspace:
                Debug.Log("<Key> Pressed Backspace!");
                _anim.SetBool("Pulsating", false);
                SignalBus<SignalKeyboardBackspacePress>.Fire();
                break;
            case SpecialKey.Pause:
                Debug.Log("<Key> Pressed Pause!");
                SignalBus<SignalGamePaused>.Fire(new SignalGamePaused { paused = true });
                break;
            case SpecialKey.Unusable:
                Debug.Log("<Key> Unusable Key.");
                break;
            default:
                Debug.Log($"<Key> Pressed {letter}!");
                SignalBus<SignalKeyboardKeyPress>.Fire(new SignalKeyboardKeyPress { Letter = letter });
                break;
        }

        StartCoroutine(DoKeyUp());
    }

    private IEnumerator DoKeyUp()
    {
        yield return new WaitForSeconds(1f);
        _anim.SetBool("Pressed", false);
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

    private void StartPulsatingKey(SignalKeyboardMistakeMade context)
    {
        _anim.SetBool("Pulsating", true);
    }
}