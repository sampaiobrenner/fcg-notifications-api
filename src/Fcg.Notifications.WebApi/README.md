# Camada WebApi

Esta camada é a porta de entrada e o ponto de composição do serviço. Hospeda a Minimal API, configura autenticação, health checks e RabbitMQ, além de receber os eventos de integração publicados pelos outros microsserviços.

> **Estado atual:** os fluxos funcionais são consumidores assíncronos. Não há endpoints REST de negócio implementados; a API HTTP expõe documentação OpenAPI/Scalar e verificações de saúde.

## Arquivos principais

| Arquivo ou diretório | Responsabilidade |
|---|---|
| `Fcg.Notifications.WebApi.csproj` | Configura a aplicação web e suas dependências de autenticação, contratos compartilhados, OpenAPI, Scalar, Serilog, MassTransit e EF Design. Define o identificador de User Secrets para desenvolvimento. |
| `Program.cs` | Compõe os quatro módulos, aplica migrations configuradas, registra middlewares e mapeia OpenAPI, Scalar, health checks e endpoints. |
| `FcgNotificationsWebApiModule.cs` | Registra Problem Details, handler global, serialização de enums, usuário atual, autenticação JWT, mensageria, health checks e endpoints. |
| `appsettings.json` | Define configurações não secretas e defaults de runtime para banco, RabbitMQ, nomes de filas, JWT e Serilog. Chaves e senhas não devem ser armazenadas aqui. |
| `Properties/launchSettings.json` | Perfil local HTTP, porta `5104`, ambiente `Development` e URL inicial do Scalar. |
| `Properties/WebApiResources.resx` | Mensagens da camada, incluindo mensagens de erro HTTP e validação da configuração JWT. |

## Consumers

| Arquivo ou diretório | Responsabilidade |
|---|---|
| `Messaging/UserCreated/UserCreatedConsumer.cs` | Consome `UserCreatedEvent` e registra a simulação do e-mail de boas-vindas sem expor nome ou e-mail pessoal nos logs. |
| `Messaging/UserCreated/UserCreatedConsumerDefinition.cs` | Define o nome da fila de cadastro usando `Messaging:Queues:NotificationsUserCreated` ou a constante do pacote de contratos. |
| `Messaging/PaymentProcessed/PaymentProcessedConsumer.cs` | Consome `PaymentProcessedEvent`; registra confirmação apenas para pagamento aprovado e registra o motivo quando rejeitado. |
| `Messaging/PaymentProcessed/PaymentProcessedConsumerDefinition.cs` | Define a fila de resultado de pagamento por configuração ou pela constante do contrato. |

Os consumers apenas simulam o envio e escrevem logs estruturados; não há integração SMTP nem persistência do histórico de notificações.

## Serviços compartilhados

| Diretório/arquivo | Responsabilidade |
|---|---|
| `_Shared/Database/MigrationExtensions.cs` | Aplica migrations pendentes quando `Database:ApplyMigrationsOnStartup` está habilitado. |
| `_Shared/Endpoints/IEndpoint.cs` | Contrato para endpoints HTTP organizados em classes próprias. |
| `_Shared/Endpoints/EndpointExtensions.cs` | Descobre implementações de `IEndpoint` no assembly e as registra/mapa na aplicação. Não há implementações de negócio atualmente. |
| `_Shared/Errors/GlobalExceptionHandler.cs` | Converte exceções conhecidas em Problem Details: 400, 401, 403, 404, 409 e 422; erros inesperados retornam 500 sem expor detalhes internos. |
| `_Shared/HealthChecks/HealthCheckEndpointExtensions.cs` | Mapeia `/health/live` e `/health/ready`. O primeiro verifica apenas se o processo responde; o segundo executa verificações marcadas como `ready`. |
| `_Shared/HealthChecks/HealthCheckTags.cs` | Centraliza a tag usada nas verificações de prontidão; a conexão PostgreSQL é registrada com essa tag no módulo. |
| `_Shared/Messaging/MessagingServiceCollectionExtensions.cs` | Registra consumers, RabbitMQ, retry e middleware EF Core do MassTransit para inbox/outbox. Os retries são executados após 1, 5, 15 e 30 segundos; erros de negócio e validação não são repetidos. |
| `_Shared/Messaging/QueueNameConfiguration.cs` | Resolve o nome configurado de uma fila e usa o nome padrão do contrato se o valor estiver ausente ou em branco. |
| `_Shared/Messaging/RabbitMqOptions.cs` | Modelo de opções para host, virtual host e credenciais do RabbitMQ. |
| `_Shared/Security/AuthenticationServiceCollectionExtensions.cs` | Configura validação de issuer, audience, assinatura, validade, claims e roles do JWT. Exige chave com pelo menos 32 caracteres. |
| `_Shared/Security/AuthorizationPolicies.cs` | Centraliza a política de administrador usando o role definido nos contratos compartilhados. |
| `_Shared/Security/HttpContextCurrentUser.cs` | Adapta as claims do `HttpContext` à abstração `ICurrentUser` da Application. |
| `_Shared/Security/JwtOptions.cs` | Define opções JWT e o tamanho mínimo da chave. |

## Fluxo de inicialização

`Program.cs` registra Domain, Application, Infrastructure e WebApi. Em seguida, constrói o host, aplica migrations se habilitado, configura os middlewares HTTP e mapeia os endpoints. O host MassTransit inicia os consumers registrados no assembly.

## Pontos para evolução

- Implementar os endpoints REST somente quando houver operações HTTP próprias para o serviço.
- Se for necessário enviar e-mails reais, introduzir uma porta de saída na Application e uma implementação configurável em Infrastructure, mantendo o comportamento simulado disponível para desenvolvimento.
- Avaliar uma política de retenção para as tabelas de inbox/outbox e monitorar as filas de erro do MassTransit.
