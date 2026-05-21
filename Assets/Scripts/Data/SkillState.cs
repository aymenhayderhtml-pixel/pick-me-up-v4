using PickMeUp.Data;

namespace PickMeUp.Data
{
    public class SkillState
    {
        public SkillDefinition Definition { get; set; }
        public int CurrentCooldown { get; set; }
    }
}