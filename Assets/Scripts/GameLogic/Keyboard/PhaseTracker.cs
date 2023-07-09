using UnityEngine;

namespace GameLogic.Keyboard
{
    public class PhaseTracker : MonoBehaviour
    {

        [SerializeField] private Sprite inactiveSprite, activeSprite, passedSprite, mistakeSprite;
        [SerializeField] private SpriteRenderer spriteRenderer;
        public TrackerStatus CurrentStatus { get; private set; } = TrackerStatus.Inactive;

        private void Start()
        {
            spriteRenderer.color = Color.gray;
        }
        
        public void ChangeStatus(TrackerStatus status)
        {
            spriteRenderer.sprite = status switch
            {
                TrackerStatus.Active => activeSprite,
                TrackerStatus.Inactive => inactiveSprite,
                TrackerStatus.Passed => passedSprite,
                TrackerStatus.Mistake => mistakeSprite,
                _ => spriteRenderer.sprite
            };

            CurrentStatus = status;

            if (CurrentStatus == TrackerStatus.Passed)
                spriteRenderer.color = Color.green;

            if (CurrentStatus == TrackerStatus.Mistake)
                spriteRenderer.color = Color.red;
        }
    }

    public enum TrackerStatus
    {
        Active,
        Inactive,
        Passed,
        Mistake,
    }
}