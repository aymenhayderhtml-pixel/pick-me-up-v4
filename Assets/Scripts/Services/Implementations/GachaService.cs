// Assets/Scripts/Services/Implementations/GachaService.cs
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
    /// Concrete implementation of IGachaService handling summoning mechanics and pity tracking.
    /// </summary>
    public class GachaService : IGachaService
    {
        #region Fields

        private GachaPityData _pityData;
        private readonly object _lock = new object();
        private readonly System.Random _rng = new System.Random();

        #endregion

        #region Constructor

        public GachaService()
        {
            InitPity();
        }

        #endregion

        #region IGachaService Implementation

        public HeroInstance Pull(int bannerId)
        {
            try
            {
                HeroDefinition[] allHeroes = Resources.LoadAll<HeroDefinition>("");

                if (allHeroes == null || allHeroes.Length == 0)
                {
                    Debug.LogError("[GachaService] No hero definitions available for summoning");
                    return null;
                }

                int randomIndex = _rng.Next(0, allHeroes.Length);
                HeroDefinition selected = allHeroes[randomIndex];

                HeroInstance instance = new HeroInstance(selected);
                
                // Auto-save to roster
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

                // Recalculate stats via HeroProgressionService
                if (ServiceRegistry.HasService<IHeroProgressionService>())
                {
                    var prog = ServiceRegistry.Resolve<IHeroProgressionService>();
                    prog.RecalculateStats(instance);
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
                if (result != null)
                {
                    results.Add(result);
                }
            }
            return results.ToArray();
        }

        public void TrackPity(int bannerId)
        {
            lock (_lock)
            {
                BannerPityEntry? existingEntry = _pityData.BannerPityCounters
                    .FirstOrDefault(e => e.BannerId == bannerId);

                if (existingEntry.HasValue)
                {
                    int index = _pityData.BannerPityCounters
                        .FindIndex(e => e.BannerId == bannerId);
                    if (index >= 0)
                    {
                        BannerPityEntry updated = new BannerPityEntry
                        {
                            BannerId = bannerId,
                            PullCount = existingEntry.Value.PullCount + 1
                        };
                        _pityData.BannerPityCounters[index] = updated;
                    }
                }
                else
                {
                    _pityData.BannerPityCounters.Add(new BannerPityEntry
                    {
                        BannerId = bannerId,
                        PullCount = 1
                    });
                }
            }
        }

        public int GetPityCount(int bannerId)
        {
            lock (_lock)
            {
                BannerPityEntry? entry = _pityData.BannerPityCounters
                    .FirstOrDefault(e => e.BannerId == bannerId);
                
                return entry.HasValue ? entry.Value.PullCount : 0;
            }
        }

        #endregion

        #region Private Methods

        private void InitPity()
        {
            lock (_lock)
            {
                if (_pityData == null)
                {
                    _pityData = new GachaPityData();
                }
            }
        }

        #endregion
    }
}