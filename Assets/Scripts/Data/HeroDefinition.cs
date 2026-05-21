// Assets/Scripts/Data/HeroDefinition.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PickMeUp.Data
{
    /// <summary>
    /// Reference link to a skill, allowing for level overrides or specific configurations.
    /// </summary>
    [Serializable]
    public struct SkillReference
    {
        [SerializeField] private SkillDefinition _skill;
        [SerializeField] private int _unlockLevel;

        public SkillDefinition Skill => _skill;
        public int UnlockLevel => _unlockLevel;
    }

    /// <summary>
    /// Reference link to a trait.
    /// </summary>
    [Serializable]
    public struct TraitReference
    {
        [SerializeField] private TraitDefinition _trait;
        [SerializeField, Range(0f, 1f)] private float _rollWeight;

        public TraitDefinition Trait => _trait;
        public float RollWeight => _rollWeight;
    }

    /// <summary>
    /// Static data definition for a hero template. 
    /// Contains base stats, classifications, and available skill/trait pools.
    /// </summary>
    [CreateAssetMenu(fileName = "NewHeroDefinition", menuName = "PickMeUp/Data/Hero Definition")]
    public class HeroDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _heroId;
        [SerializeField] private string _heroName;
        [SerializeField] private Sprite _portrait;

        [Header("Classification")]
        [SerializeField] private ElementType _element;
        [SerializeField] private ClassType _classType;

        [Header("Base Stats")]
        [SerializeField] private int _baseHP;
        [SerializeField] private int _baseATK;
        [SerializeField] private int _baseDEF;
        [SerializeField] private int _baseSPD;
        [SerializeField] private int _baseCritRate;
        [SerializeField] private int _baseCritDmg;

        [Header("Skills & Traits")]
        [SerializeField] private List<SkillReference> _skills;
        [SerializeField] private List<TraitReference> _possibleTraits;

        public string HeroId => _heroId;
        public string HeroName => _heroName;
        public Sprite Portrait => _portrait;
        public ElementType Element => _element;
        public ClassType ClassType => _classType;
        public int BaseHP => _baseHP;
        public int BaseATK => _baseATK;
        public int BaseDEF => _baseDEF;
        public int BaseSPD => _baseSPD;
        public int BaseCritRate => _baseCritRate;
        public int BaseCritDmg => _baseCritDmg;
        public IReadOnlyList<SkillReference> Skills => _skills;
        public IReadOnlyList<TraitReference> PossibleTraits => _possibleTraits;
    }
}