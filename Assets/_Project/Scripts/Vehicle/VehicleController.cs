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

        [Header("Grounding")]
        [SerializeField] private bool alignToGroundOnStart = true;
        [SerializeField, Min(0f)] private float groundProbeHeight = 5f;
        [SerializeField, Min(0.1f)] private float groundProbeDistance = 25f;
        [SerializeField, Min(0f)] private float rootGroundClearance = 0.02f;

        private Rigidbody busRigidbody;
        private PlayerInputReader inputReader;
        private GameManager gameManager;

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
            float speedLimitFactor = Mathf.Clamp01(1f - Mathf.InverseLerp(vehicleData.MaxSpeedKph * 0.92f, vehicleData.MaxSpeedKph, CurrentSpeedKph));
            float motorTorque = acceleration * vehicleData.MotorTorque * speedLimitFactor;
            float brakeTorque = brake * vehicleData.BrakeTorque;
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
