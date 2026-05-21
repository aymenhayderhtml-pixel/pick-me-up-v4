// Assets/Scripts/Data/Enums.cs
namespace PickMeUp.Data
{
    public enum GameState
    {
        Boot,
        Hub,
        Combat,
        Tower,
        Saving
    }

    public enum ElementType
    {
        Fire,
        Water,
        Earth,
        Wind,
        Light,
        Dark
    }

    public enum ClassType
    {
        Vanguard,
        Sniper,
        Support,
        Mage,
        Tank
    }

    public enum SkillType
    {
        Active,
        Passive,
        Buff,
        Debuff,
        Damage
    }
}