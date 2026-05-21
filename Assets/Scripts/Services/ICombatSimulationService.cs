// Assets/Scripts/Services/ICombatSimulationService.cs
namespace PickMeUp.Services
{
    /// <summary>
    /// Provides deterministic combat resolution for both visual and headless (offline/idle) scenarios.
    /// </summary>
    public interface ICombatSimulationService
    {
        /// <summary>
        /// Runs a full combat simulation and returns the final result.
        /// Used primarily for headless/offline calculations.
        /// </summary>
        CombatResult Simulate(CombatInput input, int seed);

        /// <summary>
        /// Runs a combat simulation and generates a step-by-step event log.
        /// Used for visual playback and replays in the UI.
        /// </summary>
        CombatEventLog RunHeadless(CombatInput input, int seed);
    }
}