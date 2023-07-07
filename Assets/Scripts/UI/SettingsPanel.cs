using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static TMPro.TMP_Dropdown;

namespace UI
{
    public class SettingsPanel : MonoBehaviour
    {
        [Header("Panel elements")]
        [SerializeField] private Button audioButton;
        [SerializeField] private Button videoButton;
        [SerializeField] private Button gameButton;

        [Header("Panels")]
        [SerializeField] private GameObject audioPanel;
        [SerializeField] private GameObject desktopVideoPanel;
        [SerializeField] private GameObject webVideoPanel;
        [SerializeField] private GameObject gamePanel;

        [Header("Audio Panel controls")] 
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        
        [Header("Video Panel controls")]
        [SerializeField] private TMP_Dropdown resDrop;
        [SerializeField] private TMP_Dropdown fsModesDrop;
        [SerializeField] private Toggle webFsToggle;

        [Header("Game Panel controls")]
        [SerializeField] private GameObject _goBtnResetScores;

        private Audio.FMODMixer fmodMixer;
        
        private static readonly FullScreenMode[] FullScreenModes = {
            FullScreenMode.ExclusiveFullScreen,
            FullScreenMode.FullScreenWindow,
            FullScreenMode.Windowed
        };
        
        private void Awake()
        {
            PopulateVideoDropdowns();
            webFsToggle.SetIsOnWithoutNotify(Screen.fullScreen);
            fmodMixer = FindObjectOfType<Canvas>().GetComponent<Audio.FMODMixer>();
        }

        private void OnEnable()
        {
            var currentRes = Screen.width + " x " + Screen.height;
            // set current video dropdown values
            var currentResIndex = resDrop.options
                .Select(optionData => optionData.text)
                .ToList()
                .IndexOf(currentRes);
            var currentFsModeIndex = fsModesDrop.options
                .Select(optionData => optionData.text)
                .ToList()
                .IndexOf(Screen.fullScreenMode.ToString());
            resDrop.value = currentResIndex != -1 ? currentResIndex : 0;
            fsModesDrop.value = currentFsModeIndex != -1 ? currentFsModeIndex : 0;
            
            // set current audio slider values
            musicSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("music"));
            sfxSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("sfx"));
        }

        public void ToggleAudioPanel()
        {
            audioButton.interactable = false;
            videoButton.interactable = true;
            gameButton.interactable = true;
            audioPanel.SetActive(true);
            desktopVideoPanel.SetActive(false);
            webVideoPanel.SetActive(false);
            gamePanel.SetActive(false);
        }

        public void ToggleVideoPanel()
        {
            audioButton.interactable = true;
            videoButton.interactable = false;
            gameButton.interactable = true;
            audioPanel.SetActive(false);
#if UNITY_WEBGL
            desktopVideoPanel.SetActive(false);
            webVideoPanel.SetActive(true);
#else
            desktopVideoPanel.SetActive(true);
            webVideoPanel.SetActive(false);
#endif
            gamePanel.SetActive(false);
        }

        public void ToggleGamePanel()
        {
            audioButton.interactable = true;
            videoButton.interactable = true;
            gameButton.interactable = false;
            audioPanel.SetActive(false);
            desktopVideoPanel.SetActive(false);
            webVideoPanel.SetActive(false);
            gamePanel.SetActive(true);

            if (SceneManager.GetActiveScene().name != "MainMenu")
                _goBtnResetScores.SetActive(false);
        }

        private void PopulateVideoDropdowns()
        {
            // TODO should we iterate over all supported resolutions or should we define a specific list?
            var resolutionOptions = Screen.resolutions
                .Select(res =>
                {
                    var text = res.ToString()[..(res.ToString().IndexOf('@') - 1)];
                    return new OptionData { text = text };
                })
                .DistinctBy(optionData => optionData.text)
                .ToList();
            var fsmOptions = FullScreenModes
                .Select(fsm => new OptionData { text = fsm.ToString() })
                .ToList();

            resDrop.AddOptions(resolutionOptions);
            fsModesDrop.AddOptions(fsmOptions);
        }

        public void PassMusicVolChange(float dB)
        {
            fmodMixer.ChangeMusicVolume(dB);
        }

        public void PassSfxVolChange(float dB)
        {
            fmodMixer.ChangeSfxVolume(dB);
        }
        
        public void ApplyVideoSettings()
        {
#if UNITY_WEBGL
            Screen.fullScreen = webFsToggle.isOn;
#else
            var desiredResolution = Screen.resolutions
                .First(res => res.ToString().StartsWith(resDrop.captionText.text));
            var desiredFsMode = FullScreenModes
                .First(fsm => fsModesDrop.captionText.text == fsm.ToString());
            
            Screen.SetResolution(desiredResolution.width, desiredResolution.height, desiredFsMode);
#endif
        }
        public void ResetHighscores()
        {
            HighScoreManagement.ResetLevelScores();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}