using System;
using Domain.Entities;

namespace Application.Abstractions.Catalogue
{
	public interface IUserRepository
	{
        Task<User> CreateAsync(User user);
    }
}

