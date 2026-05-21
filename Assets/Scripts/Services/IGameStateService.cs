// Assets/Scripts/Services/IGameStateService.cs
using System;
using PickMeUp.Data;

namespace PickMeUp.Services
{
    /// <summary>
    /// Manages the high-level state machine of the application.
    /// </summary>
    public interface IGameStateService
    {
        /// <summary>
        /// Gets the current active state of the game.
        /// </summary>
        GameState CurrentState { get; }

        /// <summary>
        /// Event triggered when the game state changes.
        /// </summary>
        event Action<GameState> OnStateChanged;

        /// <summary>
        /// Requests a transition to a new game state.
        /// </summary>
        void ChangeState(GameState newState);
    }
}