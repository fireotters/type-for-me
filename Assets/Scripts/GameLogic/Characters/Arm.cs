using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arm : MonoBehaviour
{
    [Header("Movement Properties")]
    private float _pokeSpeed; // How quick between pokes
    private float _heightAfterPoke; // How high the arm will raise before coming back down

    // Unimplemented - how far from the center will the finger deviate per poke?
    private float _swingMaxHori;
    private float _swingMaxVert;

    [SerializeField] private Transform _pokeOrigin; // Moving this PokeOrigin object around will adjust where the arm pokes.
    private Vector2 _offsetForFingertip;

    private void Start()
    {
        // The sprite's fingertip is offset slightly from where 0,0 actually is.
        _offsetForFingertip = transform.position;
    }

    private void Update()
    {
        Vector2 _posOrigin = _pokeOrigin.position;
        float newX = _posOrigin.x
            + _offsetForFingertip.x;
        float newY = Mathf.Sin(Time.time * _pokeSpeed) * _heightAfterPoke
            + _posOrigin.y // Compensate for where the pokeOrigin is moved to
            + _heightAfterPoke // Ensure the bottom of the poke is at pokeOrigin by adding the height
            + _offsetForFingertip.y; 
        transform.position = new Vector2(newX, newY);
    }

    public void SetMovementProperties(float pokeSpeed, float perPokeRaiseHeight, float swingMaxHori, float swingMaxVert)
    {
        _pokeSpeed = pokeSpeed;
        _heightAfterPoke = perPokeRaiseHeight;
        _swingMaxHori = swingMaxHori;
        _swingMaxVert = swingMaxVert;
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
