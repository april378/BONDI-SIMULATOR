using BondiSimulator.Core;
using UnityEngine;

namespace BondiSimulator.Vehicle
{
    /// <summary>
    /// Builds the lightweight vehicle proving ground used by PhysicsTest.unity.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PhysicsTestSceneBuilder : MonoBehaviour
    {
        [SerializeField] private GameObject vehiclePrefab;
        [SerializeField] private Vector3 spawnPosition = new(0f, 0f, 0f);
        [SerializeField] private Vector3 spawnEulerAngles;
        [SerializeField] private bool startRaceOnLoad = true;

        private void Awake()
        {
            BuildSurface("Large_Test_Ground", new Vector3(0f, -0.5f, 260f), new Vector3(520f, 1f, 920f), Vector3.zero);
            BuildSurface("Long_Braking_Strip", new Vector3(0f, -0.09f, 70f), new Vector3(12f, 0.24f, 150f), Vector3.zero);
            BuildSurface("Wide_Turn_Area", new Vector3(42f, -0.08f, 130f), new Vector3(58f, 0.24f, 58f), Vector3.zero);
            BuildSurface("Ramp_Test", new Vector3(-34f, 0.55f, 148f), new Vector3(9f, 0.25f, 20f), new Vector3(-10f, 0f, 0f));
            BuildSurface("Ramp_Landing", new Vector3(-34f, 1.2f, 170f), new Vector3(18f, 0.1f, 26f), Vector3.zero);
            BuildPhysicsChallenges();

            if (vehiclePrefab == null)
            {
                Debug.LogWarning($"{nameof(PhysicsTestSceneBuilder)} has no vehicle prefab assigned.", this);
                return;
            }

            Instantiate(vehiclePrefab, spawnPosition, Quaternion.Euler(spawnEulerAngles));
        }

        private void Start()
        {
            if (startRaceOnLoad && ServiceLocator.TryGet(out GameManager gameManager))
            {
                gameManager.StartRace();
            }
        }

        private static void BuildSurface(string objectName, Vector3 position, Vector3 scale, Vector3 eulerAngles)
        {
            GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
            surface.name = objectName;
            surface.transform.SetPositionAndRotation(position, Quaternion.Euler(eulerAngles));
            surface.transform.localScale = scale;
        }

        private static void BuildPhysicsChallenges()
        {
            BuildSpeedBumps();
            BuildSlalomCourse();
            BuildNarrowGateCourse();
            BuildCurbAndSidewalkTest();
            BuildUnevenPatch();
            BuildSideSlopeTest();
            BuildReverseParkingBay();
        }

        private static void BuildSpeedBumps()
        {
            for (int i = 0; i < 5; i++)
            {
                float z = 28f + i * 8f;
                BuildBox($"Speed_Bump_{i + 1}", new Vector3(0f, 0.11f, z), new Vector3(9f, 0.22f, 0.55f), Vector3.zero, new Color(0.9f, 0.72f, 0.18f));
            }

            BuildBox("Hard_Braking_Wall", new Vector3(0f, 0.9f, 86f), new Vector3(8f, 1.8f, 0.35f), Vector3.zero, new Color(0.8f, 0.18f, 0.14f));
        }

        private static void BuildSlalomCourse()
        {
            for (int i = 0; i < 8; i++)
            {
                float x = i % 2 == 0 ? -3.2f : 3.2f;
                float z = 105f + i * 8f;
                BuildCylinder($"Slalom_Bollard_{i + 1}", new Vector3(x, 0.55f, z), 0.45f, 1.1f, new Color(0.95f, 0.38f, 0.08f));
            }
        }

        private static void BuildNarrowGateCourse()
        {
            for (int i = 0; i < 4; i++)
            {
                float z = 108f + i * 15f;
                float halfWidth = 1.75f + i * 0.12f;
                BuildCylinder($"Narrow_Gate_{i + 1}_Left", new Vector3(-halfWidth, 1.1f, z), 0.28f, 2.2f, new Color(0.1f, 0.45f, 0.95f));
                BuildCylinder($"Narrow_Gate_{i + 1}_Right", new Vector3(halfWidth, 1.1f, z), 0.28f, 2.2f, new Color(0.1f, 0.45f, 0.95f));
            }
        }

        private static void BuildCurbAndSidewalkTest()
        {
            BuildBox("Left_Curb_Run", new Vector3(-11.5f, 0.14f, 138f), new Vector3(1.2f, 0.28f, 42f), Vector3.zero, new Color(0.35f, 0.35f, 0.35f));
            BuildBox("Right_Curb_Run", new Vector3(11.5f, 0.14f, 138f), new Vector3(1.2f, 0.28f, 42f), Vector3.zero, new Color(0.35f, 0.35f, 0.35f));
            BuildBox("Sidewalk_Plate_Left", new Vector3(-15f, 0.09f, 138f), new Vector3(5.8f, 0.18f, 42f), Vector3.zero, new Color(0.48f, 0.48f, 0.46f));
            BuildBox("Sidewalk_Plate_Right", new Vector3(15f, 0.09f, 138f), new Vector3(5.8f, 0.18f, 42f), Vector3.zero, new Color(0.48f, 0.48f, 0.46f));
        }

        private static void BuildUnevenPatch()
        {
            for (int row = 0; row < 5; row++)
            {
                for (int column = 0; column < 4; column++)
                {
                    float height = 0.08f + ((row + column) % 3) * 0.045f;
                    float x = -4.5f + column * 3f;
                    float z = 176f + row * 4.2f;
                    BuildBox($"Uneven_Patch_{row + 1}_{column + 1}", new Vector3(x, height * 0.5f, z), new Vector3(2.2f, height, 2.1f), Vector3.zero, new Color(0.18f, 0.18f, 0.18f));
                }
            }
        }

        private static void BuildSideSlopeTest()
        {
            BuildBox("Side_Slope_Test", new Vector3(43f, 0.15f, 72f), new Vector3(28f, 0.3f, 48f), new Vector3(0f, 0f, 6f), new Color(0.42f, 0.43f, 0.42f));
            BuildBox("Side_Slope_Exit", new Vector3(43f, 0.08f, 101f), new Vector3(28f, 0.16f, 10f), Vector3.zero, new Color(0.34f, 0.34f, 0.34f));
        }

        private static void BuildReverseParkingBay()
        {
            Vector3 origin = new(-42f, 0f, 55f);
            BuildBox("Reverse_Bay_Back_Stop", origin + new Vector3(0f, 0.45f, 9f), new Vector3(9f, 0.9f, 0.35f), Vector3.zero, new Color(0.75f, 0.18f, 0.18f));
            BuildBox("Reverse_Bay_Left_Wall", origin + new Vector3(-4.5f, 0.45f, 2.5f), new Vector3(0.35f, 0.9f, 13f), Vector3.zero, new Color(0.75f, 0.18f, 0.18f));
            BuildBox("Reverse_Bay_Right_Wall", origin + new Vector3(4.5f, 0.45f, 2.5f), new Vector3(0.35f, 0.9f, 13f), Vector3.zero, new Color(0.75f, 0.18f, 0.18f));
            BuildCylinder("Reverse_Bay_Entry_Left", origin + new Vector3(-6.5f, 0.75f, -6f), 0.35f, 1.5f, new Color(0.95f, 0.38f, 0.08f));
            BuildCylinder("Reverse_Bay_Entry_Right", origin + new Vector3(6.5f, 0.75f, -6f), 0.35f, 1.5f, new Color(0.95f, 0.38f, 0.08f));
        }

        private static GameObject BuildBox(string objectName, Vector3 position, Vector3 scale, Vector3 eulerAngles, Color color)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = objectName;
            box.transform.SetPositionAndRotation(position, Quaternion.Euler(eulerAngles));
            box.transform.localScale = scale;
            ApplyColor(box, color);
            return box;
        }

        private static GameObject BuildCylinder(string objectName, Vector3 position, float diameter, float height, Color color)
        {
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.name = objectName;
            cylinder.transform.position = position;
            cylinder.transform.localScale = new Vector3(diameter, height * 0.5f, diameter);
            ApplyColor(cylinder, color);
            return cylinder;
        }

        private static void ApplyColor(GameObject target, Color color)
        {
            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (shader != null)
            {
                renderer.sharedMaterial = new Material(shader)
                {
                    color = color
                };
            }
        }
    }
}
