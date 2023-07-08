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
        foreach (Arm arm in _armObjects)
        {
            float randPokeSpeed = Random.Range(_minPokeSpeed, _maxPokeSpeed);
            float randHeightAfterPoke = Random.Range(_minHeightAfterPoke, _maxHeightAfterPoke);
            float randSwingHori = Random.Range(_minSwingHori, _maxSwingHori);
            float randSwingVert = Random.Range(_minSwingVert, _maxSwingVert);
            arm.SetMovementProperties(randPokeSpeed, randHeightAfterPoke, randSwingHori, randSwingVert);
        }
    }
}
