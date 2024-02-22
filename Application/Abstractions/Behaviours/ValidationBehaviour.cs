using Application.Abstractions.Messaging;
using Application.Exceptions;
using FluentValidation;
using MediatR;

namespace Application.Abstractions.Behaviours
{
    public class ValidationBehaviour<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : ICommandBase
    {

        private readonly IEnumerable<IValidator<TRequest>> _validators;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationBehaviour{TRequest,TResponse}"/> class.
        /// </summary>
        /// <param name="validators">The validator for the current request type.</param>
        public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

        /// <inheritdoc />       
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var context = new ValidationContext<TRequest>(request);

            var validationFailures = await Task.WhenAll(
                _validators.Select(validator => validator.ValidateAsync(context)));

            var erros = validationFailures
                .Where(validationResult => !validationResult.IsValid)
                .SelectMany(validationResult => validationResult.Errors)
                .Select(validationFailure => new ValidationError(
                    validationFailure.PropertyName,
                    validationFailure.ErrorMessage))
                .ToList();

            if (erros.Any())
            {
                throw new Exceptions.ValidationException(erros);
            }

            var response = await next();
            return response;
        }

    }
}
