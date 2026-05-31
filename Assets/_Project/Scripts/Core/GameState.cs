namespace BondiSimulator.Core
{
    /// <summary>
    /// Represents the minimal game flow states required for the first playable foundation.
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// The player is choosing route, company or vehicle before gameplay starts.
        /// </summary>
        MainMenu,

        /// <summary>
        /// The race is about to start and vehicle movement should remain locked.
        /// </summary>
        Countdown,

        /// <summary>
        /// The race timer is active and gameplay systems may accept player control.
        /// </summary>
        Racing,

        /// <summary>
        /// The race has ended and result screens can consume the final time.
        /// </summary>
        Results
    }
}
