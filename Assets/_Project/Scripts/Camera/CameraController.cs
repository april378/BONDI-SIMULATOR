using BondiSimulator.Core;
using UnityEngine;

namespace BondiSimulator.Cameras
{
    /// <summary>
    /// Runtime camera switcher for third-person and first-person bus views.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 thirdPersonOffset = new(0f, 5.5f, -11f);
        [SerializeField] private Vector3 firstPersonOffset = new(-0.45f, 2.45f, 3.75f);
        [SerializeField, Min(0.01f)] private float positionSharpness = 9f;
        [SerializeField, Min(0.01f)] private float rotationSharpness = 12f;

        private PlayerInputReader inputReader;
        private bool inputSubscribed;
        private bool firstPersonActive;

        public bool FirstPersonActive => firstPersonActive;

        public void SetTarget(Transform followTarget)
        {
            target = followTarget;
            SnapToTarget();
        }

        private void OnEnable()
        {
            TrySubscribeInput();
        }

        private void OnDisable()
        {
            if (inputReader != null && inputSubscribed)
            {
                inputReader.ChangeCameraPressed -= ToggleCamera;
            }

            inputSubscribed = false;
            inputReader = null;
        }

        private void LateUpdate()
        {
            TrySubscribeInput();

            if (target == null)
            {
                TryResolveTarget();
            }

            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = firstPersonActive
                ? target.TransformPoint(firstPersonOffset)
                : target.TransformPoint(thirdPersonOffset);
            Quaternion desiredRotation = firstPersonActive
                ? target.rotation
                : Quaternion.LookRotation((target.position + Vector3.up * 1.8f) - desiredPosition, Vector3.up);

            float positionT = 1f - Mathf.Exp(-positionSharpness * Time.deltaTime);
            float rotationT = 1f - Mathf.Exp(-rotationSharpness * Time.deltaTime);
            transform.SetPositionAndRotation(
                Vector3.Lerp(transform.position, desiredPosition, positionT),
                Quaternion.Slerp(transform.rotation, desiredRotation, rotationT));
        }

        private void ToggleCamera()
        {
            firstPersonActive = !firstPersonActive;
            SnapToTarget();
        }

        private void SnapToTarget()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = firstPersonActive
                ? target.TransformPoint(firstPersonOffset)
                : target.TransformPoint(thirdPersonOffset);
            Quaternion desiredRotation = firstPersonActive
                ? target.rotation
                : Quaternion.LookRotation((target.position + Vector3.up * 1.8f) - desiredPosition, Vector3.up);

            transform.SetPositionAndRotation(desiredPosition, desiredRotation);
        }

        private void TrySubscribeInput()
        {
            if (inputSubscribed)
            {
                return;
            }

            if (ServiceLocator.TryGet(out inputReader))
            {
                inputReader.ChangeCameraPressed += ToggleCamera;
                inputSubscribed = true;
            }
        }

        private void TryResolveTarget()
        {
            Vehicle.VehicleController vehicle = FindFirstObjectByType<Vehicle.VehicleController>();
            if (vehicle != null)
            {
                SetTarget(vehicle.transform);
            }
        }
    }
}
