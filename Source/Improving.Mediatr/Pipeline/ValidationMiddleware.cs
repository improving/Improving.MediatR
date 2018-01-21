namespace Improving.MediatR.Pipeline
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentValidation;
    using FluentValidation.Results;
    using global::MediatR;

    [RelativeOrder(Stage.Validation)]
    public class ValidationMiddleware<TRequest, TResponse>
            : RequestMiddleware<TRequest, TResponse>
            where TRequest : IAsyncRequest<TResponse>
    {
        private readonly IValidator<TRequest>[]  _forRequest;
        private readonly IValidator<TResponse>[] _forResponse;

        public ValidationMiddleware(IValidator<TRequest>[]  forRequest,
                                    IValidator<TResponse>[] forResponse)
        {
            _forRequest  = forRequest;
            _forResponse = forResponse;
            Array.Sort(_forRequest, RelativeOrderAttribute.Compare);
            Array.Sort(_forResponse, RelativeOrderAttribute.Compare);
        }

        public override async Task<TResponse> Apply(TRequest request, 
            Func<TRequest, Task<TResponse>> next)
        {
            var failures = new List<ValidationFailure>();
            await Validate(request, _forRequest, failures);

            var response = await next(request);
            if (response != null)
              await Validate(response, _forResponse, failures);

            return response;
        }

        private static async Task Validate<T>(
            T item, IEnumerable<IValidator<T>> validators,
            List<ValidationFailure> failures)
        {
            var context = new ValidationContext(item);

            foreach (var validator in validators)
            {
                var result = await validator.ValidateAsync(context);
                if (result.IsValid) continue;
                failures.AddRange(result.Errors);
                if (StopOnFailureAttribute.IsDefined(validator))
                    break;
            }

            if (failures.Count > 0)
                throw new ValidationException(failures);
        }
    }
}