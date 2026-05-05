using PB.Proposta.Application.Events;

namespace PB.Proposta.Application.Interfaces
{
    public interface IPropostaService
    {
        Task ProcessarAsync(ClienteCadastradoEvent evento);
    }
}