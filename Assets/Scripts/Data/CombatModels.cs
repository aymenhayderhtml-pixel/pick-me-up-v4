// Assets/Scripts/Data/CombatModels.cs
using System;
using System.Collections.Generic;

namespace PickMeUp.Data
{
    /// <summary>
    /// Types of events that can occur during combat simulation.
    /// </summary>
    public enum CombatEventType
    {
        Damage,
        Heal,
        Buff,
        Debuff,
        SkillUsed,
        Death,
        Victory,
        Defeat
    }

    /// <summary>
    /// Represents the runtime state of a skill during a combat encounter.
    /// </summary>
    [Serializable]
    public class CombatSkillState
    {
        public string SkillId;
        public SkillType Type;
        public int CooldownMax;
        public int CooldownCurrent;
        public int EnergyMax;
        public int EnergyCurrent;
        public float PowerMultiplier;
    }

    /// <summary>
    /// Represents the runtime state of a trait during a combat encounter.
    /// </summary>
    [Serializable]
    public class CombatTraitState
    {
        public string TraitId;
        public bool IsActive;
    }

    /// <summary>
    /// Represents a single unit (hero or enemy) participating in combat.
    /// Pure C# class, completely decoupled from Unity engine types.
    /// </summary>
    [Serializable]
    public class CombatUnit
    {
        public string Name;
        public string InstanceId;
        public int MaxHP;
        public int CurrentHP;
        public int ATK;
        public int DEF;
        public int SPD;
        public int CritRate; // Basis points (10000 = 100%)
        public int CritDmg;  // Basis points (15000 = 150%)
        public ElementType Element;
        public ClassType ClassType;
        public int Position; // 0 = Front Row, 1 = Back Row
        public bool IsHero;
        public List<CombatSkillState> Skills;
        public List<CombatTraitState> Traits;

        public bool IsAlive => CurrentHP > 0;
    }

    /// <summary>
    /// Records a single discrete event that occurred during the combat simulation.
    /// </summary>
    [Serializable]
    public class CombatEvent
    {
        public int TurnNumber;
        public string Description;
        public CombatEventType Type;
    }

    /// <summary>
    /// A complete snapshot of the combat state. Used for initializing, resuming, 
    /// and storing the results of a deterministic battle.
    /// </summary>
    [Serializable]
    public class CombatSnapshot
    {
        public List<CombatUnit> Heroes;
        public List<CombatUnit> Enemies;
        public int FloorLevel;
        public int Seed;
        public int TurnCount;
        public bool IsComplete;
        public bool IsVictory;
        public List<CombatEvent> EventLog;

        public CombatSnapshot()
        {
            Heroes = new List<CombatUnit>();
            Enemies = new List<CombatUnit>();
            EventLog = new List<CombatEvent>();
        }
    }
}