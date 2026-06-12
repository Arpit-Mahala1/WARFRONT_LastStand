using System.Collections.Generic;
using UnityEngine;

namespace Deprecated
{
    public class OldSmokeScreen : MonoBehaviour
    {
        // Public static collection used elsewhere (e.g. UnitCombat)
        public static List<OldSmokeScreen> ActiveScreens { get; private set; } = new List<OldSmokeScreen>();

        [Tooltip("Radius of the smoke area used for line-of-sight checks.")]
        public float radius = 5f;

        private void OnEnable()
        {
            if (!ActiveScreens.Contains(this))
                ActiveScreens.Add(this);
        }

        private void OnDisable()
        {
            ActiveScreens.Remove(this);
        }

        // Returns true if point is inside the smoke radius
        public bool ContainsPoint(Vector3 point)
        {
            return Vector3.SqrMagnitude(transform.position - point) <= radius * radius;
        }
    }
}