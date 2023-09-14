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
                if (Debug.isDebugBuild == false)
                    GetBrowserSpec();
            #else
                desktopButtons.SetActive(true);
                webButtons.SetActive(false);
            #endif


            // Main Menu start tasks
            ConfigureVersionText();
            Input.multiTouchEnabled = false; // All scenes after this will obey the No Multitouch rule
            SignalBus<SignalUiMainMenuStartGame>.Subscribe(StartGame).AddTo(_disposables);
            if (!PlayerPrefs.HasKey("TypePrompt_IsTop"))
            {
                PlayerPrefs.SetInt("TypePrompt_IsTop", 1);
            }
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
            Debug.Log($"[SettingsPanel] Current browser {browserName}");

            if (browserName == "Safari")
            {
                Debug.Log("[SettingsPanel] Automatically enable 'mac compatibility mode'");
                PlayerPrefs.SetInt("Toggle_Control", 1);
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
