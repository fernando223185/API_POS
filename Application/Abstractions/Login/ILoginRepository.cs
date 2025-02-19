using Domain.DTOs;
using Application.Core.Login.Commands;
using Domain.Entities;


namespace Application.Abstractions.Login
{
	public interface ILoginRepository 
	{
		Task<User> Login(User data);
	}
}

