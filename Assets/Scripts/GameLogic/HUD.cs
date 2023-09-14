using GameLogic.Keyboard;
using Other;
using Signals;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace GameLogic
{
    public class HUD : MonoBehaviour
    {
        [Header("Drag your chosen Prefabs into this HUD parent,\nthen drag those references here")]
        [SerializeField] private Character.Character chosenCharacter;
        [SerializeField] private GameObject chosenKeyboard;
        [SerializeField] private TypingBox chosenTypingBox;
        [SerializeField] private Transform chosenBackground;
        private List<Animator> _animators = new();
        private bool _hudShouldBeVisible;

        [Header("Tutorial Options")]
        private GameObject _tutorialUi;
        private GameObject _tutBtnTop, _tutBtnBot;
        [SerializeField] private bool willTutorialShow;

        private readonly CompositeDisposable _disposables = new();

        // --------------------------------------------------------------------------------------------------------------
        // Start & End
        // --------------------------------------------------------------------------------------------------------------
        private void Awake()
        {
            if (willTutorialShow)
            {
                _tutorialUi = GameObject.Find("/CanvasGameUi").transform.Find("TutorialArea").gameObject;
                _tutBtnTop = _tutorialUi.transform.Find("btn-bg-top").gameObject;
                _tutBtnBot = _tutorialUi.transform.Find("btn-bg-bottom").gameObject;
                if (!_tutorialUi)
                    Debug.LogWarning("HUD.cs: Can't find 'CanvasGameUi/TutorialArea'. Tutorial won't be activated/deactivated by this script.");
            }

            // Start the HUD as invisible
            _hudShouldBeVisible = false;
            SetHudVisbility();

            // Alter level transition animations, depending if TypingBox is on top/bottom of the HUD
            _animators.Add(chosenKeyboard.GetComponentInChildren<Animator>());
            _animators.Add(chosenTypingBox.typingUi.GetComponent<Animator>());
            SignalBus<SignalSettingsChange>.Subscribe(FlipDisplaySig).AddTo(_disposables);
            SignalBus<SignalGameRetryFromCheckpoint>.Subscribe(RetryLevelFromCheckpoint).AddTo(_disposables);
            CheckFlipDisplay();
        }
        private void OnDestroy()
        {
            _disposables.Dispose();
        }


        // --------------------------------------------------------------------------------------------------------------
        // HUD Visibility
        // --------------------------------------------------------------------------------------------------------------

        public void HudLevelTransition(bool state)
        {
            _hudShouldBeVisible = state;
            if (_hudShouldBeVisible)
            {
                // Make HUD visible, activate animators, play animation, then deactivate animators
                SetHudVisbility();
                foreach (Animator anim in _animators)
                {
                    anim.enabled = true;
                    anim.SetBool("typingIsTop", PlayerPrefs.GetInt("TypePrompt_IsTop") == 1);
                    anim.Play("SlideIn");
                }
                Invoke(nameof(DisableHudAnimators), 1f);
            }
            else
            {
                // Activate animators, play animation, then deactivate animators and make HUD invisible
                foreach (Animator anim in _animators)
                {
                    anim.enabled = true;
                    anim.SetBool("typingIsTop", PlayerPrefs.GetInt("TypePrompt_IsTop") == 1);
                    anim.Play("SlideOut");
                }
                Invoke(nameof(DisableHudAnimators), 1f);
                Invoke(nameof(SetHudVisbility), 1f);
            }
        }

        private void SetHudVisbility()
        {
            chosenCharacter.gameObject.SetActive(_hudShouldBeVisible);
            chosenKeyboard.gameObject.SetActive(_hudShouldBeVisible);
            chosenTypingBox.typingUi.SetActive(_hudShouldBeVisible);
            if (willTutorialShow)
                _tutorialUi.SetActive(_hudShouldBeVisible);
        }

        private void DisableHudAnimators()
        {
            foreach (Animator anim in _animators)
                anim.enabled = false;
        }

        public void TellArmToRaiseQuickly()
        {
            chosenCharacter.LevelTransitionEnd_TellArmToRaiseAway();
        }

        private void RetryLevelFromCheckpoint(SignalGameRetryFromCheckpoint s)
        {
            Animator anim = chosenKeyboard.GetComponentInChildren<Animator>();
            anim.enabled = true;
            anim.SetBool("typingIsTop", PlayerPrefs.GetInt("TypePrompt_IsTop") == 1);
            anim.Play("SlideIn");
            Invoke(nameof(DisableHudAnimators), 1f);
        }


        // --------------------------------------------------------------------------------------------------------------
        // Misc
        // --------------------------------------------------------------------------------------------------------------
        private void CheckFlipDisplay()
        {
            // When the TypingPrompt is at the top, shift Background down by 1.8f. And vice-versa.
            var bgTra = chosenBackground.transform.position;
            if (PlayerPrefs.GetInt("TypePrompt_IsTop") == 1)
                chosenBackground.transform.position = new Vector3(bgTra.x, -1.8f, bgTra.z);
            else if (PlayerPrefs.GetInt("TypePrompt_IsTop") == 0)
                chosenBackground.transform.position = new Vector3(bgTra.x, 0f, bgTra.z);

            if (willTutorialShow)
            {
                // Flip tutorial button
                if (PlayerPrefs.GetInt("TypePrompt_IsTop") == 1)
                {
                    _tutBtnTop.SetActive(true);
                    _tutBtnBot.SetActive(false);
                }
                else if (PlayerPrefs.GetInt("TypePrompt_IsTop") == 0)
                {
                    _tutBtnTop.SetActive(false);
                    _tutBtnBot.SetActive(true);
                }
            }
        }

        private void FlipDisplaySig(SignalSettingsChange context)
        {
            CheckFlipDisplay();
        }
    }
}
