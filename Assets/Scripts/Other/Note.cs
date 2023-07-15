using UnityEngine;

namespace Other
{
    public class Note : MonoBehaviour
    {
        // Just to leave notes on GameObjects in the Inspector
        [TextArea] public string Notes = "";
    }
}