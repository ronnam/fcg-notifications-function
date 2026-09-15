# ⚡ FCG - FIAP Cloud Games · Notifications Function
Função Serverless (**Azure Functions**) que **substitui a NotificationsAPI** do FCG. É acionada por **eventos publicados no RabbitMQ** e simula, via log, o envio dos e-mails de **boas-vindas** e de **confirmação de compra**.
Componente do **Tech Challenge da Fase 3** da pós-graduação **Arquitetura de Sistemas .NET – FIAP**, dentro da arquitetura de **microsserviços orientada a eventos** do FCG.

## 👤 Projeto Individual – Turma 12NETT
| Integrante | GitHub |
|---|---|
| Ronnam de Lima da Silva | [@ronnam](https://github.com/ronnam) |

## 🏗️ Arquitetura
```text
        Users API                          Payments API
            │                                   │
            │ publica UserCreatedEvent          │ publica PaymentProcessedEvent
            ▼                                   ▼
┌───────────────────────────────────────────────────────────┐
│                      RABBITMQ (broker)                    │
│                                                           │
│  exchange  Fgc.MessageContracts.Events:UserCreatedEvent   │
│  exchange  Fgc.MessageContracts.Events:PaymentProcessed…  │
│                                                           │
│  filas     notifications-user-created-queue               │
│            notifications-payment-processed-queue          │
└──────────────────────────┬────────────────────────────────┘
                           │ entrega a mensagem (envelope)
                           ▼
┌───────────────────────────────────────────────────────────┐
│             AZURE FUNCTION — este repositório             │
│                                                           │
│  UserCreatedTrigger        PaymentProcessedTrigger        │
│         └──────── MassTransitEnvelopeParser ────────┘     │
│                          │                                │
│   UserCreatedHandler  |  PaymentProcessedHandler          │
│                          ▼                                │
│        log do e-mail simulado (boas-vindas / compra)      │
└───────────────────────────────────────────────────────────┘
```

## 🔗 Fluxo de Eventos
**1. Cadastro de Usuário**
- Users API cadastra o usuário → publica `UserCreatedEvent` no RabbitMQ
- Esta Function consome a fila `notifications-user-created-queue` → `UserCreatedHandler` → loga o e-mail de boas-vindas

**2. Compra de Jogo**
- Payments API processa o pagamento → publica `PaymentProcessedEvent` no RabbitMQ
- Esta Function consome a fila `notifications-payment-processed-queue` → `PaymentProcessedHandler`
  - **Approved** → loga o e-mail de confirmação de compra
  - **Rejected** → loga o aviso de pagamento recusado

## 📂 Estrutura do Projeto
```text
fcg-notifications-function/
├── src/
│   ├── Fgc.Notifications.

Application/     → Handlers (regras de notificação)
│   │   └── Handlers/
│   │       ├── UserCreatedHandler.cs
│   │       └── PaymentProcessedHandler.cs
│   ├── Fgc.Notifications.

Domain/          → Entidades e contratos de domínio
│   ├── Fgc.Notifications.

Function/        → Host da Azure Function e triggers
│   │   ├── Program.cs
│   │   ├── host.json
│   │   ├── HealthFunction.cs
│   │   ├── RabbitMqBindingInitializer.cs
│   │   ├── MassTransitEnvelopeParser.cs
│   │   ├── UserCreatedTrigger.cs
│   │   └── PaymentProcessedTrigger.cs
│   └── Fgc.Notifications.

Infrastructure/  → Injeção de dependência
│       └── DependencyInjection.cs
├── tests/
│   ├── Fgc.Notifications.

UnitTests/
│   │   ├── UserCreatedHandlerTests.cs
│   │   ├── PaymentProcessedHandlerTests.cs
│   │   └── TestLogger.cs
│   └── Fgc.Notifications.

IntegrationTests/
├── LocalPackages/                         → Pacote NuGet local (Fgc.MessageContracts)
├── nuget.config
└── Fgc.Notifications.slnx
```

## ⚙️ Como Funciona
### 1. Topologia das filas (`RabbitMqBindingInitializer`)
Na inicialização do host, a Function garante a **exchange**, a **fila** e o **binding** antes de consumir. O nome da exchange segue a **convenção do MassTransit** a partir do tipo da mensagem — por isso a Users API, que publica o mesmo evento via MassTransit, cai exatamente nela.
```csharp
channel.

ExchangeDeclare("Fgc.MessageContracts.

Events:UserCreatedEvent", ExchangeType.

Fanout, durable: true);
channel.

QueueDeclare("notifications-user-created-queue", durable: true, exclusive: false, autoDelete: false);
channel.

QueueBind("notifications-user-created-queue", "Fgc.MessageContracts.

Events:UserCreatedEvent", routingKey: "");
```

### 2. Triggers (`[RabbitMQTrigger]`)
Cada evento tem seu próprio trigger, apontando para a fila correspondente.
```csharp
[Function("UserCreatedTrigger")]
public async Task Run(
    [RabbitMQTrigger("notifications-user-created-queue", ConnectionStringSetting = "RabbitMQConnection")] string envelope)
{
    _logger.

LogInformation("Mensagem recebida na fila notifications-user-created-queue.");
    var @event = MassTransitEnvelopeParser.

Parse<UserCreatedEvent>(envelope);
    await _handler.

Handle(@event);
}
```

### 3. Parser de envelope (`MassTransitEnvelopeParser`)
O MassTransit **não publica o evento puro**: ele embrulha a mensagem num **envelope** com `messageId`, `messageType` etc. O evento real fica no campo `message` — é ele que o parser extrai antes de desserializar.
```csharp
using var doc = JsonDocument.

Parse(envelope);
var message = doc.RootElement.

GetProperty("message");
return message.

Deserialize<T>(Options)!;
```

### 4. Handlers
Recebem o evento já tipado e simulam o envio (log). Ficam isolados na camada **Application**, o que permite testá-los sem subir a Function.

## 🔧 Configuração
### Variáveis
| Chave | Usada por | Exemplo |
|---|---|---|
| `RabbitMQConnection` | Triggers (`[RabbitMQTrigger]`) | amqp://admin:admin@localhost:5672/ |
| `RabbitMq:Host` | Initializer | localhost |
| `MassTransit:Username` | Initializer | admin |
| `MassTransit:Password` | Initializer | admin |
| `MassTransit:VirtualHost` | Initializer | / |
| `RabbitMq:UserCreatedQueueName` | Initializer | notifications-user-created-queue |
| `RabbitMq:PaymentProcessedQueueName` | Initializer | notifications-payment-processed-queue |

### local.settings.json (exemplo — não versionado)
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "RabbitMQConnection": "amqp://admin:admin@localhost:5672/",
    "RabbitMq:Host": "localhost",
    "MassTransit:Username": "admin",
    "MassTransit:Password": "admin",
    "MassTransit:VirtualHost": "/",
    "RabbitMq:UserCreatedQueueName": "notifications-user-created-queue",
    "RabbitMq:PaymentProcessedQueueName": "notifications-payment-processed-queue"
  }
}
```
> No **Azure Portal** (App Settings), chaves aninhadas usam separador duplo: `RabbitMq__Host`, `MassTransit__Username`.

## ▶️ Execução Local
### Pré-requisitos
- .NET 8 SDK
- Azure Functions Core Tools (`func`)
- RabbitMQ em execução (local ou via `docker compose` do fgc-orchestration)

### Passos
```bash
# restaurar e compilar
dotnet restore
dotnet build
# subir a Function (o RabbitMqBindingInitializer cria filas e bindings)
func start
```
Disparar um evento: publique um `UserCreatedEvent` (ex.: cadastrando um usuário pela Users API) e acompanhe o log da Function.

## ☁️ Infraestrutura como Código (IaC)

A infraestrutura da Function é provisionada via **Bicep** — a linguagem declarativa nativa do Azure — atendendo ao requisito da Fase 3 de manter a infraestrutura como código no próprio repositório da função.

### Arquivos

| Arquivo | Função |
|---|---|
| `main.bicep` | Provisiona a infraestrutura completa da Function |
| `main.parameters.example.json` | Modelo de parâmetros (copie para `main.parameters.json`) |

### Recursos provisionados

| Recurso | Tipo Azure | Papel |
|---|---|---|
| Storage Account | `Microsoft.Storage/storageAccounts` | Armazenamento interno do runtime das Functions |
| App Service Plan | `Microsoft.Web/serverfarms` (Y1 / Dynamic) | Plano **Consumption** — paga por execução, sem servidor 24/7 |
| Function App | `Microsoft.Web/sites` | A função .NET 8 (isolated worker) com trigger RabbitMQ |

### App Settings aplicados pela IaC

| Configuração | Consumida por |
|---|---|
| `RabbitMQConnection` | `[RabbitMQTrigger]` |
| `RabbitMq__Host` · `MassTransit__Username` · `MassTransit__Password` · `MassTransit__VirtualHost` | `RabbitMqBindingInitializer` |
| `RabbitMq__UserCreatedQueueName` · `RabbitMq__PaymentProcessedQueueName` | `RabbitMqBindingInitializer` |

### Validar o template (sem deploy)
```bash
az bicep build --file main.bicep
```

### Deploy (opcional)
```bash
az group create --name rg-fgc-notifications --location brazilsouth

az deployment group create \
  --resource-group rg-fgc-notifications \
  --template-file main.bicep \
  --parameters main.parameters.json
```

## ✅ Testes
```bash
cd fgc-notifications-function && dotnet test
```
Cobertura atual: handlers de `UserCreatedEvent` e `PaymentProcessedEvent` (Approved/Rejected) — **4 testes verdes**.

## 🧰 Tecnologias
| Item | Tecnologia |
|---|---|
| Runtime | .NET 8 (isolated worker) |
| Computação | Azure Functions |
| Mensageria | RabbitMQ + MassTransit |
| Contratos | Fgc.MessageContracts (pacote NuGet local) |
| Testes | Unitários (handlers) + Integração |

## 📄 Relatório de Entrega
Projeto: Individual – Turma 12NETT
Integrante: Ronnam de Lima da Silva
Usernames no Discord: [A PREENCHER]
Data: 15 de setembro de 2026
Repositório: este
Vídeo: [LINK DO VÍDEO DA FASE 3 A INSERIR]