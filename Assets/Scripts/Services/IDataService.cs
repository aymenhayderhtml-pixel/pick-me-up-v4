// Assets/Scripts/Services/IDataService.cs
using System.Collections.Generic;
using PickMeUp.Data;
using UnityEngine;

namespace PickMeUp.Services
{
    /// <summary>
    /// Manages the loading, caching, and retrieval of static game configurations and definitions.
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// Loads a specific ScriptableObject configuration asset.
        /// </summary>
        T LoadConfig<T>() where T : ScriptableObject;

        /// <summary>
        /// Retrieves a hero definition by its unique identifier.
        /// </summary>
        HeroDefinition GetHeroDefinition(string heroId);

        /// <summary>
        /// Retrieves all loaded hero definitions.
        /// </summary>
        IReadOnlyList<HeroDefinition> GetAllHeroDefinitions();

        /// <summary>
        /// Retrieves a skill definition by its unique identifier.
        /// </summary>
        SkillDefinition GetSkillDefinition(string skillId);

        /// <summary>
        /// Retrieves a trait definition by its unique identifier.
        /// </summary>
        TraitDefinition GetTraitDefinition(string traitId);
    }
}