// Assets/Scripts/Data/TraitDefinition.cs
using System;
using UnityEngine;

namespace PickMeUp.Data
{
    /// <summary>
    /// Placeholder structure for trait effects.
    /// </summary>
    [Serializable]
    public struct TraitEffect
    {
        [SerializeField] private string _effectId;
        [SerializeField] private float _value;

        public string EffectId => _effectId;
        public float Value => _value;
    }

    /// <summary>
    /// Static data definition for a passive hero trait.
    /// </summary>
    [CreateAssetMenu(fileName = "NewTraitDefinition", menuName = "PickMeUp/Data/Trait Definition")]
    public class TraitDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _traitId;
        [SerializeField] private string _traitName;
        [SerializeField, TextArea(3, 5)] private string _description;

        [Header("Effects")]
        [SerializeField] private TraitEffect[] _effects;

        public string TraitId => _traitId;
        public string TraitName => _traitName;
        public string Description => _description;
        public TraitEffect[] Effects => _effects;
    }
}