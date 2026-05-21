// Assets/Scripts/Editor/CreateSampleData.cs
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using PickMeUp.Data;

namespace PickMeUp.EditorTools
{
    /// <summary>
    /// Editor utility to generate sample ScriptableObject assets for MVP testing.
    /// Access via: Tools > PickMeUp > Create Sample Data
    /// </summary>
    public static class CreateSampleData
    {
        private const string MENU_ROOT = "Tools/PickMeUp";
        private const string MENU_ITEM = MENU_ROOT + "/Create Sample Data";

        private const string HEROES_FOLDER = "Assets/Resources/Heroes";
        private const string SKILLS_FOLDER = "Assets/Resources/Skills";
        private const string TRAITS_FOLDER = "Assets/Resources/Traits";

        [MenuItem(MENU_ITEM)]
        public static void CreateSampleAssets()
        {
            Debug.Log("[CreateSampleData] Starting sample data generation...");

            CreateFolderIfNotExists(HEROES_FOLDER);
            CreateFolderIfNotExists(SKILLS_FOLDER);
            CreateFolderIfNotExists(TRAITS_FOLDER);

            SkillDefinition slashSkill = CreateOrGetSkillDefinition();
            TraitDefinition braveTrait = CreateOrGetTraitDefinition();
            HeroDefinition championHero = CreateOrGetHeroDefinition(slashSkill);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[CreateSampleData] Sample data creation complete. Created/verified: {championHero.HeroName}, {slashSkill.SkillName}, {braveTrait.TraitName}");
        }

        private static void CreateFolderIfNotExists(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                Debug.Log($"[CreateSampleData] Created folder: {folderPath}");
            }
        }

        private static SkillDefinition CreateOrGetSkillDefinition()
        {
            const string assetPath = SKILLS_FOLDER + "/Slash.asset";
            const string skillId = "skill_slash";

            SkillDefinition existing = AssetDatabase.LoadAssetAtPath<SkillDefinition>(assetPath);
            if (existing != null && existing.SkillId == skillId)
            {
                Debug.Log($"[CreateSampleData] SkillDefinition already exists: {skillId}");
                return existing;
            }

            SkillDefinition skill = ScriptableObject.CreateInstance<SkillDefinition>();
            skill.name = "Slash";
            SetSkillDefinitionFields(skill, skillId, "Slash", SkillType.Active, 3, 10, "A basic melee attack that deals physical damage.");

            AssetDatabase.CreateAsset(skill, assetPath);
            Debug.Log($"[CreateSampleData] Created SkillDefinition: {skillId}");
            return skill;
        }

        private static TraitDefinition CreateOrGetTraitDefinition()
        {
            const string assetPath = TRAITS_FOLDER + "/Brave.asset";
            const string traitId = "trait_brave";

            TraitDefinition existing = AssetDatabase.LoadAssetAtPath<TraitDefinition>(assetPath);
            if (existing != null && existing.TraitId == traitId)
            {
                Debug.Log($"[CreateSampleData] TraitDefinition already exists: {traitId}");
                return existing;
            }

            TraitDefinition trait = ScriptableObject.CreateInstance<TraitDefinition>();
            trait.name = "Brave";
            SetTraitDefinitionFields(trait, traitId, "Brave", "Increases morale recovery rate by 10%.");

            AssetDatabase.CreateAsset(trait, assetPath);
            Debug.Log($"[CreateSampleData] Created TraitDefinition: {traitId}");
            return trait;
        }

        private static HeroDefinition CreateOrGetHeroDefinition(SkillDefinition linkedSkill)
        {
            const string assetPath = HEROES_FOLDER + "/Champion.asset";
            const string heroId = "hero_champion";

            HeroDefinition existing = AssetDatabase.LoadAssetAtPath<HeroDefinition>(assetPath);
            if (existing != null && existing.HeroId == heroId)
            {
                Debug.Log($"[CreateSampleData] HeroDefinition already exists: {heroId}");
                return existing;
            }

            HeroDefinition hero = ScriptableObject.CreateInstance<HeroDefinition>();
            hero.name = "Champion";
            SetHeroDefinitionFields(hero, heroId, "Champion", ElementType.Fire, ClassType.Vanguard,
                100, 20, 15, 10, 500, 15000);
            SetHeroSkills(hero, linkedSkill, 1);

            AssetDatabase.CreateAsset(hero, assetPath);
            Debug.Log($"[CreateSampleData] Created HeroDefinition: {heroId}");
            return hero;
        }

        private static void SetSkillDefinitionFields(SkillDefinition skill, string id, string name,
            SkillType type, int cooldown, int energy, string description)
        {
            SerializedObject so = new SerializedObject(skill);
            so.FindProperty("_skillId").stringValue = id;
            so.FindProperty("_skillName").stringValue = name;
            so.FindProperty("_type").enumValueIndex = (int)type;
            so.FindProperty("_cooldownTurns").intValue = cooldown;
            so.FindProperty("_energyCost").intValue = energy;
            so.FindProperty("_description").stringValue = description;
            so.ApplyModifiedProperties();
        }

        private static void SetTraitDefinitionFields(TraitDefinition trait, string id, string name, string description)
        {
            SerializedObject so = new SerializedObject(trait);
            so.FindProperty("_traitId").stringValue = id;
            so.FindProperty("_traitName").stringValue = name;
            so.FindProperty("_description").stringValue = description;
            so.ApplyModifiedProperties();
        }

        private static void SetHeroDefinitionFields(HeroDefinition hero, string id, string name,
            ElementType element, ClassType classType, int hp, int atk, int def, int spd, int critRate, int critDmg)
        {
            SerializedObject so = new SerializedObject(hero);
            so.FindProperty("_heroId").stringValue = id;
            so.FindProperty("_heroName").stringValue = name;
            so.FindProperty("_element").enumValueIndex = (int)element;
            so.FindProperty("_classType").enumValueIndex = (int)classType;
            so.FindProperty("_baseHP").intValue = hp;
            so.FindProperty("_baseATK").intValue = atk;
            so.FindProperty("_baseDEF").intValue = def;
            so.FindProperty("_baseSPD").intValue = spd;
            so.FindProperty("_baseCritRate").intValue = critRate;
            so.FindProperty("_baseCritDmg").intValue = critDmg;
            so.ApplyModifiedProperties();
        }

        private static void SetHeroSkills(HeroDefinition hero, SkillDefinition skill, int unlockLevel)
        {
            SerializedObject so = new SerializedObject(hero);
            SerializedProperty skillsArray = so.FindProperty("_skills");
            skillsArray.arraySize = 1;
            SerializedProperty skillRef = skillsArray.GetArrayElementAtIndex(0);
            skillRef.FindPropertyRelative("_skill").objectReferenceValue = skill;
            skillRef.FindPropertyRelative("_unlockLevel").intValue = unlockLevel;
            so.ApplyModifiedProperties();
        }

        [MenuItem(MENU_ITEM, true)]
        private static bool ValidateCreateSampleAssets()
        {
            return true;
        }
    }
}
#endif