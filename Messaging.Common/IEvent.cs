using Messaging.Common.Enums;

namespace Messaging.Common
{
    /// <summary>
    /// A base type for messages.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public interface IEvent<out TData> where TData : class
    {
        /// <summary>
        /// Type of change which occurred.
        /// </summary>
        public EventType EventType { get; }

        /// <summary>
        /// Id of the user who initiated this change.
        /// </summary>
        public string UserId { get; }

        /// <summary>
        /// Event data.
        /// </summary>
        public TData? Data { get; }
    }
}
