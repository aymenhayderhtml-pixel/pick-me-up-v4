// Assets/Scripts/Data/SkillDefinition.cs
using System;
using UnityEngine;

namespace PickMeUp.Data
{
    /// <summary>
    /// Placeholder structure for skill effects. 
    /// Will be expanded into a polymorphic or data-driven effect system later.
    /// </summary>
    [Serializable]
    public struct SkillEffect
    {
        [SerializeField] private string _effectId;
        [SerializeField] private float _value;

        public string EffectId => _effectId;
        public float Value => _value;
    }

    /// <summary>
    /// Static data definition for a hero or enemy skill.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSkillDefinition", menuName = "PickMeUp/Data/Skill Definition")]
    public class SkillDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _skillId;
        [SerializeField] private string _skillName;
        [SerializeField, TextArea(3, 5)] private string _description;

        [Header("Mechanics")]
        [SerializeField] private SkillType _type;
        [SerializeField] private int _cooldownTurns;
        [SerializeField] private int _energyCost;

        [Header("Effects")]
        [SerializeField] private SkillEffect[] _effects;

        public string SkillId => _skillId;
        public string SkillName => _skillName;
        public string Description => _description;
        public SkillType Type => _type;
        public int CooldownTurns => _cooldownTurns;
        public int EnergyCost => _energyCost;
        public SkillEffect[] Effects => _effects;
    }
}