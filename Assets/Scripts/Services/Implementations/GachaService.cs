using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PickMeUp.Core;
using PickMeUp.Data;

namespace PickMeUp.Services.Implementations
{
    public class GachaService : IGachaService
    {
        // Banner IDs
        private const int BANNER_STANDARD = 0;
        private const int BANNER_PREMIUM = 1;

        // Costs
        private const int STD_COST_1 = 1000;
        private const int STD_COST_10 = 9000; // 10% discount
        private const int PREM_COST_1 = 300;
        private const int PREM_COST_10 = 2700; // 10% discount

        // Pity Thresholds
        private const int STD_PITY_THRESHOLD = 10; // Guarantees 2★ or higher
        private const int PREM_PITY_THRESHOLD = 10; // Guarantees 4★ or higher
        private const int PREM_GUARANTEE_THRESHOLD = 7; // UI shows badge at 7

        private GachaPityData _pityData;
        private readonly object _lock = new object();
        private readonly System.Random _rng = new System.Random(Environment.TickCount);
        
        private HeroDefinition[] _allDefs;

        public GachaService()
        {
            InitPity();
            LoadHeroDefinitions();
        }

        private void LoadHeroDefinitions()
        {
            _allDefs = Resources.LoadAll<HeroDefinition>("");
            if (_allDefs == null || _allDefs.Length == 0)
            {
                Debug.LogError("[GachaService] No hero definitions found in Resources!");
            }
        }

        public List<HeroInstance> PullStandard(int count)
        {
            return ExecutePull(BANNER_STANDARD, count);
        }

        public List<HeroInstance> PullPremium(int count)
        {
            return ExecutePull(BANNER_PREMIUM, count);
        }

        public bool CanAffordStandard(int count)
        {
            var save = ServiceRegistry.Resolve<ISaveLoadService>().Load();
            int cost = count >= 10 ? STD_COST_10 : STD_COST_1;
            return save.Gold >= cost;
        }

        public bool CanAffordPremium(int count)
        {
            var save = ServiceRegistry.Resolve<ISaveLoadService>().Load();
            int cost = count >= 10 ? PREM_COST_10 : PREM_COST_1;
            return save.Gems >= cost;
        }

        // NEW: Expose Pity and Guarantee for UI
                public int GetPityCount(int bannerId)
        {
            lock (_lock)
            {
                // Use a simple foreach to avoid LINQ null/struct issues
                foreach (var entry in _pityData.BannerPityCounters)
                {
                    if (entry.BannerId == bannerId) return entry.PullCount;
                }
                return 0;
            }
        }

        public bool IsPremiumGuaranteed()
        {
            return GetPityCount(BANNER_PREMIUM) >= PREM_GUARANTEE_THRESHOLD;
        }

        public int GetPremiumGuaranteeThreshold()
        {
            return PREM_GUARANTEE_THRESHOLD;
        }

        public int GetStandardPityThreshold()
        {
            return STD_PITY_THRESHOLD;
        }

        public int GetPremiumPityThreshold()
        {
            return PREM_PITY_THRESHOLD;
        }

        private List<HeroInstance> ExecutePull(int bannerId, int count)
        {
            lock (_lock)
            {
                var saveService = ServiceRegistry.Resolve<ISaveLoadService>();
                var save = saveService.Load();

                // 1. Check & Deduct Cost
                int cost = 0;
                if (bannerId == BANNER_STANDARD)
                {
                    cost = count >= 10 ? STD_COST_10 : STD_COST_1;
                    if (save.Gold < cost) { Debug.LogWarning("[Gacha] Not enough Gold!"); return new List<HeroInstance>(); }
                    save.Gold -= cost;
                }
                else
                {
                    cost = count >= 10 ? PREM_COST_10 : PREM_COST_1;
                    if (save.Gems < cost) { Debug.LogWarning("[Gacha] Not enough Gems!"); return new List<HeroInstance>(); }
                    save.Gems -= cost;
                }

                // 2. Roll Heroes
                List<HeroInstance> results = new List<HeroInstance>();
                bool isTenPull = count >= 10;
                
                for (int i = 0; i < count; i++)
                {
                    // Check if this is the last pull of a 10-pull to apply pity guarantee
                    bool applyPityGuarantee = isTenPull && i == count - 1; 
                    
                    int rolledStar = RollRarity(bannerId, applyPityGuarantee);
                    HeroDefinition def = GetRandomHeroOfStar(rolledStar, bannerId);
                    
                    if (def != null)
                    {
                        results.Add(new HeroInstance(def));
                    }
                    
                    TrackPity(bannerId, rolledStar);
                }

                // 3. Save State
                saveService.Save(save);
                SavePityState();

                Debug.Log($"[GachaService] Pulled {count} heroes from Banner {bannerId}. Cost: {cost}");
                return results;
            }
        }

        private int RollRarity(int bannerId, bool forceHighRarity)
        {
            if (bannerId == BANNER_STANDARD)
            {
                if (forceHighRarity) return RollWeighted(new[] { (2, 80), (3, 20) }); // Guarantee 2★+
                return RollWeighted(new[] { (1, 70), (2, 25), (3, 5) });
            }
            else // Premium
            {
                if (forceHighRarity) return RollWeighted(new[] { (4, 80), (5, 20) }); // Guarantee 4★+
                return RollWeighted(new[] { (3, 60), (4, 30), (5, 10) });
            }
        }

        private int RollWeighted((int star, int weight)[] pool)
        {
            int totalWeight = pool.Sum(p => p.weight);
            int roll = _rng.Next(totalWeight);
            int current = 0;

            foreach (var p in pool)
            {
                current += p.weight;
                if (roll < current) return p.star;
            }
            return pool[0].star; // Fallback
        }

        private HeroDefinition GetRandomHeroOfStar(int star, int bannerId)
        {
            if (_allDefs == null || _allDefs.Length == 0) LoadHeroDefinitions();
            
            // Filter by star rating
            var pool = _allDefs.Where(h => h.BaseStar == star).ToArray();
            
            // Fallback if no heroes of that exact star exist in the game yet
            if (pool.Length == 0)
            {
                pool = _allDefs.Where(h => h.BaseStar == star - 1).ToArray();
                if (pool.Length == 0) pool = _allDefs; // Ultimate fallback
            }

            return pool[_rng.Next(pool.Length)];
        }

        private void TrackPity(int bannerId, int rolledStar)
        {
            var entry = _pityData.BannerPityCounters.FirstOrDefault(e => e.BannerId == bannerId);
            int index = _pityData.BannerPityCounters.FindIndex(e => e.BannerId == bannerId);

            // Reset pity if we hit the "jackpot" for that banner
            bool hitJackpot = (bannerId == BANNER_STANDARD && rolledStar >= 3) || 
                              (bannerId == BANNER_PREMIUM && rolledStar >= 5);

            if (index >= 0)
            {
                var updated = _pityData.BannerPityCounters[index];
                updated.PullCount = hitJackpot ? 0 : updated.PullCount + 1;
                _pityData.BannerPityCounters[index] = updated;
            }
            else
            {
                _pityData.BannerPityCounters.Add(new BannerPityEntry { BannerId = bannerId, PullCount = hitJackpot ? 0 : 1 });
            }
        }

                private void InitPity()
        {
            lock (_lock)
            {
                try 
                { 
                    var loadedData = ServiceRegistry.Resolve<ISaveLoadService>().Load().Pity;
                    // Safe assignment whether GachaPityData is a class or struct
                    _pityData = loadedData ?? new GachaPityData(); 
                }
                catch 
                { 
                    _pityData = new GachaPityData(); 
                }
            }
        }

        private void SavePityState()
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