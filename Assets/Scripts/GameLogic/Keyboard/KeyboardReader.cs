using System;
using System.Linq;
using System.Collections;
using FMODUnity;
using Signals;
using TMPro;
using UnityEngine;

namespace GameLogic.Keyboard
{
    public class KeyboardReader : MonoBehaviour
    {
        [Header("Text related variables")]
        [SerializeField] private string[] phrases;
        [SerializeField] private TMP_Text textPreview;
        [SerializeField] private TMP_Text inputtedText;

        [Header("Progress Tracker")]
        [SerializeField] private GameObject progressTracker;
        [SerializeField] private GameObject wordTrackerPrefab;
        
        private StudioEventEmitter phraseFinishedSound;

        private readonly CompositeDisposable disposables = new();

        private void Start()
        {
            SignalBus<SignalKeyboardKeyPress>.Subscribe(ReadFromKeyboard).AddTo(disposables);
            SignalBus<SignalKeyboardBackspacePress>.Subscribe(BackspaceAction).AddTo(disposables);
            phraseFinishedSound = GetComponent<StudioEventEmitter>();
            
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
                    var tracker = newGameObject.GetComponent<PhaseTracker>();
                    tracker.ChangeStatus(i == phrases.Length - 1 ? TrackerStatus.Active : TrackerStatus.Inactive);
                }
                textPreview.text = phrases[0];
            }
        }

        private void CheckForCompletedTest()
        {
            if (textPreview.text.Equals(inputtedText.text))
            {
                phraseFinishedSound.Play();
                Debug.Log("gaming yo yo gaming gaming yo");
                inputtedText.text = $"<color=#4EFF00>{inputtedText.text}</color>";
                StartCoroutine(UpdateGameState());
            }
            //else
            //{
            //    Debug.Log("keep goin");
            //}
        }

        private IEnumerator UpdateGameState()
        {
            yield return new WaitForSeconds(2f);
            var currentWord = textPreview.text;
            var trackers = progressTracker.GetComponentsInChildren<PhaseTracker>();
            
            if (currentWord.Equals(phrases[^1]))
            {
                Debug.Log("Turns out you DONT suck!");
                var lastTracker = trackers[0];
                lastTracker.ChangeStatus(TrackerStatus.Passed);
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
                for (var i = trackers.Length - 1; i >= 0; i--)
                {
                    var tracker = trackers[i];
                    if (tracker.CurrentStatus.Equals(TrackerStatus.Passed))
                        continue;
                    if (tracker.CurrentStatus.Equals(TrackerStatus.Active))
                    {
                        // that's the current one we just beat, mark as valid and the next one as current
                        tracker.ChangeStatus(TrackerStatus.Passed);
                        var nextTracker = trackers[i - 1];
                        nextTracker.ChangeStatus(TrackerStatus.Active);
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
            catch (IndexOutOfRangeException)
            {
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
        
        private void OnDestroy()
        {
            disposables.Dispose();
        }
    }
}
