using BondiSimulator.Core;
using UnityEngine;

namespace BondiSimulator.Vehicle
{
    /// <summary>
    /// Four-wheel bus controller for the first playable vehicle slice.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class VehicleController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private VehicleData vehicleData;
        [SerializeField] private bool requireRacingState = true;

        [Header("Wheel Colliders")]
        [SerializeField] private WheelCollider frontLeftWheel;
        [SerializeField] private WheelCollider frontRightWheel;
        [SerializeField] private WheelCollider rearLeftWheel;
        [SerializeField] private WheelCollider rearRightWheel;

        [Header("Wheel Visuals")]
        [SerializeField] private Transform frontLeftVisual;
        [SerializeField] private Transform frontRightVisual;
        [SerializeField] private Transform rearLeftVisual;
        [SerializeField] private Transform rearRightVisual;
        [SerializeField] private bool createRuntimeWheelVisuals = true;
        [SerializeField, Min(0.05f)] private float runtimeWheelWidth = 0.32f;
        [SerializeField] private Color runtimeWheelColor = new(0.02f, 0.02f, 0.02f, 1f);

        [Header("Visual Model Fit")]
        [SerializeField] private Transform visualModelRoot;
        [SerializeField] private bool fitVisualModelToVehicleBounds = true;
        [SerializeField] private Vector3 fittedVisualLocalPosition = new(0f, 2.15f, 4.54f);
        [SerializeField] private Vector3 fittedVisualLocalEulerAngles = new(0f, 90f, 0f);
        [SerializeField] private Vector3 fittedVisualLocalScale = new(2.57f, 1.81f, 1.43f);
        [SerializeField] private bool alignVisualModelToWheelColliders = true;
        [SerializeField] private Vector3 visualModelAlignmentOffset = new(0f, 0f, -0.75f);
        [SerializeField] private bool hideEmbeddedWheelRenderers = true;

        [Header("Grounding")]
        [SerializeField] private bool alignToGroundOnStart = true;
        [SerializeField, Min(0f)] private float groundProbeHeight = 5f;
        [SerializeField, Min(0.1f)] private float groundProbeDistance = 25f;
        [SerializeField, Min(0f)] private float rootGroundClearance = 0.02f;

        private Rigidbody busRigidbody;
        private PlayerInputReader inputReader;
        private GameManager gameManager;
        private Material runtimeWheelMaterial;

        public float CurrentSpeedKph => busRigidbody != null ? busRigidbody.linearVelocity.magnitude * 3.6f : 0f;
        public float NormalizedSpeed => vehicleData != null && vehicleData.MaxSpeedKph > 0f
            ? Mathf.Clamp01(CurrentSpeedKph / vehicleData.MaxSpeedKph)
            : 0f;

        private void Awake()
        {
            busRigidbody = GetComponent<Rigidbody>();
            ServiceLocator.TryGet(out inputReader);
            ServiceLocator.TryGet(out gameManager);
            ApplyVehicleData();
            PrepareVisualModel();
            EnsureRuntimeWheelVisuals();
        }

        private void Start()
        {
            if (alignToGroundOnStart)
            {
                AlignRootToGround();
            }
        }

        private void FixedUpdate()
        {
            if (vehicleData == null)
            {
                return;
            }

            float steering = 0f;
            float acceleration = 0f;
            float brake = 0f;
            bool handbrake = false;

            if (CanReadGameplayInput())
            {
                ReadInput(out steering, out acceleration, out brake, out handbrake);
            }

            ApplySteering(steering);
            ApplyDrive(acceleration, brake, handbrake);
            ApplyAntiRoll(frontLeftWheel, frontRightWheel);
            ApplyAntiRoll(rearLeftWheel, rearRightWheel);
            ApplyArcadeStability();
            UpdateWheelVisuals();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            ApplyVehicleData();
        }

        private void ApplyVehicleData()
        {
            if (busRigidbody == null || vehicleData == null)
            {
                return;
            }

            busRigidbody.mass = vehicleData.MassKg;
            busRigidbody.centerOfMass = vehicleData.CenterOfMass;
            busRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            busRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            busRigidbody.solverIterations = 12;
            busRigidbody.solverVelocityIterations = 6;

            ConfigureWheel(frontLeftWheel);
            ConfigureWheel(frontRightWheel);
            ConfigureWheel(rearLeftWheel);
            ConfigureWheel(rearRightWheel);
        }

        private void ConfigureWheel(WheelCollider wheel)
        {
            if (wheel == null || vehicleData == null)
            {
                return;
            }

            wheel.radius = vehicleData.WheelRadius;
            wheel.suspensionDistance = vehicleData.SuspensionDistance;
            wheel.mass = vehicleData.WheelMass;
            wheel.suspensionSpring = new JointSpring
            {
                spring = vehicleData.SuspensionSpring,
                damper = vehicleData.SuspensionDamper,
                targetPosition = vehicleData.SuspensionTargetPosition
            };
            wheel.ConfigureVehicleSubsteps(5f, 12, 15);
        }

        private void AlignRootToGround()
        {
            if (busRigidbody == null || !TryFindGroundBelow(out RaycastHit hit))
            {
                return;
            }

            Vector3 position = busRigidbody.position;
            position.y = hit.point.y + rootGroundClearance;
            busRigidbody.position = position;
            transform.position = position;
            busRigidbody.linearVelocity = Vector3.zero;
            busRigidbody.angularVelocity = Vector3.zero;
            Physics.SyncTransforms();
        }

        private bool TryFindGroundBelow(out RaycastHit groundHit)
        {
            Vector3 origin = transform.position + Vector3.up * groundProbeHeight;
            RaycastHit[] hits = Physics.RaycastAll(
                origin,
                Vector3.down,
                groundProbeHeight + groundProbeDistance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);

            System.Array.Sort(hits, static (a, b) => a.distance.CompareTo(b.distance));
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider != null && !hit.collider.transform.IsChildOf(transform))
                {
                    groundHit = hit;
                    return true;
                }
            }

            groundHit = default;
            return false;
        }

        private void ReadInput(out float steering, out float acceleration, out float brake, out bool handbrake)
        {
            if (inputReader == null)
            {
                ServiceLocator.TryGet(out inputReader);
            }

            if (inputReader == null)
            {
                steering = 0f;
                acceleration = 0f;
                brake = 0f;
                handbrake = false;
                return;
            }

            steering = Mathf.Clamp(inputReader.Steering, -1f, 1f);
            acceleration = Mathf.Clamp01(inputReader.Acceleration);
            brake = Mathf.Clamp01(inputReader.Brake);
            handbrake = inputReader.HandbrakeHeld;
        }

        private bool CanReadGameplayInput()
        {
            if (!requireRacingState)
            {
                return true;
            }

            if (gameManager == null)
            {
                ServiceLocator.TryGet(out gameManager);
            }

            return gameManager != null && gameManager.IsRacing;
        }

        private void ApplySteering(float input)
        {
            float steerFactor = Mathf.Lerp(1f, vehicleData.HighSpeedSteerFactor, NormalizedSpeed);
            float steerAngle = input * vehicleData.MaxSteerAngle * steerFactor;

            if (frontLeftWheel != null)
            {
                frontLeftWheel.steerAngle = steerAngle;
            }

            if (frontRightWheel != null)
            {
                frontRightWheel.steerAngle = steerAngle;
            }
        }

        private void ApplyDrive(float acceleration, float brake, bool handbrake)
        {
            float signedSpeedKph = Vector3.Dot(busRigidbody.linearVelocity, transform.forward) * 3.6f;
            float forwardLimitFactor = Mathf.Clamp01(1f - Mathf.InverseLerp(vehicleData.MaxSpeedKph * 0.92f, vehicleData.MaxSpeedKph, Mathf.Max(0f, signedSpeedKph)));
            float reverseLimitFactor = Mathf.Clamp01(1f - Mathf.InverseLerp(vehicleData.MaxReverseSpeedKph * 0.85f, vehicleData.MaxReverseSpeedKph, Mathf.Max(0f, -signedSpeedKph)));

            bool movingForward = signedSpeedKph > vehicleData.ReverseEngageSpeedKph;
            bool movingBackward = signedSpeedKph < -vehicleData.ReverseEngageSpeedKph;

            float motorTorque = 0f;
            float brakeTorque = 0f;

            if (acceleration > 0f)
            {
                if (movingBackward)
                {
                    brakeTorque = acceleration * vehicleData.BrakeTorque;
                }
                else
                {
                    motorTorque = acceleration * vehicleData.MotorTorque * forwardLimitFactor;
                }
            }

            if (brake > 0f)
            {
                if (movingForward)
                {
                    brakeTorque = Mathf.Max(brakeTorque, brake * vehicleData.BrakeTorque);
                }
                else
                {
                    motorTorque = -brake * vehicleData.ReverseTorque * reverseLimitFactor;
                }
            }

            float handbrakeTorque = handbrake ? vehicleData.HandbrakeTorque : 0f;

            if (rearLeftWheel != null)
            {
                rearLeftWheel.motorTorque = motorTorque;
                rearLeftWheel.brakeTorque = Mathf.Max(brakeTorque, handbrakeTorque);
            }

            if (rearRightWheel != null)
            {
                rearRightWheel.motorTorque = motorTorque;
                rearRightWheel.brakeTorque = Mathf.Max(brakeTorque, handbrakeTorque);
            }

            if (frontLeftWheel != null)
            {
                frontLeftWheel.brakeTorque = brakeTorque;
            }

            if (frontRightWheel != null)
            {
                frontRightWheel.brakeTorque = brakeTorque;
            }
        }

        private void PrepareVisualModel()
        {
            if (visualModelRoot == null)
            {
                visualModelRoot = ResolveVisualModelRoot();
            }

            if (visualModelRoot == null)
            {
                return;
            }

            if (fitVisualModelToVehicleBounds)
            {
                visualModelRoot.SetLocalPositionAndRotation(
                    fittedVisualLocalPosition,
                    Quaternion.Euler(fittedVisualLocalEulerAngles));
                visualModelRoot.localScale = fittedVisualLocalScale;
            }

            if (alignVisualModelToWheelColliders)
            {
                AlignVisualModelToWheelColliders();
            }

            if (hideEmbeddedWheelRenderers)
            {
                HideEmbeddedWheelRenderers();
            }
        }

        private Transform ResolveVisualModelRoot()
        {
            foreach (Transform child in transform)
            {
                if (child.name.Contains("Visual_Model", System.StringComparison.OrdinalIgnoreCase))
                {
                    return child;
                }
            }

            return null;
        }

        private void AlignVisualModelToWheelColliders()
        {
            if (!TryGetVisualWheelCenter("Front", out Vector3 visualFrontCenter)
                || !TryGetVisualWheelCenter("Back", out Vector3 visualRearCenter)
                || !TryGetWheelColliderAxleCenter(frontLeftWheel, frontRightWheel, out Vector3 colliderFrontCenter)
                || !TryGetWheelColliderAxleCenter(rearLeftWheel, rearRightWheel, out Vector3 colliderRearCenter))
            {
                visualModelRoot.localPosition += visualModelAlignmentOffset;
                return;
            }

            Vector3 visualAxleMidpoint = (visualFrontCenter + visualRearCenter) * 0.5f;
            Vector3 colliderAxleMidpoint = (colliderFrontCenter + colliderRearCenter) * 0.5f;
            Vector3 correction = colliderAxleMidpoint - visualAxleMidpoint + visualModelAlignmentOffset;
            correction.y = visualModelAlignmentOffset.y;
            visualModelRoot.localPosition += correction;
        }

        private bool TryGetVisualWheelCenter(string marker, out Vector3 center)
        {
            Renderer[] renderers = visualModelRoot.GetComponentsInChildren<Renderer>(true);
            Bounds bounds = default;
            bool hasBounds = false;

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null
                    || !renderer.name.Contains("Wheel", System.StringComparison.OrdinalIgnoreCase)
                    || !renderer.name.Contains(marker, System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            center = hasBounds ? transform.InverseTransformPoint(bounds.center) : default;
            return hasBounds;
        }

        private bool TryGetWheelColliderAxleCenter(WheelCollider leftWheel, WheelCollider rightWheel, out Vector3 center)
        {
            if (leftWheel == null || rightWheel == null)
            {
                center = default;
                return false;
            }

            center = (leftWheel.transform.localPosition + rightWheel.transform.localPosition) * 0.5f;
            return true;
        }

        private void HideEmbeddedWheelRenderers()
        {
            Renderer[] renderers = visualModelRoot.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null && renderer.name.Contains("Wheel", System.StringComparison.OrdinalIgnoreCase))
                {
                    renderer.enabled = false;
                }
            }
        }

        private void EnsureRuntimeWheelVisuals()
        {
            if (!createRuntimeWheelVisuals || vehicleData == null)
            {
                return;
            }

            runtimeWheelMaterial = CreateRuntimeWheelMaterial();
            frontLeftVisual = CreateRuntimeWheelVisual("RuntimeWheel_FrontLeft", frontLeftWheel);
            frontRightVisual = CreateRuntimeWheelVisual("RuntimeWheel_FrontRight", frontRightWheel);
            rearLeftVisual = CreateRuntimeWheelVisual("RuntimeWheel_RearLeft", rearLeftWheel);
            rearRightVisual = CreateRuntimeWheelVisual("RuntimeWheel_RearRight", rearRightWheel);
        }

        private Transform CreateRuntimeWheelVisual(string objectName, WheelCollider sourceWheel)
        {
            if (sourceWheel == null)
            {
                return null;
            }

            GameObject pivot = new(objectName);
            pivot.transform.SetParent(transform, false);

            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.name = "WheelMesh";
            cylinder.transform.SetParent(pivot.transform, false);
            cylinder.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            cylinder.transform.localScale = new Vector3(vehicleData.WheelRadius * 2f, runtimeWheelWidth * 0.5f, vehicleData.WheelRadius * 2f);

            Collider wheelCollider = cylinder.GetComponent<Collider>();
            if (wheelCollider != null)
            {
                Destroy(wheelCollider);
            }

            Renderer renderer = cylinder.GetComponent<Renderer>();
            if (renderer != null && runtimeWheelMaterial != null)
            {
                renderer.sharedMaterial = runtimeWheelMaterial;
            }

            sourceWheel.GetWorldPose(out Vector3 position, out Quaternion rotation);
            pivot.transform.SetPositionAndRotation(position, rotation);
            return pivot.transform;
        }

        private Material CreateRuntimeWheelMaterial()
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
                color = runtimeWheelColor
            };
        }

        private void ApplyAntiRoll(WheelCollider leftWheel, WheelCollider rightWheel)
        {
            if (leftWheel == null || rightWheel == null)
            {
                return;
            }

            float leftTravel = GetSuspensionTravel(leftWheel);
            float rightTravel = GetSuspensionTravel(rightWheel);
            float antiRoll = (leftTravel - rightTravel) * vehicleData.AntiRollForce;

            if (leftWheel.isGrounded)
            {
                busRigidbody.AddForceAtPosition(leftWheel.transform.up * -antiRoll, leftWheel.transform.position);
            }

            if (rightWheel.isGrounded)
            {
                busRigidbody.AddForceAtPosition(rightWheel.transform.up * antiRoll, rightWheel.transform.position);
            }
        }

        private float GetSuspensionTravel(WheelCollider wheel)
        {
            if (wheel.GetGroundHit(out WheelHit hit))
            {
                return (-wheel.transform.InverseTransformPoint(hit.point).y - wheel.radius) / wheel.suspensionDistance;
            }

            return 1f;
        }

        private void ApplyArcadeStability()
        {
            Vector3 velocity = busRigidbody.linearVelocity;
            busRigidbody.AddForce(-transform.up * velocity.magnitude * vehicleData.Downforce, ForceMode.Force);

            float rollAngle = Vector3.Angle(transform.up, Vector3.up);
            if (rollAngle < vehicleData.RolloverAssistStartAngle)
            {
                return;
            }

            Vector3 correctionAxis = Vector3.Cross(transform.up, Vector3.up);
            busRigidbody.AddTorque(correctionAxis * vehicleData.RolloverAssistStrength, ForceMode.Acceleration);
        }

        private void UpdateWheelVisuals()
        {
            UpdateWheelVisual(frontLeftWheel, frontLeftVisual);
            UpdateWheelVisual(frontRightWheel, frontRightVisual);
            UpdateWheelVisual(rearLeftWheel, rearLeftVisual);
            UpdateWheelVisual(rearRightWheel, rearRightVisual);
        }

        private static void UpdateWheelVisual(WheelCollider wheel, Transform visual)
        {
            if (wheel == null || visual == null)
            {
                return;
            }

            wheel.GetWorldPose(out Vector3 position, out Quaternion rotation);
            visual.SetPositionAndRotation(position, rotation);
        }
    }
}
