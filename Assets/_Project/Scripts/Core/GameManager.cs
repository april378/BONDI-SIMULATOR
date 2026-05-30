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

        public GameState CurrentState { get; private set; }
        public float LastRaceTimeSeconds { get; private set; }
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

        public void ReturnToMainMenu()
        {
            SetState(GameState.MainMenu);
        }

        public void BeginCountdown()
        {
            if (countdownRoutine != null)
            {
                StopCoroutine(countdownRoutine);
            }

            SetState(GameState.Countdown);
            countdownRoutine = StartCoroutine(RunCountdown());
        }

        public void StartRace()
        {
            raceStartTime = Time.time;
            LastRaceTimeSeconds = 0f;
            SetState(GameState.Racing);
            GameEvents.RaiseRaceStarted();
        }

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
