// Assets/Scripts/Services/IDataService.cs
using System.Collections.Generic;
using PickMeUp.Data;

namespace PickMeUp.Services
{
    public interface IDataService
    {
        void LoadAllDefinitions();
        object LoadConfig(string configName);
        HeroDefinition GetHeroDefinition(string heroId);
        List<HeroDefinition> GetAllHeroDefinitions();
        SkillDefinition GetSkillDefinition(string skillId);
        TraitDefinition GetTraitDefinition(string traitId);
    }
}