using UnityEngine;

namespace PickMeUp.Data
{
    [CreateAssetMenu(fileName = "Trait_", menuName = "PickMeUp/Trait Definition")]
    public class TraitDefinition : ScriptableObject
    {
        public string TraitId;
        public string DisplayName;
        public string Description;
        public int StatBonus;
    }
}