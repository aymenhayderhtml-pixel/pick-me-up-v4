using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PickMeUp.Data;

namespace PickMeUp.Services.Implementations
{
    public class DataService : MonoBehaviour, IDataService
    {
        private List<HeroDefinition> _heroDefinitions;
        private List<SkillDefinition> _skillDefinitions;
        private List<TraitDefinition> _traitDefinitions;

        private void Awake()
        {
            LoadDefinitions();
        }

        private void LoadDefinitions()
        {
            _heroDefinitions = Resources.LoadAll<HeroDefinition>("Data/Heroes").ToList();
            _skillDefinitions = Resources.LoadAll<SkillDefinition>("Data/Skills").ToList();
            _traitDefinitions = Resources.LoadAll<TraitDefinition>("Data/Traits").ToList();
        }

        public List<HeroDefinition> GetHeroDefinitions() => _heroDefinitions;
        public List<SkillDefinition> GetSkillDefinitions() => _skillDefinitions;
        public List<TraitDefinition> GetTraitDefinitions() => _traitDefinitions;
    }
}