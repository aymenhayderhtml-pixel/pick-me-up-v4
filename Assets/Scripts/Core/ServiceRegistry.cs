// Assets/Scripts/Core/ServiceRegistry.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PickMeUp.Core
{
    /// <summary>
    /// A thread-safe, generic service locator used for dependency injection.
    /// Acts as the central registry for all core game services.
    /// </summary>
    public static class ServiceRegistry
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private static readonly object _lock = new object();

        /// <summary>
        /// Registers a service instance to its corresponding interface type.
        /// </summary>
        public static void Register<T>(T instance) where T : class
        {
            if (instance == null)
            {
                Debug.LogError($"[ServiceRegistry] Cannot register null instance of type {typeof(T).Name}.");
                return;
            }

            lock (_lock)
            {
                Type type = typeof(T);
                if (_services.ContainsKey(type))
                {
                    Debug.LogWarning($"[ServiceRegistry] Service {type.Name} is already registered. Overwriting.");
                }

                _services[type] = instance;
                Debug.Log($"[ServiceRegistry] Registered: {type.Name}");
            }
        }

        /// <summary>
        /// Resolves and returns a registered service instance.
        /// </summary>
        public static T Resolve<T>() where T : class
        {
            lock (_lock)
            {
                Type type = typeof(T);
                if (_services.TryGetValue(type, out object service))
                {
                    return (T)service;
                }

                Debug.LogError($"[ServiceRegistry] Service {type.Name} not registered.");
                throw new InvalidOperationException($"Service {type.Name} not registered.");
            }
        }

        /// <summary>
        /// Checks if a service of the specified type is currently registered.
        /// </summary>
        public static bool HasService<T>() where T : class
        {
            lock (_lock)
            {
                return _services.ContainsKey(typeof(T));
            }
        }

        /// <summary>
        /// Unregisters a service, removing it from the registry.
        /// </summary>
        public static void Unregister<T>() where T : class
        {
            lock (_lock)
            {
                Type type = typeof(T);
                if (_services.Remove(type))
                {
                    Debug.Log($"[ServiceRegistry] Unregistered: {type.Name}");
                }
            }
        }

        /// <summary>
        /// Clears all registered services. Should only be called during application shutdown or testing.
        /// </summary>
        public static void Clear()
        {
            lock (_lock)
            {
                _services.Clear();
                Debug.Log("[ServiceRegistry] All services cleared");
            }
        }
    }
}