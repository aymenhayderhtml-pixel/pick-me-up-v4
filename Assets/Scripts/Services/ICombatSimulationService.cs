// Assets/Scripts/Services/ICombatSimulationService.cs
namespace PickMeUp.Services
{
    public interface ICombatSimulationService
    {
        CombatResult Simulate(CombatInput input);
        CombatResult RunHeadless(CombatInput input);
    }
}