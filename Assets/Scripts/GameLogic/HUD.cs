using GameLogic.Keyboard;
using Other;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [Header("Drag your chosen Prefabs for the level")]
    [SerializeField] private Character chosenCharacter;
    [SerializeField] private Draggable chosenKeyboard;
    [SerializeField] private TypingBox chosenTypingBox;

    private void Awake()
    {
        Instantiate(chosenCharacter, gameObject.transform);
        Instantiate(chosenKeyboard, gameObject.transform);
        Instantiate(chosenTypingBox, gameObject.transform);
    }
}
