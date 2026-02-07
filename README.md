# Finance App

Aplicação de controle financeiro pessoal construída em .NET 10, com API REST e app móvel multiplataforma (MAUI + Blazor).

## Visão geral

O projeto segue uma arquitetura em camadas (Clean Architecture), separando domínio, regras de negócio, infraestrutura, API e cliente móvel.

## Estrutura da solução

```
FinanceApp/
└── src/
    ├── Finance.Api/           # API REST (ASP.NET Core, OpenAPI)
    ├── Finance.Application/   # Casos de uso e serviços de aplicação
    ├── Finance.Domain/        # Entidades e regras de domínio
    ├── Finance.Infrastructure/# Persistência (EF Core, repositórios)
    └── Finance.Mobile/        # App MAUI + Blazor (Android, iOS, Windows, Mac Catalyst)
```

### Camadas

| Projeto | Descrição |
|--------|-----------|
| **Finance.Domain** | Entidades (`Account`, `Category`, `Transaction`), enums e base comum. Sem dependências externas. |
| **Finance.Application** | Orquestração e regras de aplicação. Depende apenas do Domain. |
| **Finance.Infrastructure** | Acesso a dados (Entity Framework Core, SQLite). Depende da Application. |
| **Finance.Api** | API HTTP com OpenAPI/Swagger. Consome Application e Infrastructure. |
| **Finance.Mobile** | App .NET MAUI com Blazor Hybrid e MudBlazor. Consome Application e Infrastructure. |

## Domínio

### Entidades principais

- **Account** – Conta financeira (nome e saldo inicial).
- **Category** – Categoria para classificar transações.
- **Transaction** – Movimentação financeira (conta, categoria, valor, data, descrição e tipo: receita ou despesa).

Todas as entidades herdam de `BaseEntity` (`Id`, `CreatedAt`).

### Tecnologias

- **.NET 10**
- **ASP.NET Core** – API com OpenAPI
- **Entity Framework Core 10** + **SQLite**
- **.NET MAUI** – App móvel/desktop (Android, iOS, Windows, Mac Catalyst)
- **Blazor Hybrid** – UI no MAUI com componentes Blazor
- **MudBlazor** – Componentes UI no app móvel

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Para **Finance.Mobile**: workloads MAUI e SDKs das plataformas desejadas (Android, iOS, Windows, etc.)

## Como executar

### API

```bash
cd src/Finance.Api
dotnet run
```

Em desenvolvimento, o OpenAPI fica disponível em `/openapi/v1.json`. A API usa HTTPS; a porta padrão está em `launchSettings.json` (ex.: 5102).

### App móvel (MAUI)

```bash
cd src/Finance.Mobile
dotnet build -t:Run -f net10.0-windows10.0.19041.0   # Windows
# ou
dotnet build -t:Run -f net10.0-android               # Android
# ou
dotnet build -t:Run -f net10.0-ios                    # iOS (em Mac)
```

Ajuste o `-f` conforme a plataforma que deseja executar.

### Solução completa

```bash
dotnet build
```

## Licença

Uso conforme definido no repositório do projeto.
