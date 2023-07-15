using System.Collections;
using UnityEngine;
using DG.Tweening;

namespace Other
{
    public class Scaler : MonoBehaviour
    {
        [SerializeField] private float scaleSpeed;
        [SerializeField] private float initialScaleSpeed;
        [SerializeField] private float amplitude;
        public AnimationCurve curve;
        private Vector3 _initPos;

        private void Start()
        {
            var currentTransform = transform;
            
            _initPos = currentTransform.localScale;
            currentTransform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, initialScaleSpeed).OnComplete(() => StartCoroutine(Scale()));
        }

        IEnumerator Scale()
        {
            float insideTimer = 0;
            while (true)
            {
                transform.localScale = curve.Evaluate(insideTimer) * Vector3.up * amplitude + _initPos;
                insideTimer += Time.deltaTime;

                if (insideTimer > scaleSpeed)
                    insideTimer -= scaleSpeed;

                yield return null;
            }
        }
    }
}

