using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Signals;
using TMPro;

public class ButtonLevelSelect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string levelNum, levelName;
    [HideInInspector] public string attachedLevel;
    private Button button;
    private string _highAccuracy = "-", _highCombo = "-";
    private TextMeshProUGUI _textLevelNum, _txtCombo, _txtAccuracy;

    private void Awake()
    {
        attachedLevel = levelNum.Length == 1 ? "Level0" + levelNum : "Level" + levelNum;
        button = GetComponentInChildren<Button>();
        button.onClick.AddListener(LoadLevel);
        _textLevelNum = transform.Find("TxtLvlNum").GetComponent<TextMeshProUGUI>();
        _textLevelNum.text = levelNum;
        _txtCombo = transform.Find("ScoreRecords").Find("TxtCombo").GetComponent<TextMeshProUGUI>();
        _txtAccuracy = transform.Find("ScoreRecords").Find("TxtAccuracy").GetComponent<TextMeshProUGUI>();
    }

    private void LoadLevel()
    {
        SignalBus<SignalUiMainMenuStartGame>.Fire(new SignalUiMainMenuStartGame { levelToLoad = attachedLevel });
    }

    public void UpdateLevelHighscores(int combo, int accuracy, bool perfectCombo, bool perfectAccuracy)
    { 
        _txtCombo.text = $"C:{combo}";
        _txtAccuracy.text = $"{accuracy}%";
        _highCombo = $"{combo}";
        _highAccuracy = $"{accuracy}";
        if (perfectCombo)
            _txtCombo.color = Color.yellow;
        if (perfectAccuracy)
            _txtAccuracy.color = Color.yellow;

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Show the tooltip if the button is interactible
        if (button.interactable)
        {
            string tempLevelName = levelNum + "." + levelName;
            string tempAccuracy = _highAccuracy != "-" ? $"{_highAccuracy}%" : "...";
            string tempCombo = _highCombo != "-" ? _highCombo : "...";
            SignalBus<SignalUiMainMenuTooltipChange>.Fire(new SignalUiMainMenuTooltipChange
            {
                Showing = true,
                LevelName = tempLevelName,
                Acc = tempAccuracy,
                Com = tempCombo
            });
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        SignalBus<SignalUiMainMenuTooltipChange>.Fire(new SignalUiMainMenuTooltipChange { Showing = false, LevelName = "", Acc = "", Com = "" });
    }
}
