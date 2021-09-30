namespace Common.Errors
{
    public class UnauthorizedError : ErrorBase
    {
        public override ErrorType Type => ErrorType.Unauthorized;

        public UnauthorizedError(string? localizedMessage = null, string? internalMessage = null)
            : base(localizedMessage, internalMessage)
        {
        }
    }
}
