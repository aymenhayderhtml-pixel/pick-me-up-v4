namespace PickMeUp.Services
{
    public interface ICombatSimulationService
    {
        CombatResult SimulateCombat(CombatInput input);
    }
}