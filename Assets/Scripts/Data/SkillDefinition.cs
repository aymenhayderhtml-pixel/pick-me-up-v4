using UnityEngine;
using PickMeUp.Data;

namespace PickMeUp.Data
{
    [CreateAssetMenu(fileName = "Skill_", menuName = "PickMeUp/Skill Definition")]
    public class SkillDefinition : ScriptableObject
    {
        public string SkillId;
        public string DisplayName;
        public SkillType Type;
        public int BasePower;
        public int Cooldown;
        public string Description;
    }
}