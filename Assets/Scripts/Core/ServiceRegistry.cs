// Assets/Scripts/Core/ServiceRegistry.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PickMeUp.Core
{
    public static class ServiceRegistry
    {
        private static readonly Dictionary<Type, object> _services = new();
        private static readonly object _lockObject = new();

        public static void Register<T>(T service) where T : class
        {
            if (service == null)
            {
                Debug.LogError($"[ServiceRegistry] Attempted to register null service of type {typeof(T).Name}");
                return;
            }

            lock (_lockObject)
            {
                _services[typeof(T)] = service;
                Debug.Log($"[ServiceRegistry] Registered: {typeof(T).Name}");
            }
        }

        public static T Resolve<T>() where T : class
        {
            lock (_lockObject)
            {
                if (_services.TryGetValue(typeof(T), out var service))
                {
                    return service as T;
                }

                Debug.LogError($"[ServiceRegistry] Service not found: {typeof(T).Name}");
                throw new InvalidOperationException($"Service {typeof(T).Name} not registered.");
            }
        }

        public static bool HasService<T>() where T : class
        {
            lock (_lockObject)
            {
                return _services.ContainsKey(typeof(T));
            }
        }

        public static void Unregister<T>() where T : class
        {
            lock (_lockObject)
            {
                if (_services.Remove(typeof(T)))
                {
                    Debug.Log($"[ServiceRegistry] Unregistered: {typeof(T).Name}");
                }
            }
        }

        public static void Clear()
        {
            lock (_lockObject)
            {
                _services.Clear();
                Debug.Log("[ServiceRegistry] All services cleared");
            }
        }
    }
}