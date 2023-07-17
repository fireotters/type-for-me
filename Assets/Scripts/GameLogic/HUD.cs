using GameLogic.Keyboard;
using Other;
using Signals;
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
        private List<Animator> _animators = new();
        private bool _hudShouldBeVisible;

        [Header("Tutorial Options")]
        private GameObject tutorialUi;
        [SerializeField] private bool willTutorialShow;

        // --------------------------------------------------------------------------------------------------------------
        // Start
        // --------------------------------------------------------------------------------------------------------------
        private void Awake()
        {
            if (willTutorialShow)
            {
                tutorialUi = GameObject.Find("/CanvasGameUi").transform.Find("TutorialArea").gameObject;
                if (!tutorialUi)
                    Debug.LogWarning("HUD.cs: Can't find 'CanvasGameUi/TutorialArea'. Tutorial won't be activated/deactivated by this script.");
            }

            // Find the level transition animators of the HUD
            //_animators.Append(chosenCharacter.GetComponent<Animator>());
            _animators.Add(chosenKeyboard.GetComponentInChildren<Animator>());
            //_animators.Append(chosenTypingBox.GetComponent<Animator>());
            //_animators.Append(tutorialUi.GetComponent<Animator>());

            // Start the HUD as invisible
            _hudShouldBeVisible = false;
            SetHudVisbility();
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
                tutorialUi.SetActive(_hudShouldBeVisible);
        }

        private void DisableHudAnimators()
        {
            foreach (Animator anim in _animators)
                anim.enabled = false;
        }
    }
}
