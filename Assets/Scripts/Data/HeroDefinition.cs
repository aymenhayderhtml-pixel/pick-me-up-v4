// Assets/Scripts/Data/HeroDefinition.cs
using UnityEngine;
using PickMeUp.Data;

namespace PickMeUp.Data
{
    [CreateAssetMenu(fileName = "Hero_", menuName = "PickMeUp/Hero Definition")]
    public class HeroDefinition : ScriptableObject
    {
        [SerializeField] private string _heroId;
        [SerializeField] private string _heroName;
        [SerializeField] private Sprite _portrait;
        [SerializeField] private ElementType _element;
        [SerializeField] private ClassType _classType;
        
        // Base stats
        [SerializeField] private int _baseHP = 100;
        [SerializeField] private int _baseATK = 20;
        [SerializeField] private int _baseDEF = 15;
        [SerializeField] private int _baseSPD = 10;
        [SerializeField] private float _critRate = 0.05f;
        [SerializeField] private float _critDmg = 1.5f;
        
        [SerializeField] private SkillReference[] _skills;
        [SerializeField] private TraitReference[] _possibleTraits;

        // Read-only properties
        public string HeroId => _heroId;
        public string HeroName => _heroName;
        public Sprite Portrait => _portrait;
        public ElementType Element => _element;
        public ClassType ClassType => _classType;
        public int BaseHP => _baseHP;
        public int BaseATK => _baseATK;
        public int BaseDEF => _baseDEF;
        public int BaseSPD => _baseSPD;
        public float CritRate => _critRate;
        public float CritDmg => _critDmg;
        public SkillReference[] Skills => _skills;
        public TraitReference[] PossibleTraits => _possibleTraits;
    }

    [System.Serializable]
    public struct SkillReference
    {
        public SkillDefinition SkillDef;
    }

    [System.Serializable]
    public struct TraitReference
    {
        public TraitDefinition TraitDef;
    }
}