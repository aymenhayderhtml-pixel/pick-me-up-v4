#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using PickMeUp.Data;
using System.IO;

namespace PickMeUp.Editor
{
    public class CreateSampleData
    {
        [MenuItem("Tools/PickMeUp/Create Sample Data")]
        public static void CreateSampleAssets()
        {
            var dataPath = "Assets/Resources/Data";
            Directory.CreateDirectory($"{dataPath}/Heroes");
            Directory.CreateDirectory($"{dataPath}/Skills");
            Directory.CreateDirectory($"{dataPath}/Traits");

            // Create sample skill
            var skill = ScriptableObject.CreateInstance<SkillDefinition>();
            skill.SkillId = "skill_fireball";
            skill.DisplayName = "Fireball";
            skill.Type = SkillType.Attack;
            skill.BasePower = 50;
            skill.Cooldown = 2;
            skill.Description = "Deal fire damage to all enemies.";
            AssetDatabase.CreateAsset(skill, $"{dataPath}/Skills/Skill_Fireball.asset");

            // Create sample trait
            var trait = ScriptableObject.CreateInstance<TraitDefinition>();
            trait.TraitId = "trait_flame_boost";
            trait.DisplayName = "Flame Boost";
            trait.Description = "Increase fire damage by 20%.";
            trait.StatBonus = 20;
            AssetDatabase.CreateAsset(trait, $"{dataPath}/Traits/Trait_FlameBoost.asset");

            // Create sample hero
            var hero = ScriptableObject.CreateInstance<HeroDefinition>();
            hero.HeroId = "hero_mage_001";
            hero.DisplayName = "Flame Mage";
            hero.BaseHealth = 100;
            hero.BaseAttack = 80;
            hero.BaseDefense = 40;
            hero.Element = ElementType.Fire;
            hero.Class = ClassType.Mage;
            hero.Skills = new[] { new SkillReference { Skill = skill } };
            hero.Traits = new[] { new TraitReference { Trait = trait } };
            hero.Description = "A mage who commands the power of flames.";
            AssetDatabase.CreateAsset(hero, $"{dataPath}/Heroes/Hero_FlameMage.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Sample data created successfully!");
        }
    }
}
#endif