// Assets/Scripts/Services/Implementations/HeroRosterService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PickMeUp.Data;
using PickMeUp.Services;

namespace PickMeUp.Services.Implementations
{
    /// <summary>
    /// Concrete implementation of IHeroRosterService.
    /// Thread-safe management of the player's hero collection.
    /// </summary>
    public class HeroRosterService : IHeroRosterService
    {
        #region Fields

        private readonly List<HeroInstance> _heroes = new List<HeroInstance>();
        private readonly object _lock = new object();

        #endregion

        #region Events

        public event Action<HeroInstance> OnHeroAdded;
        public event Action<string> OnHeroRemoved;

        #endregion

        #region IHeroRosterService Implementation

        public void AddHero(HeroInstance hero)
        {
            if (hero == null)
            {
                Debug.LogWarning("[HeroRosterService] Attempted to add a null hero instance.");
                return;
            }

            lock (_lock)
            {
                _heroes.Add(hero);
                Debug.Log($"[HeroRosterService] Added hero: {hero.HeroDefId} (Instance: {hero.InstanceId})");
            }

            try
            {
                OnHeroAdded?.Invoke(hero);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HeroRosterService] Error in OnHeroAdded event: {ex.Message}");
            }
        }

        public bool RemoveHero(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId)) return false;

            lock (_lock)
            {
                HeroInstance heroToRemove = _heroes.FirstOrDefault(h => h.InstanceId == instanceId);
                if (heroToRemove != null)
                {
                    _heroes.Remove(heroToRemove);
                    Debug.Log($"[HeroRosterService] Removed hero: {heroToRemove.HeroDefId} (Instance: {instanceId})");
                    
                    try
                    {
                        OnHeroRemoved?.Invoke(instanceId);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[HeroRosterService] Error in OnHeroRemoved event: {ex.Message}");
                    }
                    
                    return true;
                }
            }

            Debug.LogWarning($"[HeroRosterService] Hero with InstanceId {instanceId} not found for removal.");
            return false;
        }

        public HeroInstance GetHero(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId)) return null;

            lock (_lock)
            {
                return _heroes.FirstOrDefault(h => h.InstanceId == instanceId);
            }
        }

        public List<HeroInstance> GetAllHeroes()
        {
            lock (_lock)
            {
                // Return a copy to prevent external modification of the internal list
                return new List<HeroInstance>(_heroes);
            }
        }

        public int GetHeroCount()
        {
            lock (_lock)
            {
                return _heroes.Count;
            }
        }

        #endregion
    }
}