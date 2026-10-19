# Camada Application

Esta camada define abstrações de aplicação e a composição de handlers, validações e serviços usados pelos casos de uso. Ela depende do domínio e dos contratos compartilhados, mas não conhece detalhes do ASP.NET Core nem do RabbitMQ.

> **Estado atual:** ainda não há comandos, consultas, handlers ou validadores específicos de notificações. Os arquivos presentes são blocos reutilizáveis para casos de uso futuros; os consumers atuais recebem eventos diretamente na camada WebApi.

## Arquivos

| Arquivo ou diretório | Responsabilidade |
|---|---|
| `Fcg.Notifications.Application.csproj` | Declara dependências de MediatR, FluentValidation e o projeto Domain. |
| `FcgNotificationsApplicationModule.cs` | Registra handlers MediatR, o comportamento de validação e validadores encontrados no assembly. |
| `_Shared/Behaviors/ValidationBehavior.cs` | Executa validadores FluentValidation antes do próximo handler MediatR e lança `ValidationException` quando há falhas. |
| `_Shared/Contexts/INotificationsDbContext.cs` | Abstrai consultas sem tracking e a gravação de alterações no contexto de persistência. |
| `_Shared/Exceptions/ConflictException.cs` | Indica conflito com o estado atual; a Web API mapeia para HTTP 409. |
| `_Shared/Exceptions/ForbiddenException.cs` | Indica falta de permissão; a Web API mapeia para HTTP 403. |
| `_Shared/Exceptions/NotFoundException.cs` | Formata a mensagem de recurso não encontrado usando `ApplicationResources`; a Web API mapeia para HTTP 404. |
| `_Shared/Exceptions/UnauthorizedException.cs` | Indica ausência de autenticação; a Web API mapeia para HTTP 401. |
| `_Shared/Messaging/IIntegrationEventPublisher.cs` | Abstrai a publicação de eventos de integração sem expor MassTransit aos casos de uso. |
| `_Shared/Parsers/IParser.cs` e `Parser.cs` | Define conversores explícitos entre tipos de entrada e saída, incluindo conversão de coleções. |
| `_Shared/Security/ICurrentUser.cs` | Expõe o estado e as claims necessárias do usuário autenticado sem depender de `HttpContext`. |
| `Properties/ApplicationResources.resx` | Contém mensagens reutilizadas pela camada, como o texto de recurso não encontrado. |

## Fluxo previsto para casos de uso

Um caso de uso pode ser implementado com um request/handler MediatR, um validador FluentValidation e um parser de resposta. Para acesso a dados, use a abstração `INotificationsDbContext`; para publicar eventos, use `IIntegrationEventPublisher`. A implementação concreta dessas portas fica em Infrastructure.

## Pontos para evolução

- Criar handlers e validadores somente quando houver casos de uso pertencentes a este serviço.
- Manter `ICurrentUser` e as exceções independentes do transporte HTTP; o mapeamento para status HTTP fica na WebApi.
- Evitar adicionar operações de banco ou mensageria diretamente nos handlers por meio de dependências concretas.
- Manter contratos de eventos e claims como dependência direta da WebApi, que é a camada que os consome; adicionar uma dependência à Application apenas quando um caso de uso precisar desses tipos.
