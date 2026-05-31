using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BondiSimulator.Core
{
    /// <summary>
    /// Reads the project input actions and exposes gameplay-friendly values and button events.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string playerActionMap = "Player";

        private InputActionMap actionMap;
        private InputAction steeringAction;
        private InputAction accelerationAction;
        private InputAction brakeAction;
        private InputAction handbrakeAction;
        private InputAction hornAction;
        private InputAction airHornAction;
        private InputAction lightsAction;
        private InputAction changeCameraAction;
        private InputAction pauseAction;

        /// <summary>
        /// Raised when the normal horn input is pressed.
        /// </summary>
        public event Action HornPressed;

        /// <summary>
        /// Raised when the air horn input is pressed.
        /// </summary>
        public event Action AirHornPressed;

        /// <summary>
        /// Raised when the lights input is pressed.
        /// </summary>
        public event Action LightsPressed;

        /// <summary>
        /// Raised when the camera toggle input is pressed.
        /// </summary>
        public event Action ChangeCameraPressed;

        /// <summary>
        /// Raised when the pause input is pressed.
        /// </summary>
        public event Action PausePressed;

        /// <summary>
        /// Current steering axis, where negative steers left and positive steers right.
        /// </summary>
        public float Steering { get; private set; }

        /// <summary>
        /// Current acceleration axis.
        /// </summary>
        public float Acceleration { get; private set; }

        /// <summary>
        /// Current brake axis.
        /// </summary>
        public float Brake { get; private set; }

        /// <summary>
        /// True while the handbrake button is held.
        /// </summary>
        public bool HandbrakeHeld { get; private set; }

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void OnEnable()
        {
            if (!ResolveActions())
            {
                return;
            }

            Subscribe();
            actionMap.Enable();
            ReadAxisValues();
        }

        private void OnDisable()
        {
            Unsubscribe();
            actionMap?.Disable();
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister(this);
        }

        private bool ResolveActions()
        {
            if (inputActions == null)
            {
                Debug.LogWarning($"{nameof(PlayerInputReader)} has no InputActionAsset assigned.", this);
                return false;
            }

            actionMap = inputActions.FindActionMap(playerActionMap, false);
            if (actionMap == null)
            {
                Debug.LogError($"Input action map '{playerActionMap}' was not found.", this);
                return false;
            }

            steeringAction = FindRequiredAction("Steering");
            accelerationAction = FindRequiredAction("Acceleration");
            brakeAction = FindRequiredAction("Brake");
            handbrakeAction = FindRequiredAction("Handbrake");
            hornAction = FindRequiredAction("Horn");
            airHornAction = FindRequiredAction("AirHorn");
            lightsAction = FindRequiredAction("Lights");
            changeCameraAction = FindRequiredAction("ChangeCamera");
            pauseAction = FindRequiredAction("Pause");

            return steeringAction != null
                && accelerationAction != null
                && brakeAction != null
                && handbrakeAction != null
                && hornAction != null
                && airHornAction != null
                && lightsAction != null
                && changeCameraAction != null
                && pauseAction != null;
        }

        private InputAction FindRequiredAction(string actionName)
        {
            InputAction action = actionMap.FindAction(actionName, false);
            if (action == null)
            {
                Debug.LogError($"Input action '{actionName}' was not found in '{playerActionMap}'.", this);
            }

            return action;
        }

        private void Subscribe()
        {
            steeringAction.performed += OnAxisChanged;
            steeringAction.canceled += OnAxisChanged;
            accelerationAction.performed += OnAxisChanged;
            accelerationAction.canceled += OnAxisChanged;
            brakeAction.performed += OnAxisChanged;
            brakeAction.canceled += OnAxisChanged;
            handbrakeAction.performed += OnHandbrakeChanged;
            handbrakeAction.canceled += OnHandbrakeChanged;
            hornAction.performed += OnHornPressed;
            airHornAction.performed += OnAirHornPressed;
            lightsAction.performed += OnLightsPressed;
            changeCameraAction.performed += OnChangeCameraPressed;
            pauseAction.performed += OnPausePressed;
        }

        private void Unsubscribe()
        {
            if (steeringAction != null)
            {
                steeringAction.performed -= OnAxisChanged;
                steeringAction.canceled -= OnAxisChanged;
            }

            if (accelerationAction != null)
            {
                accelerationAction.performed -= OnAxisChanged;
                accelerationAction.canceled -= OnAxisChanged;
            }

            if (brakeAction != null)
            {
                brakeAction.performed -= OnAxisChanged;
                brakeAction.canceled -= OnAxisChanged;
            }

            if (handbrakeAction != null)
            {
                handbrakeAction.performed -= OnHandbrakeChanged;
                handbrakeAction.canceled -= OnHandbrakeChanged;
            }

            if (hornAction != null)
            {
                hornAction.performed -= OnHornPressed;
            }

            if (airHornAction != null)
            {
                airHornAction.performed -= OnAirHornPressed;
            }

            if (lightsAction != null)
            {
                lightsAction.performed -= OnLightsPressed;
            }

            if (changeCameraAction != null)
            {
                changeCameraAction.performed -= OnChangeCameraPressed;
            }

            if (pauseAction != null)
            {
                pauseAction.performed -= OnPausePressed;
            }
        }

        private void OnAxisChanged(InputAction.CallbackContext context)
        {
            ReadAxisValues();
        }

        private void ReadAxisValues()
        {
            Steering = steeringAction.ReadValue<float>();
            Acceleration = accelerationAction.ReadValue<float>();
            Brake = brakeAction.ReadValue<float>();
        }

        private void OnHandbrakeChanged(InputAction.CallbackContext context)
        {
            HandbrakeHeld = context.ReadValueAsButton();
        }

        private void OnHornPressed(InputAction.CallbackContext context)
        {
            HornPressed?.Invoke();
        }

        private void OnAirHornPressed(InputAction.CallbackContext context)
        {
            AirHornPressed?.Invoke();
        }

        private void OnLightsPressed(InputAction.CallbackContext context)
        {
            LightsPressed?.Invoke();
        }

        private void OnChangeCameraPressed(InputAction.CallbackContext context)
        {
            ChangeCameraPressed?.Invoke();
        }

        private void OnPausePressed(InputAction.CallbackContext context)
        {
            PausePressed?.Invoke();
        }
    }
}
