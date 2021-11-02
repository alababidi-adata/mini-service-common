namespace VH.MiniService.Common.Errors
{
    public class ResourceExhaustedError : ErrorBase
    {
        public override ErrorType Type => ErrorType.ResourceExhausted;


        public ResourceExhaustedError(string? localizedMessage = null, string? internalMessage = null)
            : base(localizedMessage, internalMessage)
        {
        }
    }
}
