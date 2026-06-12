using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeScreen : MonoBehaviour
{
    public float duration = 10f;
    public float radius = 6f;
    public static List<SmokeScreen> ActiveScreens = new List<SmokeScreen>();

    private void OnEnable()
    {
        ActiveScreens.Add(this);
        StartCoroutine(Expire());
    }

    private void OnDisable()
    {
        ActiveScreens.Remove(this);
    }

    private IEnumerator Expire()
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }

    public bool ContainsPoint(Vector3 position)
    {
        return Vector3.Distance(transform.position, position) <= radius;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.35f);
        Gizmos.DrawSphere(transform.position, radius);
    }
}
// Force recompile
