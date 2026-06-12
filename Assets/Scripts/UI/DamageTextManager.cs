using UnityEngine;

/// <summary>
/// Singleton that spawns floating damage numbers.
/// Place on a single empty GameObject in the scene and assign the DamageText prefab.
/// </summary>
public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager Instance { get; private set; }

    [Header("Prefab")]
    [Tooltip("Drag the DamageText prefab here.")]
    public GameObject damageTextPrefab;

    [Header("Colors")]
    [Tooltip("Color when a player unit / structure / HQ takes damage.  →  RED")]
    public Color playerDamageColor = new Color(1f, 0.22f, 0.22f, 1f);

    [Tooltip("Color when an enemy takes damage.  →  GREEN")]
    public Color enemyDamageColor  = new Color(0.18f, 1f, 0.32f, 1f);

    [Header("Font Size")]
    [Tooltip("Change this ONE value to resize every damage number in the game.")]
    public float fontSize = 5f;

    [Header("Spawn")]
    [Tooltip("Random horizontal scatter so stacked hits spread out.")]
    public float spawnScatter      = 0.5f;

    [Tooltip("How high above the target position the number spawns.")]
    public float spawnHeightOffset = 1.5f;

    // ───────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Spawn a floating damage number.
    /// isPlayerSideDamage = true  → player unit/structure is hit  → RED
    /// isPlayerSideDamage = false → enemy is hit                  → GREEN
    /// </summary>
    public void SpawnDamageText(Vector3 worldPosition, float amount, bool isPlayerSideDamage)
    {
        if (damageTextPrefab == null)
        {
            Debug.LogWarning("[DamageTextManager] damageTextPrefab is not assigned!");
            return;
        }

        if (amount <= 0f)
            return;

        // Scatter so simultaneous hits don't perfectly overlap
        Vector3 scatter  = new Vector3(
            Random.Range(-spawnScatter, spawnScatter),
            0f,
            Random.Range(-spawnScatter, spawnScatter));

        Vector3 spawnPos = worldPosition + Vector3.up * spawnHeightOffset + scatter;

        // Instantiate → Awake fires, then we set fields, then Start fires next frame.
        GameObject obj   = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);
        DamageText dt    = obj.GetComponent<DamageText>();

        if (dt == null)
        {
            Debug.LogError("[DamageTextManager] DamageText prefab is missing the DamageText script component!");
            Destroy(obj);
            return;
        }

        // Inject values BEFORE Start() runs on the next frame.
        dt.damageAmount = amount;
        dt.displayColor = isPlayerSideDamage ? playerDamageColor : enemyDamageColor;
        dt.displaySize  = fontSize;
    }
}
