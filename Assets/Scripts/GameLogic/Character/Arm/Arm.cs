using Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arm : MonoBehaviour
{
    // Movement Properties
    private float[] _rangeArmRaiseSpeed = { 0f, 0f };       // Arm: How quick between pokes?
    private float[] _rangeArmRaiseHeight = { 0f, 0f };      // Arm: How high to raise before coming back down?
    private float _armSpeed, _armHeightAfterPoke;

    // Stopping Movement. At the apex of a Raise, 'iWantToStopArm' is checked.
    // When unchecked, it'll wait until the next apex of the raise to resume again.
    public bool iWantToStopArm = true;
    public bool armStopped = true;

    [SerializeField] private PokeDot _pokedot;      // Arm will follow wherever the PokeDot moves to
    [SerializeField] private Fingertip _fingertip;  // Fingertip will check if it's touching the PokeDot, then press any Keys it finds
    private Vector3 _fingertipOffset;

    private readonly CompositeDisposable _disposables = new();


    // --------------------------------------------------------------------------------------------------------------
    // Start & End
    // --------------------------------------------------------------------------------------------------------------
    private void Start()
    {
        _fingertipOffset = -_fingertip.transform.localPosition;

        SignalBus<SignalArmStopMovement>.Subscribe(StopArm).AddTo(_disposables);
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
        RaiseLowerArm();
        if (!armStopped)
            _fingertip.CheckIfPoking(_pokedot.Pos);
            _pokedot.Move(_armSpeed);
    }


    // --------------------------------------------------------------------------------------------------------------
    // Arm Per-Frame Movements
    // --------------------------------------------------------------------------------------------------------------
    private void RaiseLowerArm()
    {
        float raiseSin = -Mathf.Sin(Time.time * _armSpeed);
        float raiseHeight = (raiseSin * _armHeightAfterPoke) + _armHeightAfterPoke; // Add ArmHeight so the sin wave bottoms out where the PokeDot is
        if (ArmShouldBeStopped(raiseSin))
            return;

        // Move Arm (to where PokeDot is, and offset depending on where Fingertip is & how high the raiseHeight is)
        float newX = _pokedot.Pos.x + _fingertipOffset.x;
        float newY = _pokedot.Pos.y + _fingertipOffset.y + raiseHeight;
        transform.position = new Vector2(newX, newY);

        _pokedot.ChangeSize(raiseSin);
    }

    private bool ArmShouldBeStopped(float raiseHeight)
    {
        // If we want to stop arm, and it's stopped... Keep it stopped.
        if (iWantToStopArm && armStopped)
            return true;

        // If we want to stop arm, but it isn't yet... Continue to process movements, and wait til the hand is raised to above .99f
        else if (iWantToStopArm && raiseHeight > 0.99f)
        {
            armStopped = true;
            return true;
        }

        // If we want to resume arm, and it's stopped... Wait til the sin wave is above .99f before resuming. Ensures smooth transition
        else if (!iWantToStopArm && armStopped)
        {
            if (raiseHeight > 0.99f)
                armStopped = false;
            else
                return true;
        }

        return false;
    }


    // --------------------------------------------------------------------------------------------------------------
    // Setting Properties
    // --------------------------------------------------------------------------------------------------------------

    public void StopArm(SignalArmStopMovement context)
    {
        iWantToStopArm = context.iWantToStopArm;
    }
    
    public void SetNewPropertiesOnRaise()
    {
        _armSpeed = Random.Range(_rangeArmRaiseSpeed[0], _rangeArmRaiseSpeed[1]);
        _armHeightAfterPoke = Random.Range(_rangeArmRaiseHeight[0], _rangeArmRaiseHeight[1]);
        _pokedot.SetDestination();
    }

    public void FirstTimeSetProperties(float[] rangeArmRaiseSpeed, float[] rangeArmRaiseHeight, Vector2 limitPokeNW, Vector2 limitPokeSE)
    {
        _rangeArmRaiseSpeed = rangeArmRaiseSpeed;
        _rangeArmRaiseHeight = rangeArmRaiseHeight;
        _pokedot.InitLimits(limitPokeNW, limitPokeSE);

        SetNewPropertiesOnRaise();
    }

}
