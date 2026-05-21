using System;
using System.Collections.Generic;

namespace PickMeUp.Core
{
    public static class ServiceRegistry
    {
        private static readonly Dictionary<Type, object> Services = new();
        private static readonly object LockObject = new();

        public static void Register<T>(T service) where T : class
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            lock (LockObject)
            {
                Services[typeof(T)] = service;
            }
        }

        public static T Resolve<T>() where T : class
        {
            lock (LockObject)
            {
                if (Services.TryGetValue(typeof(T), out var service))
                    return service as T;

                throw new InvalidOperationException($"Service {typeof(T).Name} not registered.");
            }
        }

        public static void Clear()
        {
            lock (LockObject)
            {
                Services.Clear();
            }
        }
    }
}