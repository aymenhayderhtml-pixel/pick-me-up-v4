// Assets/Scripts/Services/ICombatEngineService.cs
using System.Collections.Generic;
using PickMeUp.Data;

namespace PickMeUp.Services
{
    /// <summary>
    /// Provides deterministic combat resolution for both visual and headless (offline/idle) scenarios.
    /// </summary>
    public interface ICombatEngineService
    {
        /// <summary>
        /// Initializes and runs a full combat simulation from scratch.
        /// </summary>
        /// <param name="heroParty">The player's hero instances.</param>
        /// <param name="enemyParty">The enemy instances (can be generated or predefined).</param>
        /// <param name="floorLevel">The current tower floor level for scaling/context.</param>
        /// <param name="seed">The deterministic seed for RNG operations.</param>
        /// <returns>The final completed snapshot of the battle.</returns>
        CombatSnapshot SimulateCombat(List<HeroInstance> heroParty, List<HeroInstance> enemyParty, int floorLevel, int seed);

        /// <summary>
        /// Resumes and completes an existing, partially-run combat snapshot.
        /// Used for background/offline simulation completion.
        /// </summary>
        /// <param name="snapshot">The incomplete combat state.</param>
        /// <returns>The final completed snapshot of the battle.</returns>
        CombatSnapshot SimulateHeadless(CombatSnapshot snapshot);
    }
}