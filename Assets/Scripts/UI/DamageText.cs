using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Floating damage number. Works entirely from Start() so coroutines
/// are guaranteed to run. Set public fields before the first frame via
/// DamageTextManager, or call Setup() directly.
/// </summary>
[RequireComponent(typeof(TextMeshPro))]
public class DamageText : MonoBehaviour
{
    // ── Values written by the spawner BEFORE Start() runs ──────────────────
    [HideInInspector] public float  damageAmount   = 10f;
    [HideInInspector] public Color  displayColor   = Color.white;
    [HideInInspector] public float  displaySize    = 5f;

    // ── Tweakable on the prefab ─────────────────────────────────────────────
    [Header("Animation")]
    public float lifetime        = 1.4f;
    public float floatSpeed      = 1.8f;
    public float horizontalDrift = 0.3f;
    public float holdTime        = 0.3f;

    // ───────────────────────────────────────────────────────────────────────
    private void Start()
    {
        TextMeshPro tmp = GetComponent<TextMeshPro>();

        // Apply values that were injected before this Start() frame.
        tmp.SetText(Mathf.RoundToInt(damageAmount).ToString());
        tmp.color    = displayColor;
        tmp.fontSize = displaySize;
        tmp.ForceMeshUpdate(true, true);

        StartCoroutine(Animate(tmp, Camera.main));
    }

    private IEnumerator Animate(TextMeshPro tmp, Camera cam)
    {
        float elapsed      = 0f;
        float fadeDuration = Mathf.Max(0.01f, lifetime - holdTime);
        float drift        = Random.Range(-horizontalDrift, horizontalDrift);
        Color baseColor    = displayColor;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;

            // Rise + drift
            transform.position += new Vector3(drift, floatSpeed, 0f) * Time.deltaTime;

            // Always face the camera
            if (cam != null)
                transform.rotation = cam.transform.rotation;

            // Quick scale punch on spawn
            float scalePunch = Mathf.Lerp(1.3f, 1f, Mathf.Clamp01(elapsed / 0.12f));
            transform.localScale = Vector3.one * scalePunch;

            // Fade alpha after hold time
            if (elapsed > holdTime)
            {
                float t    = (elapsed - holdTime) / fadeDuration;
                Color c    = baseColor;
                c.a        = Mathf.Lerp(1f, 0f, t);
                tmp.color  = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
