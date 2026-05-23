using System; // ADD THIS (Fixes DateTime and TimeSpan)
using System.Linq; // ADD THIS (Fixes the .Take() method)
using UnityEngine;
using UnityEngine.SceneManagement;
using PickMeUp.Data;
using PickMeUp.Services;
using PickMeUp.Services.Implementations;

namespace PickMeUp.Core
{
    public class BootInstaller : MonoBehaviour
    {
        // FIX: Changed 'private set' to 'set' so the UI can clear it when collected
        public static IdleReward PendingOfflineReward { get; set; }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            
            ServiceRegistry.Register<IGameStateService>(new GameStateService());
            ServiceRegistry.Register<IEventBus>(new EventBus());
            ServiceRegistry.Register<ISaveLoadService>(new SaveLoadService());
            ServiceRegistry.Register<IIdleProgressionService>(new IdleProgressionService());
            ServiceRegistry.Register<IGachaService>(new GachaService());
            ServiceRegistry.Register<IHeroRosterService>(new HeroRosterService());
            ServiceRegistry.Register<ISynthesisService>(new SynthesisService());
            ServiceRegistry.Register<IHeroProgressionService>(new HeroProgressionService());
            ServiceRegistry.Register<ICombatEngineService>(new CombatEngineService());
            ServiceRegistry.Register<ITowerService>(new TowerService());
            
            DataService dataService = gameObject.AddComponent<DataService>();
            ServiceRegistry.Register<IDataService>(dataService);
            dataService.LoadAllDefinitions();

            CalculateOfflineProgress();
        }

        private void Start()
        {
            if (ServiceRegistry.HasService<IGameStateService>())
                ServiceRegistry.Resolve<IGameStateService>().ChangeState(GameState.Hub);
            
            if (SceneManager.GetActiveScene().name != "Hub")
                SceneManager.LoadScene("Hub", LoadSceneMode.Single);
        }

        private void CalculateOfflineProgress()
        {
            var saveService = ServiceRegistry.Resolve<ISaveLoadService>();
            var save = saveService.Load();

            if (save.LastLoginTicks == 0) 
            {
                SaveCurrentState(save); // First time playing
                return;
            }

            DateTime lastLogin = new DateTime(save.LastLoginTicks);
TimeSpan timeAway = DateTime.UtcNow - lastLogin;
            if (timeAway.TotalMinutes < 1) 
            {
                SaveCurrentState(save); // Less than 1 minute, ignore
                return;
            }

            Debug.Log($"[BootInstaller] Player was away for {timeAway.TotalHours:F2} hours. Calculating offline gains...");
            
            var idleService = ServiceRegistry.Resolve<IIdleProgressionService>();
            PendingOfflineReward = idleService.CalculateOfflineGains(timeAway, save);

            // Apply the rewards to the actual save!
            save.Gold += PendingOfflineReward.GoldEarned;
            if (PendingOfflineReward.FinalFloorReached > save.HighestFloorCleared)
                save.HighestFloorCleared = PendingOfflineReward.FinalFloorReached;

            // Distribute XP to the offline party (if they exist in the roster)
            var roster = ServiceRegistry.Resolve<IHeroRosterService>();
            var progression = ServiceRegistry.Resolve<IHeroProgressionService>();
            if (save.OfflinePartySnapshot != null)
            {
                int xpPerHero = PendingOfflineReward.XpEarned / Mathf.Max(1, save.OfflinePartySnapshot.Count);
                foreach (var offlineHero in save.OfflinePartySnapshot)
                {
                    var realHero = roster.GetHero(offlineHero.InstanceId);
                    if (realHero != null) progression.AddXP(realHero, xpPerHero);
                }
            }

            SaveCurrentState(save); // Update timestamp and save
        }

        private void SaveCurrentState(GameSaveData save)
        {
            save.LastLoginTicks = DateTime.UtcNow.Ticks;
            
            // Save current roster as the offline party (up to 4 heroes)
            var roster = ServiceRegistry.Resolve<IHeroRosterService>();
            var allHeroes = roster.GetAllHeroes();
            save.OfflinePartySnapshot = allHeroes.Take(4).ToList();
            save.OfflineFloorLevel = Mathf.Max(1, save.HighestFloorCleared);

            ServiceRegistry.Resolve<ISaveLoadService>().Save(save);
        }

                private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && ServiceRegistry.HasService<ISaveLoadService>())
            {
                SaveCurrentState(ServiceRegistry.Resolve<ISaveLoadService>().Load());
            }
        }

        private void OnApplicationQuit()
        {
            // FIX: Only save if services are still active to prevent shutdown crashes
            if (ServiceRegistry.HasService<ISaveLoadService>())
            {
                SaveCurrentState(ServiceRegistry.Resolve<ISaveLoadService>().Load());
            }
            ServiceRegistry.Clear();
        } }
}