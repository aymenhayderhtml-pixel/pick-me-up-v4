#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PickMeUp.Data;

namespace PickMeUp.EditorTools
{
    public static class GenerateManhwaHeroes
    {
        [MenuItem("Tools/PickMeUp/Generate Manhwa Heroes (Wipe & Rebuild)")]
        public static void Generate()
        {
            Debug.Log("<color=cyan>[ManhwaHeroes] Wiping old heroes and generating authentic 1-star Novices...</color>");

            string heroesFolder = "Assets/Resources/Heroes";
            
            // 1. Wipe old heroes
            if (Directory.Exists(heroesFolder))
            {
                string[] oldFiles = Directory.GetFiles(heroesFolder, "*.asset");
                foreach (string file in oldFiles) AssetDatabase.DeleteAsset(file);
            }
            else
            {
                Directory.CreateDirectory(heroesFolder);
            }

            // 2. Define the Manhwa 1-Star Novice Roster (Based on your table)
            List<HeroBlueprint> blueprints = new List<HeroBlueprint>
            {
                // Stats mapped: HP=120, STR(ATK)=14, INT(DEF)=10, AGI(SPD)=12
                new HeroBlueprint("hero_islat_han", "Islat Han", 1, ElementType.Light, ClassType.Striker, 
                    120, 14, 10, 12, 500, 15000, "The true identity of the master. A novice with hidden, limitless potential."),
                
                new HeroBlueprint("hero_han_israt", "Han Israt", 1, ElementType.Dark, ClassType.Vanguard, 
                    120, 14, 10, 12, 500, 15000, "The 5-star hero trapped in a 1-star body. The protagonist of the Abyss."),
                
                new HeroBlueprint("hero_jenna_cirai", "Jenna Cirai", 1, ElementType.Water, ClassType.Ranger, 
                    120, 14, 10, 12, 800, 16000, "The early version of the sharpshooter. Needs training to reach her true potential."),
                
                new HeroBlueprint("hero_aaron_delcut", "Aaron Delcut", 1, ElementType.Fire, ClassType.Vanguard, 
                    120, 14, 10, 12, 500, 15000, "The loyal knight of Townia. A reliable novice vanguard."),
                
                new HeroBlueprint("hero_enok", "Enok", 1, ElementType.Wood, ClassType.Tactician, 
                    120, 14, 10, 12, 500, 15000, "A young novice. Weak now, but loyal to the end."),
                
                new HeroBlueprint("hero_chloe", "Chloe", 1, ElementType.Water, ClassType.Caster, 
                    120, 14, 10, 12, 500, 15000, "A novice mage. Her magic is still unrefined."),
                
                new HeroBlueprint("hero_gide", "Gide", 1, ElementType.Fire, ClassType.Vanguard, 
                    120, 14, 10, 12, 500, 15000, "A sturdy novice. Takes hits so others don't have to."),
                
                new HeroBlueprint("hero_hansen", "Hansen", 1, ElementType.Wood, ClassType.Ranger, 
                    120, 14, 10, 12, 500, 15000, "A novice archer. Still learning to aim."),
                
                new HeroBlueprint("hero_dika", "Dika", 1, ElementType.Dark, ClassType.Striker, 
                    120, 14, 10, 12, 500, 15000, "A quiet novice. Strikes from the shadows."),
                
                new HeroBlueprint("hero_antaris", "Antaris", 1, ElementType.Light, ClassType.Caster, 
                    120, 14, 10, 12, 500, 15000, "A novice priest. His healing light is just a spark.")
            };

            // 3. Generate and Save
            foreach (var bp in blueprints)
            {
                CreateHeroAsset(bp, heroesFolder);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"<color=green>[ManhwaHeroes] SUCCESS! Generated {blueprints.Count} authentic 1-star Novices.</color>");
        }

        private static void CreateHeroAsset(HeroBlueprint bp, string folder)
        {
            HeroDefinition hero = ScriptableObject.CreateInstance<HeroDefinition>();
            hero.name = bp.Id;

            SerializedObject so = new SerializedObject(hero);
            so.FindProperty("_heroId").stringValue = bp.Id;
            so.FindProperty("_heroName").stringValue = bp.Name;
            so.FindProperty("_heroFlavorText").stringValue = bp.FlavorText;
            so.FindProperty("_element").enumValueIndex = (int)bp.Element;
            so.FindProperty("_classType").enumValueIndex = (int)bp.Class;
            so.FindProperty("_baseStar").intValue = bp.Star;
            
            SerializedProperty maxLevels = so.FindProperty("_maxLevelPerStar");
            maxLevels.arraySize = 7;
            for (int i = 0; i < 7; i++) maxLevels.GetArrayElementAtIndex(i).intValue = (i + 1) * 10;

            so.FindProperty("_baseHP").intValue = bp.HP;
            so.FindProperty("_baseATK").intValue = bp.ATK;
            so.FindProperty("_baseDEF").intValue = bp.DEF;
            so.FindProperty("_baseSPD").intValue = bp.SPD;
            so.FindProperty("_baseCritRate").intValue = bp.CritRate;
            so.FindProperty("_baseCritDmg").intValue = bp.CritDmg;

            // AUTO-ASSIGN PORTRAIT IF IT EXISTS
            string portraitName = bp.Id.Replace("hero_", ""); // e.g., "islat_han"
            Sprite portrait = Resources.Load<Sprite>($"Portraits/{portraitName}");
            if (portrait != null)
            {
                so.FindProperty("_portrait").objectReferenceValue = portrait;
                Debug.Log($"[ManhwaHeroes] Assigned portrait for {bp.Name}!");
            }

            so.ApplyModifiedProperties();

            string path = $"{folder}/{bp.Id}.asset";
            AssetDatabase.CreateAsset(hero, path);
        }

        private struct HeroBlueprint
        {
            public string Id, Name, FlavorText;
            public int Star, HP, ATK, DEF, SPD, CritRate, CritDmg;
            public ElementType Element;
            public ClassType Class;

            public HeroBlueprint(string id, string name, int star, ElementType element, ClassType cls, 
                int hp, int atk, int def, int spd, int critRate, int critDmg, string flavor)
            {
                Id = id; Name = name; Star = star; Element = element; Class = cls;
                HP = hp; ATK = atk; DEF = def; SPD = spd; CritRate = critRate; CritDmg = critDmg; FlavorText = flavor;
            }
        }
    }
}
#endif