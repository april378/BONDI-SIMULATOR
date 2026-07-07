using UnityEngine;

namespace BondiSimulator.Vehicle
{
    /// <summary>
    /// Tunable physics and handling values for a bus model.
    /// </summary>
    [CreateAssetMenu(fileName = "VehicleData", menuName = "Bondi Simulator/Vehicle Data")]
    public sealed class VehicleData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string displayName = "Urban Bus";

        [Header("Mass")]
        [SerializeField, Min(1f)] private float massKg = 14000f;
        [SerializeField] private Vector3 centerOfMass = new(0f, -0.85f, 0.15f);

        [Header("Powertrain")]
        [SerializeField, Min(1f)] private float maxSpeedKph = 78f;
        [SerializeField, Min(0f)] private float motorTorque = 5200f;
        [SerializeField, Min(0f)] private float brakeTorque = 14500f;
        [SerializeField, Min(0f)] private float handbrakeTorque = 22000f;

        [Header("Steering")]
        [SerializeField, Min(1f)] private float maxSteerAngle = 32f;
        [SerializeField, Range(0.1f, 1f)] private float highSpeedSteerFactor = 0.38f;

        [Header("Wheels")]
        [SerializeField, Min(0.05f)] private float wheelRadius = 0.48f;
        [SerializeField, Min(0.01f)] private float suspensionDistance = 0.55f;
        [SerializeField, Min(1f)] private float suspensionSpring = 420000f;
        [SerializeField, Min(0f)] private float suspensionDamper = 52000f;
        [SerializeField, Range(0f, 1f)] private float suspensionTargetPosition = 0.55f;
        [SerializeField, Min(1f)] private float wheelMass = 120f;

        [Header("Arcade Assistance")]
        [SerializeField, Min(0f)] private float antiRollForce = 85000f;
        [SerializeField, Min(0f)] private float downforce = 45f;
        [SerializeField, Range(1f, 80f)] private float rolloverAssistStartAngle = 10f;
        [SerializeField, Min(0f)] private float rolloverAssistStrength = 12f;

        public string DisplayName => displayName;
        public float MassKg => massKg;
        public Vector3 CenterOfMass => centerOfMass;
        public float MaxSpeedKph => maxSpeedKph;
        public float MotorTorque => motorTorque;
        public float BrakeTorque => brakeTorque;
        public float HandbrakeTorque => handbrakeTorque;
        public float MaxSteerAngle => maxSteerAngle;
        public float HighSpeedSteerFactor => highSpeedSteerFactor;
        public float WheelRadius => wheelRadius;
        public float SuspensionDistance => suspensionDistance;
        public float SuspensionSpring => suspensionSpring;
        public float SuspensionDamper => suspensionDamper;
        public float SuspensionTargetPosition => suspensionTargetPosition;
        public float WheelMass => wheelMass;
        public float AntiRollForce => antiRollForce;
        public float Downforce => downforce;
        public float RolloverAssistStartAngle => rolloverAssistStartAngle;
        public float RolloverAssistStrength => rolloverAssistStrength;
    }
}
