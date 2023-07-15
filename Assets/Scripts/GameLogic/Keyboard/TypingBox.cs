using TMPro;
using UnityEngine;

namespace GameLogic.Keyboard
{
    public class TypingBox : MonoBehaviour
    {
        [Header("Typing Preview / Input")]
        [SerializeField] private TMP_Text _textPreview;
        [SerializeField] private TMP_Text _textInput;

        [Header("Progress Trackers")]
        [SerializeField] private GameObject _trackerProgress;
        [SerializeField] private GameObject _trackerMistakes;
        [SerializeField] private GameObject _wordTrackerPrefab; //change to PhaseTracker

        // --------------------------------------------------------------------------------------------------------------
        // Preview & Input
        // --------------------------------------------------------------------------------------------------------------
        public void ChangeWord(string t)
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


        // --------------------------------------------------------------------------------------------------------------
        // Progress/Mistake Trackers
        // --------------------------------------------------------------------------------------------------------------
        public void InitTrackerProgress(int numOfPhrases)
        {
            var posProg = _trackerProgress.transform.position;
            var x = posProg.x;
            for (var i = 0; i < numOfPhrases; i++)
            {
                var newGameObject
                    = Instantiate(_wordTrackerPrefab, new Vector3(x, posProg.y),
                        Quaternion.identity, _trackerProgress.transform);
                x -= .5f;
                var tracker = newGameObject.GetComponent<PhaseTracker>();
                tracker.ChangeStatus(i == numOfPhrases - 1 ? TrackerStatus.Active : TrackerStatus.Inactive);
            }
        }

        public void InitTrackerMistakes(int permittedNumOfMistakes)
        {
            var posMist = _trackerMistakes.transform.position;
            var x = posMist.x;
            for (var i = 0; i < permittedNumOfMistakes; i++)
            {
                Instantiate(_wordTrackerPrefab, new Vector3(x, posMist.y),
                        Quaternion.identity, _trackerMistakes.transform);
                x -= .5f;
            }
        }

        public void IncrementProgress()
        {
            var trackers = _trackerProgress.GetComponentsInChildren<PhaseTracker>();

            // Update HUD (the objects are instanced backwards, so iterate through the list backwards)
            for (var i = trackers.Length - 1; i >= 0; i--)
            {
                var tracker = trackers[i];
                // Ignore levels already 'Passed'
                if (tracker.CurrentStatus.Equals(TrackerStatus.Passed))
                    continue;
                // If a level is 'Active', set it to 'Passed'. Set next level (if it exists) to 'Active'
                if (tracker.CurrentStatus.Equals(TrackerStatus.Active))
                {
                    tracker.ChangeStatus(TrackerStatus.Passed);
                    if (i > 0)
                    {
                        var nextTracker = trackers[i - 1];
                        nextTracker.ChangeStatus(TrackerStatus.Active);
                    }
                    break; // Don't mark any more levels
                }
            }
        }

        public void IncrementMistake()
        {
            var mistakeTrackers = _trackerMistakes.GetComponentsInChildren<PhaseTracker>();

            // Update HUD (the objects are instanced backwards, so iterate through the list backwards)
            for (var i = mistakeTrackers.Length - 1; i >= 0; i--)
            {
                var currentTracker = mistakeTrackers[i];
                if (currentTracker.CurrentStatus.Equals(TrackerStatus.Mistake))
                    continue;
                if (currentTracker.CurrentStatus.Equals(TrackerStatus.Inactive))
                {
                    currentTracker.ChangeStatus(TrackerStatus.Mistake);
                    break;
                }
            }
        }
    }
}