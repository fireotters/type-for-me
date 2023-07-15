using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.UI_Elements
{
    public class TMPro_BtnText_HoverColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        // Script to be attached to TextMeshPro buttons. It sets separate colours for highlighting, not highlighting, and clicking the button.

        [SerializeField] private Color baseColor, highlightColor, clickColor;
        [SerializeField] private bool _forceBaseColorAfterClick;
        private TextMeshProUGUI _text;
        private bool _highlighted, _clicked;

        private void Start()
        {
            _text = GetComponentInChildren<TextMeshProUGUI>();
            _text.color = baseColor;
        }

        public void OnPointerEnter(PointerEventData data)
        {
            _highlighted = true;
            if (!_clicked)
                _text.color = highlightColor;
            else if (_clicked)
                _text.color = clickColor;
        }

        public void OnPointerExit(PointerEventData data)
        {
            _highlighted = false;
            _text.color = baseColor;
        }

        public void OnPointerDown(PointerEventData data)
        {
            _clicked = true;
            _text.color = clickColor;
        }

        public void OnPointerUp(PointerEventData data)
        {
            _clicked = false;
            if (_forceBaseColorAfterClick)
                _text.color = baseColor;
            else if (_highlighted)
                _text.color = highlightColor;
            else if (!_highlighted)
                _text.color = baseColor;
        }
    }
}