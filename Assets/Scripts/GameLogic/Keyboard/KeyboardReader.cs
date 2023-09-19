using System;
using System.Collections;
using FMODUnity;
using Signals;
using UnityEngine;

namespace GameLogic.Keyboard
{
    public class KeyboardReader : MonoBehaviour
    {
        [Header("Level Variables")]
        [SerializeField] private string[] phrases;
        [Range(3, 15)][SerializeField] private int allowedMistakes = 3;

        [Header("Stats")]
        private int currentPhrase = 0;
        private int numOfPresses = 0, numOfCorrectPresses = 0, numOfIncorrectPresses = 0;
        private int highestCombo = 0, currentCombo = 0;
        private bool _previousTypeWasMistake = false;

        [Header("Components")]
        private TypingBox _typingBox;
        [SerializeField] private StudioEventEmitter phraseFinishedSound;
        [SerializeField] private StudioEventEmitter voiceBarks;
        private readonly CompositeDisposable disposables = new();


        // --------------------------------------------------------------------------------------------------------------
        // Start & End
        // --------------------------------------------------------------------------------------------------------------
        private void Start()
        {
            var hud = FindFirstObjectByType<HUD>();
            _typingBox = hud.GetComponentInChildren<TypingBox>();

            SignalBus<SignalKeyboardKeyPress>.Subscribe(ReadFromKeyboard).AddTo(disposables);
            SignalBus<SignalKeyboardBackspacePress>.Subscribe(BackspaceAction).AddTo(disposables);
            SignalBus<SignalGameRetryFromCheckpoint>.Subscribe(RetryLevelFromCheckpoint).AddTo(disposables);
            SignalBus<SignalVoiceFrequencyChange>.Subscribe(UpdateVoiceFrequency).AddTo(disposables);

            if (phrases.Length == 0)
            {
                Debug.LogError("NO PHRASES SET IN INSPECTOR! how tf u gonna play binch");
            }
            else
            {
                _typingBox.InitTrackerProgress(phrases.Length);
                _typingBox.InitTrackerMistakes(allowedMistakes);

                string firstWord = phrases[0];
                _typingBox.ChangeWord(firstWord);
            }

            Debug.Log($"[KeyboardReader] frequency from settings set to {PlayerPrefs.GetInt("Voice_Frequency")}");
            voiceBarks.SetParameter("voiceFrequency", PlayerPrefs.GetInt("Voice_Frequency"));
        }

        private void OnDestroy()
        {
            disposables.Dispose();
        }

        private IEnumerator UpdateGameState()
        {
            currentPhrase++;
            _typingBox.IncrementProgress();

            if (currentPhrase == phrases.Length)
            {
                Debug.Log("Level Complete - Turns out you DONT suck!");

                // Stop Arm
                SignalBus<SignalArmStopMovement>.Fire(new SignalArmStopMovement { iWantToStopArm = true });
                yield return new WaitForSeconds(2f);

                // Calculate stats & report SignalGameEnded
                int accuracy = (int)((double)numOfCorrectPresses / numOfPresses * 100);
                int levelHighestComboPossible = 0;
                foreach (string phrase in phrases)
                    levelHighestComboPossible += phrase.Length;
                SignalBus<SignalGameEnded>.Fire(new SignalGameEnded
                {
                    result = GameEndCondition.Win,
                    bestCombo = highestCombo,
                    accuracy = accuracy,
                    levelHighestComboPossible = levelHighestComboPossible
                });
            }
            else
            {
                Debug.Log("Level Checkpoint reached");
                SignalBus<SignalWeGotAtLeastOneWordCorrect>.Fire(new SignalWeGotAtLeastOneWordCorrect { }); // Tell GameUi to un-hide 'Retry from Checkpoint' button

                // Stop Arm
                SignalBus<SignalArmStopMovement>.Fire(new SignalArmStopMovement { iWantToStopArm = true });
                yield return new WaitForSeconds(1f);

                // Change the word in typingBox
                var nextWord = phrases[currentPhrase];
                _typingBox.ChangeWord(nextWord);

                // Start Arm
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
                    IncrementStats(correct: true);
                else
                    IncrementStats(correct: false);
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
            _previousTypeWasMistake = false;
            ResetCombo();
            _typingBox.PerformBackspace();
        }


        private void IncrementStats(bool correct)
        {
            numOfPresses++;
            if (correct)
            {
                _previousTypeWasMistake = false;
                numOfCorrectPresses++;

                currentCombo++;
                if (currentCombo > highestCombo)
                    highestCombo = currentCombo;
                _typingBox.SetCombo(currentCombo);
            }
            else
            {
                // Only penalise the player for one mistake in a row. Increment the accuracy calculation, but don't count a 'mistake'.
                if (!_previousTypeWasMistake)
                {
                    _previousTypeWasMistake = true;
                    SignalBus<SignalKeyboardMistakeMade>.Fire(new SignalKeyboardMistakeMade { });

                    ResetCombo();
                    _typingBox.IncrementMistake();

                    numOfIncorrectPresses++;
                    if (numOfIncorrectPresses >= allowedMistakes)
                    {
                        // you fucking suck
                        SignalBus<SignalArmStopMovement>.Fire(new SignalArmStopMovement { iWantToStopArm = true });
                        SignalBus<SignalGameEnded>.Fire(new SignalGameEnded { result = GameEndCondition.Loss });
                    }
                }
            }
        }

        private void ResetCombo()
        {
            if (currentCombo > highestCombo)
                highestCombo = currentCombo;
            currentCombo = 0;
            _typingBox.SetCombo(currentCombo);
        }

        private void RetryLevelFromCheckpoint(SignalGameRetryFromCheckpoint s)
        {
            // Reset mistake counter, not checkpoint counter
            _previousTypeWasMistake = false;
            numOfIncorrectPresses = 0;
            _typingBox.IncrementMistake(resetFromCheckpoint: true);

            // Reset typing box and restart hand movement
            var currentWord = phrases[currentPhrase];
            _typingBox.ChangeWord(currentWord);
            SignalBus<SignalArmStopMovement>.Fire(new SignalArmStopMovement { iWantToStopArm = false });
        }

        private void UpdateVoiceFrequency(SignalVoiceFrequencyChange signal)
        {
            Debug.Log($"[KeyboardReader] Updating frequency value to {signal.newValue}");
            voiceBarks.SetParameter("voiceFrequency", signal.newValue);
        }
    }
}
