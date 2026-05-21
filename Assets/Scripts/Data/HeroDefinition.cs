using UnityEngine;
using PickMeUp.Data;

namespace PickMeUp.Data
{
    [CreateAssetMenu(fileName = "Hero_", menuName = "PickMeUp/Hero Definition")]
    public class HeroDefinition : ScriptableObject
    {
        public string HeroId;
        public string DisplayName;
        public int BaseHealth;
        public int BaseAttack;
        public int BaseDefense;
        public ElementType Element;
        public ClassType Class;
        public SkillReference[] Skills;
        public TraitReference[] Traits;
        public string Description;
    }

    [System.Serializable]
    public class SkillReference
    {
        public SkillDefinition Skill;
    }

    [System.Serializable]
    public class TraitReference
    {
        public TraitDefinition Trait;
    }
}