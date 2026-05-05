using PB.Cartao.Application.Events;
using PB.Cartao.Application.Interfaces;
using PB.Cartao.Domain.Entities;
using PB.Cartao.Domain.Interfaces;

namespace PB.Cartao.Application.Services
{
    public class CartaoService : ICartaoService
    {
        private readonly ICartaoRepository _cartaoRepository;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IEmailService _emailService;

        public CartaoService(
            ICartaoRepository cartaoRepository,
            IMessagePublisher messagePublisher,
            IEmailService emailService)
        {
            _cartaoRepository = cartaoRepository;
            _messagePublisher = messagePublisher;
            _emailService = emailService;
        }

        public async Task ProcessarAsync(CreditoAprovadoEvent evento)
        {
            var cartoesExistentes = await _cartaoRepository
                .ObterPorClienteIdAsync(evento.ClienteId);

            if (cartoesExistentes.Count >= evento.QuantidadeCartoes)
                return;

            for (int i = cartoesExistentes.Count + 1; i <= evento.QuantidadeCartoes; i++)
            {
                var cartao = new CartaoEntity(
                    evento.ClienteId,
                    evento.PropostaId,
                    evento.LimiteAprovado,
                    sequencial: i
                );

                await _cartaoRepository.AdicionarAsync(cartao);

                await _emailService.EnviarCartaoEmitidoAsync(
                    evento.Email,
                    evento.Nome,
                    numeroCartao: i,
                    cartao.Limite
                );

                var cartaoEmitido = new CartaoEmitidoEvent
                {
                    CartaoId = cartao.Id,
                    ClienteId = cartao.ClienteId,
                    PropostaId = cartao.PropostaId,
                    Email = evento.Email,
                    Nome = evento.Nome,
                    Limite = cartao.Limite,
                    Numero = i,
                    OcorridoEm = DateTime.UtcNow
                };

                await _messagePublisher.PublicarAsync(cartaoEmitido, "cartao.emitido");
            }
        }
    }
}