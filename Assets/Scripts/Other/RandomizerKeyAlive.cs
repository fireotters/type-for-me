using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizerKeyAlive : MonoBehaviour
{
    [SerializeField] bool enableSound;

    void OnEnable()
    {
        gameObject.GetComponent<Animator>().Play("Idle", -1, Random.Range(0.0f, 1.0f));
    }

    public void Click()
    {
        if(enableSound)
            gameObject.GetComponent<FMODUnity.StudioEventEmitter>().Play();
    }
}
