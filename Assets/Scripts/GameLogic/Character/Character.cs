using Signals;
using UnityEngine;

namespace GameLogic.Character
{
    public class Character : MonoBehaviour
    {
        // Characters can have multiple arms. Use this character script to control each character's arm behaviour.
        [SerializeField] private Arm.Arm _arm;
        [Space(20)]
        [Header("Arm: How fast to move up/down")]
        [SerializeField] private float _minArmRaiseSpeed;
        [SerializeField] private float _maxArmRaiseSpeed;
        [Space(20)]
        [Header("Arm: How far to move up/down")]
        [SerializeField] private float _minArmRaiseHeight;
        [SerializeField] private float _maxArmRaiseHeight;
        [Space(20)]
        [Header("Use the PokeRangeNW/SE gameobjects to restrict the maximum range of the PokeDot")]
        [SerializeField] private Transform _limitPokeRangeNW;
        [SerializeField] private Transform _limitPokeRangeSE;
        private Vector3 _typingBoxModifierVerB = new (0, 1.8f, 0);
        private Vector2 _coordVerA_NW, _coordVerA_SE, _coordVerB_NW, _coordVerB_SE; // Save coords to memory

        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            SignalBus<SignalSettingsChange>.Subscribe(FlipDisplaySig).AddTo(_disposables);
            _coordVerA_NW = _limitPokeRangeNW.position;
            _coordVerA_SE = _limitPokeRangeSE.position;
            _coordVerB_NW = _limitPokeRangeNW.position + _typingBoxModifierVerB;
            _coordVerB_SE = _limitPokeRangeSE.position + _typingBoxModifierVerB;
            CheckFlipDisplay();

            float[] rangeArmRaiseSpeed = { _minArmRaiseSpeed, _maxArmRaiseSpeed };
            float[] rangeArmRaiseHeight = { _minArmRaiseHeight, _maxArmRaiseHeight };

            _arm.FirstTimeSetProperties(rangeArmRaiseSpeed, rangeArmRaiseHeight);
        }

        private void CheckFlipDisplay()
        {
            // When the TypingPrompt is at the top, shift the PokeDot limits down by 1.8f. And vice-versa.
            if (PlayerPrefs.GetInt("TypePrompt_IsTop") == 1)
                _arm.pokedot.InitLimits(_coordVerA_NW, _coordVerA_SE);
            else if (PlayerPrefs.GetInt("TypePrompt_IsTop") == 0)
                _arm.pokedot.InitLimits(_coordVerB_NW, _coordVerB_SE);
        }

        private void FlipDisplaySig(SignalSettingsChange context)
        {
            CheckFlipDisplay();
        }

        public void LevelTransitionEnd_TellArmToRaiseAway()
        {
            _arm.LevelTransitionEnd_RaiseAway();
        }
}
}
