using MediatR;
using Domain.Entities;
using Application.Abstractions.CRM;

namespace Application.Core.CRM.CommandHandlers
{
	public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Customer>
	{
		private readonly ICustomerRepository _repository;

		public UpdateCustomerCommandHandler(ICustomerRepository repository)
		{
			_repository = repository;
		}

        public async Task<Customer> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
			var Customer = new Customer
			{
				ID = request.ID,
				Code = request.Code,
				Name = request.Name,
				LastName = request.LastName,
				Phone = request.Phone,
				Email = request.Email,
				Address = request.Address,
				TaxId = request.TaxId,
				ZipCode = request.ZipCode,
				Commentary = request.Commentary,
				CountryId = request.CountryId,
				StateId = request.StateId,
				InteriorNumber = request.Interiornumber,
				ExteriorNumber = request.ExteriorNumber
			};

			return await _repository.UpdateAsync(Customer);
        }
    }
}

