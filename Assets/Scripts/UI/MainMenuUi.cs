using FMODUnity;
using Signals;
using System;
using Unity.VisualScripting;
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
        
        private readonly CompositeDisposable disposables = new();

        private void Start()
        {
            // WebGL & Debug-Only Stuff
            #if UNITY_WEBGL
                        desktopButtons.SetActive(false);
                        webButtons.SetActive(true);
            #else
                        desktopButtons.SetActive(true);
                        webButtons.SetActive(false);
            #endif
            if (Debug.isDebugBuild)
                base.CheckForIncorrectlySetupComponents();


            // Main Menu start tasks
            base.ConfigureVersionText();
            SignalBus<SignalUiMainMenuStartGame>.Subscribe(StartGame).AddTo(disposables);
        }

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
            disposables.Dispose();
        }
    }
}
