using Application.Abstractions.Messaging;
using Domain.Entities;
namespace Application.Core.Login.Commands
{
	public class LoginCommand : ICommand<User>
	{
		public string Code { get; set; }
		public string pass { get; set; }
	}
}

