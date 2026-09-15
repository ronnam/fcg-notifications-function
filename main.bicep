// ============================================================================
// FCG - FIAP Cloud Games · Notifications Function
// Infraestrutura como Código (IaC) — em Bicep (linguagem nativa do Azure)
// ----------------------------------------------------------------------------
// Provisiona a Function Serverless (Linux Consumption) e suas dependências:
//
//   1. Storage Account  → armazenamento interno do runtime das Functions
//   2. App Service Plan → plano Consumption (Y1): sem servidor 24/7,
//                         paga por execução — é a economia perseguida na Fase 3
//   3. Function App     → a função .NET 8 (isolated worker) com o
//                         trigger RabbitMQ e os App Settings do broker
//
// Validar o template (sem fazer deploy):
//   az bicep build --file main.bicep
//
// Deploy:
//   az deployment group create --resource-group <rg> `
//     --template-file main.bicep --parameters main.parameters.json
// ============================================================================

@description('Prefixo dos nomes dos recursos (minúsculas, sem espaços).')
@minLength(3)
@maxLength(15)
param projectName string = 'fcg-notif'

@description('Região do Azure onde os recursos serão criados.')
param location string = resourceGroup().location

@description('Ambiente do deploy.')
@allowed([
  'dev'
  'prod'
])
param environment string = 'dev'

// --- Conexão com o RabbitMQ (viram App Settings na Function App) -----------

@description('Connection string AMQP — consumida pelo [RabbitMQTrigger].')
@secure()
param rabbitMqConnection string

@description('Host do RabbitMQ — consumido pelo RabbitMqBindingInitializer.')
param rabbitMqHost string

@description('Usuário do RabbitMQ.')
@secure()
param massTransitUsername string

@description('Senha do RabbitMQ.')
@secure()
param massTransitPassword string

@description('Virtual host do RabbitMQ.')
param massTransitVirtualHost string = '/'

@description('Nome da fila do UserCreatedEvent.')
param userCreatedQueueName string = 'notifications-user-created-queue'

@description('Nome da fila do PaymentProcessedEvent.')
param paymentProcessedQueueName string = 'notifications-payment-processed-queue'

// ============================================================================
// Variáveis derivadas
// ============================================================================

// Storage Account e Function App exigem nome GLOBALMENTE único no Azure.
// uniqueString() gera um sufixo determinístico a partir do resource group.
var uniqueSuffix = uniqueString(resourceGroup().id)

var storageApiVersion = '2023-01-01'

// Storage Account: só minúsculas e números, 3–24 caracteres, SEM hífen.
var storageAccountName = toLower('${replace(projectName, '-', '')}${take(uniqueSuffix, 6)}sa')

var appServicePlanName = '${projectName}-${environment}-plan'
var functionAppName = '${projectName}-${environment}-${take(uniqueSuffix, 6)}'

var tags = {
  project: 'FCG - FIAP Cloud Games'
  component: 'Notifications Function'
  environment: environment
  managedBy: 'bicep'
}

// ============================================================================
// 1. Storage Account — exigida pelo runtime das Azure Functions
//    Guarda o estado interno, logs de execução e o pacote da função.
// ============================================================================
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  tags: tags
  sku: {
    name: 'Standard_LRS' // redundância local — mais barato, suficiente aqui
  }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    allowBlobPublicAccess: false
  }
}

// ============================================================================
// 2. App Service Plan — plano Consumption (Y1)
//    É o "serverless" das Functions: não existe servidor fixo ligado 24/7.
//    Você paga por execução — exatamente o que substitui o container antigo.
// ============================================================================
resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appServicePlanName
  location: location
  tags: tags
  kind: 'functionapp'
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {
    reserved: true // true = Linux (necessário para .NET isolated)
  }
}

// ============================================================================
// 3. Function App — a própria função
// ============================================================================
resource functionApp 'Microsoft.Web/sites@2022-09-01' = {
  name: functionAppName
  location: location
  tags: tags
  kind: 'functionapp,linux'
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNET-ISOLATED|8.0'
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      appSettings: [
        // --- Runtime das Functions ---
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${listKeys(storageAccount.id, storageApiVersion).keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }

        // --- RabbitMQ: consumido pelo [RabbitMQTrigger] ---
        {
          name: 'RabbitMQConnection'
          value: rabbitMqConnection
        }

        // --- RabbitMQ: consumido pelo RabbitMqBindingInitializer ---
        //     Chaves aninhadas usam separador duplo (__) no Azure.
        {
          name: 'RabbitMq__Host'
          value: rabbitMqHost
        }
        {
          name: 'MassTransit__Username'
          value: massTransitUsername
        }
        {
          name: 'MassTransit__Password'
          value: massTransitPassword
        }
        {
          name: 'MassTransit__VirtualHost'
          value: massTransitVirtualHost
        }

        // --- Nomes das filas ---
        {
          name: 'RabbitMq__UserCreatedQueueName'
          value: userCreatedQueueName
        }
        {
          name: 'RabbitMq__PaymentProcessedQueueName'
          value: paymentProcessedQueueName
        }
      ]
    }
  }
}

// ============================================================================
// Saídas — valores úteis logo após o deploy
// ============================================================================
output functionAppName string = functionApp.name
output functionAppDefaultHostName string = functionApp.properties.defaultHostName