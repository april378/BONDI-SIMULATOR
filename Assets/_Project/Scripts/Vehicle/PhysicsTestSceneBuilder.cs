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
    }
}
