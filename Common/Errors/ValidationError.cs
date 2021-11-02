namespace VH.MiniService.Common.Errors
{
    public class ValidationError : ErrorBase
    {
        public override ErrorType Type => ErrorType.Validation;
        protected internal override string DefaultLocalizedMessage => "Occurs when an application request does not pass validation";

        public ValidationError(string? localizedMessage = null, string? internalMessage = null)
            : base(localizedMessage, internalMessage)
        {
        }
    }
}
