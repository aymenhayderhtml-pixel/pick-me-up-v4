// Assets/Scripts/Core/BootInstaller.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using PickMeUp.Data;
using PickMeUp.Services;
using PickMeUp.Services.Implementations;

namespace PickMeUp.Core
{
    /// <summary>
    /// The entry point MonoBehaviour responsible for initializing the game.
    /// Bootstraps the ServiceRegistry, loads initial configurations, and loads the Hub scene.
    /// </summary>
    public class BootInstaller : MonoBehaviour
    {
        #region Unity Lifecycle

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            
            ServiceRegistry.Register<IGameStateService>(new GameStateService());
            ServiceRegistry.Register<IEventBus>(new EventBus());
            ServiceRegistry.Register<ISaveLoadService>(new SaveLoadService());
            ServiceRegistry.Register<IIdleProgressionService>(new IdleProgressionService());
            ServiceRegistry.Register<IGachaService>(new GachaService());
            ServiceRegistry.Register<IHeroRosterService>(new HeroRosterService());
            
            DataService dataService = gameObject.AddComponent<DataService>();
            ServiceRegistry.Register<IDataService>(dataService);
            
            dataService.LoadAllDefinitions();
        }

        private void Start()
        {
            if (ServiceRegistry.HasService<IGameStateService>())
            {
                IGameStateService gameStateService = ServiceRegistry.Resolve<IGameStateService>();
                gameStateService.ChangeState(GameState.Hub);
            }
            
            if (SceneManager.GetActiveScene().name != "Hub")
            {
                SceneManager.LoadScene("Hub", LoadSceneMode.Single);
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && ServiceRegistry.HasService<ISaveLoadService>())
            {
                // Auto-save stub
            }
        }

        private void OnApplicationQuit()
        {
            if (ServiceRegistry.HasService<ISaveLoadService>())
            {
                // Final save stub
            }
            
            ServiceRegistry.Clear();
        }

        #endregion
    }
}