// Assets/Scripts/Data/Enums.cs
namespace PickMeUp.Data
{
    public enum GameState
    {
        Boot,
        Hub,
        Combat,
        Saving
    }

    public enum ElementType
    {
        None,
        Fire,
        Water,
        Wood,
        Light,
        Dark
    }

    public enum ClassType
    {
        Vanguard,
        Striker,
        Caster,
        Tactician,
        Ranger
    }

    public enum SkillType
    {
        Active,
        Passive
    }
}
