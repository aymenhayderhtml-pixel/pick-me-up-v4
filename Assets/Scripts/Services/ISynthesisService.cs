// Assets/Scripts/Services/ISynthesisService.cs
using System;
using System.Collections.Generic;
using PickMeUp.Data;

namespace PickMeUp.Services
{
    /// <summary>
    /// Manages the synthesis (fusion) mechanics for promoting heroes to higher star tiers.
    /// </summary>
    public interface ISynthesisService
    {
        /// <summary>
        /// Validates if the provided fodder heroes can be synthesized.
        /// </summary>
        /// <param name="fodder">The list of heroes to be consumed.</param>
        /// <param name="error">Output error message if validation fails.</param>
        /// <returns>True if synthesis is valid, false otherwise.</returns>
        bool CanSynthesize(List<HeroInstance> fodder, out string error);

        /// <summary>
        /// Executes the synthesis process, consuming fodder and attempting to promote a target.
        /// </summary>
        /// <param name="fodder">The list of heroes to consume.</param>
        /// <param name="target">Optional specific hero to promote. If null, one is chosen from fodder.</param>
        /// <returns>The resulting promoted hero, or a consolation hero on failure.</returns>
        HeroInstance Synthesize(List<HeroInstance> fodder, HeroInstance target = null);

        /// <summary>
        /// Triggered when a synthesis successfully promotes a hero.
        /// </summary>
        event Action<HeroInstance> OnSynthesisSuccess;

        /// <summary>
        /// Triggered when a risky synthesis fails, consuming the fodder.
        /// </summary>
        event Action OnSynthesisFailure;
    }
}