using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class ThanksMenuUi : BaseUI
    {
        public void VisitCredits()
        {
            SceneManager.LoadScene("Scenes/CreditsMenu");
        }
    }
}