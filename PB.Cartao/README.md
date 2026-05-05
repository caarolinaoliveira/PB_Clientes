# PB.Cartao — MS Emissão de Cartões

Microsserviço responsável pela emissão de cartões de crédito. Consome o evento `CreditoAprovado` do RabbitMQ, emite 1 ou 2 cartões conforme o score da proposta e notifica o cliente por e-mail.

## Tecnologias

- .NET 8
- Worker Service (BackgroundService)
- Entity Framework Core + SQL Server
- RabbitMQ.Client 6.8.1
- Clean Architecture + DDD

## Estrutura

```
PB.Cartao/
  PB.Cartao.Domain/          # Entidades, interfaces
  PB.Cartao.Application/     # Services, interfaces, eventos
  PB.Cartao.Infrastructure/  # EF Core, repositórios, RabbitMQ, e-mail
  Program.cs                 # Entry point, DI, migration automática
```

## Regras de emissão

| QuantidadeCartoes (do evento) | Cartões emitidos | Limite cada |
|-------------------------------|------------------|-------------|
| 1 | Cartão #1 | R$ 1.000,00 |
| 2 | Cartão #1 e Cartão #2 | R$ 5.000,00 |

> O número do cartão é gerado de forma aleatória no formato `0000 0000 0000 0000`. Em produção, viria de uma integradora.

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- MS Cliente e MS Proposta rodando

## Como rodar localmente

### 1. Subir a infraestrutura (RabbitMQ + SQL Server)

Na raiz do projeto (onde está o `docker-compose.yml`):

```bash
docker-compose up -d rabbitmq sqlserver
```

### 2. Configurar o appsettings

No arquivo `PB.Cartao/appsettings.json`, configure:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=PB_Cartoes;User Id=sa;Password=Pb@123456;TrustServerCertificate=True"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "User": "guest",
    "Password": "guest"
  }
}
```

> Para usar o Azure SQL Server, substitua o `Initial Catalog` para `PB_Cartoes` e ajuste as credenciais.

### 3. Criar e aplicar migrations

```bash
cd PB.Cartao
dotnet ef migrations add InitialCreate --project PB.Cartao.Infrastructure --startup-project .
dotnet ef database update --project PB.Cartao.Infrastructure --startup-project .
```

> A migration também é aplicada automaticamente na inicialização via `MigrateAsync()`.

### 4. Rodar o Worker

```bash
cd PB.Cartao
dotnet run
```

O Worker ficará aguardando mensagens na fila `credito.aprovado`:

```
[CONSUMER] Aguardando mensagens na fila credito.aprovado...
```

## Fluxo do evento

```
RabbitMQ: credito.aprovado
        ↓
Verifica idempotência (cartões já emitidos para o ClienteId?)
        ↓
Loop: emite cartão #1 (e #2 se QuantidadeCartoes = 2)
        ↓
Persiste no banco
        ↓
[EMAIL] Cartão #N emitido para cliente
        ↓
Publica → RabbitMQ: cartao.emitido
```

## Fluxo completo dos 3 microsserviços

```
[MS Cliente] POST /api/clientes/registrar
        ↓ publica
RabbitMQ: cliente.cadastrado
        ↓ consome
[MS Proposta] calcula score → aprova/nega
        ↓ publica (se aprovado)
RabbitMQ: credito.aprovado
        ↓ consome
[MS Cartão] emite 1 ou 2 cartões → notifica por e-mail
        ↓ publica
RabbitMQ: cartao.emitido
```

