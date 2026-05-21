using System;
using System.Collections.Generic;

namespace PickMeUp.Services.Implementations
{
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers = new();

        public void Subscribe<T>(Action<T> handler) where T : class
        {
            if (handler == null) return;

            var type = typeof(T);
            if (!_subscribers.ContainsKey(type))
                _subscribers[type] = new List<Delegate>();

            _subscribers[type].Add(handler);
        }

        public void Publish<T>(T eventData) where T : class
        {
            var type = typeof(T);
            if (!_subscribers.ContainsKey(type)) return;

            foreach (var handler in _subscribers[type])
            {
                try
                {
                    ((Action<T>)handler)?.Invoke(eventData);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"Error in event handler: {ex.Message}");
                }
            }
        }
    }
}