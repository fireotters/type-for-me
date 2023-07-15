using UnityEngine;

namespace Other
{
    public class Parallax : MonoBehaviour
    {
        private float _length, _startpos;
        [SerializeField] private GameObject cam;
        [SerializeField] private float parallaxEffect;

        private void Start()
        {
            _startpos = transform.position.x;
            _length = GetComponent<SpriteRenderer>().bounds.size.x;
        }

        private void FixedUpdate()
        {
            var cameraPos = cam.transform.position;
            var currentPos = transform.position;
            
            var temp = (cameraPos.x * (1-parallaxEffect));
            var dist = (cameraPos.x * parallaxEffect);

            transform.position = new Vector3(_startpos + dist, currentPos.y, currentPos.z);

            if (temp > _startpos + _length) 
                _startpos += _length;
            else if (temp < _startpos - _length) 
                _startpos -= _length;
        }
    }
}
