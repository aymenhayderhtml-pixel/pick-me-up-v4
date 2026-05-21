// Assets/Scripts/Services/IEventBus.cs
using System;

namespace PickMeUp.Services
{
    /// <summary>
    /// A centralized publish-subscribe messaging system to decouple game systems and UI.
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Publishes a message to all subscribed handlers.
        /// </summary>
        void Publish<T>(T message);

        /// <summary>
        /// Subscribes a handler to receive messages of a specific type.
        /// </summary>
        void Subscribe<T>(Action<T> handler);

        /// <summary>
        /// Unsubscribes a handler from receiving messages of a specific type.
        /// </summary>
        void Unsubscribe<T>(Action<T> handler);
    }
}