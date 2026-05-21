// Assets/Scripts/Services/Implementations/DataService.cs
using System.Collections.Generic;
using UnityEngine;
using PickMeUp.Data;
using PickMeUp.Services;

namespace PickMeUp.Services.Implementations
{
    /// <summary>
    /// MonoBehaviour implementation of IDataService that loads ScriptableObject definitions
    /// from Resources folders at runtime.
    /// </summary>
    public class DataService : MonoBehaviour, IDataService
    {
        private readonly Dictionary<string, HeroDefinition> _heroDefinitions = new Dictionary<string, HeroDefinition>();
        private readonly Dictionary<string, SkillDefinition> _skillDefinitions = new Dictionary<string, SkillDefinition>();
        private readonly Dictionary<string, TraitDefinition> _traitDefinitions = new Dictionary<string, TraitDefinition>();
        private bool _isLoaded = false;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            LoadAllDefinitions();
        }

        public void LoadAllDefinitions()
        {
            if (_isLoaded)
            {
                return;
            }

            LoadHeroDefinitions();
            LoadSkillDefinitions();
            LoadTraitDefinitions();
            _isLoaded = true;

            Debug.Log($"[DataService] Loaded {_heroDefinitions.Count} heroes, {_skillDefinitions.Count} skills, {_traitDefinitions.Count} traits");
        }

        public T LoadConfig<T>() where T : ScriptableObject
        {
            return null;
        }

        public HeroDefinition GetHeroDefinition(string heroId)
        {
            if (string.IsNullOrEmpty(heroId))
            {
                Debug.LogWarning("[DataService] GetHeroDefinition called with null/empty ID");
                return null;
            }

            if (_heroDefinitions.TryGetValue(heroId, out HeroDefinition definition))
            {
                return definition;
            }

            Debug.LogWarning($"[DataService] HeroDefinition not found for ID: {heroId}");
            return null;
        }

        public IReadOnlyList<HeroDefinition> GetAllHeroDefinitions()
        {
            return new List<HeroDefinition>(_heroDefinitions.Values);
        }

        public SkillDefinition GetSkillDefinition(string skillId)
        {
            if (string.IsNullOrEmpty(skillId))
            {
                Debug.LogWarning("[DataService] GetSkillDefinition called with null/empty ID");
                return null;
            }

            if (_skillDefinitions.TryGetValue(skillId, out SkillDefinition definition))
            {
                return definition;
            }

            Debug.LogWarning($"[DataService] SkillDefinition not found for ID: {skillId}");
            return null;
        }

        public TraitDefinition GetTraitDefinition(string traitId)
        {
            if (string.IsNullOrEmpty(traitId))
            {
                Debug.LogWarning("[DataService] GetTraitDefinition called with null/empty ID");
                return null;
            }

            if (_traitDefinitions.TryGetValue(traitId, out TraitDefinition definition))
            {
                return definition;
            }

            Debug.LogWarning($"[DataService] TraitDefinition not found for ID: {traitId}");
            return null;
        }

        private void LoadHeroDefinitions()
        {
            HeroDefinition[] definitions = Resources.LoadAll<HeroDefinition>("Heroes");
            foreach (HeroDefinition def in definitions)
            {
                if (!string.IsNullOrEmpty(def.HeroId) && !_heroDefinitions.ContainsKey(def.HeroId))
                {
                    _heroDefinitions[def.HeroId] = def;
                }
            }
        }

        private void LoadSkillDefinitions()
        {
            SkillDefinition[] definitions = Resources.LoadAll<SkillDefinition>("Skills");
            foreach (SkillDefinition def in definitions)
            {
                if (!string.IsNullOrEmpty(def.SkillId) && !_skillDefinitions.ContainsKey(def.SkillId))
                {
                    _skillDefinitions[def.SkillId] = def;
                }
            }
        }

        private void LoadTraitDefinitions()
        {
            TraitDefinition[] definitions = Resources.LoadAll<TraitDefinition>("Traits");
            foreach (TraitDefinition def in definitions)
            {
                if (!string.IsNullOrEmpty(def.TraitId) && !_traitDefinitions.ContainsKey(def.TraitId))
                {
                    _traitDefinitions[def.TraitId] = def;
                }
            }
        }
    }
}