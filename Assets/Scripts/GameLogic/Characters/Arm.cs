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
    private Vector3 _offsetForFingertip;

    // Checking whether fingertip goes offscreen before asking Arm to assign new properties.
    public bool _isOffscreen = false;
    private float _yEdgeOfUpperScreen = 5.5f;

    private void Start()
    {
        // The sprite's fingertip is offset slightly from where 0,0 actually is.
        _offsetForFingertip = transform.position;
    }

    private void Update()
    {
        RaiseLowerArm();
        // CheckIfFingertipOffScreen();
    }

    private void RaiseLowerArm()
    {
        Vector2 _posPokeOrigin = _pokeOrigin.position;
        float newX = _posPokeOrigin.x
            + _offsetForFingertip.x;
        float newY = Mathf.Sin(Time.time * _pokeSpeed) * _heightAfterPoke
            + _posPokeOrigin.y // Compensate for where the pokeOrigin is moved to
            + _heightAfterPoke // Ensure the bottom of the poke is at pokeOrigin by adding the sin wave's amplitude
            + _offsetForFingertip.y;
        transform.position = new Vector2(newX, newY);
    }

    //private void CheckIfFingertipOffScreen()
    //{
    //    // Instead of checking the center of the Arm, check the fingertip's location
    //    Vector2 _posFingerTip = transform.position - _offsetForFingertip;
    //    if (!_isOffscreen && _posFingerTip.y > _yEdgeOfUpperScreen)
    //    {
    //        _isOffscreen = true;
    //        SetNewPropertiesOnRaise();
    //    }
    //    else if (_isOffscreen && _posFingerTip.y < _yEdgeOfUpperScreen)
    //    {
    //        _isOffscreen = false;
    //    }
    //} TODO Reimplement. It's actually better for the fingertip to stay on-screen. The fingertip should move from place to place while the player can see it.

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

    private void PokeCompleted()
    {
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out RaycastHit hit, Mathf.Infinity))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            Debug.Log("Did Hit");
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
            Debug.Log("Did not Hit");
        }
    }

}
