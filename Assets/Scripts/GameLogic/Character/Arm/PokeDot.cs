using UnityEngine;

namespace GameLogic.Character.Arm
{
    public class PokeDot : MonoBehaviour
    {
        private Vector3 _pos;
        private Vector3 _dotBaseSize = new Vector2(0.15f, 0.15f);

        private Vector2 _limitPokeNW, _limitPokeSE = new(0, 0); // PokeDot: What are the bounds of where PokeDot can go?
        private Vector2 _destination;
        private bool firstPoke = true;


        // --------------------------------------------------------------------------------------------------------------
        // Per-Frame Updates
        // --------------------------------------------------------------------------------------------------------------
        private void Update()
        {
            _pos = transform.position;
        }


        // --------------------------------------------------------------------------------------------------------------
        // Positioning
        // --------------------------------------------------------------------------------------------------------------
        public Vector2 Pos
        {
            get { return _pos; }
        }

        public void Move(float armSpeed)
        {
            // Move the PokeDot, taking into consideration how fast the arm is and how far the journey is
            float distance = Vector2.Distance(_pos, _destination);
            float step = armSpeed * distance * Time.deltaTime;
            transform.position = Vector2.MoveTowards(_pos, _destination, step);
        }

        public void SetDestination()
        {
            float x = Random.Range(_limitPokeNW.x, _limitPokeSE.x);
            float y = Random.Range(_limitPokeNW.y, _limitPokeSE.y);

            if (firstPoke)
            {
                _destination = new Vector2(0, 0);
                firstPoke = false;
            }
            else
                _destination = new Vector2(x, y);
        }

        public void InitLimits(Vector2 limitPokeNW, Vector2 limitPokeSE)
        {
            _limitPokeNW = limitPokeNW;
            _limitPokeSE = limitPokeSE;
        }

        public void ChangeSize(float howHighIsArmRaised)
        {
            // Make dot grow/shrink
            float howFarFromKeyIsFinger = -(howHighIsArmRaised - 1f) / 3f;
            transform.localScale = _dotBaseSize + (_dotBaseSize * howFarFromKeyIsFinger);
        }
    }
}