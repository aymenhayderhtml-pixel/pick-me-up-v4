#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using PickMeUp.Data;

namespace PickMeUp.EditorTools
{
    public static class SetupHeroes
    {
        [MenuItem("Tools/PickMeUp/Generate Hero Roster (15 Heroes)")]
        public static void GenerateHeroes()
        {
            string heroPath = "Assets/Resources/Heroes";
            string portraitPath = "Assets/Resources/Portraits";
            string crestPath = "Assets/Resources/Crests";

            // 1. Create necessary folders
            if (!Directory.Exists(heroPath)) Directory.CreateDirectory(heroPath);
            if (!Directory.Exists(portraitPath)) Directory.CreateDirectory(portraitPath);
            if (!Directory.Exists(crestPath)) Directory.CreateDirectory(crestPath);

            // 2. Clear old heroes to prevent duplicates when re-running
            string[] oldHeroes = Directory.GetFiles(heroPath, "*.asset");
            foreach (var file in oldHeroes) AssetDatabase.DeleteAsset(file);

            // 3. Define the Roster
            List<(string name, int star, PersonalityTrait trait, string organ)> roster = new List<(string, int, PersonalityTrait, string)>
            {
                // 1★ (Fodder)
                ("Goblin Grunt", 1, PersonalityTrait.Cowardly, ""),
                ("Slime", 1, PersonalityTrait.Disciplined, ""),
                ("Skeleton", 1, PersonalityTrait.Reckless, ""),
                // 2★
                ("Village Guard", 2, PersonalityTrait.Loyal, ""),
                ("Apprentice Mage", 2, PersonalityTrait.Traumatized, ""),
                // 3★
                ("Ironclad Knight", 3, PersonalityTrait.Brave, ""),
                ("Shadow Rogue", 3, PersonalityTrait.Reckless, ""),
                ("Forest Ranger", 3, PersonalityTrait.Disciplined, ""),
                ("Cleric of Dawn", 3, PersonalityTrait.Loyal, ""),
                // 4★
                ("Vespera the Void", 4, PersonalityTrait.Traumatized, "Void-Touched Eye"),
                ("Kaelen Sunbreaker", 4, PersonalityTrait.Brave, "Solar Marrow"),
                ("Jin the Silent", 4, PersonalityTrait.Disciplined, ""),
                // 5★ (Legendary)
                ("Aria, Star-Weaver", 5, PersonalityTrait.Loyal, "Astral Heart"),
                ("Thorne, Blood-Drinker", 5, PersonalityTrait.Reckless, "Crimson Gland"),
                ("Loki, Trickster God", 5, PersonalityTrait.Cowardly, "Illusion Cortex")
            };

            int created = 0;
            foreach (var h in roster)
            {
                HeroDefinition def = ScriptableObject.CreateInstance<HeroDefinition>();
                def.HeroId = h.name.Replace(" ", "_").Replace(",", "").ToLower();
                def.HeroName = h.name;
                def.BaseStar = h.star;
                def.Personality = h.trait;
                def.SpecialOrgan = h.organ;
                def.HiddenPotential = Random.Range(0.1f, 1.0f);
                def.DefaultMorale = 100f;

                // Scale stats by star rating
                int baseStat = h.star * 10;
                def.BaseHP = baseStat * 10;
                def.BaseATK = baseStat * 2;
                def.BaseDEF = baseStat;
                def.BaseSPD = 10 + h.star;
                def.BaseCritRate = 5 + h.star;
                def.BaseCritDmg = 150 + (h.star * 10);

                // Auto-assign art if it exists in the folders
                def.Portrait = FindSprite($"Portrait_{def.HeroId}");
                def.Crest = FindSprite($"Crest_{def.HeroId}");

                string assetPath = $"{heroPath}/{def.HeroId}.asset";
                AssetDatabase.CreateAsset(def, assetPath);
                created++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"<color=green>[SetupHeroes] SUCCESS! Generated {created} heroes in {heroPath}.</color>");
            EditorUtility.DisplayDialog("Heroes Generated", $"Successfully created {created} heroes!\n\nNext step: Generate their portraits using DALL-E and drop them into Assets/Resources/Portraits/", "OK");
        }

        private static Sprite FindSprite(string name)
        {
            string[] guids = AssetDatabase.FindAssets($"{name} t:Sprite");
            if (guids.Length > 0) return AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guids[0]));
            return null;
        }
    }
}
#endif