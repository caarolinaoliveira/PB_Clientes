# PB.Cliente — MS Cadastro de Clientes

Microsserviço responsável pelo cadastro de clientes via API REST. Ao registrar um cliente, publica o evento `ClienteCadastrado` no RabbitMQ para ser consumido pelo MS Proposta.

## Tecnologias

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core + SQL Server
- RabbitMQ.Client 6.8.1
- Swagger/OpenAPI
- Clean Architecture + DDD

## Estrutura

```
PB.Cliente/
  PB.Cliente.Domain/          # Entidades, interfaces, exceções
  PB.Cliente.Application/     # Use cases, services, DTOs, eventos
  PB.Cliente.Infrastructure/  # EF Core, repositórios, RabbitMQ
  PB.Cliente.Presentation/    # Controllers, middlewares, Program.cs
```

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

## Como rodar localmente

### 1. Subir a infraestrutura (RabbitMQ + SQL Server)

Na raiz do projeto (onde está o `docker-compose.yml`):

```bash
docker-compose up -d rabbitmq sqlserver
```

Confirma que estão rodando:

```bash
docker ps
```

### 2. Configurar o appsettings

No arquivo `PB.Cliente.Presentation/appsettings.json`, configure:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=PB_Clientes;User Id=sa;Password=Pb@123456;TrustServerCertificate=True"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "User": "guest",
    "Password": "guest"
  }
}
```

> Para usar o Azure SQL Server, substitua a connection string pela string do seu servidor.

### 3. Aplicar migrations

```bash
cd PB.Cliente.Infrastructure
dotnet ef database update --startup-project ../PB.Cliente.Presentation
```

### 4. Rodar a API

```bash
cd PB.Cliente.Presentation
dotnet run
```

A API estará disponível em `http://localhost:5260`.

### 5. Acessar o Swagger

```
http://localhost:5260/swagger
```

## Endpoints

| Método | Rota | Descrição |
|--------|------|-----------|
| POST | /api/clientes/registrar | Cadastra um novo cliente |

## Exemplo de requisição

```json
POST /api/clientes/registrar
{
  "nome": "Carolina Oliveira",
  "email": "carolina@example.com",
  "senha": "Teste123#",
  "confirmacaoSenha": "Teste123#",
  "dataNascimento": "1999-03-15",
  "cpf": "12345678901",
  "telefone": "41999999999"
}
```

## Fluxo do evento

Após o cadastro bem-sucedido, o MS Cliente publica o evento `ClienteCadastrado` na fila `cliente.cadastrado` do RabbitMQ. O MS Proposta consome esse evento para gerar a análise de crédito.

```
POST /api/clientes/registrar
        ↓
Persiste no banco
        ↓
Publica → RabbitMQ: cliente.cadastrado
```

## Decisões arquiteturais

- **Clean Architecture**: separação em Domain, Application, Infrastructure e Presentation. A camada Application não conhece detalhes de infraestrutura.
- **IMessagePublisher**: abstração na camada Application — permite trocar RabbitMQ por Azure Service Bus sem alterar regras de negócio.
- **Validação de CPF duplicado**: feita na camada Application antes de persistir, retornando `409 Conflict` caso já exista.
- **Evento rico**: o `ClienteCadastradoEvent` carrega todos os dados necessários para os serviços downstream, evitando consultas adicionais.