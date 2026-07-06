using BondiSimulator.Core;
using UnityEngine;

namespace BondiSimulator.Route
{
    /// <summary>
    /// Connects a route scene to route data, the player vehicle prefab and generated blockout geometry.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RouteSystem))]
    public sealed class RouteSceneBinder : MonoBehaviour
    {
        [SerializeField] private RouteData routeData;
        [SerializeField] private GameObject playerVehiclePrefab;
        [SerializeField] private bool buildBlockoutGeometry = true;
        [SerializeField] private bool autoBeginCountdown = true;
        [SerializeField] private bool buildWaypointMarkers = true;

        private RouteSystem routeSystem;
        private Transform generatedRoot;
        private GameObject playerInstance;

        private void Awake()
        {
            routeSystem = GetComponent<RouteSystem>();

            if (routeData == null)
            {
                Debug.LogError($"{nameof(RouteSceneBinder)} has no route data assigned.", this);
                return;
            }

            if (buildBlockoutGeometry)
            {
                BuildBlockoutGeometry();
            }

            SpawnPlayer();
            routeSystem.Initialize(routeData, playerInstance != null ? playerInstance.transform : null);
        }

        private void Start()
        {
            if (!autoBeginCountdown)
            {
                return;
            }

            if (ServiceLocator.TryGet(out GameManager gameManager))
            {
                gameManager.BeginCountdown();
            }
        }

        private void SpawnPlayer()
        {
            if (playerVehiclePrefab == null)
            {
                Debug.LogError($"{nameof(RouteSceneBinder)} has no player vehicle prefab assigned.", this);
                return;
            }

            playerInstance = Instantiate(playerVehiclePrefab, routeData.SpawnPosition, routeData.SpawnRotation);
            playerInstance.name = "PlayerVehicle";
        }

        private void BuildBlockoutGeometry()
        {
            generatedRoot = new GameObject($"Route_{routeData.RouteId}_Generated").transform;
            generatedRoot.SetParent(transform, false);

            Material roadMaterial = CreateMaterial(new Color(0.18f, 0.18f, 0.18f, 1f));
            Material guideMaterial = CreateMaterial(new Color(0.15f, 0.65f, 1f, 1f));
            Material markerMaterial = CreateMaterial(new Color(1f, 0.92f, 0.25f, 1f));
            Material finishMaterial = CreateMaterial(new Color(0.1f, 0.9f, 0.25f, 1f));

            for (int i = 0; i < routeData.WaypointCount - 1; i++)
            {
                Vector3 from = routeData.GetWaypoint(i);
                Vector3 to = routeData.GetWaypoint(i + 1);
                BuildSegment($"Road_{i:00}_{i + 1:00}", from, to, routeData.RoadWidth, 0.12f, -0.06f, roadMaterial, true);
                BuildSegment($"Guide_{i:00}_{i + 1:00}", from, to, routeData.GuideWidth, 0.04f, 0.03f, guideMaterial, false);
            }

            if (buildWaypointMarkers)
            {
                BuildWaypointMarkers(markerMaterial, finishMaterial);
            }
        }

        private void BuildWaypointMarkers(Material markerMaterial, Material finishMaterial)
        {
            for (int i = 0; i < routeData.WaypointCount; i++)
            {
                Vector3 position = routeData.GetWaypoint(i);
                float markerSize = i == routeData.WaypointCount - 1 ? routeData.FinishRadius : routeData.WaypointRadius;
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                marker.name = i == routeData.WaypointCount - 1 ? "Finish_Marker" : $"Waypoint_{i:00}";
                marker.transform.SetParent(generatedRoot, false);
                marker.transform.position = position + Vector3.up * 0.04f;
                marker.transform.localScale = new Vector3(markerSize * 2f, 0.05f, markerSize * 2f);
                marker.GetComponent<Renderer>().sharedMaterial = i == routeData.WaypointCount - 1 ? finishMaterial : markerMaterial;
                Collider markerCollider = marker.GetComponent<Collider>();
                markerCollider.isTrigger = true;
            }
        }

        private void BuildSegment(string objectName, Vector3 from, Vector3 to, float width, float height, float yOffset, Material material, bool colliderEnabled)
        {
            Vector3 delta = to - from;
            delta.y = 0f;

            if (delta.sqrMagnitude < 0.01f)
            {
                return;
            }

            GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segment.name = objectName;
            segment.transform.SetParent(generatedRoot, false);
            segment.transform.SetPositionAndRotation((from + to) * 0.5f + Vector3.up * yOffset, Quaternion.LookRotation(delta.normalized, Vector3.up));
            segment.transform.localScale = new Vector3(width, height, delta.magnitude);
            segment.GetComponent<Renderer>().sharedMaterial = material;
            segment.GetComponent<Collider>().enabled = colliderEnabled;
        }

        private static Material CreateMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (shader == null)
            {
                return null;
            }

            return new Material(shader)
            {
                color = color
            };
        }
    }
}
