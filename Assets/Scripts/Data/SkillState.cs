// Assets/Scripts/Data/SkillState.cs
using System;

namespace PickMeUp.Data
{
    /// <summary>
    /// Represents the runtime state of a skill attached to a specific HeroInstance.
    /// Tracks cooldowns, energy, and activation status during combat.
    /// </summary>
    [Serializable]
    public class SkillState
    {
        public string SkillDefId;
        public int CurrentCooldown;
        public int EnergyAccumulated;
        public bool IsUnlocked;

        public SkillState() { }

        public SkillState(SkillDefinition definition)
        {
            SkillDefId = definition.SkillId;
            CurrentCooldown = 0;
            EnergyAccumulated = 0;
            IsUnlocked = true;
        }
    }
}