using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class CreditsMenuUi : BaseUI
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                BackToMainMenu();
            }
        }

        public void VisitSite(string who)
        {
            switch (who)
            {
                case "bench":
                    Application.OpenURL("https://about.rubenbermejoromero.com/");
                    break;
                case "cross":
                    Application.OpenURL("https://crossfirecam.itch.io/");
                    break;
                case "danirbu":
                    Application.OpenURL("https://danirbu.itch.io/");
                    break;
                case "darelt":
                    Application.OpenURL("https://darelt.itch.io/");
                    break;
                case "tesla":
                    Application.OpenURL("https://teslasp2.com/");
                    break;
                case "fireotters":
                    Application.OpenURL("https://fireotters.com");
                    break;
            }
        }

        public void BackToMainMenu()
        {
            Invoke(nameof(BackToMainMenu2), 0.2f);
        }

        private void BackToMainMenu2()
        {
            SceneManager.LoadScene("Scenes/MainMenu");
        }
    }
}