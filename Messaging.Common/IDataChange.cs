using Messaging.Common.Enums;

namespace Messaging.Common
{
    /// <summary>
    /// A message indicating that system data was changed.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public interface IDataChange<out TData> where TData : class
    {
        /// <summary>
        /// The type of change which occurred.
        /// </summary>
        public DataChangeType ChangeType { get; }

        /// <summary>
        /// The id of the user who initiated this change.
        /// </summary>
        public string UserId { get; }

        /// <summary>
        /// The new version of the model.
        /// </summary>
        public TData? Data { get; }
    }
}
