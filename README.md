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
| **Finance.Mobile** | App .NET MAUI com Blazor Hybrid e MudBlazor. **Consome a API REST via HTTP** (não usa banco local). |

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

### Erro XA5300 / XA0034 (Android SDK ou Java não encontrado)

Se ao compilar para Android aparecer *"Não foi possível encontrar o diretório do SDK do Android"* ou *"Falha ao obter a versão do SDK do Java"*:

1. **Opção A – Instalar Android SDK e Java pelo .NET** (sem Android Studio):

   É obrigatório informar **Android SDK** e **JDK (Java 11+)**. Use caminhos sem espaços (ex.: `C:\android-sdk`, `C:\jdk`).

   ```powershell
   cd src/Finance.Mobile
   dotnet build -t:InstallAndroidDependencies -f net10.0-android `
     -p:AndroidSdkDirectory=C:\android-sdk `
     -p:JavaSdkDirectory=C:\jdk `
     -p:AcceptAndroidSdkLicenses=True
   ```

   Depois defina as variáveis de ambiente (ou use “Variáveis de Ambiente” no Windows):
   - `ANDROID_HOME` = `C:\android-sdk`
   - `JAVA_HOME` = `C:\jdk`

2. **Opção B – Instalar o Android Studio**  
   Instale o [Android Studio](https://developer.android.com/studio); o SDK será instalado em `%LOCALAPPDATA%\Android\Sdk` e o Java já vem incluído. O projeto já está configurado para usar esse caminho no Windows.

## Como executar

### Banco de dados e Migrations

O banco SQLite é criado automaticamente na primeira execução da API através de migrations do Entity Framework Core.

Para criar uma nova migration:
```bash
cd src/Finance.Api
dotnet ef migrations add NomeDaMigration --project ../Finance.Infrastructure/Finance.Infrastructure.csproj --startup-project Finance.Api.csproj --context AppDbContext
```

Para aplicar migrations:
```bash
dotnet ef database update --project ../Finance.Infrastructure/Finance.Infrastructure.csproj --startup-project Finance.Api.csproj --context AppDbContext
```

As migrations são aplicadas automaticamente na inicialização da API.

### API

```bash
cd src/Finance.Api
dotnet run
```

A API estará disponível em `http://localhost:5102` (porta configurada em `launchSettings.json`).

**Endpoints disponíveis:**
- `POST /transactions` - Criar nova transação
- `GET /transactions` - Listar todas as transações
- `GET /accounts` - Listar todas as contas
- `GET /categories` - Listar todas as categorias

**Importante:** A API está configurada com CORS para permitir requisições do app mobile. Em produção, configure CORS adequadamente.

### App móvel (MAUI)

**⚠️ Importante:** O app mobile **requer que a API esteja rodando** antes de iniciar, pois ele consome os endpoints via HTTP.

1. **Inicie a API primeiro** (veja seção acima)
2. **Configure a URL da API** se necessário em `src/Finance.Mobile/Services/ApiConfiguration.cs`
   - Para Android Emulator: `http://10.0.2.2:5102`
   - Para iOS Simulator/Windows/Mac: `http://localhost:5102`
3. **Execute o app:**

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
