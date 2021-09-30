using Common.Extensions;

namespace Common.Errors
{
    public class NotFoundError : ErrorBase
    {
        public override ErrorType Type => ErrorType.NotFound;
        protected internal override string DefaultLocalizedMessage => "Item not found";

        public NotFoundError(string? localizedMessage = null, string? internalMessage = null)
            : base(localizedMessage, internalMessage)
        {
        }
    }

    public class NotFoundError<TType, THandler> : ErrorBase
    {
        public override ErrorType Type => ErrorType.NotFound;
        protected internal override string DefaultLocalizedMessage => $"{typeof(TType).Name.CamelToSentenceCase()} not found";

        public NotFoundError(string? localizedMessage = null, string? internalMessage = null)
            : base(localizedMessage, internalMessage)
        {
        }
    }
}
