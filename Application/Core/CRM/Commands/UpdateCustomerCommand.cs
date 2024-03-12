using MediatR;
using Domain.Entities;
using System.ComponentModel.DataAnnotations;
namespace Application.Core.CRM.CommandHandlers
{
	public class UpdateCustomerCommand : IRequest<Customer>
	{
		[Required]
		public int ID { get; set; }
		public string Code { get; set; }
		public string Name { get; set; }
		public string LastName { get; set; }
		public string Phone { get; set; }
		public string Email { get; set; }
		public string Address { get; set; }
		public string TaxId { get; set; }
		public string ZipCode { get; set; }
		public string Commentary { get; set; }
		public int CountryId { get; set; }
		public int StateId { get; set; }
		public string Interiornumber { get; set; }
		public string ExteriorNumber { get; set; }
	}
}

