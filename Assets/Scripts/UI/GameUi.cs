using System;
using Audio;
using FMODUnity;
using GameLogic;
using Saving;
using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class GameUi : MonoBehaviour
    {
        [Header("These are unique for every level!")]
        public string nextSceneToLoad;
        public string currentCharacter;
        public Animator bgAnimator;

        [Header("Components")]
        [SerializeField] private GameUiDialogs _dialogs;
        [SerializeField] private GameUiPlayerUi _playerUi;
        [SerializeField] private GameUiSound _sound;
        private HUD _hud;

        private readonly CompositeDisposable _disposables = new();

        // --------------------------------------------------------------------------------------------------------------
        // Start & End
        // --------------------------------------------------------------------------------------------------------------
        private void Start()
        {
            _hud = FindFirstObjectByType<HUD>();
            _hud.HudIsVisible(false);

            // Debug stuff
            if (nextSceneToLoad == "")
                Debug.LogWarning("No 'CanvasGameUi.nextSceneToLoad' set! Selecting 'Next Level' will fail.");
            if (currentCharacter == "")
                Debug.LogWarning("No 'CanvasGameUi.currentCharacter' set! Loss dialogs and other contextual messages will fail.");
            if (!bgAnimator)
                Debug.LogWarning("No 'CanvasGameUi.bgAnimator' set! Background objects won't animate during levelEnd transition.");

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
        //private void Update()
        //{

        //}

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

            _sound.musicStage.SetParameter("Win", 1);
            bgAnimator.SetBool("levelClose", true);

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

            Invoke(nameof(HideGameplayEndLevel), 1f);
            Invoke(nameof(ShowGameWon), 3f);
        }

        private void ShowGameplayStartLevel()
        {
            _hud.HudIsVisible(true);
            Invoke(nameof(TellArmStart), 0.3f);
        }

        private void TellArmStart()
        {
            SignalBus<SignalArmStopMovement>.Fire(new SignalArmStopMovement
            {
                iWantToStopArm = false
            });
        }
        private void HideGameplayEndLevel()
        {
            _hud.HudIsVisible(false);
            bgAnimator.SetBool("levelClose", true);
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
            if (Time.timeScale == 1f)
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
        // Support for victory screen, high scores
        public GameObject paused, options;
        public GameObject gameLost, gameWon;
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