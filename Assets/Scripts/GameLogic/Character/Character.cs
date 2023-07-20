using GameLogic.Character.Arm;
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
        private Vector3 _modifierForTypingPromptTop = new (0, -0.9f, 0);
        private Vector2 _coordPdNW_top, _coordPdSE_top, _coordPdNW_bot, _coordPdSE_bot; // Save coords to memory

        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            SignalBus<SignalSettingsChange>.Subscribe(FlipDisplaySig).AddTo(_disposables);
            _coordPdNW_bot = _limitPokeRangeNW.position;
            _coordPdSE_bot = _limitPokeRangeSE.position;
            _coordPdNW_top = _limitPokeRangeNW.position + _modifierForTypingPromptTop;
            _coordPdSE_top = _limitPokeRangeSE.position + _modifierForTypingPromptTop;
            CheckFlipDisplay();

            float[] rangeArmRaiseSpeed = { _minArmRaiseSpeed, _maxArmRaiseSpeed };
            float[] rangeArmRaiseHeight = { _minArmRaiseHeight, _maxArmRaiseHeight };

            _arm.FirstTimeSetProperties(rangeArmRaiseSpeed, rangeArmRaiseHeight);
        }

        private void CheckFlipDisplay()
        {
            // When the TypingPrompt is at the top, shift the PokeDot limits down by 1.8f. And vice-versa.
            if (PlayerPrefs.GetInt("TypePrompt_IsTop") == 1)
                _arm.pokedot.InitLimits(_coordPdNW_top, _coordPdSE_top);
            else if (PlayerPrefs.GetInt("TypePrompt_IsTop") == 0)
                _arm.pokedot.InitLimits(_coordPdNW_bot, _coordPdSE_bot);
            else
                Debug.LogError("Character.CheckFlipDisplay() invalid PlayerPrefs state");
        }

        private void FlipDisplaySig(SignalSettingsChange context)
        {
            CheckFlipDisplay();
        }
}
}
