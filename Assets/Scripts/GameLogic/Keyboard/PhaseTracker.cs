using UnityEngine;

namespace GameLogic.Keyboard
{
    public class PhaseTracker : MonoBehaviour
    {

        [SerializeField] private Sprite inactiveSprite, activeSprite, passedSprite;
        private SpriteRenderer spriteRenderer;
        public TrackerStatus CurrentStatus { get; private set; } = TrackerStatus.INACTIVE;

        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void ChangeStatus(TrackerStatus status)
        {
            spriteRenderer.sprite = status switch
            {
                TrackerStatus.ACTIVE => activeSprite,
                TrackerStatus.INACTIVE => inactiveSprite,
                TrackerStatus.PASSED => passedSprite,
                _ => spriteRenderer.sprite
            };

            CurrentStatus = status;
        }
    }

    public enum TrackerStatus
    {
        ACTIVE,
        INACTIVE,
        PASSED,
    }
}