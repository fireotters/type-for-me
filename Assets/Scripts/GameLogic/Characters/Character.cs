using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    // Characters can have multiple arms. Use this character script to control each character's arm behaviour.
    [SerializeField] private Arm[] _armObjects;
    [Space(20)]
    [SerializeField] private int _minPokeSpeed;
    [SerializeField] private int _maxPokeSpeed;
    [Space(20)]
    [SerializeField] private int _minHeightAfterPoke;
    [SerializeField] private int _maxHeightAfterPoke;
    [Space(20)]
    [SerializeField] private int _minSwingHori;
    [SerializeField] private int _maxSwingHori;
    [SerializeField] private int _minSwingVert;
    [SerializeField] private int _maxSwingVert;

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
