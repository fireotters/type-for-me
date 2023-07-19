using System;
using System.Runtime.InteropServices;
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
        [SerializeField] private LayerMask _mouseLayerMask;
        [SerializeField] private GameUiDialogs _dialogs;
        [SerializeField] private GameUiPlayerUi _playerUi;
        [SerializeField] private GameUiSound _sound;
        private HUD _hud;

        #if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void registerVisibilityChangeEvent();
        #endif
        
        private readonly CompositeDisposable _disposables = new();

        // --------------------------------------------------------------------------------------------------------------
        // Start & End
        // --------------------------------------------------------------------------------------------------------------
        private void Start()
        {
            # if UNITY_WEBGL
            if (!Application.isEditor)
                registerVisibilityChangeEvent();
            #endif
            // Disable mouse input to certain layers (by asking Camera)
            Camera.main.eventMask = _mouseLayerMask;

            _hud = FindFirstObjectByType<HUD>();

            // Debug stuff
            if (nextSceneToLoad == "")
                Debug.LogWarning("No 'CanvasGameUi.nextSceneToLoad' set! Selecting 'Next Level' will fail.");
            if (currentCharacter == "")
                Debug.LogWarning("No 'CanvasGameUi.currentCharacter' set! Loss dialogs and other contextual messages will fail.");
            if (!bgAnimator)
                Debug.LogWarning("No 'CanvasGameUi.bgAnimator' set! Background objects won't animate during levelEnd transition.");

            SignalBus<SignalGameEnded>.Subscribe(HandleEndGame).AddTo(_disposables);
            SignalBus<SignalGamePaused>.Subscribe(PauseGame).AddTo(_disposables);

            CheckFlipDisplay();
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
            if (Input.GetKeyDown(KeyCode.Escape) &&
                !_dialogs.gameLost.activeInHierarchy &&
                !_dialogs.gameWon.activeInHierarchy &&
                !_dialogs.options.activeInHierarchy)
            {
                if (Time.timeScale == 0)
                    SignalBus<SignalGamePaused>.Fire(new SignalGamePaused { paused = false });
                else
                    SignalBus<SignalGamePaused>.Fire(new SignalGamePaused { paused = true });
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
                _dialogs.SetupLossDialog(currentCharacter);
                return;
            }

            _sound.musicStage.SetParameter("Win", 1);
            CheckFlipDisplay();
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
            _hud.HudLevelTransition(true);
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
            _hud.HudLevelTransition(false);
            CheckFlipDisplay();
            bgAnimator.SetBool("levelClose", true);
        }
        public void ShowGameWon()
        {
            Time.timeScale = 0;
            _dialogs.gameWon.SetActive(true);
        }
        private void CheckFlipDisplay()
        {
            if (PlayerPrefs.GetInt("TypePrompt_IsTop") == 1)
                bgAnimator.SetBool("typingIsTop", true);
            else if (PlayerPrefs.GetInt("TypePrompt_IsTop") == 0)
                bgAnimator.SetBool("typingIsTop", false);
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

        private void PauseGame(SignalGamePaused context)
        {
            if (context.paused)
                GameIsPaused(true);
            else
                GameIsPaused(false);
        }
        
        public void ResumeGame()
        {
            GameIsPaused(false);
            SignalBus<SignalGamePaused>.Fire(new SignalGamePaused { paused = false });
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

        #if UNITY_WEBGL
        // no usages detected because it'll be called from the web browser
        private void OnVisibilityChange(string visibilityState)
        {
            Debug.Log($"Tab is currently {visibilityState}");
            var pauseStatus = visibilityState.Equals("hidden");
            GameIsPaused(pauseStatus);
        }
        #endif
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