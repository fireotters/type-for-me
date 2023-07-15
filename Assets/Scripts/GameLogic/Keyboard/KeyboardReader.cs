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
        private int currentPhrase = 0;

        [Header("Progress Trackers")]
        [SerializeField] private GameObject progressTracker;
        [SerializeField] private GameObject mistakeTracker;
        [SerializeField] private GameObject wordTrackerPrefab;
        [Range(3, 15)] [SerializeField] private int allowedMistakes = 3;

        [SerializeField] private TypingBox _typingBox;
        
        
        public int numOfPresses = 0, numOfCorrectPresses = 0;
        public int highestCombo = 0, currentCombo = 0;
        
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
                SetUpProgressTracker();
                SetUpMistakeTracker();

                string firstWord = phrases[0];
                _typingBox.Init(firstWord);
            }
        }

        private void SetUpProgressTracker()
        {
            var startingXAxisPos = progressTracker.transform.position.x;
            for (var i = 0; i < phrases.Length; i++)
            {
                var newGameObject
                    = Instantiate(wordTrackerPrefab, new Vector3(startingXAxisPos, progressTracker.transform.position.y),
                        Quaternion.identity, progressTracker.transform);
                startingXAxisPos -= .5f;
                var tracker = newGameObject.GetComponent<PhaseTracker>();
                tracker.ChangeStatus(i == phrases.Length - 1 ? TrackerStatus.Active : TrackerStatus.Inactive);
            }
        }

        private void SetUpMistakeTracker()
        {
            var startingXAxisPos = mistakeTracker.transform.position.x;
            for (var i = 0; i < allowedMistakes; i++)
            {
                Instantiate(wordTrackerPrefab, new Vector3(startingXAxisPos, mistakeTracker.transform.position.y),
                        Quaternion.identity, mistakeTracker.transform);
                startingXAxisPos -= .5f;
            }
        }

        private IEnumerator UpdateGameState()
        {
            var trackers = progressTracker.GetComponentsInChildren<PhaseTracker>();

            var finalWordAvailable = phrases[^1];
            if (_typingBox.CompletedWordWasFinal(finalWordAvailable))
            {
                // Set final tracker to Passed, and tell Arm to stop moving
                Debug.Log("Level Complete - Turns out you DONT suck!");
                var lastTracker = trackers[0];
                lastTracker.ChangeStatus(TrackerStatus.Passed);
                SignalBus<SignalArmStopMovement>.Fire(new SignalArmStopMovement { iWantToStopArm = true });
                yield return new WaitForSeconds(2f);

                // Calculate stats & report SignalGameEnded
                int accuracy = (int)((double)numOfCorrectPresses / numOfPresses * 100);
                int levelHighestComboPossible = 0;
                foreach (string phrase in phrases)
                    levelHighestComboPossible += phrase.Length;
                SignalBus<SignalGameEnded>.Fire(new SignalGameEnded {
                    result = GameEndCondition.Win,
                    bestCombo = highestCombo,
                    accuracy = accuracy,
                    levelHighestComboPossible = levelHighestComboPossible });
            }
            else
            {
                // Stop arm
                Debug.Log("Part of Level Complete");
                SignalBus<SignalArmStopMovement>.Fire(new SignalArmStopMovement { iWantToStopArm = true });

                // Init the typingBox after 1s & update HUD
                yield return new WaitForSeconds(1f);
                currentPhrase++;
                var nextWord = phrases[currentPhrase];
                _typingBox.Init(nextWord);
                
                // Update HUD
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

                // Start moving arm after 1s
                yield return new WaitForSeconds(1f);
                SignalBus<SignalArmStopMovement>.Fire(new SignalArmStopMovement { iWantToStopArm = false });
            }
        }
        
        private void ReadFromKeyboard(SignalKeyboardKeyPress keyPress)
        {
            Debug.Log($"<KeyboardReader> {keyPress.Letter} was pressed!");
            try
            {
                if (_typingBox.InputIsValid(keyPress.Letter))
                    IncrementStats(correct:true);
                else
                    IncrementStats(correct:false);
            }
            catch (IndexOutOfRangeException)
            {
                // There is already an incorrect letter, and another has been attempted.
                IncrementStats(false);
            }

            if (_typingBox.InputIsFinished())
            {
                phraseFinishedSound.Play();
                StartCoroutine(UpdateGameState());
            }
        }

        private void BackspaceAction(SignalKeyboardBackspacePress backspacePress)
        {
            Debug.Log("<KeyboardReader> Backspace pressed!");
            ResetCombo();
            _typingBox.PerformBackspace();
        }
        
        private void OnDestroy()
        {
            disposables.Dispose();
        }

        private void IncrementStats(bool correct)
        {
            numOfPresses++;
            if (correct)
            {
                numOfCorrectPresses++;
                currentCombo++;
                if (currentCombo > highestCombo)
                    highestCombo = currentCombo;
            }
            else
            {
                ResetCombo();
                IncreaseMistakeCounter();
                if (Mathf.Abs(numOfPresses - numOfCorrectPresses) == allowedMistakes)
                {
                    // you fucking suck
                    SignalBus<SignalArmStopMovement>.Fire(new SignalArmStopMovement { iWantToStopArm = true });
                    SignalBus<SignalGameEnded>.Fire(new SignalGameEnded { result = GameEndCondition.Loss });
                }
            }
        }

        private void IncreaseMistakeCounter()
        {
            var mistakeTrackers = mistakeTracker.GetComponentsInChildren<PhaseTracker>();

            for (var i = mistakeTrackers.Length - 1; i >= 0; i--)
            {
                var currentTracker = mistakeTrackers[i];
                if (currentTracker.CurrentStatus.Equals(TrackerStatus.Mistake))
                    continue;
                if (currentTracker.CurrentStatus.Equals(TrackerStatus.Inactive))
                {
                    currentTracker.ChangeStatus(TrackerStatus.Mistake);
                    break;
                }
            }
        }
        
        private void ResetCombo()
        {
            if (currentCombo > highestCombo)
                highestCombo = currentCombo;
            currentCombo = 0;
        }
    }
}
