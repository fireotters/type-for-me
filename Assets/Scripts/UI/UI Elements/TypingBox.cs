using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TypingBox : MonoBehaviour
{
    [SerializeField] private TMP_Text _textPreview;
    [SerializeField] private TMP_Text _textInput;

    public void Init(string t)
    {
        _textPreview.text = t;
        _textInput.text = "";
    }

    public bool InputIsValid(string input)
    {
        // Find the expected character. I.E. The preview says 'Dog', input says 'Do'. The expected character is 'g'.
        var expectedCharacter = _textPreview.text[_textInput.text.Length];

        if (expectedCharacter.ToString() == input)
        {
            _textInput.text += input;
            return true;
        }
        else
        {
            if (input == " ")
                input = "_"; // Indicate an incorrect SpaceKey usage
            _textInput.text += $"<color=#FF0000>{input}</color>";
            return false;
        }
    }

    public bool InputIsFinished()
    {
        if (_textPreview.text.Equals(_textInput.text))
        {
            _textInput.text = $"<color=#4EFF00>{_textInput.text}</color>";
            return true;
        }
        return false;
    }

    public void PerformBackspace()
    {
        var cur = _textInput.text;
        string newText;
        if (cur.Length == 0)
            return;
        else if (cur.EndsWith(">"))
        {
            // Remove RTF tags. E.g: 123<color>4</color>
            // Everything from the last '>' to the second-last '<' will be deleted
            int posOfSecondLastRTFOpener = cur[..cur.LastIndexOf("<")].LastIndexOf("<");
            newText = cur[..posOfSecondLastRTFOpener];
        }
        else
            newText = cur[..^1]; // Remove last character

        _textInput.text = newText;
    }

    public bool CompletedWordWasFinal(string finalWordAvailable)
    {
        if (_textPreview.text.Equals(finalWordAvailable))
            return true;
        else
            return false;
    }
}
