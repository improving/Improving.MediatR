namespace Improving.MediatR.ServiceBus
{
    using FluentValidation.Results;

    public class ValidationFailureShim
    {
        public string ErrorCode      { get; set; }

        public string PropertyName   { get; set; }

        public string ErrorMessage   { get; set; }

        public object AttemptedValue { get; set; }

        public ValidationFailureShim()
        {        
        }

        public ValidationFailureShim(ValidationFailure failure)
        {
            ErrorCode      = failure.ErrorCode;
            PropertyName   = failure.PropertyName;
            ErrorMessage   = failure.ErrorMessage;
            AttemptedValue = failure.AttemptedValue;
        }

        public static implicit operator ValidationFailure(ValidationFailureShim shim)
        {
            return new ValidationFailure(shim.PropertyName, shim.ErrorMessage, shim.AttemptedValue)
            {
                ErrorCode = shim.ErrorCode
            };
        }
    }
}
