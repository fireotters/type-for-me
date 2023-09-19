using System;
using System.Linq;
using Saving;
using Signals;
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
        [SerializeField] private TMP_Dropdown voiceFrequency;
        [SerializeField] private TMP_Text voiceFrequencyLabel;
        [SerializeField] private Toggle spatialAudioToggle;
        
        [Header("Video Panel controls")]
        [SerializeField] private TMP_Dropdown resDrop;
        [SerializeField] private TMP_Dropdown fsModesDrop;
        [SerializeField] private Toggle webFsToggle;

        [Header("Game Panel controls")]
        [SerializeField] private GameObject _goBtnResetScores;
        [SerializeField] private Toggle _btnFlipTypePrompt;
        [SerializeField] private Toggle _btnMacCompat;
        [SerializeField] private TMP_Text _safariText;

        private Audio.FMODMixer fmodMixer;
        
        private static readonly FullScreenMode[] FullScreenModes = {
            FullScreenMode.ExclusiveFullScreen,
            FullScreenMode.FullScreenWindow,
            FullScreenMode.Windowed
        };

        private readonly CompositeDisposable _compositeDisposable = new();

        // --------------------------------------------------------------------------------------------------------------
        // Start & OnEnable
        // --------------------------------------------------------------------------------------------------------------
        private void Awake()
        {
            PopulateVideoDropdowns();
            fmodMixer = FindObjectOfType<Canvas>().GetComponent<Audio.FMODMixer>();
            SignalBus<SignalSafariDisableControl>.Subscribe(DisableControlMode_Toggle).AddTo(_compositeDisposable);
            
            if (PlayerPrefs.GetInt("Detected_Safari") == 1)
            {
                DisableControlMode_Toggle(new SignalSafariDisableControl { });
            }

            if (SceneManager.GetActiveScene().name.StartsWith("Level"))
            {
                // don't allow to change voice frequency during gameplay - for some reason updating the parameters wont work during it
                voiceFrequency.gameObject.SetActive(false);
                voiceFrequencyLabel.gameObject.SetActive(false);
            }
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
            webFsToggle.SetIsOnWithoutNotify(Screen.fullScreen);

            // set current audio panel values
            musicSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("music"));
            sfxSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("sfx"));
            voiceFrequency.SetValueWithoutNotify(PlayerPrefs.GetInt("Voice_Frequency"));
            spatialAudioToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("Spatial_Audio") == 0);

            // game panel values
            _btnFlipTypePrompt.SetIsOnWithoutNotify(PlayerPrefs.GetInt("TypePrompt_IsTop") == 0);
            _btnMacCompat.SetIsOnWithoutNotify(PlayerPrefs.GetInt("Toggle_Control") == 1);
        }

        private void OnDestroy()
        {
            _compositeDisposable.Dispose();
        }
        // --------------------------------------------------------------------------------------------------------------
        // Opening Panels
        // --------------------------------------------------------------------------------------------------------------
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


        // --------------------------------------------------------------------------------------------------------------
        // Video
        // --------------------------------------------------------------------------------------------------------------
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

        // --------------------------------------------------------------------------------------------------------------
        // Audio
        // --------------------------------------------------------------------------------------------------------------
        public void PassMusicVolChange(float dB)
        {
            fmodMixer.ChangeMusicVolume(dB);
        }

        public void PassSfxVolChange(float dB)
        {
            fmodMixer.ChangeSfxVolume(dB);
        }
        
        // --------------------------------------------------------------------------------------------------------------
        // Game
        // --------------------------------------------------------------------------------------------------------------
        public void TypePromptFlip_Toggle()
        {
            if (PlayerPrefs.GetInt("TypePrompt_IsTop") == 1)
                PlayerPrefs.SetInt("TypePrompt_IsTop", 0);
            else
                PlayerPrefs.SetInt("TypePrompt_IsTop", 1);
            SignalBus<SignalSettingsChange>.Fire(new SignalSettingsChange { });
        }
        
        public void ResetHighscores()
        {
            HighScoreManagement.ResetLevelScores();
            fmodMixer.KillEverySound();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ControlMode_Toggle()
        {
            if (PlayerPrefs.GetInt("Toggle_Control") == 1)
                PlayerPrefs.SetInt("Toggle_Control", 0);
            else
                PlayerPrefs.SetInt("Toggle_Control", 1);
            _btnMacCompat.SetIsOnWithoutNotify(PlayerPrefs.GetInt("Toggle_Control") == 1);
            SignalBus<SignalSettingsChange>.Fire(new SignalSettingsChange { });
        }

        public void SpatialAudioToggle_Change(bool value)
        {
            var newValue = value ? 0 : 1;
            PlayerPrefs.SetInt("Spatial_Audio", newValue);
            SignalBus<SignalSettingsChange>.Fire(new SignalSettingsChange { });
        }

        public void AudioFrequency_Change(Int32 value)
        {
            Debug.Log($"Setting value to {value}");
            PlayerPrefs.SetInt("Voice_Frequency", value);
            SignalBus<SignalVoiceFrequencyChange>.Fire(new SignalVoiceFrequencyChange { newValue = value });
        }

        private void DisableControlMode_Toggle(SignalSafariDisableControl signal)
        {
            _btnMacCompat.gameObject.SetActive(false);
            _safariText.gameObject.SetActive(true);
        }
    }
}