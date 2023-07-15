using GameLogic.Keyboard;
using Other;
using UnityEngine;

namespace GameLogic
{
    public class HUD : MonoBehaviour
    {
        [Header("Drag your chosen Prefabs into this HUD parent,\nthen drag those references here")]
        [SerializeField] private Character.Character chosenCharacter;
        [SerializeField] private GameObject chosenKeyboard;
        [SerializeField] private TypingBox chosenTypingBox;

        [Header("Tutorial Options")]
        [SerializeField] private GameObject tutorialUi;
        [SerializeField] private bool willTutorialShow;

        // --------------------------------------------------------------------------------------------------------------
        // 
        // --------------------------------------------------------------------------------------------------------------

        public void HudIsVisible(bool state)
        {
            chosenCharacter.gameObject.SetActive(state);
            chosenKeyboard.gameObject.SetActive(state);
            chosenTypingBox.typingUi.SetActive(state);
            if (willTutorialShow)
                tutorialUi.SetActive(state);
        }
    }
}
