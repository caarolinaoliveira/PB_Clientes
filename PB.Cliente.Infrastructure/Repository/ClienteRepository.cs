using System.Linq.Expressions;
using PB.Cliente.Domain.Entities;
using PB.Cliente.Domain.Interfaces;
using PB.Cliente.Infrastructure.Context;

namespace PB.Cliente.Infrastructure.Repository
{
    public class ClienteRepository : Repository<ClienteEntity>, IClienteRepository
    {
        public ClienteRepository(ClienteDbContext context) : base(context)
        {
        }
    }
}