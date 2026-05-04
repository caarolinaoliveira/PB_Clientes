using PB.Cliente.Application.Interfaces;
using PB.Cliente.Application.Requests.Clientes;
using PB.Cliente.Application.Responses.Clientes;
using PB.Cliente.Domain.Entities;
using PB.Cliente.Domain.Interfaces;
using PB.Cliente.Domain.Exceptions;

namespace PB.Cliente.Application.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepository;

        public ClienteService(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
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