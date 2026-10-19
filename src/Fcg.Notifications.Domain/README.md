# Camada Domain

Esta camada reúne os tipos básicos do domínio e os contratos usados para representar modelos de domínio, eventos e módulos de composição. Ela não depende do Entity Framework, do RabbitMQ nem do ASP.NET Core.

> **Estado atual:** a camada contém infraestrutura conceitual reutilizável, mas ainda não possui agregados ou regras específicas do domínio de notificações. O módulo de DI está vazio.

## Arquivos

| Arquivo ou diretório | Responsabilidade |
|---|---|
| `Fcg.Notifications.Domain.csproj` | Define o projeto e suas dependências de abstrações de configuração e injeção de dependência. |
| `FcgNotificationsDomainModule.cs` | Ponto de composição de serviços do domínio. Atualmente não registra serviços. |
| `_Shared/Events/IDomainEvent.cs` | Marcador para eventos internos de domínio. Não representa os eventos de integração RabbitMQ. |
| `_Shared/Exceptions/BusinessException.cs` | Exceção base para violações de regras de negócio. A Web API a converte em HTTP 422. |
| `_Shared/Models/IAggregateRoot.cs` | Marcador para raízes de agregado. |
| `_Shared/Models/PersistenceModelBase.cs` | Base para modelos persistidos; inicializa o identificador com `Guid.CreateVersion7()`. |
| `_Shared/Models/DomainModelBase.cs` | Base para modelos de domínio que envolvem um modelo persistido e mantêm uma coleção de eventos de domínio. |
| `_Shared/Modules/IModule.cs` | Contrato comum de registro de dependências por camada. |
| `_Shared/Modules/ModuleServiceCollectionExtensions.cs` | Extensão que instancia e executa o registro de um módulo no `IServiceCollection`. |
| `Properties/DomainResources.resx` | Recursos localizados da camada, incluindo a mensagem genérica de campo obrigatório. |

## Convenções

- Regras de negócio e agregados pertencem aqui quando forem introduzidos; dependências de infraestrutura não devem ser adicionadas a esta camada.
- `IDomainEvent` é reservado a eventos internos do domínio. Eventos publicados entre microsserviços pertencem aos contratos compartilhados.
- `DomainModelBase<TPersistenceModel>` oferece uma lista explícita de eventos, que deve ser limpa depois que a aplicação tratar esses eventos.

## Pontos para evolução

Quando surgirem regras específicas de notificações, definir agregados e eventos de domínio concretos antes de acrescentar abstrações genéricas. Se essa camada continuar sem comportamento próprio, reavaliar a necessidade do módulo vazio e das bases que ainda não são usadas.
