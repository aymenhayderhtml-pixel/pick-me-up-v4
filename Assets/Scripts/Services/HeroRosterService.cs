using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PickMeUp.Core;
using PickMeUp.Data;
using PickMeUp.Services;

namespace PickMeUp.Services.Implementations
{
    public class HeroRosterService : IHeroRosterService
    {
        private readonly List<HeroInstance> _heroes = new List<HeroInstance>();
        private readonly object _lock = new object();

        public event Action<HeroInstance> OnHeroAdded;
        public event Action<string> OnHeroRemoved;

        public HeroRosterService()
        {
            // Load saved roster from disk on initialization
            try
            {
                var saveService = ServiceRegistry.Resolve<ISaveLoadService>();
                var save = saveService.Load();
                if (save.HeroRoster != null)
                {
                    _heroes.AddRange(save.HeroRoster);
                    Debug.Log($"[HeroRosterService] Loaded {_heroes.Count} heroes from save file.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HeroRosterService] Failed to load roster: {ex.Message}");
            }
        }

        public void AddHero(HeroInstance hero)
        {
            if (hero == null) return;

            lock (_lock)
            {
                _heroes.Add(hero);
                Debug.Log($"[HeroRosterService] Added hero: {hero.HeroDefId}");
            }

            SaveState(); // Persist to disk
            OnHeroAdded?.Invoke(hero);
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
                    Debug.Log($"[HeroRosterService] Removed hero: {instanceId}");
                    SaveState(); // Persist to disk
                    OnHeroRemoved?.Invoke(instanceId);
                    return true;
                }
            }
            return false;
        }

        public HeroInstance GetHero(string instanceId)
        {
            lock (_lock) { return _heroes.FirstOrDefault(h => h.InstanceId == instanceId); }
        }

        public List<HeroInstance> GetAllHeroes()
        {
            lock (_lock) { return new List<HeroInstance>(_heroes); }
        }

        public int GetHeroCount()
        {
            lock (_lock) { return _heroes.Count; }
        }

        private void SaveState()
        {
            try
            {
                var saveService = ServiceRegistry.Resolve<ISaveLoadService>();
                var save = saveService.Load();
                save.HeroRoster = new List<HeroInstance>(_heroes);
                saveService.Save(save);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HeroRosterService] Failed to save roster: {ex.Message}");
            }
        }
    }
}