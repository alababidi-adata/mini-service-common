using System;
using System.Collections.Generic;
using System.Net;
using Common.Extensions;
using FluentResults;

namespace Common.Errors
{
    public abstract class ErrorBase : Error
    {
        protected ErrorBase(string? localizedMessage, string? internalMessage)
        {
            Message = localizedMessage ?? DefaultLocalizedMessage;
            InternalMessage = $"{DefaultInternalMessage}: {internalMessage}";
        }

        /// <summary>
        /// Error type
        /// </summary>
        public abstract ErrorType Type { get; }

        /// <summary>
        /// User friendly message which is appropriate to display in the UI.
        /// </summary>
        public string LocalizedMessage => Message;
        protected internal virtual string DefaultLocalizedMessage => $"{Enum.GetName(Type).CamelToSentenceCase()} error";

        /// <summary>
        /// Developer friendly description
        /// </summary>
        public string InternalMessage { get; }
        protected internal virtual string DefaultInternalMessage => $"{Enum.GetName(Type)} error";

        /// <summary>
        /// The route cause of an error
        /// </summary>
        public List<Error> Reasons { get; } = new(0);

        public static implicit operator Result(ErrorBase errorBase) => Result.Fail(errorBase);
    }

    public abstract class ErrorBase<TEntity, TWhere> : ErrorBase
    {
        protected internal override string DefaultLocalizedMessage => $"{Enum.GetName(Type).CamelToSentenceCase()} error for {typeof(TEntity).Name.CamelToSentenceCase(false)}";
        protected internal override string DefaultInternalMessage => $"{Enum.GetName(Type)} for {typeof(TEntity).Name} at {typeof(TWhere).Name}";

        protected ErrorBase(string? localizedMessage = null, string? internalMessage = null)
            : base(localizedMessage, internalMessage)
        {
        }
    }
}
