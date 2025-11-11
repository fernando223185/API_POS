using Application.Abstractions.Messaging;
using Application.DTOs.Customer;
using Domain.Entities;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Core.CRM.Commands
{
    public class CreateCustomerCommand : IRequest<CustomerResponseDto>
    {
        public CreateCustomerRequestDto CustomerData { get; set; }
        public int CreatedByUserId { get; set; }

        public CreateCustomerCommand(CreateCustomerRequestDto customerData, int createdByUserId)
        {
            CustomerData = customerData;
            CreatedByUserId = createdByUserId;
        }
    }
}

