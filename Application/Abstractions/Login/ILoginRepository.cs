using Domain.DTOs;
using Application.Core.Login.Commands;
using Domain.Entities;


namespace Application.Abstractions.Login
{
	public interface ILoginRepository 
	{
		Task<Users> Login(Users data);
	}
}

