using UnityEngine;

public class CaptureZone : MonoBehaviour
{
    public float captureSupplyPerSecond = 10f;
    public string playerUnitTag = "PlayerUnit";

    private int playerUnitsInside = 0;
    private bool isCaptured = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerUnitTag))
            return;

        playerUnitsInside++;

        if (playerUnitsInside > 0)
            isCaptured = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerUnitTag))
            return;

        playerUnitsInside--;

        if (playerUnitsInside <= 0)
        {
            playerUnitsInside = 0;
            isCaptured = false;
        }
    }

    private void Update()
    {
        if (isCaptured)
            GameManager.Instance.AddSupply((int)(captureSupplyPerSecond * Time.deltaTime));
    }
}
