using System.Collections.Generic;
using PickMeUp.Data;

namespace PickMeUp.Data
{
    public class HeroInstance
    {
        public string HeroInstanceId { get; set; }
        public HeroDefinition Definition { get; set; }
        public int Level { get; set; }
        public long Experience { get; set; }
        public int CurrentHealth { get; set; }
        public List<SkillState> ActiveSkills { get; set; } = new();
        public List<TraitState> ActiveTraits { get; set; } = new();
        public int AscensionLevel { get; set; }
    }
}