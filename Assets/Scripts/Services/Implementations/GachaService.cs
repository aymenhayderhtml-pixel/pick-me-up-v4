using System.Collections.Generic;
using UnityEngine;
using PickMeUp.Core;
using PickMeUp.Data;

namespace PickMeUp.Services.Implementations
{
    public class GachaService : IGachaService
    {
        public GachaPullResult Pull(GameSaveData saveData)
        {
            var dataService = ServiceRegistry.Resolve<IDataService>();
            var heroes = dataService.GetHeroDefinitions();

            if (heroes.Count == 0)
                return null;

            var randomIndex = Random.Range(0, heroes.Count);
            var selectedHero = heroes[randomIndex];

            var heroInstance = new HeroInstance
            {
                HeroInstanceId = System.Guid.NewGuid().ToString(),
                Definition = selectedHero,
                Level = 1,
                Experience = 0,
                CurrentHealth = selectedHero.BaseHealth,
                AscensionLevel = 0
            };

            return new GachaPullResult
            {
                PulledHero = heroInstance,
                PullCount = 1,
                IsPityBreak = false
            };
        }
    }
}