using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    // Characters can have multiple arms. Use this character script to control each character's arm behaviour.
    [SerializeField] private Arm[] _armObjects;
    [Space(20)]
    [SerializeField] private float _minPokeSpeed;
    [SerializeField] private float _maxPokeSpeed;
    [Space(20)]
    [SerializeField] private float _minHeightAfterPoke;
    [SerializeField] private float _maxHeightAfterPoke;
    [Space(20)]
    [SerializeField] private float _minSwingHori;
    [SerializeField] private float _maxSwingHori;
    [SerializeField] private float _minSwingVert;
    [SerializeField] private float _maxSwingVert;

    private void Start()
    {
        float[] rangePokeSpeed = { _minPokeSpeed, _maxPokeSpeed };
        float[] rangeHeightAfterPoke = { _minPokeSpeed, _maxPokeSpeed };
        float[] rangeSwingHori = { _minPokeSpeed, _maxPokeSpeed };
        float[] rangeSwingVert = { _minPokeSpeed, _maxPokeSpeed };
        foreach (Arm arm in _armObjects)
        {
            arm.FirstTimeSetProperties(rangePokeSpeed, rangeHeightAfterPoke, rangeSwingHori, rangeSwingVert);
        }
    }
}
