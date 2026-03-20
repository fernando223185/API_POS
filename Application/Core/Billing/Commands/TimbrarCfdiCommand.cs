using Application.DTOs.Billing;
using MediatR;

namespace Application.Core.Billing.Commands
{
    /// <summary>
    /// Comando para timbrar un CFDI a través del PAC Sapiens
    /// </summary>
    public class TimbrarCfdiCommand : IRequest<TimbrarCfdiResponseDto>
    {
        public TimbrarCfdiRequestDto RequestData { get; set; }
        public int UserId { get; set; }

        public TimbrarCfdiCommand(TimbrarCfdiRequestDto requestData, int userId)
        {
            RequestData = requestData;
            UserId = userId;
        }
    }
}
