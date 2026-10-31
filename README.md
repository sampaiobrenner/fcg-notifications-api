# fcg-notifications-api

Microsservico de **Notificacoes** da plataforma **FIAP Cloud Games (FCG) - Fase 2**. Envia (simula, gravando log estruturado no console) o e-mail de **boas-vindas** quando um usuario e cadastrado e o de **confirmacao de compra** quando um pagamento e aprovado. Nao expoe endpoints de negocio: toda a entrada e assincrona, via RabbitMQ.

Os contratos de integracao (eventos, filas, JWT, convencoes) sao definidos no repositorio [`fcg-contracts`](https://github.com/sampaiobrenner/fcg-contracts) (pacote `PosTech.Fiap.CloudGames.Contracts`) e a orquestracao (docker-compose e guia de Kubernetes) no [`fcg-orchestration`](https://github.com/sampaiobrenner/fcg-orchestration).

## Eventos

### Consumidos

| Evento | Publicador | Fila | Variavel (ConfigMap) | O que faz |
|---|---|---|---|---|
| `UserCreatedEvent` | UsersAPI | `notifications.user-created` | `Messaging__Queues__NotificationsUserCreated` | Upsert do usuario na projecao `known_users` e e-mail de boas-vindas (uma vez por `EventId`) |
| `PaymentProcessedEvent` | PaymentsAPI | `notifications.payment-processed` | `Messaging__Queues__NotificationsPaymentProcessed` | `Approved`: e-mail de confirmacao de compra (uma vez por `EventId`). `Rejected`: apenas log informativo |

O nome de cada fila vem da configuracao `Messaging:Queues:<Fila>` (ConfigMap); se ausente, usa a constante de `Fcg.Contracts.Messaging.QueueNames` (`ConsumerDefinition.EndpointName`).

### Publicados

Nenhum.

### Regras de consumo

- **Projecao local de usuarios:** `PaymentProcessedEvent` nao traz nome/e-mail, entao o servico mantem `known_users` alimentada pelo `UserCreatedEvent` (sem chamada sincrona a UsersAPI). O mesmo `UserId` com dados novos atualiza a projecao.
- **Idempotencia:** inbox do MassTransit (por `MessageId`) + tabela `email_log` com `event_id` unico: um `EventId` ja registrado nunca gera outro e-mail.
- **Usuario ausente:** se o `PaymentProcessedEvent` (`Approved`) chega antes do `UserCreatedEvent`, o handler lanca `UserNotKnownYetException` (nao e `BusinessException`), que entra no retry 1s/5s/15s/30s; quando o usuario chega na projecao, o e-mail e enviado. Esgotado o retry, a mensagem vai para `notifications.payment-processed_error`.
- **Erros de validacao** (`ValidationException`) e de regra (`BusinessException`) nao sao retentados: vao direto para a fila `<fila>_error`.

### Formato do log de e-mail

O envio e simulado pelo `ConsoleEmailSender` (`IEmailSender`), com log estruturado Serilog (propriedades `To`, `Subject` e `Body`):

```
E-mail enviado para {To} | Assunto: {Subject} | {Body}
```

| Tipo (`email_log.kind`) | Assunto | Corpo |
|---|---|---|
| `Welcome` | `Bem-vindo(a) a FIAP Cloud Games!` | `Ola {Name}, sua conta foi criada com sucesso. Aproveite o catalogo de jogos!` |
| `PurchaseConfirmed` | `Compra confirmada - pedido {OrderId}` | `Ola {Name}, o pagamento de R$ {Price:0.00} foi aprovado e o jogo ja esta disponivel na sua biblioteca.` |

O valor e formatado em pt-BR (ex.: `R$ 199,90`). Exemplo no console (`docker logs fcg-notifications-api`):

```
[12:00:01 INF] Fcg.Notifications.Infrastructure.Email.ConsoleEmailSender: E-mail enviado para ada@fcg.dev | Assunto: Bem-vindo(a) a FIAP Cloud Games! | Ola Ada Lovelace, sua conta foi criada com sucesso. Aproveite o catalogo de jogos!
[12:05:03 INF] Fcg.Notifications.Infrastructure.Email.ConsoleEmailSender: E-mail enviado para ada@fcg.dev | Assunto: Compra confirmada - pedido 0199a1b2-0000-7000-8000-0000000000c1 | Ola Ada Lovelace, o pagamento de R$ 199,90 foi aprovado e o jogo ja esta disponivel na sua biblioteca.
[12:06:10 INF] Fcg.Notifications.Application.Notifications.NotifyPayment.NotifyPaymentCommandHandler: Pagamento 0199a1b2-0000-7000-8000-0000000000c2 rejeitado: Valor acima do limite de aprovacao
```

## Persistencia (`fcg_notifications`)

| Tabela | Colunas |
|---|---|
| `known_users` | `user_id uuid pk`, `name varchar(120)`, `email varchar(320)`, `updated_at timestamptz` |
| `email_log` | `id uuid pk`, `event_id uuid` (**unico**), `kind varchar(40)` (`Welcome`/`PurchaseConfirmed`), `to_email varchar(320)`, `subject varchar(200)`, `sent_at timestamptz` |

Alem das tabelas `inbox_state`, `outbox_message` e `outbox_state` do MassTransit.

## Endpoints

| Metodo | Rota | Descricao |
|---|---|---|
| `GET` | `/health/live` | Processo em execucao (sem dependencias) |
| `GET` | `/health/ready` | PostgreSQL e bus MassTransit prontos |
| `GET` | `/openapi/v1.json` e `/scalar/v1` | Documentacao |

Nenhum endpoint de negocio (a validacao JWT do template continua configurada).

## Stack

.NET 10 · Minimal API · MediatR 12 · FluentValidation · EF Core 10 + PostgreSQL · MassTransit 8 + RabbitMQ (EF Core Outbox/Inbox) · JWT · Serilog · OpenAPI + Scalar · xUnit.

## Arquitetura

Camadas com dependencia estritamente para dentro: `WebApi -> Infrastructure -> Application -> Domain`.

```
src/
  Fcg.Notifications.Domain/           Regras de negocio. Organizado por agregado.
    _Shared/                    Modules, DomainModelBase/PersistenceModelBase, IDomainEvent, BusinessException
  Fcg.Notifications.Application/      Casos de uso (MediatR). Organizado por <Agregado>/<CasoDeUso>/.
    _Shared/                    INotificationsDbContext (leitura), Parser<TIn,TOut>, IIntegrationEventPublisher, ValidationBehavior
  Fcg.Notifications.Infrastructure/   Portas de saida: DbContext, repositorios, publicacao de eventos. Organizado por agregado.
    _Shared/                    NotificationsDbContext (+ tabelas de outbox/inbox), MassTransitIntegrationEventPublisher
  Fcg.Notifications.WebApi/           Portas de entrada: endpoints REST e consumers. Organizado por <Agregado>/<CasoDeUso>/.
    _Shared/                    Endpoints (IEndpoint), erros (ProblemDetails), JWT, mensageria, health checks, migrations
test/
  Fcg.Notifications.UnitTests/        Espelha src/ 1:1
  Fcg.Notifications.IntegrationTests/ WebApplicationFactory + MassTransit test harness
```

Diretrizes:

- **Um caso de uso = uma pasta** com `Command`/`Query`, `Handler`, `Validator`, `Parser` e, na WebApi, o `Endpoint` (implementa `IEndpoint`) ou o `Consumer`.
- **Escrita:** handler -> servico/modelo de dominio -> repositorio do agregado (recebe e devolve o modelo de dominio).
- **Leitura:** handler projeta direto de `INotificationsDbContext.DataSet<T>()`, sem repositorio.
- **Dominio:** `DomainModel` (comportamento) envolve o `PersistenceModel` (mapeado pelo EF via `IEntityTypeConfiguration<T>`, descoberto automaticamente).
- **Mapeamento:** `Parser<TIn,TOut>` explicito, sem AutoMapper.
- **Mensagens de erro:** sempre em `Properties/*Resources.resx` da propria camada, nunca string literal.
- **DI:** cada camada registra seus servicos no seu `FcgNotifications<Camada>Module`; `Program.cs` apenas compoe os 4 modulos.
- **Eventos de integracao:** publicados via `IIntegrationEventPublisher` antes do `SaveChangesAsync` - o outbox grava na mesma transacao.
- **Consumers:** ficam na WebApi, delegam para um `Command` via MediatR e sao idempotentes (inbox do MassTransit). Cada consumer tem um `ConsumerDefinition` com `EndpointName` lido de `Messaging:Queues:<Fila>` (ConfigMap) e default na constante de `Fcg.Contracts.Messaging.QueueNames`. Retry (1s/5s/15s/30s, sem `BusinessException`/`ValidationException`) e outbox/inbox sao aplicados por endpoint.
- **Contratos:** eventos, filas, claims e papeis vem do pacote `PosTech.Fiap.CloudGames.Contracts`; nunca redefina localmente.
- **Usuario autenticado:** injete `ICurrentUser` (Application/_Shared/Security), que le as claims `sub`, `email`, `name` e `role` do token.
- **Erros HTTP:** `ValidationException` 400, `UnauthorizedException` 401, `ForbiddenException` 403, `NotFoundException` 404, `ConflictException` 409, `BusinessException` 422.
- **JSON:** camelCase e enums como string.

Exemplo de caso de uso:

```
Application/Games/CreateGame/CreateGameCommand.cs
Application/Games/CreateGame/CreateGameCommandHandler.cs
Application/Games/CreateGame/CreateGameCommandValidator.cs
Application/Games/CreateGame/CreateGameResponseParser.cs
WebApi/Games/CreateGame/CreateGameEndpoint.cs
```

## Executando localmente

Pre-requisitos: .NET SDK 10 e a infraestrutura do [`fcg-orchestration`](https://github.com/sampaiobrenner/fcg-orchestration) (`docker compose up -d`).

```bash
dotnet run --project src/Fcg.Notifications.WebApi
```

- API: http://localhost:5104
- Documentacao (Scalar): http://localhost:5104/scalar/v1
- Health: `/health/live` e `/health/ready`
- RabbitMQ (management): http://localhost:15672 - as filas `notifications.user-created` e `notifications.payment-processed` sao criadas no startup.

Para simular os eventos sem as outras APIs, publique pelo management do RabbitMQ nas exchanges `fcg.user-created.v1` / `fcg.payment-processed.v1` usando os exemplos de [`fcg-contracts/samples`](https://github.com/sampaiobrenner/fcg-contracts/tree/main/samples) (envelope MassTransit) ou, de ponta a ponta, cadastre um usuario na UsersAPI e faca uma compra na CatalogAPI.

Migrations (as pendentes sao aplicadas no startup quando `Database__ApplyMigrationsOnStartup=true`):

```bash
dotnet tool restore
dotnet ef migrations add <Nome> -p src/Fcg.Notifications.Infrastructure -s src/Fcg.Notifications.WebApi -o _Shared/Context/Migrations
```

## Testes

```bash
dotnet test
```

- `Fcg.Notifications.UnitTests`: dominio, handlers (Moq), validators, parsers, `ConsoleEmailSender` (log estruturado) e consumers/definitions.
- `Fcg.Notifications.IntegrationTests`: PostgreSQL real via **Testcontainers** (requer Docker) + `ITestHarness` do MassTransit. Cenarios: usuario novo, evento duplicado, atualizacao de nome; pagamento aprovado, rejeitado, duplicado e usuario ausente (retry e sucesso apos o `UserCreatedEvent`); nome exato das filas; `/health/ready`.

## CI/CD

Workflow unico [`ci.yml`](.github/workflows/ci.yml):

| Job | Quando | O que faz |
|---|---|---|
| `build-test` | PR, push na `main`, tags `v*` | restore, build, testes com cobertura e analise no SonarCloud (projeto `sampaiobrenner_fcg-notifications-api`, requer o secret `SONAR_TOKEN`) |
| `docker` | PR | build da imagem |
| `publish` | push na `main` e tags `v*`, apos os testes | publica `ghcr.io/sampaiobrenner/fcg-notifications-api` (`latest`, `sha-<commit>`, semver) usando o [`docker-publish.yml`](https://github.com/sampaiobrenner/fcg-orchestration/blob/main/.github/workflows/docker-publish.yml) do `fcg-orchestration` |

## Docker

```bash
docker build -t ghcr.io/sampaiobrenner/fcg-notifications-api:latest .
docker run --rm -p 5104:8080 \
  -e ConnectionStrings__Default="Host=host.docker.internal;Port=5432;Database=fcg_notifications;Username=fcg;Password=fcg" \
  -e RabbitMq__Host=host.docker.internal -e RabbitMq__Username=fcg -e RabbitMq__Password=fcg \
  -e Jwt__Key=fcg-dev-only-signing-key-change-me-0123456789 \
  ghcr.io/sampaiobrenner/fcg-notifications-api:latest
```

Imagem multi-stage (`sdk:10.0` restore + publish `-c Release` -> `aspnet:10.0`), usuario non-root (`USER $APP_UID`), porta `8080`. O `.dockerignore` exclui `bin/`, `obj/`, `test/` e `.git/`.

## Kubernetes

Manifests em [`k8s/`](k8s):

| Arquivo | Conteudo |
|---|---|
| `deployment.yaml` | `Deployment` `notifications-api`, 2 replicas (competem pelas mesmas filas), `envFrom` ConfigMap + Secret, `startupProbe`/`livenessProbe` em `/health/live`, `readinessProbe` em `/health/ready`, requests `100m/128Mi`, limits `500m/256Mi` |
| `service.yaml` | `Service` `ClusterIP` `notifications-api:80` -> `http` (8080) |
| `configmap.yaml` | `notifications-api-config`: configuracoes nao sensiveis, incluindo os nomes das filas |
| `secret.yaml` | `notifications-api-secret`: connection string, credenciais do RabbitMQ e chave JWT (valores de dev) |

```bash
docker build -t ghcr.io/sampaiobrenner/fcg-notifications-api:latest .
kind load docker-image ghcr.io/sampaiobrenner/fcg-notifications-api:latest   # ou: minikube image load
kubectl apply -f k8s/
kubectl get pods -l app=notifications-api
kubectl port-forward svc/notifications-api 8080:80 && curl localhost:8080/health/ready
```

As migrations rodam no startup (`Database__ApplyMigrationsOnStartup=true`); o EF Core 10 usa lock de migracao, entao as 2 replicas podem subir juntas. O `/health/ready` so fica `Healthy` com PostgreSQL e RabbitMQ no cluster (ver `fcg-orchestration`).

## Variaveis de ambiente

| Variavel | Origem K8s | Descricao | Exemplo |
|---|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | ConfigMap | Ambiente | `Production` |
| `ASPNETCORE_HTTP_PORTS` | Dockerfile | Porta HTTP | `8080` |
| `ConnectionStrings__Default` | Secret | Conexao PostgreSQL | `Host=postgres;Port=5432;Database=fcg_notifications;Username=fcg;Password=***` |
| `Database__ApplyMigrationsOnStartup` | ConfigMap | Aplica migrations no startup | `true` |
| `RabbitMq__Host` | ConfigMap | Host do RabbitMQ | `rabbitmq` |
| `RabbitMq__VirtualHost` | ConfigMap | Virtual host | `/` |
| `RabbitMq__Username` | Secret | Usuario do RabbitMQ | `fcg` |
| `RabbitMq__Password` | Secret | Senha do RabbitMQ | `***` |
| `Jwt__Issuer` | ConfigMap | Emissor do token | `fcg-users-api` |
| `Jwt__Audience` | ConfigMap | Audiencia do token | `fcg` |
| `Jwt__Key` | Secret | Chave simetrica (min. 32 caracteres, igual em todos os servicos) | `***` |
| `Messaging__Queues__NotificationsUserCreated` | ConfigMap | Fila do `UserCreatedEvent` (default `QueueNames.NotificationsUserCreated`) | `notifications.user-created` |
| `Messaging__Queues__NotificationsPaymentProcessed` | ConfigMap | Fila do `PaymentProcessedEvent` (default `QueueNames.NotificationsPaymentProcessed`) | `notifications.payment-processed` |
