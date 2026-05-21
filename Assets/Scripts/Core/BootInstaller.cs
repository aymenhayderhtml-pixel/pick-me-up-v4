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
    /// 
    /// === SCENE SETUP INSTRUCTIONS ===
    /// 1. Create a new scene: File > New Scene, save as "Boot.unity"
    /// 2. Create an empty GameObject named "BootLoader" in the Hierarchy
    /// 3. Attach this BootInstaller script to the BootLoader GameObject
    /// 4. Ensure Boot.unity is scene index 0 in Build Settings
    /// 5. Ensure Hub.unity is also added (index 1 or higher)
    /// 6. The BootInstaller GameObject will persist across scenes via DontDestroyOnLoad
    /// ================================
    /// </summary>
    public class BootInstaller : MonoBehaviour
    {
        private void Awake()
        {
            // Persist this GameObject across scene loads to maintain service registry
            DontDestroyOnLoad(gameObject);

            // Register non-MonoBehaviour services
            ServiceRegistry.Register<IGameStateService>(new GameStateService());
            ServiceRegistry.Register<IEventBus>(new EventBus());
            ServiceRegistry.Register<ISaveLoadService>(new SaveLoadService());
            ServiceRegistry.Register<IIdleProgressionService>(new IdleProgressionService());
            ServiceRegistry.Register<IGachaService>(new GachaService());

            // DataService is a MonoBehaviour, so add it as a component to this persistent GameObject
            DataService dataService = gameObject.AddComponent<DataService>();
            ServiceRegistry.Register<IDataService>(dataService);

            // Ensure all definitions are loaded before proceeding
            dataService.LoadAllDefinitions();
        }

        private void Start()
        {
            // Update game state before scene transition
            if (ServiceRegistry.HasService<IGameStateService>())
            {
                IGameStateService gameStateService = ServiceRegistry.Resolve<IGameStateService>();
                gameStateService.ChangeState(GameState.Hub);
            }

            // Load the Hub scene after initialization completes
            if (SceneManager.GetActiveScene().name != "Hub")
            {
                SceneManager.LoadScene("Hub", LoadSceneMode.Single);
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && ServiceRegistry.HasService<ISaveLoadService>())
            {
                // Save logic would go here; MVP stub
            }
        }

        private void OnApplicationQuit()
        {
            if (ServiceRegistry.HasService<ISaveLoadService>())
            {
                // Final save logic would go here; MVP stub
            }

            ServiceRegistry.Clear();
        }
    }
}