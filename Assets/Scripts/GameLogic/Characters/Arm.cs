using GameLogic.Keyboard;
using Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arm : MonoBehaviour
{
    // Movement Properties of Arm and PokeOrigin
    private float[] _rangeArmRaiseSpeed = { 0f, 0f };       // Arm: How quick between pokes?
    private float[] _rangeArmRaiseHeight = { 0f, 0f };      // Arm: How high to raise before coming back down?
    private Vector2 _limitPokeNW, _limitPokeSE = new(0, 0); // PokeOrigin: What are the bounds of where PokeOrigin can go?
    private float[] _rangePokeMove = { 0f, 0f };            // PokeOrigin: What's the min/max distance the PokeOrigin can travel per poke?
    private float _armSpeed, _armHeightAfterPoke;
    private Vector2 _pokeDestination;

    // Stopping arm movement. At the apex of an Arm Raise, 'iWantToStopArm' is checked.
    // When unchecked, it'll wait until the next apex of the raise to resume again.
    private bool iWantToStopArm = true;
    private bool armStopped = false;

    // Fingertip
    [SerializeField] private Transform _tFingertip;
    private Vector3 _posFingertip;
    private Vector3 _offsetFromArmToFingertip;
    private float _pokingKeyEndThreshold = 0.1f; // How much of a Y-level above the key be considered a 'Press'?
    private bool _isPokingKey;

    // PokeOrigin. Moving this around will adjust where the fingertip pokes
    [SerializeField] private Transform _pokeOrigin;
    private Vector3 _posPokeOrigin;
    private Vector3 _baseSizeForShadow = new Vector2(0.15f, 0.15f);

    private readonly CompositeDisposable _disposables = new();

    private void Start()
    {
        // The Arm's fingertip is offset slightly 0,0 of the Arm is.
        _offsetFromArmToFingertip = -_tFingertip.localPosition;

        // Attempt to stop the arm when asked
        SignalBus<SignalArmStopMovement>.Subscribe(StopArm).AddTo(_disposables);
    }

    private void Update()
    {
        // Set positions for the fingertip's current location, and where the poke origin is.
        _posFingertip = _tFingertip.position;
        _posPokeOrigin = _pokeOrigin.position;

        RaiseLowerArm();
        if (!armStopped)
            CheckIfPokeFinished();
            MovePokeOrigin();
    }

    private void RaiseLowerArm()
    {
        float howHighIsArmRaised = -Mathf.Sin(Time.time * _armSpeed); // Reverse the sinwave so levels start with hand in the air

        // Stop Arm logic.
        if (iWantToStopArm && armStopped)
            // If we want to stop arm, and it's stopped... Keep it stopped.
            return;
        else if (iWantToStopArm && howHighIsArmRaised > 0.99f)
        {
            // If we want to stop arm, but it isn't yet... Continue to process movements, and wait til the hand is raised to above .99f
            armStopped = true;
            return;
        }
        else if (!iWantToStopArm && armStopped)
        {
            // If we want to resume arm, and it's stopped... Wait til the sin wave would be above .99f before resuming. Ensures smooth transition
            if (howHighIsArmRaised > 0.99f)
                armStopped = false;
            else
                return;
        }

        // Deciding Arm coordinates
        float newX = _posPokeOrigin.x
            + _offsetFromArmToFingertip.x;
        float newY = howHighIsArmRaised * _armHeightAfterPoke
            + _posPokeOrigin.y // Compensate for where the pokeOrigin is moved to
            + _armHeightAfterPoke // Ensure the bottom of the poke is at pokeOrigin by adding the sin wave's amplitude
            + _offsetFromArmToFingertip.y;

        // Move arm
        transform.position = new Vector2(newX, newY);
        // Make shadow grow/shrink
        float howFarFromKeyIsFinger = -(howHighIsArmRaised - 1f) / 3f;
        _pokeOrigin.localScale = _baseSizeForShadow + (_baseSizeForShadow * howFarFromKeyIsFinger);

    }

    private void CheckIfPokeFinished()
    {
        if (_posFingertip.y < _posPokeOrigin.y + _pokingKeyEndThreshold)
            IsPokingKey(true);
        else
            IsPokingKey(false);
    }

    private void IsPokingKey(bool pokeState) {
        if (pokeState == _isPokingKey)
            return;
        _isPokingKey = pokeState;

        // Only trigger these actions once the boolean changes. All other frames are ignored until bool changes again.
        if (_isPokingKey)
            PokeTarget();
        else
            Invoke(nameof(SetNewPropertiesOnRaise), 0.5f);
            
    }

    public void StopArm(SignalArmStopMovement context)
    {
        iWantToStopArm = context.iWantToStopArm;
    }

    private void MovePokeOrigin()
    {
        // Move the PokeOrigin, taking into consideration how fast the arm is and how far the journey is
        float distance = Vector2.Distance(_pokeOrigin.position, _pokeDestination);
        float step = _armSpeed * distance * Time.deltaTime;
        _pokeOrigin.position = Vector2.MoveTowards(_pokeOrigin.position, _pokeDestination, step);
    }

    private void SetNewPropertiesOnRaise()
    {
        _armSpeed = Random.Range(_rangeArmRaiseSpeed[0], _rangeArmRaiseSpeed[1]);
        _armHeightAfterPoke = Random.Range(_rangeArmRaiseHeight[0], _rangeArmRaiseHeight[1]);

        float _pokeHoriDest = Random.Range(_limitPokeNW.x, _limitPokeSE.x);
        float _pokeVertDest = Random.Range(_limitPokeNW.y, _limitPokeSE.y);

        _pokeDestination = new Vector2(_pokeHoriDest, _pokeVertDest);
    }

    public void FirstTimeSetProperties(float[] rangeArmRaiseSpeed, float[] rangeArmRaiseHeight, Vector2 limitPokeNW, Vector2 limitPokeSE, float[] rangePokeMove)
    {
        _rangeArmRaiseSpeed = rangeArmRaiseSpeed;
        _rangeArmRaiseHeight = rangeArmRaiseHeight;
        _limitPokeNW = limitPokeNW;
        _limitPokeSE = limitPokeSE;
        _rangePokeMove = rangePokeMove;

        SetNewPropertiesOnRaise();
        _pokeDestination = new Vector2(0, 0); // First poke is always at 0,0
    }

    private void PokeTarget()
    {
        int layerMask = LayerMask.GetMask("Keys"); // Only hit Key layer objects
        RaycastHit2D hit = Physics2D.Raycast(_posPokeOrigin, -Vector2.up, 1.0f, layerMask);
        if (hit.collider != null)
        {
            Key key = hit.collider.GetComponent<Key>();
            if (key)
                key.KeyPress();
        }
    }
}
