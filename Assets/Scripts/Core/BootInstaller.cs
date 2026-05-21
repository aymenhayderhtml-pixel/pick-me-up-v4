// Assets/Scripts/Core/BootInstaller.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using PickMeUp.Core;
using PickMeUp.Services;
using PickMeUp.Services.Implementations;

namespace PickMeUp.Core
{
    /// <summary>
    /// Entry point MonoBehaviour for the application.
    /// 
    /// SETUP INSTRUCTIONS:
    /// 1. Create a new scene named "Boot.unity" in Assets/Scenes/
    /// 2. Create an empty GameObject named "BootLoader"
    /// 3. Add this BootInstaller component to the BootLoader GameObject
    /// 4. Add Boot.unity to Build Settings at index 0
    /// 5. Create Hub.unity scene and add it to Build Settings at index 1
    /// </summary>
    public class BootInstaller : MonoBehaviour
    {
        private void Awake()
        {
            // Prevent this object from being destroyed on scene load
            DontDestroyOnLoad(gameObject);

            Debug.Log("[BootInstaller] Initializing services...");

            // Register all services in order
            ServiceRegistry.Register<IGameStateService>(new GameStateService());
            ServiceRegistry.Register<IEventBus>(new EventBus());
            ServiceRegistry.Register<ISaveLoadService>(new SaveLoadService());
            ServiceRegistry.Register<IIdleProgressionService>(new IdleProgressionService());
            ServiceRegistry.Register<IGachaService>(new GachaService());

            // DataService is a MonoBehaviour, so add it to this GameObject
            var dataService = gameObject.AddComponent<DataService>();
            ServiceRegistry.Register<IDataService>(dataService);

            // Load all data definitions (Heroes, Skills, Traits)
            dataService.LoadAllDefinitions();

            Debug.Log("[BootInstaller] All services registered and data loaded.");
        }

        private void Start()
        {
            // Change game state to Hub
            var gameStateService = ServiceRegistry.Resolve<IGameStateService>();
            gameStateService.ChangeState(GameState.Hub);

            Debug.Log("[BootInstaller] Loading Hub scene...");

            // Load Hub scene
            SceneManager.LoadScene("Hub");
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            // TODO: Handle application pause (save progress, stop timers)
            if (pauseStatus)
            {
                Debug.Log("[BootInstaller] Application paused");
            }
            else
            {
                Debug.Log("[BootInstaller] Application resumed");
            }
        }

        private void OnApplicationQuit()
        {
            // TODO: Handle application quit (save progress, cleanup)
            Debug.Log("[BootInstaller] Application quitting");
            
            // Clear all services
            ServiceRegistry.Clear();
        }
    }
}