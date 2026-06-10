using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public TMP_Text resultText;

    private void Start()
    {
        resultText.text = GameManager.PlayerWon ? "You Win!" : "You Lose.";
    }

    public void OnRetryButton()
    {
        SceneManager.LoadScene("Gameplay");
    }

    public void OnMainMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
