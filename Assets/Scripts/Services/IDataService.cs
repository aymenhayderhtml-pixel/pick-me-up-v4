using System.Collections.Generic;
using PickMeUp.Data;

namespace PickMeUp.Services
{
    public interface IDataService
    {
        List<HeroDefinition> GetHeroDefinitions();
        List<SkillDefinition> GetSkillDefinitions();
        List<TraitDefinition> GetTraitDefinitions();
    }
}