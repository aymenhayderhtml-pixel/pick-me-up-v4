using PickMeUp.Data;

namespace PickMeUp.Services.Implementations
{
    public class GameStateService : IGameStateService
    {
        public GameState CurrentState { get; private set; } = GameState.Boot;

        public void SetState(GameState state)
        {
            CurrentState = state;
        }
    }
}