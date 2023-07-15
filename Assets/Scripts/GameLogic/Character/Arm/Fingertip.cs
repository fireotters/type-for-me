using UnityEngine;

namespace GameLogic.Character.Arm
{
    public class Fingertip : MonoBehaviour
    {
        private Vector3 _pos;

        private readonly float
            _pokingKeyEndThreshold = 0.05f; // How much of a Y-level above the key be considered a 'Press'?

        private bool _isPoking;
        private Arm _attachedArm;


        // --------------------------------------------------------------------------------------------------------------
        // Start
        // --------------------------------------------------------------------------------------------------------------
        private void Awake()
        {
            _attachedArm = GetComponentInParent<Arm>();
        }


        // --------------------------------------------------------------------------------------------------------------
        // Per-Frame Updates
        // --------------------------------------------------------------------------------------------------------------
        private void Update()
        {
            _pos = transform.position;
        }


        // --------------------------------------------------------------------------------------------------------------
        // Check if Fingertip is able to poke, what to do when it is poking
        // --------------------------------------------------------------------------------------------------------------
        public void CheckIfPoking(Vector2 posPokeDot)
        {
            if (_pos.y < posPokeDot.y + _pokingKeyEndThreshold)
                IsPoking(true);
            else
                IsPoking(false);
        }

        private void IsPoking(bool pokeState)
        {
            if (pokeState == _isPoking)
                return;
            _isPoking = pokeState;

            // Only trigger these actions once the boolean changes. All other frames are ignored until bool changes again.
            if (_isPoking)
                Poke();
            else
                _attachedArm.Invoke(nameof(_attachedArm.SetNewPropertiesOnRaise), 0.5f);
        }

        private void Poke()
        {
            int layerMask = LayerMask.GetMask("Keys");
            RaycastHit2D hit = Physics2D.Raycast(_pos, -Vector2.up, 1.0f, layerMask);
            if (hit.collider != null)
            {
                Key key = hit.collider.GetComponent<Key>();
                if (key)
                    key.KeyPress();
            }
        }
    }
}