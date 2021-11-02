using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VH.MiniService.Common.Errors;
using FluentResults;
using FluentValidation;
using MediatR;

namespace VH.MiniService.Common.Application.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : ResultBase, new()
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken ct, RequestHandlerDelegate<TResponse> next)
        {
            if (!_validators.Any()) return await next();

            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, ct)));

            var errors = validationResults
                .Where(o => !o.IsValid)
                .SelectMany(o => o.Errors)
                .Select(o => new ValidationError(o.ErrorMessage))
                .ToList();

            if (errors.Count == 0)
                return await next();

            var result = new TResponse();
            var rootError = new ValidationError();
            rootError.Reasons.AddRange(errors);
            result.Reasons.Add(rootError);
            return result;
        }
    }
}
