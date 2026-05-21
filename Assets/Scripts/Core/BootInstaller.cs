using UnityEngine;
using UnityEngine.SceneManagement;
using PickMeUp.Core;
using PickMeUp.Services;
using PickMeUp.Services.Implementations;

namespace PickMeUp.Core
{
    public class BootInstaller : MonoBehaviour
    {
        private void Awake()
        {
            // Register all services in order
            ServiceRegistry.Register<IGameStateService>(new GameStateService());
            ServiceRegistry.Register<IEventBus>(new EventBus());
            ServiceRegistry.Register<ISaveLoadService>(new SaveLoadService());
            ServiceRegistry.Register<IIdleProgressionService>(new IdleProgressionService());
            ServiceRegistry.Register<IGachaService>(new GachaService());

            // DataService is a MonoBehaviour, so add it to this GameObject
            var dataService = gameObject.AddComponent<DataService>();
            ServiceRegistry.Register<IDataService>(dataService);

            // Transition to Hub scene
            SceneManager.LoadScene("Hub");
        }
    }
}