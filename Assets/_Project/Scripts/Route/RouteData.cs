using UnityEngine;

namespace BondiSimulator.Route
{
    /// <summary>
    /// Data asset that describes a playable route, including spawn, waypoints, finish and soft boundaries.
    /// </summary>
    [CreateAssetMenu(fileName = "RouteData", menuName = "Bondi Simulator/Route Data")]
    public sealed class RouteData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string routeId = "route";
        [SerializeField] private string displayName = "Route";

        [Header("Spawn")]
        [SerializeField] private Vector3 spawnPosition;
        [SerializeField] private Vector3 spawnEulerAngles;

        [Header("Progress")]
        [SerializeField] private Vector3[] waypoints = new Vector3[0];
        [SerializeField, Min(1f)] private float waypointRadius = 14f;
        [SerializeField, Min(1f)] private float finishRadius = 18f;

        [Header("Blockout")]
        [SerializeField, Min(1f)] private float roadWidth = 12f;
        [SerializeField, Min(0.1f)] private float guideWidth = 1.5f;

        [Header("Bounds")]
        [SerializeField] private Vector3 boundsCenter;
        [SerializeField] private Vector3 boundsSize = new(100f, 30f, 100f);
        [SerializeField, Min(0f)] private float fallResetY = -8f;

        public string RouteId => routeId;
        public string DisplayName => displayName;
        public Vector3 SpawnPosition => spawnPosition;
        public Quaternion SpawnRotation => Quaternion.Euler(spawnEulerAngles);
        public Vector3 SpawnEulerAngles => spawnEulerAngles;
        public int WaypointCount => waypoints?.Length ?? 0;
        public float WaypointRadius => waypointRadius;
        public float FinishRadius => finishRadius;
        public float RoadWidth => roadWidth;
        public float GuideWidth => guideWidth;
        public Vector3 BoundsCenter => boundsCenter;
        public Vector3 BoundsSize => boundsSize;
        public float FallResetY => fallResetY;

        public Vector3 GetWaypoint(int index)
        {
            if (waypoints == null || index < 0 || index >= waypoints.Length)
            {
                return spawnPosition;
            }

            return waypoints[index];
        }

        public bool IsInsideBounds(Vector3 position)
        {
            Vector3 halfSize = boundsSize * 0.5f;
            Vector3 localPosition = position - boundsCenter;
            return Mathf.Abs(localPosition.x) <= halfSize.x
                && Mathf.Abs(localPosition.y) <= halfSize.y
                && Mathf.Abs(localPosition.z) <= halfSize.z
                && position.y >= fallResetY;
        }

        private void OnValidate()
        {
            waypointRadius = Mathf.Max(1f, waypointRadius);
            finishRadius = Mathf.Max(1f, finishRadius);
            roadWidth = Mathf.Max(1f, roadWidth);
            guideWidth = Mathf.Max(0.1f, guideWidth);
            boundsSize.x = Mathf.Max(1f, boundsSize.x);
            boundsSize.y = Mathf.Max(1f, boundsSize.y);
            boundsSize.z = Mathf.Max(1f, boundsSize.z);
        }
    }
}
