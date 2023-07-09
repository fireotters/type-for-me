using UnityEngine;

public class Character : MonoBehaviour
{
    // Characters can have multiple arms. Use this character script to control each character's arm behaviour.
    [SerializeField] private Arm _arm;
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
        float[] rangeHeightAfterPoke = { _minHeightAfterPoke, _maxHeightAfterPoke };
        float[] rangeSwingHori = { _minSwingHori, _maxSwingHori };
        float[] rangeSwingVert = { _minSwingVert, _maxSwingVert };
        
        _arm.FirstTimeSetProperties(rangePokeSpeed, rangeHeightAfterPoke, rangeSwingHori, rangeSwingVert);
    }
}
