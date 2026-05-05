using PB.Cliente.Application.Interfaces;
using PB.Cliente.Application.Requests.Clientes;
using PB.Cliente.Application.Responses.Clientes;
using PB.Cliente.Application.Events;
using PB.Cliente.Domain.Entities;
using PB.Cliente.Domain.Interfaces;
using PB.Cliente.Domain.Exceptions;

namespace PB.Cliente.Application.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IMessagePublisher _messagePublisher;

        public ClienteService(IClienteRepository clienteRepository, IMessagePublisher messagePublisher)
        {
            _clienteRepository = clienteRepository;
            _messagePublisher = messagePublisher;
        }

        public async Task<ClienteResponse> RegistrarCliente(RegistrarClienteRequest request)
        {
            if (await _clienteRepository.ObterPorCpfAsync(request.Cpf) != null)
            {
                throw new ConflictException("CPF já cadastrado.");
            }

            var cliente = new ClienteEntity(
                request.Nome,
                request.Email,
                request.Cpf,
                request.DataNascimento.Value,
                request.Telefone
            );

            await _clienteRepository.AdicionarAsync(cliente);

            var evento = new ClienteCadastradoEvent
            {
                ClienteId = cliente.Id,
                Nome = cliente.Nome,
                Email = cliente.Email,
                Cpf = cliente.Cpf,
                DataNascimento = cliente.DataNascimento,
                Telefone = cliente.Telefone
            };

            await _messagePublisher.PublicarAsync(evento, "cliente.cadastrado");

            return new ClienteResponse
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Email = cliente.Email,
                Cpf = cliente.Cpf,
                DataNascimento = cliente.DataNascimento,
                Telefone = cliente.Telefone,
                Status = cliente.Status.ToString()
            };
        }
    }
}