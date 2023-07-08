using System;
using System.Linq;
using System.Collections;
using Signals;
using TMPro;
using UnityEngine;

namespace GameLogic.Keyboard
{
    public class KeyboardReader : MonoBehaviour
    {
        [SerializeField] private string[] phrases;
        [SerializeField] private TMP_Text textPreview;
        [SerializeField] private TMP_Text inputtedText;
        private readonly CompositeDisposable disposables = new();

        [SerializeField] private GameObject progressTracker;
        [SerializeField] private GameObject wordTrackerPrefab;
        
        private void Start()
        {
            SignalBus<SignalKeyboardKeyPress>.Subscribe(ReadFromKeyboard).AddTo(disposables);
            SignalBus<SignalKeyboardBackspacePress>.Subscribe(BackspaceAction).AddTo(disposables);

            if (phrases.Length == 0)
            {
                Debug.LogError("NO PHRASES SET IN INSPECTOR! how tf u gonna play binch");
            }
            else
            {
                var startingXAxisPos = progressTracker.transform.position.x;
                for (var i = 0; i < phrases.Length; i++)
                {
                    var newGameObject 
                        = Instantiate(wordTrackerPrefab, new Vector3(startingXAxisPos, progressTracker.transform.position.y), Quaternion.identity,  progressTracker.transform);
                    startingXAxisPos -= .5f;
                    var spriteRenderer = newGameObject.GetComponent<SpriteRenderer>();
                    spriteRenderer.color = i == phrases.Length - 1 ? Color.white : Color.gray;
                }
                textPreview.text = phrases[0];
            }
        }

        private void CheckForCompletedTest()
        {
            if (textPreview.text.Equals(inputtedText.text))
            {
                Debug.Log("gaming yo yo gaming gaming yo");
                inputtedText.text = $"<color=#4EFF00>{inputtedText.text}</color>";
                StartCoroutine(UpdateGameState());
            }
            else
            {
                Debug.Log("keep goin");
            }
        }

        private IEnumerator UpdateGameState()
        {
            yield return new WaitForSeconds(2f);
            var currentWord = textPreview.text;
            var trackerSpriteRenderers = progressTracker.GetComponentsInChildren<SpriteRenderer>();
            
            if (currentWord.Equals(phrases[^1]))
            {
                Debug.Log("Turns out you DONT suck!");
                var lastTrackerRenderer = trackerSpriteRenderers[0];
                lastTrackerRenderer.color = Color.green;
                SignalBus<SignalGameEnded>.Fire(new SignalGameEnded { result = GameEndCondition.WinType1, score = 0 });
            }
            else
            {
                // last word wasn't reached yet, NEEXT!
                inputtedText.text = string.Empty;
                var nextWordIndex = phrases.ToList().IndexOf(currentWord) + 1;
                textPreview.text = phrases[nextWordIndex];
                
                // also update hud
                // the way these are instance we gotta iterate through the list backwards
                for (var i = trackerSpriteRenderers.Length - 1; i >= 0; i--)
                {
                    var sr = trackerSpriteRenderers[i];
                    if (sr.color.Equals(Color.green))
                        continue;
                    if (sr.color.Equals(Color.white))
                    {
                        // that's the current one we just beat, mark as valid and the next one as current
                        sr.color = Color.green;
                        var nextSpriteRenderer = trackerSpriteRenderers[i - 1];
                        nextSpriteRenderer.color = Color.white;
                        break;
                    }
                }
            }
        }
        
        private void ReadFromKeyboard(SignalKeyboardKeyPress keyPress)
        {
            Debug.Log($"<KeyboardReader> {keyPress.Letter} was pressed!");
            try
            {
                if (keyPress.Letter == "||")
                {
                    Debug.Log("Paused"); // TODO program in pausing signal
                    return;
                }

                var expectedCharacter = textPreview.text[inputtedText.text.Length];
                if (expectedCharacter.ToString() != keyPress.Letter)
                {
                    if (keyPress.Letter == " ")
                        keyPress.Letter = "_"; // Indicate an incorrect SpaceKey usage
                    inputtedText.text += $"<color=#FF0000>{keyPress.Letter}</color>";
                }
                else
                {
                    inputtedText.text += keyPress.Letter;
                }
            }
            catch (IndexOutOfRangeException ioore)
            {
                // TODO: perhaps a "bzzt wrong" sound could play here?
                // should we let the player keep typing???
            }

            CheckForCompletedTest();
        }

        private void BackspaceAction(SignalKeyboardBackspacePress backspacePress)
        {
            Debug.Log("<KeyboardReader> Backspace pressed!");
            var curText = inputtedText.text;
            string newText;
            if (curText.Length == 0)
            {
                return;
            }
            else if (curText.EndsWith(">"))
            {
                // Remove RTF tags. E.g: 123<color>4</color>
                // Everything from the last '>' to the second-last '<' will be deleted
                int posOfSecondLastRTFOpener = curText[..curText.LastIndexOf("<")].LastIndexOf("<");
                newText = curText[..posOfSecondLastRTFOpener];
            }
            else
            {
                newText = curText[..^1]; // Remove last character
            }
            inputtedText.text = newText;
        }
    }
}
