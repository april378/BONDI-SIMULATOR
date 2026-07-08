using BondiSimulator.Core;
using UnityEngine;

namespace BondiSimulator.Route
{
    /// <summary>
    /// Tracks player progress through route waypoints and handles finish detection and simple recovery.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RouteSystem : MonoBehaviour
    {
        private const float ResetForwardOffset = 8f;

        [SerializeField] private RouteData routeData;
        [SerializeField] private Transform player;

        private Rigidbody playerRigidbody;
        private GameManager gameManager;
        private int nextWaypointIndex;
        private int lastPassedWaypointIndex;
        private bool raceFinished;

        public RouteData CurrentRoute => routeData;
        public int NextWaypointIndex => nextWaypointIndex;
        public int LastPassedWaypointIndex => lastPassedWaypointIndex;
        public float Progress01 => routeData != null && routeData.WaypointCount > 1
            ? Mathf.Clamp01(lastPassedWaypointIndex / (float)(routeData.WaypointCount - 1))
            : 0f;

        public void Initialize(RouteData data, Transform playerTransform)
        {
            routeData = data;
            player = playerTransform;
            playerRigidbody = player != null ? player.GetComponent<Rigidbody>() : null;
            ServiceLocator.TryGet(out gameManager);
            nextWaypointIndex = routeData != null && routeData.WaypointCount > 1 ? 1 : 0;
            lastPassedWaypointIndex = 0;
            raceFinished = false;
        }

        private void Awake()
        {
            if (routeData != null && player != null)
            {
                Initialize(routeData, player);
            }
        }

        private void FixedUpdate()
        {
            if (routeData == null || player == null)
            {
                return;
            }

            if (gameManager == null)
            {
                ServiceLocator.TryGet(out gameManager);
            }

            if (!routeData.IsInsideBounds(player.position))
            {
                RepositionPlayer();
                return;
            }

            if (raceFinished || gameManager == null || !gameManager.IsRacing)
            {
                return;
            }

            AdvanceProgress();
        }

        private void AdvanceProgress()
        {
            if (nextWaypointIndex >= routeData.WaypointCount)
            {
                CompleteRoute();
                return;
            }

            Vector3 target = routeData.GetWaypoint(nextWaypointIndex);
            float radius = nextWaypointIndex == routeData.WaypointCount - 1
                ? routeData.FinishRadius
                : routeData.WaypointRadius;

            if (DistanceXZ(player.position, target) > radius)
            {
                return;
            }

            lastPassedWaypointIndex = nextWaypointIndex;
            nextWaypointIndex++;

            if (lastPassedWaypointIndex >= routeData.WaypointCount - 1)
            {
                CompleteRoute();
            }
        }

        private void CompleteRoute()
        {
            if (raceFinished)
            {
                return;
            }

            raceFinished = true;
            gameManager?.CompleteRace();
        }

        private void RepositionPlayer()
        {
            int safeIndex = Mathf.Clamp(lastPassedWaypointIndex, 0, routeData.WaypointCount - 1);
            Vector3 safePosition = routeData.WaypointCount > 0 ? routeData.GetWaypoint(safeIndex) : routeData.SpawnPosition;
            Vector3 nextPosition = routeData.WaypointCount > safeIndex + 1
                ? routeData.GetWaypoint(safeIndex + 1)
                : safePosition + Vector3.forward;

            Vector3 direction = nextPosition - safePosition;
            direction.y = 0f;
            Quaternion rotation = direction.sqrMagnitude > 0.01f
                ? Quaternion.LookRotation(direction.normalized, Vector3.up)
                : routeData.SpawnRotation;

            Vector3 resetPosition = direction.sqrMagnitude > 0.01f
                ? safePosition + direction.normalized * Mathf.Min(ResetForwardOffset, direction.magnitude * 0.5f)
                : safePosition;

            player.SetPositionAndRotation(resetPosition, rotation);

            if (playerRigidbody == null)
            {
                playerRigidbody = player.GetComponent<Rigidbody>();
            }

            if (playerRigidbody != null)
            {
                playerRigidbody.linearVelocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
            }
        }

        private static float DistanceXZ(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}
