# 📧 FCG Notifications Function

Função Serverless (Azure Functions) do **FIAP Cloud Games (FCG)** que substitui a **NotificationsAPI**, acionada por eventos do RabbitMQ para simular o envio de e-mails de boas-vindas e de confirmação de compra.

## 🎯 Finalidade

A NotificationsAPI rodava como um container 24/7, mas ficava ociosa a maior parte do tempo, apenas aguardando eventos. Esta Function elimina esse desperdício: ela só é executada quando uma mensagem chega na fila, cobrando apenas pelo tempo de execução.

## 🏗️ Arquitetura
```text
┌──────────────────┐   UserCreatedEvent    ┌──────────────────────────┐
│   Users API      │ ────────────────────► │                          │
└──────────────────┘                       │  fcg-notifications-      │
                                           │  function (Azure)        │
┌──────────────────┐   PaymentProcessed    │                          │
│  Payments API    │ ────────────────────► │  (trigger RabbitMQ)      │
└──────────────────┘                       └──────────────────────────┘
```

## 📦 Eventos consumidos

| Evento | Origem | Ação |
|---|---|---|
| `UserCreatedEvent` | Users API | Simula e-mail de boas-vindas |
| `PaymentProcessedEvent` | Payments API | Se `Approved`, simula e-mail de confirmação de compra |

## 🛠️ Tecnologias

- .NET 8
- Azure Functions (Isolated Worker)
- MassTransit (consumo de eventos RabbitMQ)

## 🔧 Variáveis de Ambiente

| Variável | Descrição | Padrão |
|---|---|---|
| `MassTransit__Host` | Host do RabbitMQ | rabbitmq |
| `MassTransit__Username` | Usuário RabbitMQ | admin |
| `MassTransit__Password` | Senha RabbitMQ | admin |

> ⚠️ As variáveis locais ficam no `local.settings.json`, que **não** é versionado (ver `.gitignore`).

## 🚀 Execução local
```bash
func start
```

## ✅ Testes
```bash
dotnet test
```
`
```

## 📌 Notas sobre o que incluí

- **Finalidade** explica o *porquê* da Function existir (substituir o container ocioso) — isso é o que o enunciado da Fase 3 pede e o que vai aparecer no vídeo.
- **Arquitetura** mostra os dois eventos que ela consome, deixando claro o papel dela no fluxo.
- **Variáveis de ambiente** segue o padrão dos outros repositórios (exigência do enunciado: "README explicando finalidade e variáveis de ambiente").
- **`local.settings.json`** destacado como não versionado — reforça o `.gitignore`.

## 🎯 Antes de você colar

Duas coisas que **só você sabe** e que eu deixei como estão:

1. **O nome do trigger**: usei "trigger RabbitMQ" de forma genérica. Na prática, a Azure Function com MassTransit normalmente usa um **trigger de Service Bus ou RabbitMQ** — o detalhe exato depende de como você vai implementar (isso é decisão da próxima subtarefa, "Configurar trigger da mensageria"). Deixei genérico de propósito.
2. **O comando `func start`**: é o padrão do Azure Functions Core Tools. Se você ainda não instalou as tools, isso vem na próxima etapa.