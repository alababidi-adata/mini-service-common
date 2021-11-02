namespace VH.MiniService.Common.Errors
{
    public class ConflictError : ErrorBase
    {
        public override ErrorType Type => ErrorType.Conflict;
        protected internal override string DefaultLocalizedMessage => "Item already exist or given entry already modified";

        public ConflictError(string? localizedMessage = null, string? internalMessage = null)
            : base(localizedMessage, internalMessage)
        {
        }
    }

    public class ConflictError<TEntity, TWhere> : ErrorBase<TEntity, TWhere>
    {
        public override ErrorType Type => ErrorType.Conflict;
        protected internal override string DefaultLocalizedMessage => "Item already exist or given entry already modified";

        public ConflictError(string? localizedMessage = null, string? internalMessage = null)
            : base(localizedMessage, internalMessage)
        {
        }
    }
}
