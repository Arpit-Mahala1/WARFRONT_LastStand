using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void OnPlayButton()
    {
        SceneManager.LoadScene("Gameplay");
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }
}
