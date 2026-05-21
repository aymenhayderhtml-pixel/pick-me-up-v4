// Assets/Scripts/Combat/CombatFormulas.cs
using System;
using System.Collections.Generic;
using System.Linq;
using PickMeUp.Data;

namespace PickMeUp.Combat
{
    /// <summary>
    /// Contains pure, deterministic mathematical formulas for combat resolution.
    /// No Unity dependencies (e.g., uses System.Math instead of UnityEngine.Mathf).
    /// </summary>
    public static class CombatFormulas
    {
        /// <summary>
        /// Calculates the final damage dealt from an attacker to a defender.
        /// </summary>
        /// <param name="attacker">The unit dealing damage.</param>
        /// <param name="defender">The unit receiving damage.</param>
        /// <param name="skillMultiplier">The power multiplier of the skill used (1.0f for basic attack).</param>
        /// <param name="random">Deterministic random number generator.</param>
        /// <returns>Final integer damage value, minimum 1.</returns>
        public static int CalculateDamage(CombatUnit attacker, CombatUnit defender, float skillMultiplier, Random random)
        {
            // Base damage calculation
            float baseDamage = (attacker.ATK * skillMultiplier) - (defender.DEF * 0.5f);
            
            // Elemental multiplier
            float elementMultiplier = GetElementalMultiplier(attacker.Element, defender.Element);
            baseDamage *= elementMultiplier;

            // Crit check (Basis points: 10000 = 100%)
            bool isCrit = random.Next(0, 10000) < attacker.CritRate;
            if (isCrit)
            {
                float critMultiplier = attacker.CritDmg / 10000f;
                baseDamage *= critMultiplier;
            }

            // Ensure minimum damage is 1
            return (int)Math.Max(1, baseDamage);
        }

        /// <summary>
        /// Determines the turn order for a list of units based on SPD, with deterministic tie-breaking.
        /// </summary>
        /// <param name="units">List of active units.</param>
        /// <param name="random">Deterministic random number generator for tie-breaking.</param>
        /// <returns>Ordered list of units.</returns>
        public static List<CombatUnit> CalculateTurnOrder(List<CombatUnit> units, Random random)
        {
            return units
                .OrderByDescending(u => u.SPD)
                .ThenBy(u => random.Next()) // Deterministic tie-breaker
                .ToList();
        }

        /// <summary>
        /// Calculates the elemental damage multiplier based on attacker and defender elements.
        /// </summary>
        private static float GetElementalMultiplier(ElementType attacker, ElementType defender)
        {
            if (attacker == defender && attacker != ElementType.None) return 0.8f;

            // Fire > Wood > Water > Fire
            if (attacker == ElementType.Fire && defender == ElementType.Wood) return 1.3f;
            if (attacker == ElementType.Wood && defender == ElementType.Water) return 1.3f;
            if (attacker == ElementType.Water && defender == ElementType.Fire) return 1.3f;

            // Light <-> Dark
            if (attacker == ElementType.Light && defender == ElementType.Dark) return 1.5f;
            if (attacker == ElementType.Dark && defender == ElementType.Light) return 1.5f;

            return 1.0f;
        }
    }
}