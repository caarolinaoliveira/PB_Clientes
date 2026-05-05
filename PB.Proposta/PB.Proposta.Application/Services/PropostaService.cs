using PB.Proposta.Application.Events;
using PB.Proposta.Application.Interfaces;
using PB.Proposta.Domain.Entities;
using PB.Proposta.Domain.Interfaces;

namespace PB.Proposta.Application.Services
{
    public class PropostaService : IPropostaService
    {
        private readonly IPropostaRepository _propostaRepository;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IEmailService _emailService;

        public PropostaService(
            IPropostaRepository propostaRepository,
            IMessagePublisher messagePublisher,
            IEmailService emailService)
        {
            _propostaRepository = propostaRepository;
            _messagePublisher = messagePublisher;
            _emailService = emailService;
        }

        public async Task ProcessarAsync(ClienteCadastradoEvent evento)
        {
            var propostaExistente = await _propostaRepository
                .ObterPorClienteIdAsync(evento.ClienteId);

            if (propostaExistente != null)
                return;

            var score = GerarScore();
            var proposta = new PropostaEntity(evento.ClienteId, score);

            await _propostaRepository.AdicionarAsync(proposta);

            if (!proposta.IsAprovada())
            {
                await _emailService.EnviarPropostaNegadaAsync(
                    evento.Email, evento.Nome);
                return;
            }

            await _emailService.EnviarPropostaAprovadaAsync(
                evento.Email,
                evento.Nome,
                proposta.LimiteAprovado,
                proposta.QuantidadeCartoes);

            var creditoAprovado = new CreditoAprovadoEvent
            {
                PropostaId = proposta.Id,
                ClienteId = proposta.ClienteId,
                Email = evento.Email,
                Nome = evento.Nome,
                LimiteAprovado = proposta.LimiteAprovado,
                QuantidadeCartoes = proposta.QuantidadeCartoes,
                OcorridoEm = DateTime.UtcNow
            };

            await _messagePublisher.PublicarAsync(creditoAprovado, "credito.aprovado");
        }

        private int GerarScore()
        {
            return new Random().Next(0, 1001);
        }
    }
}