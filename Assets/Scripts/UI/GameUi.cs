using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using FMODUnity;
using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;

namespace UI
{
    public class GameUi : MonoBehaviour
    {
        public string nextSceneToLoad;
        public string currentCharacter;
        public Animator animator;
        public GameObject[] justhideeverything;

        [SerializeField] private GameUiDialogs _dialogs;
        [SerializeField] private GameUiPlayerUi _playerUi;
        [SerializeField] private GameUiSound _sound;

        private readonly CompositeDisposable _disposables = new();

        // --------------------------------------------------------------------------------------------------------------
        // Start & End
        // --------------------------------------------------------------------------------------------------------------
        private void Start()
        {
            // Debug stuff
            if (nextSceneToLoad == "")
                Debug.LogWarning("No 'CanvasGameUi.nextSceneToLoad' set! Selecting 'Next Level' will fail.");
            if (currentCharacter == "")
                Debug.LogWarning("No 'CanvasGameUi.currentCharacter' set! Loss dialogs and other contextual messages will fail.");

            SignalBus<SignalGameEnded>.Subscribe(HandleEndGame).AddTo(_disposables);
            SignalBus<SignalKeyboardPausePress>.Subscribe(PauseGame).AddTo(_disposables);

            Invoke(nameof(ShowGameplayStartLevel), 4f);
        }
        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        // --------------------------------------------------------------------------------------------------------------
        // Per-Frame Updates
        // --------------------------------------------------------------------------------------------------------------
        private void Update()
        {
            CheckKeyInputs();
        }

        private void CheckKeyInputs()
        {
            //// Testing iWantToStopArm
            //if (Debug.isDebugBuild)
            //{
            //    if (Input.GetKeyDown(KeyCode.L))
            //    {
            //        SignalBus<SignalArmStopMovement>.Fire(new SignalArmStopMovement
            //        {
            //            iWantToStopArm = true
            //        });
            //    }
            //    if (Input.GetKeyDown(KeyCode.O))
            //    {
            //        SignalBus<SignalArmStopMovement>.Fire(new SignalArmStopMovement
            //        {
            //            iWantToStopArm = false
            //        });
            //    }
            //}
        }

        // --------------------------------------------------------------------------------------------------------------
        // Game Event Functions
        // --------------------------------------------------------------------------------------------------------------

        private void HandleEndGame(SignalGameEnded context)
        {
            if (context.result == GameEndCondition.Loss)
            {
                _dialogs.gameLost.SetActive(true);
                _dialogs.SetupLossDialog(currentCharacter);
                return;
            }

            // Win Conditions trigger some similar behaviour
            _sound.musicStage.SetParameter("Win", 1);

            animator.SetBool("levelClose", true);

            string levelName = SceneManager.GetActiveScene().name;
            int bestCombo = context.bestCombo;
            int accuracy = context.accuracy;
            int levelHighestComboPossible = context.levelHighestComboPossible;
            (bool wasThisNewBestCombo, bool wasThisNewAccuracy,
                int highscoreBestCombo, int highscoreAccuracy,
                bool wasComboPerfect, bool wasAccuracyPerfect,
                bool isBrandNewScore) = HighScoreManagement.TryAddScoreThenReturnHighscore(levelName, bestCombo, accuracy, levelHighestComboPossible);
            _dialogs.SetupVictoryDialog(
                bestCombo, highscoreBestCombo, wasThisNewBestCombo, wasComboPerfect,
                accuracy, highscoreAccuracy, wasThisNewAccuracy, wasAccuracyPerfect,
                isBrandNewScore);


            foreach (GameObject gameObject in justhideeverything)
            {
                gameObject.SetActive(false);
            }
            Invoke(nameof(HideGameplayDoLevelEnd), 1f);
            Invoke(nameof(ShowGameWon), 3f);
        }

        private void ShowGameplayStartLevel()
        {
            foreach (GameObject gameObject in justhideeverything)
            {
                gameObject.SetActive(true);
            }
            Invoke(nameof(TellArmStart), 0.3f);
        }

        private void TellArmStart()
        {
            SignalBus<SignalArmStopMovement>.Fire(new SignalArmStopMovement
            {
                iWantToStopArm = false
            });
        }
        private void HideGameplayDoLevelEnd()
        {
            animator.SetBool("levelClose", true);
        }
        public void ShowGameWon()
        {
            Time.timeScale = 0;
            _dialogs.gameWon.SetActive(true);
        }

        // --------------------------------------------------------------------------------------------------------------
        // Pause Functions
        // --------------------------------------------------------------------------------------------------------------
        private void GameIsPaused(bool intent)
        {
            // Show or hide pause panel and set timescale
            _sound.musicStage.SetParameter("Menu", intent ? 1 : 0);
            _dialogs.paused.SetActive(intent);
            Time.timeScale = intent ? 0 : 1;
            _sound.fmodMixer.FindAllSfxAndPlayPause(isGamePaused: intent);
        }

        private void PauseGame(SignalKeyboardPausePress context)
        {
            if (Time.timeScale == 1)
                GameIsPaused(true);
            else
                GameIsPaused(false);
        }
        
        public void ResumeGame()
        {
            GameIsPaused(false);
        }
        
        public void TutorialPause(bool intent)
        {
            Time.timeScale = intent ? 0 : 1;
        }

        public void ToggleOptionsPanel()
        {
            _dialogs.options.SetActive(!_dialogs.options.activeInHierarchy);
        }

        public void LoadNextScene()
        {
            Time.timeScale = 1;
            _sound.fmodMixer.KillEverySound();
            SceneManager.LoadScene(nextSceneToLoad);
        }
        public void ResetCurrentLevel()
        {
            Time.timeScale = 1;
            _sound.fmodMixer.KillEverySound();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        public void ExitGameFromPause()
        {
            Time.timeScale = 1;
            _sound.fmodMixer.KillEverySound();
            SceneManager.LoadScene("MainMenu");
            Time.timeScale = 1;
        }

        public bool IsPauseInterruptingPanelOpen()
        {
            return _dialogs.gameLost.activeInHierarchy || _dialogs.gameWon.activeInHierarchy;
        }
    }

    // --------------------------------------------------------------------------------------------------------------
    // Classes for common GameUi Objects
    // --------------------------------------------------------------------------------------------------------------

    [Serializable]
    public class GameUiDialogs
    {
        // Support for victory screen, high scores, remembering the tutorials shown to the player
        public int tutorialIndex;
        public GameObject paused, options;
        public GameObject gameLost, gameWon;
        public Color clrVictoryScore1, clrVictoryScore1Best, clrVictoryScore2, clrVictoryScore2Best;
        public TextMeshProUGUI txtComboCurrent, txtComboBest, txtAccuracyCurrent, txtAccuracyBest;
        public TextMeshProUGUI txtCharLostPatience;

        public void SetupVictoryDialog(
            int combo, int bestCombo, bool wasThisNewHighCombo, bool wasComboPerfect,
            int accuracy, int bestAccuracy, bool wasThisNewAccuracy, bool wasAccuracyPerfect,
            bool isBrandNewScore)
        {
            txtComboCurrent.text = $"{combo}";
            txtAccuracyCurrent.text = $"{accuracy}%";

            // Highscore doesn't exist. Don't display a message.
            if (isBrandNewScore)
            {
                if (wasComboPerfect)
                    txtComboBest.text = "Perfect!";
                else
                    txtComboBest.text = "";

                if (wasAccuracyPerfect)
                    txtAccuracyBest.text = "Perfect!";
                else
                    txtAccuracyBest.text = "";
            }
            else
            {
                // Highscore exists. Show congrats messages if it's a new best, or a previous best if not.
                if (wasComboPerfect)
                    txtComboBest.text = "Perfect!";
                else if (wasThisNewHighCombo)
                    txtComboBest.text = "New best!";
                else
                    txtComboBest.text = $"Best: {bestCombo}";

                if (wasAccuracyPerfect)
                    txtAccuracyBest.text = "Perfect!";
                else if (wasThisNewAccuracy)
                    txtAccuracyBest.text = "New best!";
                else
                    txtAccuracyBest.text = $"Best: {bestAccuracy}%";
            }
        }

        public void SetupLossDialog(string character)
        {
            txtCharLostPatience.text = character.ToUpper();
        }
    }
    [Serializable]
    public class GameUiPlayerUi
    {
        // Implement
    }
    [Serializable]
    public class GameUiSound
    {
        public StudioEventEmitter musicStage;
        public FMODMixer fmodMixer;
    }
}