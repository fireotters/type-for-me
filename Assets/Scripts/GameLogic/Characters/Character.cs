using UnityEngine;

public class Character : MonoBehaviour
{
    // Characters can have multiple arms. Use this character script to control each character's arm behaviour.
    [SerializeField] private Arm _arm;
    [Space(20)]
    [Header("Arm: How fast to move up/down")]
    [SerializeField] private float _minPokeSpeed;
    [SerializeField] private float _maxPokeSpeed;
    [Space(20)]
    [Header("Arm: How far to move up/down")]
    [SerializeField] private float _minHeightAfterPoke;
    [SerializeField] private float _maxHeightAfterPoke;
    [Space(20)]
    [Header("Use the PokeRangeNW/SE gameobjects to restrict the maximum range of the PokeOrigin")]
    [SerializeField] private Transform _limitPokeRangeNW;
    [SerializeField] private Transform _limitPokeRangeSE;
    [Space(20)]
    [Header("This is how far the PokeOrigin is allowed to move per poke.")]
    [SerializeField] private float _minPokeMove;
    [SerializeField] private float _maxPokeMove;

    private void Start()
    {
        float[] rangePokeSpeed = { _minPokeSpeed, _maxPokeSpeed };
        float[] rangeHeightAfterPoke = { _minHeightAfterPoke, _maxHeightAfterPoke };
        Vector2 limitPokeNW = _limitPokeRangeNW.position;
        Vector2 limitPokeSE = _limitPokeRangeSE.position;
        float[] rangePokeMove = { _minPokeMove, _maxPokeMove };


        _arm.FirstTimeSetProperties(rangePokeSpeed, rangeHeightAfterPoke, limitPokeNW, limitPokeSE, rangePokeMove);
    }
}
