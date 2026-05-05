using PB.Cliente.Domain.Exceptions;

namespace PB.Cliente.Domain.ValueObjects
{
    public class Endereco
    {
        public string Logradouro { get; }
        public string Numero { get; }
        public string Bairro { get; }
        public string Cidade { get; }
        public string Estado { get; }
        public string Cep { get; }
        public string Complemento { get; }

        protected Endereco() { }

        public Endereco(
            string logradouro,
            string numero,
            string bairro,
            string cidade,
            string estado,
            string cep,
            string complemento = null)
        {
            if (string.IsNullOrWhiteSpace(logradouro))
                throw new DomainException("Logradouro é obrigatório.");

            if (string.IsNullOrWhiteSpace(cidade))
                throw new DomainException("Cidade é obrigatória.");

            if (string.IsNullOrWhiteSpace(estado))
                throw new DomainException("Estado é obrigatório.");

            if (string.IsNullOrWhiteSpace(cep))
                throw new DomainException("CEP é obrigatório.");

            Logradouro = logradouro;
            Numero = numero;
            Bairro = bairro;
            Cidade = cidade;
            Estado = estado;
            Cep = cep;
            Complemento = complemento;
        }
    }
}