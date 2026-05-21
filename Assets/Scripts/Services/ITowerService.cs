// Assets/Scripts/Services/ITowerService.cs
using System.Collections.Generic;
using PickMeUp.Data;

namespace PickMeUp.Services
{
    /// <summary>
    /// Manages the procedural generation and state of the infinite tower climbs.
    /// </summary>
    public interface ITowerService
    {
        /// <summary>
        /// Initializes a new tower run with the provided party.
        /// </summary>
        TowerRunState StartNewRun(List<HeroInstance> party, int startingFloor);

        /// <summary>
        /// Procedurally generates a floor layout based on the floor level and seed.
        /// </summary>
        TowerFloorData GenerateFloor(int floorLevel, int seed);

        /// <summary>
        /// Generates a single node with appropriate enemies or rewards.
        /// </summary>
        TowerNode GenerateNode(int floorLevel, TowerNodeType type, int seed);

        /// <summary>
        /// Resolves the outcome of a node (runs combat or applies rest/treasure effects).
        /// </summary>
        CombatSnapshot ResolveNode(TowerRunState runState, TowerNode node);

        /// <summary>
        /// Marks a node as cleared and applies rewards to the run state.
        /// </summary>
        void CompleteNode(TowerRunState runState, TowerNode node);

        /// <summary>
        /// Generates a list of enemy instances scaled to the current floor.
        /// </summary>
        List<HeroInstance> GenerateEnemies(int floorLevel, int count, int seed);

        /// <summary>
        /// Returns the currently active tower run, if any.
        /// </summary>
        TowerRunState GetActiveRun();

        /// <summary>
        /// Ends the current run and finalizes the state.
        /// </summary>
        void EndRun(TowerRunState runState);
    }
}