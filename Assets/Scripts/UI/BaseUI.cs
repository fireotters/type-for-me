using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class BaseUI : MonoBehaviour
    {
        [Header("Base UI")]
        [SerializeField] private TextMeshProUGUI versionText;
        private bool showVersionText = false;


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

            Object[] Btns = FindObjectsOfType(typeof(Button), true);
            int TMProBtns = 0;
            int TMProBtnsCorrect = 0;
            foreach (Object btn in Btns)
            {
                if (btn.GetComponentInChildren<TextMeshProUGUI>())
                {
                    TMProBtns += 1;
                    if (btn.GetComponent<TMPro_BtnText_HoverColor>())
                    {
                        TMProBtnsCorrect += 1;
                    }
                }
            }
            if (TMProBtns != TMProBtnsCorrect)
            {
                Debug.Log($"<b><color=yellow>(Debug Missing Components Warn)</color></b> - " +
                          $"TMPro Buttons without 'TMPro_BtnText_HoverColor' script attached: <color=red>{TMProBtns - TMProBtnsCorrect}</color> " +
                          $"<i>(Buttons in the entire scene with TMPro Text: {TMProBtns})</i>");
            }
        }
    }
}
