using Application.Abstractions.Messaging;
using Domain.Entities;
namespace Application.Core.Login.Commands
{
	public class LoginCommand : ICommand<Users>
	{
		public string nameUser { get; set; }
		public string pass { get; set; }
	}
}

