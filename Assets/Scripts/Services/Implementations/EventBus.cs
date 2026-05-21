// Assets/Scripts/Services/Implementations/EventBus.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using PickMeUp.Services;

namespace PickMeUp.Services.Implementations
{
    /// <summary>
    /// Concrete implementation of IEventBus using a delegate-based pub/sub pattern.
    /// Handlers are stored by message type and invoked with error isolation.
    /// </summary>
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();
        private readonly object _lock = new object();

        public void Publish<T>(T message)
        {
            lock (_lock)
            {
                Type messageType = typeof(T);
                if (!_handlers.TryGetValue(messageType, out List<Delegate> handlers) || handlers.Count == 0)
                {
                    return;
                }

                List<Delegate> handlersCopy = new List<Delegate>(handlers);

                foreach (Delegate handler in handlersCopy)
                {
                    try
                    {
                        if (handler is Action<T> action)
                        {
                            action.Invoke(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[EventBus] Error invoking handler for {messageType.Name}: {ex.Message}");
                    }
                }
            }
        }

        public void Subscribe<T>(Action<T> handler)
        {
            if (handler == null)
            {
                Debug.LogWarning("[EventBus] Attempted to subscribe null handler");
                return;
            }

            lock (_lock)
            {
                Type messageType = typeof(T);
                if (!_handlers.ContainsKey(messageType))
                {
                    _handlers[messageType] = new List<Delegate>();
                }

                if (!_handlers[messageType].Contains(handler))
                {
                    _handlers[messageType].Add(handler);
                }
            }
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null)
            {
                return;
            }

            lock (_lock)
            {
                Type messageType = typeof(T);
                if (_handlers.TryGetValue(messageType, out List<Delegate> handlers))
                {
                    handlers.Remove(handler);
                }
            }
        }
    }
}