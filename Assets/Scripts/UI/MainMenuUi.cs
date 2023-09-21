using FMODUnity;
using Saving;
using Signals;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;

namespace UI
{
    public class MainMenuUi : BaseUI
    {

        [Header("Main Menu UI")]
        [SerializeField] private GameObject desktopButtons;
        [SerializeField] private GameObject webButtons;
        [SerializeField] private StudioEventEmitter menuSong;

        [DllImport("__Internal")]
        private static extern void GetBrowserSpec();

        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            // WebGL & Debug-Only Stuff
            #if UNITY_WEBGL
                desktopButtons.SetActive(false);
                webButtons.SetActive(true);
                if (!Application.isEditor)
                    GetBrowserSpec();
            #else
                desktopButtons.SetActive(true);
                webButtons.SetActive(false);
            #endif


            // Main Menu start tasks
            ConfigureVersionText();
            Input.multiTouchEnabled = false; // All scenes after this will obey the No Multitouch rule
            SignalBus<SignalUiMainMenuStartGame>.Subscribe(StartGame).AddTo(_disposables);

            // Default PlayerPrefs
            if (!PlayerPrefs.HasKey("TypePrompt_IsTop"))
                PlayerPrefs.SetInt("TypePrompt_IsTop", 1);
            if (!PlayerPrefs.HasKey("Voice_Frequency"))
                PlayerPrefs.SetInt("Voice_Frequency", 2);
            if (!PlayerPrefs.HasKey("MouseSensitivity"))
                PlayerPrefs.SetFloat("MouseSensitivity", 0.5f);

            // If player has Highscores from a previous version, force the game to reset scores
            if (PlayerPrefs.GetInt("HighScoreVersion") != 1)
            {
                HighScoreManagement.ResetLevelScores();
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                menuSong.AllowFadeout = false;
                menuSong.Stop();
            }
        }

#if UNITY_WEBGL
        public void DetectBrowser(string browserName)
        {
            if (browserName == "Safari")
            {
                Debug.Log("Safari detected, permaenabling hold to drag");
                SignalBus<SignalSafariDisableControl>.Fire(new SignalSafariDisableControl { });
                PlayerPrefs.SetInt("Detected_Safari", 1);
            }
        }
#endif

        public void StartGame(SignalUiMainMenuStartGame signal)
        {
            menuSong.Stop();
            SceneManager.LoadScene($"Scenes/LevelScenes/{signal.levelToLoad}");
        }
        
        public void OpenCredits()
        {
            menuSong.Stop();
            SceneManager.LoadScene($"Scenes/CreditsMenu");
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
