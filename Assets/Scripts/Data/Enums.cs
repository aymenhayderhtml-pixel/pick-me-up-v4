// Assets/Scripts/Data/Enums.cs
namespace PickMeUp.Data
{
    /// <summary>
    /// Represents the current high-level state of the game application.
    /// </summary>
    public enum GameState
    {
        Boot,
        Hub,
        Combat,
        Saving
    }

    /// <summary>
    /// Elemental affinities for heroes and enemies. 
    /// Follows a cyclical advantage/disadvantage system.
    /// </summary>
    public enum ElementType
    {
        None,
        Fire,
        Water,
        Wood,
        Light,
        Dark
    }

    /// <summary>
    /// Combat classes that dictate base stat biases and skill archetypes.
    /// </summary>
    public enum ClassType
    {
        Vanguard,
        Striker,
        Caster,
        Tactician,
        Ranger
    }

    /// <summary>
    /// Defines how a skill is triggered and resolved in combat.
    /// </summary>
    public enum SkillType
    {
        Active,
        Passive
    }
}
