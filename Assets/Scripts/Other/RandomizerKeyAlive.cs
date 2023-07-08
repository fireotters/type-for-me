using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizerKeyAlive : MonoBehaviour
{
    [SerializeField] bool enableSound;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Animator>().Play("Idle", -1, Random.Range(0.0f, 1.0f));
    }

    public void Click()
    {
        if(enableSound)
            gameObject.GetComponent<FMODUnity.StudioEventEmitter>().Play();
    }
}
