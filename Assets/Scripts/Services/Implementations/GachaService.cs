using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PickMeUp.Core;
using PickMeUp.Data;
using PickMeUp.Services;

namespace PickMeUp.Services.Implementations
{
    public class GachaService : IGachaService
    {
        private GachaPityData _pityData;
        private readonly object _lock = new object();
        private readonly System.Random _rng = new System.Random(Environment.TickCount);

        public GachaService()
        {
            InitPity();
        }

        public HeroInstance Pull(int bannerId)
        {
            try
            {
                var allDefs = Resources.LoadAll<HeroDefinition>("");
                if (allDefs == null || allDefs.Length == 0)
                {
                    Debug.LogError("[GachaService] No hero definitions found in Resources!");
                    return null;
                }

                // MVP: Simple random selection
                HeroDefinition selected = allDefs[_rng.Next(allDefs.Length)];
                HeroInstance instance = new HeroInstance(selected);
                
                try
                {
                    if (ServiceRegistry.HasService<IHeroRosterService>())
                    {
                        ServiceRegistry.Resolve<IHeroRosterService>().AddHero(instance);
                    }
                }
                catch (Exception rosterEx)
                {
                    Debug.LogError($"[GachaService] Failed to add hero to roster: {rosterEx.Message}");
                }

                TrackPity(bannerId);
                Debug.Log($"[GachaService] Pulled hero: {instance.HeroDefId} (Banner: {bannerId})");
                return instance;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GachaService] Pull failed: {ex.Message}");
                return null;
            }
        }

        public HeroInstance[] PullMultiple(int bannerId, int count)
        {
            List<HeroInstance> results = new List<HeroInstance>(count);
            for (int i = 0; i < count; i++)
            {
                HeroInstance result = Pull(bannerId);
                if (result != null) results.Add(result);
            }
            return results.ToArray();
        }

        public void TrackPity(int bannerId)
        {
            lock (_lock)
            {
                var existingEntry = _pityData.BannerPityCounters.FirstOrDefault(e => e.BannerId == bannerId);

                if (existingEntry.BannerId == bannerId) // Struct default is 0, so this works if bannerId is 0, but let's be safe
                {
                    int index = _pityData.BannerPityCounters.FindIndex(e => e.BannerId == bannerId);
                    if (index >= 0)
                    {
                        var updated = _pityData.BannerPityCounters[index];
                        updated.PullCount++;
                        _pityData.BannerPityCounters[index] = updated;
                    }
                }
                else
                {
                    _pityData.BannerPityCounters.Add(new BannerPityEntry { BannerId = bannerId, PullCount = 1 });
                }
                
                SaveState(); // Persist pity to disk
            }
        }

        public int GetPityCount(int bannerId)
        {
            lock (_lock)
            {
                var entry = _pityData.BannerPityCounters.FirstOrDefault(e => e.BannerId == bannerId);
                return entry.PullCount;
            }
        }

        private void InitPity()
        {
            lock (_lock)
            {
                try
                {
                    var saveService = ServiceRegistry.Resolve<ISaveLoadService>();
                    var save = saveService.Load();
                    _pityData = save.Pity ?? new GachaPityData();
                    Debug.Log("[GachaService] Loaded pity data from save file.");
                }
                catch
                {
                    _pityData = new GachaPityData();
                }
            }
        }

        private void SaveState()
        {
            try
            {
                var saveService = ServiceRegistry.Resolve<ISaveLoadService>();
                var save = saveService.Load();
                save.Pity = _pityData;
                saveService.Save(save);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GachaService] Failed to save pity: {ex.Message}");
            }
        }
    }
}