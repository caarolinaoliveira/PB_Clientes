using Microsoft.AspNetCore.Mvc;
using PB.Cliente.Application.Interfaces;
using PB.Cliente.Application.Services;
using PB.Cliente.Application.Requests.Clientes;
using PB.Cliente.Application.Responses.Clientes;
using System.Net;

namespace PB.Cliente.Presentation.Controllers
{
    [Route("api/clientes")]
    public class ClienteController : MainController
    {
        private readonly IClienteService _clienteService;

        public ClienteController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        [HttpPost("registrar")]
        [ProducesResponseType(typeof(ClienteResponse), (int)HttpStatusCode.Created)]        
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RegistrarCliente(RegistrarClienteRequest request)
        {

            var response = await _clienteService.RegistrarCliente(request);

            return CreatedAtAction(nameof(RegistrarCliente), new { id = response.Id }, response);
        }
    }
    
}