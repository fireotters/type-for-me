using GameLogic.Keyboard;
using Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arm : MonoBehaviour
{
    // Movement Properties of Arm and PokeOrigin
    private float[] _rangeArmSpeed = { 0f, 0f };            // How quick between pokes
    private float[] _rangeArmHeightAfterPoke = { 0f, 0f };  // How high the arm will raise before coming back down
    private float[] _rangePokeHori = { 0f, 0f };            // How far from the center will the PokeOrigin deviate per poke?
    private float[] _rangePokeVert = { 0f, 0f };
    private float _armSpeed, _armHeightAfterPoke, _pokeHoriDest, _pokeVertDest;

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
            SetNewPropertiesOnRaise();
    }

    private void SetNewPropertiesOnRaise()
    {
        _armSpeed = Random.Range(_rangeArmSpeed[0], _rangeArmSpeed[1]);
        _armHeightAfterPoke = Random.Range(_rangeArmHeightAfterPoke[0], _rangeArmHeightAfterPoke[1]);
        _pokeHoriDest = Random.Range(_rangePokeHori[0], _rangePokeHori[1]);
        _pokeVertDest = Random.Range(_rangePokeVert[0], _rangePokeVert[1]);
    }

    public void FirstTimeSetProperties(float[] rangePokeSpeed, float[] rangeHeightAfterPoke, float[] rangeSwingHori, float[] rangeSwingVert)
    {
        _rangeArmSpeed = rangePokeSpeed;
        _rangeArmHeightAfterPoke = rangeHeightAfterPoke;
        _rangePokeHori = rangeSwingHori;
        _rangePokeVert = rangeSwingVert;
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
