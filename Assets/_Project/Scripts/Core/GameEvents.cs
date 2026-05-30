using System;

namespace BondiSimulator.Core
{
    /// <summary>
    /// Central event hub for gameplay notifications that have already happened.
    /// </summary>
    public static class GameEvents
    {
        public static event Action<GameState, GameState> GameStateChanged;
        public static event Action<int> CountdownStarted;
        public static event Action<int> CountdownTicked;
        public static event Action CountdownFinished;
        public static event Action RaceStarted;
        public static event Action<float> RaceFinished;

        public static void RaiseGameStateChanged(GameState previousState, GameState currentState)
        {
            GameStateChanged?.Invoke(previousState, currentState);
        }

        public static void RaiseCountdownStarted(int seconds)
        {
            CountdownStarted?.Invoke(seconds);
        }

        public static void RaiseCountdownTicked(int remainingSeconds)
        {
            CountdownTicked?.Invoke(remainingSeconds);
        }

        public static void RaiseCountdownFinished()
        {
            CountdownFinished?.Invoke();
        }

        public static void RaiseRaceStarted()
        {
            RaceStarted?.Invoke();
        }

        public static void RaiseRaceFinished(float elapsedSeconds)
        {
            RaceFinished?.Invoke(elapsedSeconds);
        }
    }
}
