using System.Collections.Generic;
using PickMeUp.Data;

namespace PickMeUp.Services
{
    public class CombatInput
    {
        public List<HeroInstance> PlayerParty { get; set; }
        public List<HeroInstance> EnemyParty { get; set; }
    }

    public class CombatResult
    {
        public bool PlayerWon { get; set; }
        public List<HeroInstance> SurvivorParty { get; set; }
    }

    public class IdleReward
    {
        public long Gold { get; set; }
        public long Experience { get; set; }
    }

    public class GachaPullResult
    {
        public HeroInstance PulledHero { get; set; }
        public int PullCount { get; set; }
        public bool IsPityBreak { get; set; }
    }
}