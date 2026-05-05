using PB.Cartao.Application.Events;

namespace PB.Cartao.Application.Interfaces
{
    public interface ICartaoService
    {
        Task ProcessarAsync(CreditoAprovadoEvent evento);
    }
}