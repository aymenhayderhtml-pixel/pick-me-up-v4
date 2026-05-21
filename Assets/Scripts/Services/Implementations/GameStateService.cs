// Assets/Scripts/Services/Implementations/GameStateService.cs
using System;
using UnityEngine;
using PickMeUp.Data;
using PickMeUp.Services;

namespace PickMeUp.Services.Implementations
{
    /// <summary>
    /// Concrete implementation of IGameStateService managing the application state machine.
    /// </summary>
    public class GameStateService : IGameStateService
    {
        private GameState _currentState = GameState.Boot;

        public GameState CurrentState => _currentState;

        public event Action<GameState> OnStateChanged;

        public void ChangeState(GameState newState)
        {
            if (_currentState == newState)
            {
                Debug.LogWarning($"[GameStateService] Already in state: {newState}");
                return;
            }

            Debug.Log($"[GameStateService] Changing state: {_currentState} -> {newState}");
            _currentState = newState;
            OnStateChanged?.Invoke(newState);
        }
    }
}