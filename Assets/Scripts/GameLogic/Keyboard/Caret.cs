using Signals;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Caret : MonoBehaviour
{
    [SerializeField] TextMeshPro _inputTextTmp;
    private Renderer _rend;
    private bool state;
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
        print(_inputTextTmp.textInfo.characterInfo.Length);
        Vector3 localCoord = _inputTextTmp.textInfo.characterInfo[^0].bottomRight;
        Vector3 worldCoord = _inputTextTmp.transform.TransformPoint(localCoord);
        transform.position = worldCoord;
    }

    //private void Press (SignalKeyboardKeyPress context)
    //{
    //    print("frick yeah");
    //    MoveCaret();
    //}
    //private void BackPress (SignalKeyboardBackspacePress context)
    //{
    //    print("frick no");
    //    MoveCaret();
    //}

}
