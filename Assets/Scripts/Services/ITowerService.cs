using System.Collections.Generic;
using PickMeUp.Data;

namespace PickMeUp.Services
{
    public interface ITowerService
    {
        TowerRunState StartNewRun(List<HeroInstance> party, int startingFloor);
        TowerFloorData GenerateFloor(int floorLevel, int seed);
        TowerNode GenerateNode(int floorLevel, TowerNodeType type, int seed);
        CombatSnapshot ResolveNode(TowerRunState runState, TowerNode node);
        void CompleteNode(TowerRunState runState, TowerNode node);
        List<HeroInstance> GenerateEnemies(int floorLevel, int count, int seed);
        TowerRunState GetActiveRun();
        void EndRun(TowerRunState runState);
    }
}