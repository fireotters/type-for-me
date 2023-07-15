using FMODUnity;
using UnityEngine;

namespace Other
{
    public class RandomizerKeyAlive : MonoBehaviour
    {
        [SerializeField] private bool enableSound;

        private void OnEnable()
        {
            GetComponent<Animator>().Play("Idle", -1, Random.Range(0.0f, 1.0f));
        }

        public void Click()
        {
            if (enableSound)
                GetComponent<StudioEventEmitter>().Play();
        }
    }
}
