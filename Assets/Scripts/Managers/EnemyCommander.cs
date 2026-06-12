using UnityEngine;

namespace Deprecated
{
    public class OldEnemyCommander : MonoBehaviour
    {
        [Range(0, 10)]
        public int AggressionLevel = 1;

        // Simple round-robin lane selection. Keeps internal state so consecutive calls cycle lanes.
        private int lastLane = -1;

        // Primary public API (WaveManager calls the parameterless variant).
        public int GetNextLane()
        {
            // Try to find WaveManager to use its actual lane count when available.
            var wm = FindObjectOfType<WaveManager>();
            int lanes = wm != null && wm.laneSpawnPoints != null && wm.laneSpawnPoints.Length > 0
                ? wm.laneSpawnPoints.Length
                : 1;

            return GetNextLane(lanes);
        }

        // Overload that accepts lane count (useful for tests or future calls)
        public int GetNextLane(int laneCount)
        {
            if (laneCount <= 0) laneCount = 1;
            lastLane = (lastLane + 1) % laneCount;
            return lastLane;
        }
    }
}