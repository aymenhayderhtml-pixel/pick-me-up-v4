// Assets/Scripts/Services/IGameStateService.cs
using System;
using PickMeUp.Data;

namespace PickMeUp.Services
{
    public interface IGameStateService
    {
        GameState CurrentState { get; }
        event Action<GameState> OnStateChanged;
        void ChangeState(GameState newState);
    }
}