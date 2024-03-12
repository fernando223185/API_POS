using Application.Abstractions.Messaging;
using Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Application.Core.CRM.Commands
{
	public class CreateCustomerCommand : ICommand<Customer>
	{
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
        public DateTime Created_at { get; set; } = DateTime.Now;
        public string InteriorNumber { get; set; }
        public string ExteriorNumber { get; set; }
        public int ExternalId { get; set; }
        public int StatusId { get; set; }
    }
}

