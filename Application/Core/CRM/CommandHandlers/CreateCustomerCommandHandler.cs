using Application.Abstractions.CRM;
using Application.Core.CRM.Commands;
using Domain.Entities;
using MediatR;
namespace Application.Core.CRM.CommandHandlers
{
	public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Customer>
	{
		private readonly ICustomerRepository _repository;

		public CreateCustomerCommandHandler(ICustomerRepository repository)
		{
			_repository = repository;
		}

		public async Task<Customer> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
		{
			var customer = new Customer
			{
				Name = request.Name,
				LastName = request.LastName,
				Phone = request.Phone,
				Email = request.Email,
				Address = request.Address,
				Code = request.Code,
				TaxId = request.TaxId,
				ZipCode = request.ZipCode,
				Commentary = request.Commentary,
				CountryId = request.CountryId,
				StateId = request.StateId,
				InteriorNumber = request.InteriorNumber,
				ExteriorNumber = request.ExteriorNumber,
				ExternalId = request.ExternalId
			};
			return await _repository.CreateAsync(customer);
		}
    }
}

