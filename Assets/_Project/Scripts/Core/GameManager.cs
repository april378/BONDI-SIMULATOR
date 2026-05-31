using System.Collections;
using UnityEngine;

namespace BondiSimulator.Core
{
    /// <summary>
    /// Owns the minimal finite state machine that drives menu, countdown, race and results flow.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private GameState initialState = GameState.MainMenu;
        [SerializeField, Min(1)] private int countdownSeconds = 3;

        private Coroutine countdownRoutine;
        private float raceStartTime;

        /// <summary>
        /// Current state of the global game flow.
        /// </summary>
        public GameState CurrentState { get; private set; }

        /// <summary>
        /// Last completed race time in seconds, or zero before any race has ended.
        /// </summary>
        public float LastRaceTimeSeconds { get; private set; }

        /// <summary>
        /// True while gameplay systems should run race logic.
        /// </summary>
        public bool IsRacing => CurrentState == GameState.Racing;

        private void Awake()
        {
            ServiceLocator.Register(this);
            CurrentState = initialState;
        }

        private void Start()
        {
            GameEvents.RaiseGameStateChanged(CurrentState, CurrentState);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister(this);
        }

        /// <summary>
        /// Returns the flow to the menu state.
        /// </summary>
        public void ReturnToMainMenu()
        {
            SetState(GameState.MainMenu);
        }

        /// <summary>
        /// Starts the countdown and automatically enters racing when it finishes.
        /// </summary>
        public void BeginCountdown()
        {
            if (countdownRoutine != null)
            {
                StopCoroutine(countdownRoutine);
            }

            SetState(GameState.Countdown);
            countdownRoutine = StartCoroutine(RunCountdown());
        }

        /// <summary>
        /// Enters racing immediately and resets race timing.
        /// </summary>
        public void StartRace()
        {
            raceStartTime = Time.time;
            LastRaceTimeSeconds = 0f;
            SetState(GameState.Racing);
            GameEvents.RaiseRaceStarted();
        }

        /// <summary>
        /// Completes the active race and records elapsed time from the race start.
        /// </summary>
        public void CompleteRace()
        {
            if (CurrentState != GameState.Racing)
            {
                return;
            }

            LastRaceTimeSeconds = Time.time - raceStartTime;
            SetState(GameState.Results);
            GameEvents.RaiseRaceFinished(LastRaceTimeSeconds);
        }

        /// <summary>
        /// Shows results for a known elapsed time, useful for tests or replay-driven flows.
        /// </summary>
        public void ShowResults(float elapsedSeconds)
        {
            LastRaceTimeSeconds = Mathf.Max(0f, elapsedSeconds);
            SetState(GameState.Results);
            GameEvents.RaiseRaceFinished(LastRaceTimeSeconds);
        }

        private IEnumerator RunCountdown()
        {
            GameEvents.RaiseCountdownStarted(countdownSeconds);

            for (int remaining = countdownSeconds; remaining > 0; remaining--)
            {
                GameEvents.RaiseCountdownTicked(remaining);
                yield return new WaitForSeconds(1f);
            }

            countdownRoutine = null;
            GameEvents.RaiseCountdownFinished();
            StartRace();
        }

        private void SetState(GameState nextState)
        {
            if (CurrentState == nextState)
            {
                return;
            }

            GameState previousState = CurrentState;
            CurrentState = nextState;
            GameEvents.RaiseGameStateChanged(previousState, CurrentState);
        }
    }
}
