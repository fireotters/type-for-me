using UnityEngine;

namespace GameLogic.Keyboard
{
    public class PhaseTracker : MonoBehaviour
    {

        [SerializeField] private Sprite inactiveSprite, activeSprite, passedSprite;
        [SerializeField ]private SpriteRenderer spriteRenderer;
        public TrackerStatus CurrentStatus { get; private set; } = TrackerStatus.Inactive;

        public void ChangeStatus(TrackerStatus status)
        {
            spriteRenderer.sprite = status switch
            {
                TrackerStatus.Active => activeSprite,
                TrackerStatus.Inactive => inactiveSprite,
                TrackerStatus.Passed => passedSprite,
                _ => spriteRenderer.sprite
            };

            CurrentStatus = status;
        }
    }

    public enum TrackerStatus
    {
        Active,
        Inactive,
        Passed,
    }
}