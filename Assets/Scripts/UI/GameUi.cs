using System;
using Audio;
using FMODUnity;
using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class GameUi : MonoBehaviour
    {
        public string nextSceneToLoad;

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

            SignalBus<SignalGameEnded>.Subscribe(HandleEndGame).AddTo(_disposables);
            SignalBus<SignalKeyboardPausePress>.Subscribe(PauseGame).AddTo(_disposables);
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
            // Testing iWantToStopArm
            if (Debug.isDebugBuild)
            {
                if (Input.GetKeyDown(KeyCode.L))
                {
                    SignalBus<SignalArmStopMovement>.Fire(new SignalArmStopMovement
                    {
                        iWantToStopArm = true
                    });
                }
                if (Input.GetKeyDown(KeyCode.O))
                {
                    SignalBus<SignalArmStopMovement>.Fire(new SignalArmStopMovement
                    {
                        iWantToStopArm = false
                    });
                }
            }
        }

        // --------------------------------------------------------------------------------------------------------------
        // Game Event Functions
        // --------------------------------------------------------------------------------------------------------------

        private void HandleEndGame(SignalGameEnded context)
        {
            if (context.result == GameEndCondition.Loss)
            {
                _dialogs.gameLost.SetActive(true);
                return;
            }

            // Win Conditions trigger some similar behaviour
            _sound.musicStage.SetParameter("Win", 1);
            _dialogs.gameWon.SetActive(true);

            string levelName = SceneManager.GetActiveScene().name;
            int bestCombo = context.bestCombo;
            int accuracy = context.accuracy;
            (bool wasThisNewBestCombo, bool wasThisNewAccuracy,
                int highscoreBestCombo, int highscoreAccuracy,
                bool isBrandNewScore) = HighScoreManagement.TryAddScoreThenReturnHighscore(levelName, bestCombo, accuracy);
            _dialogs.SetupVictoryDialog(bestCombo, highscoreBestCombo, wasThisNewBestCombo, accuracy, highscoreAccuracy, wasThisNewAccuracy, isBrandNewScore);
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
            _sound.fmodMixer.KillEverySound();
            SceneManager.LoadScene(nextSceneToLoad);
        }
        public void ResetCurrentLevel()
        {
            _sound.fmodMixer.KillEverySound();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        public void ExitGameFromPause()
        {
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

        public void SetupVictoryDialog(int combo, int bestCombo, bool wasThisNewHighCombo, int accuracy, int bestAccuracy, bool wasThisNewAccuracy, bool isBrandNewScore)
        {
            txtComboCurrent.text = $"{combo}";
            txtAccuracyCurrent.text = $"{accuracy}";

            // Highscore doesn't exist. Don't display a message.
            if (isBrandNewScore)
            {
                txtComboBest.text = "";
                txtAccuracyBest.text = "";
            }
            else
            {
                // Highscore exists. Show congrats messages if it's a new best, or a previous best if not.
                if (wasThisNewHighCombo)
                    txtComboBest.text = "New best!";
                else
                    txtComboBest.text = $"Best: {bestCombo}";

                if (wasThisNewAccuracy)
                    txtAccuracyBest.text = "New best!";
                else
                    txtAccuracyBest.text = $"Best: {bestAccuracy}";
            }
            
        }

        public void SetupLossDialog(string character)
        {

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