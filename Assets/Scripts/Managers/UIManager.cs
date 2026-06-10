using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TMP_Text supplyText;
    public TMP_Text waveText;
    public Slider playerHQSlider;
    public Slider enemyHQSlider;
    public HQHealth playerHQ;
    public HQHealth enemyHQ;
    public WaveManager waveManager;

    private void Update()
    {
        supplyText.text = "Supply: " + GameManager.Instance.supplyAmount;
        waveText.text = "Wave " + waveManager.waveNumber + " — " + Mathf.CeilToInt(waveManager.timeUntilNextWave) + "s";
        playerHQSlider.value = playerHQ.GetHealthPercent();
        enemyHQSlider.value = enemyHQ.GetHealthPercent();
    }
}
