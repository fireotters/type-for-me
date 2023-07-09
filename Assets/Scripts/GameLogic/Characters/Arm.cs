using GameLogic.Keyboard;
using Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arm : MonoBehaviour
{
    // Movement Properties. Each has a min and max assigned by 'Character' script.
    private float[] _rangePokeSpeed = { 0f, 0f };        // How quick between pokes
    private float[] _rangeHeightAfterPoke = { 0f, 0f };  // How high the arm will raise before coming back down
    private float[] _rangeSwingHori = { 0f, 0f };        // How far from the center will the finger deviate per poke?
    private float[] _rangeSwingVert = { 0f, 0f };
    private float _pokeSpeed, _heightAfterPoke, _swingHori, _swingVert;

    // PokeOrigin. Moving this around will adjust where the fingertip pokes.
    [SerializeField] private Transform _pokeOrigin;
    private Vector3 _posPokeOrigin;
    private Vector3 _baseSizeForShadow = new Vector2(0.15f, 0.15f);
    private Vector3 _offsetForFingertip;

    // Checking whether fingertip is currently pressing a key
    private Vector3 _posFingertip;
    private float _pokeEndThreshold = 0.2f;
    private bool _isPokingKey;

    private void Start()
    {
        // The sprite's fingertip is offset slightly from where 0,0 actually is.
        _offsetForFingertip = transform.position;
    }

    private void Update()
    {
        // Set positions for the fingertip's current location, and where the poke origin is.
        _posFingertip = transform.position - _offsetForFingertip;
        _posPokeOrigin = _pokeOrigin.position;

        RaiseLowerArm();
        CheckIfPokeFinished();
    }

    private void RaiseLowerArm()
    {
        float howHighIsArmRaised = Mathf.Sin(Time.time * _pokeSpeed);
        float newX = _posPokeOrigin.x
            + _offsetForFingertip.x;
        float newY = howHighIsArmRaised * _heightAfterPoke
            + _posPokeOrigin.y // Compensate for where the pokeOrigin is moved to
            + _heightAfterPoke // Ensure the bottom of the poke is at pokeOrigin by adding the sin wave's amplitude
            + _offsetForFingertip.y;

        // Move arm
        transform.position = new Vector2(newX, newY);
        // Make shadow grow/shrink
        float howFarFromKeyIsFinger = -(howHighIsArmRaised - 1f) / 3f;
        _pokeOrigin.localScale = _baseSizeForShadow + (_baseSizeForShadow * howFarFromKeyIsFinger);

    }

    private void CheckIfPokeFinished()
    {
        if (_posFingertip.y < _posPokeOrigin.y + _pokeEndThreshold)
            IsPokingKey(true);
        else
            IsPokingKey(false);
    }

    private void IsPokingKey(bool pokeState) {
        if (pokeState == _isPokingKey)
            return;
        _isPokingKey = pokeState;

        // Only trigger the Signal once, until the bool is reset later.
        if (_isPokingKey)
            PokeTarget();
    }

    private void SetNewPropertiesOnRaise()
    {
        _pokeSpeed = Random.Range(_rangePokeSpeed[0], _rangePokeSpeed[1]);
        _heightAfterPoke = Random.Range(_rangeHeightAfterPoke[0], _rangeHeightAfterPoke[1]);
        _swingHori = Random.Range(_rangeSwingHori[0], _rangeSwingHori[1]);
        _swingVert = Random.Range(_rangeSwingVert[0], _rangeSwingVert[1]);
    }

    public void FirstTimeSetProperties(float[] rangePokeSpeed, float[] rangeHeightAfterPoke, float[] rangeSwingHori, float[] rangeSwingVert)
    {
        _rangePokeSpeed = rangePokeSpeed;
        _rangeHeightAfterPoke = rangeHeightAfterPoke;
        _rangeSwingHori = rangeSwingHori;
        _rangeSwingVert = rangeSwingVert;

        _pokeSpeed = Random.Range(_rangePokeSpeed[0], _rangePokeSpeed[1]);
        _heightAfterPoke = Random.Range(_rangeHeightAfterPoke[0], _rangeHeightAfterPoke[1]);
        _swingHori = Random.Range(_rangeSwingHori[0], _rangeSwingHori[1]);
        _swingVert = Random.Range(_rangeSwingVert[0], _rangeSwingVert[1]);
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
