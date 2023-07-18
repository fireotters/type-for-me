using System.Runtime.InteropServices;
using Audio;
using FMODUnity;
using Signals;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class MainMenuUi : BaseUI
    {

        [Header("Main Menu UI")]
        [SerializeField] private GameObject desktopButtons;
        [SerializeField] private GameObject webButtons;
        [SerializeField] private StudioEventEmitter menuSong;
        
        private readonly CompositeDisposable _disposables = new();
        
        #if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void registerVisibilityChangeEvent();
        #endif
        
        private void Start()
        {
            // WebGL & Debug-Only Stuff
            #if UNITY_WEBGL
                if (!Application.isEditor)
                    registerVisibilityChangeEvent();
                
                desktopButtons.SetActive(false);
                webButtons.SetActive(true);
            #else
                desktopButtons.SetActive(true);
                webButtons.SetActive(false);
            #endif
            //if (Debug.isDebugBuild)
            //    CheckForIncorrectlySetupComponents();


            // Main Menu start tasks
            ConfigureVersionText();
            Input.multiTouchEnabled = false; // All scenes after this will obey the No Multitouch rule
            SignalBus<SignalUiMainMenuStartGame>.Subscribe(StartGame).AddTo(_disposables);
            if (!PlayerPrefs.HasKey("TypePrompt_IsTop"))
            {
                PlayerPrefs.SetInt("TypePrompt_IsTop", 1);
            }

        }

#if UNITY_WEBGL
        // no usages detected because it'll be called from the web browser
        private void OnVisibilityChange(string visibilityState)
        {
            Debug.Log($"Tab is currently {visibilityState}");
            // every scene should have a canvas with a Mixer
            var activeFmodMixer = FindObjectOfType<FMODMixer>();
            var pauseStatus = visibilityState.Equals("hidden");

            if (activeFmodMixer != null)
            {
                activeFmodMixer.FindAllSfxAndPlayPause(pauseStatus);
            }
            else
            {
                Debug.LogError("Missing FMOD Mixer in Scene/Canvas!!");
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
