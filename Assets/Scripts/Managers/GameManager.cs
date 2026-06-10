using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int supplyAmount = 100;
    public float playerHQHealth = 100f;
    public float enemyHQHealth = 100f;
    public bool gameIsOver = false;

    public static bool PlayerWon;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = 60;
    }

    public void AddSupply(int amount)
    {
        supplyAmount += amount;
    }

    public void SpendSupply(int amount)
    {
        supplyAmount = Mathf.Max(0, supplyAmount - amount);
    }

    public void DamagePlayerHQ(float amount)
    {
        playerHQHealth -= amount;
        if (playerHQHealth <= 0f)
        {
            playerHQHealth = 0f;
            EndGame(false);
        }
    }

    public void DamageEnemyHQ(float amount)
    {
        enemyHQHealth -= amount;
        if (enemyHQHealth <= 0f)
        {
            enemyHQHealth = 0f;
            EndGame(true);
        }
    }

    private void EndGame(bool playerWon)
    {
        gameIsOver = true;
        PlayerWon = playerWon;
        SceneManager.LoadScene("GameOver");
    }
}
