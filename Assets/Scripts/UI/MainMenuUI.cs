using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public TMPro.TMP_Text statsText;

    private void Start()
    {
        if (statsText != null)
        {
            float bestTime = PlayerPrefs.GetFloat("BestTime", float.MaxValue);
            int bestWaves = PlayerPrefs.GetInt("BestWaves", 0);

            string stats = "";
            if (bestTime < float.MaxValue)
                stats += $"Fastest Win: {bestTime:F1}s\n";
            if (bestWaves > 0)
                stats += $"Max Waves Survived: {bestWaves}";

            statsText.text = stats.Trim();
        }
    }
    public void OnPlayButton()
    {
        SceneManager.LoadScene("Gameplay");
    }

    public void OnQuitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
