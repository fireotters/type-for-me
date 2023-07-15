using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Other
{
    public class SineWaveMover : MonoBehaviour
    {
        [SerializeField] private float oscilationSpeed;
        [SerializeField] private float amplitude;
        private Vector3 _initPos;
        private float _timer = 0;

        private void Start()
        {
            _initPos = transform.localPosition;
        }

        private void Update()
        {
            _timer += Time.deltaTime * oscilationSpeed;
            transform.localPosition = amplitude * Mathf.Sin(_timer) * Vector3.up + _initPos;
        }
    }
}
