using TMPro;
using UI.UI_Elements;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class BaseUI : MonoBehaviour
    {
        [Header("Base UI")] [SerializeField] private TextMeshProUGUI versionText;
        [SerializeField] private bool showVersionText = false;

        protected void ConfigureVersionText()
        {
            versionText.gameObject.SetActive(Debug.isDebugBuild || showVersionText);
            SetVersionText();
        }

        private void SetVersionText()
        {
            if (versionText != null)
            {
                if (Debug.isDebugBuild)
                {
                    versionText.text = Application.isEditor
                        ? $"Version debug-{Application.version}-editor"
                        : $"Version debug-{Application.version}-{Application.buildGUID}";
                }
                else
                {
                    versionText.text = $"Version {Application.version}";
                }
            }
            else
            {
                Debug.LogWarning("No version text set!!!! please set one");
            }
        }

        protected void CheckForIncorrectlySetupComponents()
        {
            // Avoid accidentally forgetting components
            var btns = FindObjectsOfType(typeof(Button), true);
            var tmProBtns = 0;
            var tmProBtnsCorrect = 0;
            foreach (var btn in btns)
            {
                if (btn.GetComponentInChildren<TextMeshProUGUI>())
                {
                    tmProBtns += 1;
                    if (btn.GetComponent<TMPro_BtnText_HoverColor>())
                    {
                        tmProBtnsCorrect += 1;
                    }
                }
            }

            if (tmProBtns != tmProBtnsCorrect)
            {
                Debug.LogWarning($"<b><color=yellow>(Debug Missing Components Warn)</color></b> - " +
                          $"TMPro Buttons without 'TMPro_BtnText_HoverColor' script attached: <color=red>{tmProBtns - tmProBtnsCorrect}</color> " +
                          $"<i>(Buttons in the entire scene with TMPro Text: {tmProBtns})</i>");
            }
        }
    }
}