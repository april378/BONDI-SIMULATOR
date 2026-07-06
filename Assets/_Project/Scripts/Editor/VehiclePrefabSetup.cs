using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BondiSimulator.EditorTools
{
    public static class VehiclePrefabSetup
    {
        private const string BusModelPath = "Assets/_Project/Art/Models/Vehicles/Quaternius/PublicTransport/Bus.obj";
        private const string VehiclePrefabPath = "Assets/_Project/Prefabs/Vehicles/VEH_OH1618_Ugarte_Base.prefab";
        private const string PhysicsTestScenePath = "Assets/_Project/Scenes/PhysicsTest.unity";

        [MenuItem("Bondi Simulator/Setup Vehicle Base Visual")]
        public static void SetupVehicleBaseVisual()
        {
            AssetDatabase.ImportAsset(BusModelPath, ImportAssetOptions.ForceUpdate);

            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(BusModelPath);
            if (modelAsset == null)
            {
                throw new FileNotFoundException($"Could not load bus model at {BusModelPath}.");
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(VehiclePrefabPath);
            try
            {
                Transform visualRoot = prefabRoot.transform.Find("Visual_Model_GenericUrbanBus");
                if (visualRoot == null)
                {
                    visualRoot = new GameObject("Visual_Model_GenericUrbanBus").transform;
                    visualRoot.SetParent(prefabRoot.transform, false);
                }

                for (int i = visualRoot.childCount - 1; i >= 0; i--)
                {
                    Object.DestroyImmediate(visualRoot.GetChild(i).gameObject);
                }

                GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset, visualRoot);
                visual.name = "Bus_Quaternius_PublicTransport";
                FitVisualToBusBounds(visual.transform, new Vector3(2.55f, 2.9f, 10.2f));

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, VehiclePrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            Scene scene = EditorSceneManager.OpenScene(PhysicsTestScenePath, OpenSceneMode.Single);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        private static void FitVisualToBusBounds(Transform visual, Vector3 targetSize)
        {
            visual.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            visual.localScale = Vector3.one;

            Bounds bounds = CalculateLocalBounds(visual);
            if (bounds.size == Vector3.zero)
            {
                return;
            }

            bool lengthIsOnX = bounds.size.x > bounds.size.z;
            if (lengthIsOnX)
            {
                visual.localRotation = Quaternion.Euler(0f, -90f, 0f);
                bounds = CalculateLocalBounds(visual);
            }

            float scale = Mathf.Min(
                targetSize.x / Mathf.Max(bounds.size.x, 0.001f),
                targetSize.y / Mathf.Max(bounds.size.y, 0.001f),
                targetSize.z / Mathf.Max(bounds.size.z, 0.001f));

            visual.localScale = Vector3.one * scale;
            bounds = CalculateLocalBounds(visual);

            Vector3 offset = new(-bounds.center.x, -bounds.min.y + 0.12f, -bounds.center.z);
            visual.localPosition = offset;
        }

        private static Bounds CalculateLocalBounds(Transform root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return new Bounds(Vector3.zero, Vector3.zero);
            }

            Bounds bounds = new(root.InverseTransformPoint(renderers[0].bounds.center), Vector3.zero);
            foreach (Renderer renderer in renderers)
            {
                Vector3 center = root.InverseTransformPoint(renderer.bounds.center);
                Vector3 size = root.InverseTransformVector(renderer.bounds.size);
                bounds.Encapsulate(new Bounds(center, Abs(size)));
            }

            return bounds;
        }

        private static Vector3 Abs(Vector3 value)
        {
            return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
        }
    }
}
