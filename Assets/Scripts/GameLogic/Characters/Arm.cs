using GameLogic.Keyboard;
using Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arm : MonoBehaviour
{
    // Movement Properties of Arm and PokeOrigin
    private float[] _rangeArmSpeed = { 0f, 0f };            // Arm: How quick between pokes?
    private float[] _rangeArmHeightAfterPoke = { 0f, 0f };  // Arm: How high to raise before coming back down?
    private Vector2 _limitPokeNW, _limitPokeSE = new(0, 0); // PokeOrigin: What are the bounds of where PokeOrigin can go?
    private float[] _rangePokeMove = { 0f, 0f };            // PokeOrigin: What's the min/max distance the PokeOrigin can travel per poke?
    private float _armSpeed, _armHeightAfterPoke;
    private Vector2 _pokeDestination;

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

    private void Start()
    {
        // The Arm's fingertip is offset slightly 0,0 of the Arm is.
        _offsetFromArmToFingertip = -_tFingertip.localPosition;
    }

    private void Update()
    {
        // Set positions for the fingertip's current location, and where the poke origin is.
        _posFingertip = _tFingertip.position;
        _posPokeOrigin = _pokeOrigin.position;

        RaiseLowerArm();
        CheckIfPokeFinished();
        MovePokeOrigin();
    }

    private void RaiseLowerArm()
    {
        float howHighIsArmRaised = -Mathf.Sin(Time.time * _armSpeed); // Reverse the sinwave so levels start with hand in the air
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

    private void MovePokeOrigin()
    {
        var step = 10 * Time.deltaTime;
        _pokeOrigin.position = Vector2.MoveTowards(_pokeOrigin.position, _pokeDestination, step);
    }

    private void SetNewPropertiesOnRaise()
    {
        _armSpeed = Random.Range(_rangeArmSpeed[0], _rangeArmSpeed[1]);
        _armHeightAfterPoke = Random.Range(_rangeArmHeightAfterPoke[0], _rangeArmHeightAfterPoke[1]);

        float _pokeHoriDest = Random.Range(_limitPokeNW.x, _limitPokeSE.x);
        float _pokeVertDest = Random.Range(_limitPokeNW.y, _limitPokeSE.y);
        _pokeDestination = new Vector2(_pokeHoriDest, _pokeVertDest);
    }

    public void FirstTimeSetProperties(float[] rangeArmSpeed, float[] rangeArmHeightAfterPoke, Vector2 limitPokeNW, Vector2 limitPokeSE, float[] rangePokeMove)
    {
        _rangeArmSpeed = rangeArmSpeed;
        _rangeArmHeightAfterPoke = rangeArmHeightAfterPoke;
        _limitPokeNW = limitPokeNW;
        _limitPokeSE = limitPokeSE;
        _rangePokeMove = rangePokeMove;
        SetNewPropertiesOnRaise();
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
