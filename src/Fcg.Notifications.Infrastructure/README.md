# Camada Infrastructure

Esta camada implementa as portas de persistência e mensageria declaradas pela Application. Usa PostgreSQL por meio do Entity Framework Core e MassTransit para as tabelas de outbox e inbox.

> **Estado atual:** não existem entidades próprias de notificação persistidas. O schema atual contém as tabelas usadas pelo outbox/inbox do MassTransit.

## Arquivos

| Arquivo ou diretório | Responsabilidade |
|---|---|
| `Fcg.Notifications.Infrastructure.csproj` | Declara EF Core relacional, Npgsql, convenção snake_case, integração EF do MassTransit e referência à Application. |
| `FcgNotificationsInfrastructureModule.cs` | Lê `ConnectionStrings:Default`, configura o DbContext PostgreSQL e registra as implementações das abstrações de persistência e publicação. |
| `_Shared/Context/NotificationsDbContext.cs` | Implementa `INotificationsDbContext`, aplica configurações de entidades do assembly e inclui entidades de inbox/outbox do MassTransit. Consultas por `DataSet<T>()` usam `AsNoTracking()`. |
| `_Shared/Context/NotificationsDbContextDesignTimeFactory.cs` | Cria o DbContext para ferramentas EF em design time. Lê `ConnectionStrings__Default` ou usa uma conexão de desenvolvimento local como fallback. |
| `_Shared/Context/Migrations/20260922225345_InitialCreate.cs` | Migration inicial que cria as tabelas `inbox_state`, `outbox_message` e `outbox_state`. |
| `_Shared/Context/Migrations/20260922225345_InitialCreate.Designer.cs` | Metadados gerados pelo EF Core para a migration inicial. |
| `_Shared/Context/Migrations/NotificationsDbContextModelSnapshot.cs` | Snapshot gerado do modelo EF, usado para detectar alterações ao criar migrations. |
| `_Shared/Messaging/MassTransitIntegrationEventPublisher.cs` | Implementa `IIntegrationEventPublisher` delegando a publicação a `IPublishEndpoint`. |
| `Properties/InfrastructureResources.resx` | Contém mensagens da camada, incluindo o erro de connection string ausente. |

## Configuração e persistência

- A aplicação exige `ConnectionStrings:Default` para registrar o banco. A connection string do ambiente local está documentada no README principal.
- Os nomes das tabelas seguem snake_case.
- O contexto aplica as configurações de entidades presentes no assembly e registra o schema do MassTransit para inbox/outbox.
- O módulo ajusta `GssEncryptionMode` para `Disable` ao construir a connection string Npgsql. Revise essa configuração junto com os requisitos do ambiente antes de alterá-la.

## Migrations

As migrations e o snapshot são arquivos gerados pelo EF Core. Não edite os comentários, atributos ou metadados gerados manualmente. Depois de alterar o modelo, crie uma nova migration a partir da raiz do repositório:

```bash
dotnet tool restore
dotnet ef migrations add <NomeDaMigration> \
  -p src/Fcg.Notifications.Infrastructure \
  -s src/Fcg.Notifications.WebApi \
  -o _Shared/Context/Migrations
```

As migrations pendentes são aplicadas pela WebApi no início da aplicação quando `Database:ApplyMigrationsOnStartup` está habilitado.
