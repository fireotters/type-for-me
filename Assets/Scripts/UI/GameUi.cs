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
            GameEndCondition scoreType = context.result;
            int score = context.score;
            (bool wasThisNewHighscore, int highScore) = HighScoreManagement.TryAddScoreThenReturnHighscore(levelName, scoreType, score);
            _dialogs.SetupVictoryDialog(scoreType, score, highScore, wasThisNewHighscore);
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
        public TextMeshProUGUI txtVictoryCurrent, txtVictoryBest;

        public void SetupVictoryDialog(GameEndCondition victoryType, int currentScore, int bestScore, bool wasThisNewHighscore)
        {
            if (victoryType == GameEndCondition.WinType1)
            {
                txtVictoryCurrent.color = clrVictoryScore1;
                txtVictoryBest.color = clrVictoryScore1Best;
            }
            else if (victoryType == GameEndCondition.WinType2)
            {
                txtVictoryCurrent.color = clrVictoryScore2;
                txtVictoryBest.color = clrVictoryScore2Best;
            }

            txtVictoryCurrent.text = currentScore.ToString() + (currentScore > 1 ? " pts" : " pt");
            if (bestScore == -1)
            {
                txtVictoryBest.text = "";
                txtVictoryCurrent.verticalAlignment = VerticalAlignmentOptions.Middle;
            }
            else if (wasThisNewHighscore)
                txtVictoryBest.text = "New best score!";
            else
                txtVictoryBest.text = "Best: " + bestScore.ToString() + (bestScore > 1 ? " pts" : " pt");


            // Set tutorial as completed, so it won't appear next time
            if (tutorialIndex != 0)
            {
                if (PlayerPrefs.GetInt("tutorialUpTo", 0) < tutorialIndex)
                {
                    PlayerPrefs.SetInt("tutorialUpTo", tutorialIndex);
                    PlayerPrefs.Save();
                }
            }
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