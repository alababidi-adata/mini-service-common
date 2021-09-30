namespace Common.Errors
{
    public class UnknownError : ErrorBase
    {
        public override ErrorType Type => ErrorType.Unknown;
        protected internal override string DefaultLocalizedMessage => "Unknown system error";

        public UnknownError(string? localizedMessage = null, string? internalMessage = null)
            : base(localizedMessage, internalMessage)
        {
        }
    }
}
