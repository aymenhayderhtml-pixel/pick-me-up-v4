// Assets/Scripts/Services/Implementations/SynthesisService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PickMeUp.Core;
using PickMeUp.Data;
using PickMeUp.Services;

namespace PickMeUp.Services.Implementations
{
    /// <summary>
    /// Concrete implementation of the synthesis system. 
    /// Handles the high-stakes fusion mechanics central to the game's progression loop.
    /// </summary>
    public class SynthesisService : ISynthesisService
    {
        #region Fields

        // Meta-progression RNG, does not require combat determinism
        private readonly System.Random _rng = new System.Random(Environment.TickCount);

        #endregion

        #region Events

        public event Action<HeroInstance> OnSynthesisSuccess;
        public event Action OnSynthesisFailure;

        #endregion

        #region ISynthesisService Implementation

        /// <inheritdoc/>
        public bool CanSynthesize(List<HeroInstance> fodder, out string error)
        {
            error = string.Empty;

            if (fodder == null || (fodder.Count != 2 && fodder.Count != 3))
            {
                error = "Synthesis requires exactly 2 (risky) or 3 (guaranteed) fodder heroes.";
                return false;
            }

            int starLevel = fodder[0].CurrentStar;
            foreach (var hero in fodder)
            {
                if (hero.CurrentStar != starLevel)
                {
                    error = "All fodder heroes must share the exact same star level.";
                    return false;
                }
                if (hero.IsLocked)
                {
                    error = "Locked heroes cannot be used as synthesis fodder.";
                    return false;
                }
            }

            if (starLevel >= 7)
            {
                error = "Heroes have reached the absolute maximum of 7 stars.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Mimics Han's desperate fusion attempts in the Abyss lobby. 
        /// Combines fodder heroes to promote a target, with a risk of failure if only 2 fodder are used.
        /// </summary>
        public HeroInstance Synthesize(List<HeroInstance> fodder, HeroInstance target = null)
        {
            if (!CanSynthesize(fodder, out string error))
            {
                Debug.LogError($"[SynthesisService] Synthesis aborted: {error}");
                return null;
            }

            // Validate target if provided
            if (target != null)
            {
                if (target.CurrentStar != fodder[0].CurrentStar)
                {
                    Debug.LogError("[SynthesisService] Target star level must match fodder star level.");
                    return null;
                }
                if (fodder.Any(f => f.InstanceId == target.InstanceId))
                {
                    Debug.LogError("[SynthesisService] Target hero cannot be part of the fodder list.");
                    return null;
                }
            }

            IHeroRosterService rosterService = ServiceRegistry.Resolve<IHeroRosterService>();

            bool isGuaranteed = fodder.Count == 3;
            bool isSuccess = isGuaranteed || _rng.NextDouble() < 0.60; // 60% success rate for 2 fodder

            HeroInstance resultHero = null;
            int previousStar = fodder[0].CurrentStar;

            if (isSuccess)
            {
                // Determine which hero gets promoted
                if (target != null)
                {
                    resultHero = target;
                }
                else
                {
                    // Randomly select one from fodder to become the promoted hero
                    resultHero = fodder[_rng.Next(fodder.Count)];
                }

                // Promote the hero
                resultHero.CurrentStar++;
                resultHero.CurrentLevel = 1;
                resultHero.CurrentXP = 0;
                resultHero.PromotionCount++;

                // Remove consumed fodder from roster (excluding the promoted hero if it was in the fodder)
                foreach (var f in fodder)
                {
                    if (f.InstanceId != resultHero.InstanceId)
                    {
                        rosterService.RemoveHero(f.InstanceId);
                    }
                }

                Debug.Log($"[SynthesisService] Synthesis SUCCESS! {resultHero.HeroDefId} is now {resultHero.CurrentStar}★.");
                OnSynthesisSuccess?.Invoke(resultHero);
            }
            else
            {
                // Failure: all fodder consumed
                foreach (var f in fodder)
                {
                    rosterService.RemoveHero(f.InstanceId);
                }

                // Generate consolation trash using Resources (assets exist from CreateSampleData)
                HeroDefinition consolationDef = null;
                var allDefs = Resources.LoadAll<HeroDefinition>("");
                if (allDefs != null && allDefs.Length > 0)
                {
                    var oneStarHeroes = allDefs.Where(h => h.BaseStar == 1).ToList();
                    if (oneStarHeroes.Count > 0)
                        consolationDef = oneStarHeroes[_rng.Next(oneStarHeroes.Count)];
                    else
                        consolationDef = allDefs[_rng.Next(allDefs.Length)];
                }

                if (consolationDef == null)
                {
                    Debug.LogError("[SynthesisService] No HeroDefinition assets found! Run Tools > PickMeUp > Create Sample Data.");
                    // Return a minimal fallback — create instance manually so the roster isn't corrupted
                    consolationDef = ScriptableObject.CreateInstance<HeroDefinition>();
                    // Note: This fallback hero will have zero stats. Ensure sample data exists.
                }

                resultHero = new HeroInstance(consolationDef);
                rosterService.AddHero(resultHero);

                Debug.Log($"[SynthesisService] Synthesis FAILED! Fodder burned. Received consolation hero: {resultHero.HeroDefId}.");
                OnSynthesisFailure?.Invoke();
            }

            // Log synthesis history to save data
            if (ServiceRegistry.HasService<ISaveLoadService>())
            {
                var saveService = ServiceRegistry.Resolve<ISaveLoadService>();
                var save = saveService.Load();
                if (save != null)
                {
                    save.SynthesisHistory.Add(new SynthesisLogEntry {
                        TargetHeroDefId = resultHero.HeroDefId,
                        PreviousStar = isSuccess ? resultHero.CurrentStar - 1 : previousStar,
                        NewStar = resultHero.CurrentStar,
                        Success = isSuccess,
                        Timestamp = DateTime.UtcNow.ToString("o")
                    });
                    saveService.Save(save);
                }
            }

            return resultHero;
        }

        #endregion
    }
}