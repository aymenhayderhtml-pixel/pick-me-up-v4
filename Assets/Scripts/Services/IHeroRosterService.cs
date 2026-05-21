// Assets/Scripts/Services/IHeroRosterService.cs
using System;
using System.Collections.Generic;
using PickMeUp.Data;

namespace PickMeUp.Services
{
    /// <summary>
    /// Manages the player's collection of owned heroes.
    /// </summary>
    public interface IHeroRosterService
    {
        /// <summary>
        /// Adds a new hero instance to the roster.
        /// </summary>
        void AddHero(HeroInstance hero);

        /// <summary>
        /// Removes a hero instance from the roster by its unique ID.
        /// </summary>
        /// <returns>True if the hero was found and removed, false otherwise.</returns>
        bool RemoveHero(string instanceId);

        /// <summary>
        /// Retrieves a specific hero instance by its unique ID.
        /// </summary>
        HeroInstance GetHero(string instanceId);

        /// <summary>
        /// Returns a copy of all heroes currently in the roster.
        /// </summary>
        List<HeroInstance> GetAllHeroes();

        /// <summary>
        /// Gets the total number of heroes in the roster.
        /// </summary>
        int GetHeroCount();

        /// <summary>
        /// Triggered when a new hero is added to the roster.
        /// </summary>
        event Action<HeroInstance> OnHeroAdded;

        /// <summary>
        /// Triggered when a hero is removed from the roster. Passes the removed hero's InstanceId.
        /// </summary>
        event Action<string> OnHeroRemoved;
    }
}