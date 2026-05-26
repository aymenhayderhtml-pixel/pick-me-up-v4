// Assets/Scripts/Data/HeroDefinition.cs
using UnityEngine;

namespace PickMeUp.Data
{
    // Kept the new Manhwa trait enum
    public enum PersonalityTrait { Cowardly, Brave, Disciplined, Reckless, Loyal, Traumatized }

    [CreateAssetMenu(fileName = "NewHero", menuName = "PickMeUp/Hero Definition")]
    public class HeroDefinition : ScriptableObject
    {
        public string HeroId;
        public string HeroName;
        public int BaseStar;
        
        [Header("Art Assets")]
        public Sprite Portrait;
        public Sprite Crest;

        [Header("Classification")]
        // FIX: Use the exact enum names your CombatEngine expects (ElementType & ClassType)
        public ElementType Element; 
        public ClassType ClassType;

        [Header("Base Stats")]
        public int BaseHP;
        public int BaseATK;
        public int BaseDEF;
        public int BaseSPD;
        public int BaseCritRate;
        public int BaseCritDmg;

        [Header("Progression")]
        // Max level allowed for each star rating (1★ to 5★)
        public int[] MaxLevelPerStar = new int[] { 10, 20, 30, 40, 50 };

        [Header("Manhwa Traits & Lore")]
        [Range(0f, 1f)] public float HiddenPotential = 0.5f;
        public PersonalityTrait Personality = PersonalityTrait.Disciplined;
        [Tooltip("Leave blank for most heroes")] public string SpecialOrgan;
        
        [Header("Runtime Defaults")]
        public bool IsAlive = true;
        [Range(0f, 100f)] public float DefaultMorale = 100f;
    }
}