using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public TMP_Text resultText;

    private void Start()
    {
        if (GameManager.PlayerWon)
        {
            resultText.text = $"You Win!\n<size=50%>Time: {GameManager.CurrentTimeTaken:F1}s</size>";
        }
        else
        {
            resultText.text = $"You Lose.\n<size=50%>Waves Survived: {GameManager.CurrentWavesSurvived}</size>";
        }
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
