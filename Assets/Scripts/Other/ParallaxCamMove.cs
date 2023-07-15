using UnityEngine;

namespace Other
{
    public class ParallaxCamMove : MonoBehaviour
    {
        [SerializeField] private float vel = 0.005f;

        private void Update()
        {
            var currentPos = transform.position;
            
            transform.position = new Vector3(currentPos.x + vel, currentPos.y, currentPos.z);
        }
    }
}