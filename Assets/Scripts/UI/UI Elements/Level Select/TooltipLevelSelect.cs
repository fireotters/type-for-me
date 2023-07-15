using Signals;
using TMPro;
using UnityEngine;

namespace UI.UI_Elements.Level_Select
{
    public class TooltipLevelSelect : MonoBehaviour
    {
        public bool _showing;
        private GameObject _tooltipBg;
        private TextMeshProUGUI _textLevelName, _txtAcc, _txtCom;
        private RectTransform _rectTransform, _canvasRectTransform;
        private Vector3 tooltipOffset = new Vector3(10, -10, 0);
        private readonly CompositeDisposable _disposables = new();

        // Cursor changing
        public Texture2D normalCursor, tooltipCursor;
        private CursorMode cursorMode = CursorMode.Auto;
        private Vector2 cursorHotspot = Vector2.zero;

        // --------------------------------------------------------------------------------------------------------------
        // Start & End
        // --------------------------------------------------------------------------------------------------------------
        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasRectTransform = FindObjectOfType<Canvas>().GetComponent<RectTransform>();
            _tooltipBg = transform.Find("TooltipBg").gameObject;
            _textLevelName = _tooltipBg.transform.Find("TextLevelName").GetComponent<TextMeshProUGUI>();
            _txtAcc = _tooltipBg.transform.Find("TxtAcc").GetComponent<TextMeshProUGUI>();
            _txtCom = _tooltipBg.transform.Find("TxtCom").GetComponent<TextMeshProUGUI>();
            SignalBus<SignalUiMainMenuTooltipChange>.Subscribe(ChangeState).AddTo(_disposables);
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
            Vector3 desiredPosition = Input.mousePosition + tooltipOffset;
            Vector2 anchoredPosition = desiredPosition / _canvasRectTransform.localScale.x;
            if (anchoredPosition.x + _rectTransform.rect.width > _canvasRectTransform.rect.width)
                anchoredPosition.x = _canvasRectTransform.rect.width - _rectTransform.rect.width;
            if (anchoredPosition.y > _canvasRectTransform.rect.height)
                anchoredPosition.y = _canvasRectTransform.rect.height - _rectTransform.rect.height;
            _rectTransform.anchoredPosition = anchoredPosition;
        }

        // --------------------------------------------------------------------------------------------------------------
        // Signal
        // --------------------------------------------------------------------------------------------------------------
        private void ChangeState(SignalUiMainMenuTooltipChange signal)
        {
            _textLevelName.text = signal.LevelName;
            _txtAcc.text = signal.Acc;
            _txtCom.text = signal.Com;
            _tooltipBg.SetActive(signal.Showing);
            Cursor.SetCursor(signal.Showing ? tooltipCursor : normalCursor, cursorHotspot, cursorMode);
        }
    }
}
