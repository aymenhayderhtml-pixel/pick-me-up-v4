using PickMeUp.Data;

namespace PickMeUp.Services
{
    public interface IGameStateService
    {
        GameState CurrentState { get; }
        void SetState(GameState state);
    }
}