# PB.Proposta — MS Análise de Crédito

Microsserviço responsável pela análise de crédito. Consome o evento `ClienteCadastrado` do RabbitMQ, calcula o score, aplica as regras de negócio e publica o evento `CreditoAprovado` caso a proposta seja aprovada.

## Tecnologias

- .NET 8
- Worker Service (BackgroundService)
- Entity Framework Core + SQL Server
- RabbitMQ.Client 6.8.1
- Clean Architecture + DDD

## Estrutura

```
PB.Proposta/
  PB.Proposta.Domain/          # Entidades, interfaces, enums
  PB.Proposta.Application/     # Services, interfaces, eventos
  PB.Proposta.Infrastructure/  # EF Core, repositórios, RabbitMQ, e-mail
  Program.cs                   # Entry point, DI, migration automática
```

## Regras de score

| Score | Resultado | Limite | Cartões |
|-------|-----------|--------|---------|
| 0 – 100 | Negado | — | 0 |
| 101 – 500 | Aprovado | R$ 1.000,00 | 1 |
| 501 – 1000 | Aprovado | R$ 5.000,00 | 2 |

> O score é gerado de forma aleatória simulando um bureau de crédito. Em produção, viria de uma integração com Serasa/SPC.

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- MS Cliente rodando e publicando eventos

## Como rodar localmente

### 1. Subir a infraestrutura (RabbitMQ + SQL Server)

Na raiz do projeto (onde está o `docker-compose.yml`):

```bash
docker-compose up -d rabbitmq sqlserver
```

### 2. Configurar o appsettings

No arquivo `PB.Proposta/appsettings.json`, configure:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=PB_Propostas;User Id=sa;Password=Pb@123456;TrustServerCertificate=True"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "User": "guest",
    "Password": "guest"
  }
}
```

> Para usar o Azure SQL Server, substitua o `Initial Catalog` para `PB_Propostas` e ajuste as credenciais.

### 3. Criar e aplicar migrations

```bash
cd PB.Proposta
dotnet ef migrations add InitialCreate --project PB.Proposta.Infrastructure --startup-project .
dotnet ef database update --project PB.Proposta.Infrastructure --startup-project .
```

> A migration também é aplicada automaticamente na inicialização via `MigrateAsync()`.

### 4. Rodar o Worker

```bash
cd PB.Proposta
dotnet run
```

O Worker ficará aguardando mensagens na fila `cliente.cadastrado`:

```
[CONSUMER] Aguardando mensagens na fila cliente.cadastrado...
```

## Fluxo do evento

```
RabbitMQ: cliente.cadastrado
        ↓
Verifica idempotência (ClienteId já processado?)
        ↓
Calcula score (0–1000)
        ↓
Cria PropostaEntity (regras aplicadas no Domain)
        ↓
Persiste no banco
        ↓
Negada → [EMAIL] Proposta negada → fim
        ↓
Aprovada → [EMAIL] Proposta aprovada
        ↓
Publica → RabbitMQ: credito.aprovado
```

