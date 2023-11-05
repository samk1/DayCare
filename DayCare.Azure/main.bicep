@description('Specifies the location for resources.')
param location string = 'Australia East'

param container_app_name string = 'daycare20231105104246'

@secure()
param container_registry_password string

@secure()
param container_registry_username string

resource managedEnvironments_resource 'Microsoft.App/managedEnvironments@2023-05-02-preview' = {
  name: container_app_name
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: 'ad47876b-d50b-49da-a348-7f48d9f002f7'
      }
    }
    zoneRedundant: false
    kedaConfiguration: {}
    daprConfiguration: {}
    customDomainConfiguration: {}
    peerAuthentication: {
      mtls: {
        enabled: false
      }
    }
  }
}

resource registries_resource 'Microsoft.ContainerRegistry/registries@2023-08-01-preview' = {
  name: container_app_name
  location: location
  sku: {
    name: 'Standard'
  }
  properties: {
    adminUserEnabled: true
    policies: {
      quarantinePolicy: {
        status: 'disabled'
      }
      trustPolicy: {
        type: 'Notary'
        status: 'disabled'
      }
      retentionPolicy: {
        days: 7
        status: 'disabled'
      }
      exportPolicy: {
        status: 'enabled'
      }
      azureADAuthenticationAsArmPolicy: {
        status: 'enabled'
      }
      softDeletePolicy: {
        retentionDays: 7
        status: 'disabled'
      }
    }
    encryption: {
      status: 'disabled'
    }
    dataEndpointEnabled: false
    publicNetworkAccess: 'Enabled'
    networkRuleBypassOptions: 'AzureServices'
    zoneRedundancy: 'Disabled'
  }
}

resource containerapps_resource 'Microsoft.App/containerapps@2023-05-02-preview' = {
  name: container_app_name
  location: location
  identity: {
    type: 'None'
  }
  properties: {
    managedEnvironmentId: managedEnvironments_resource.id
    environmentId: managedEnvironments_resource.id
    configuration: {
      secrets: [
        {
          name: 'daycare20231105104604azurecrio-daycare20231105104604'
          value: container_registry_password
        }
      ]
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 80
        exposedPort: 0
        transport: 'Auto'
        traffic: [
          {
            weight: 100
            latestRevision: true
          }
        ]
        allowInsecure: false
      }
      registries: [
        {
          server: 'daycare20231105104604.azurecr.io'
          username: container_registry_username
          passwordSecretRef: 'daycare20231105104604azurecrio-daycare20231105104604'
        }
      ]
    }
    template: {
      containers: [
        {
          image: 'daycare20231105104604.azurecr.io/daycare.web:latest'
          name: 'daycare'
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}

resource registries_repositories_admin 'Microsoft.ContainerRegistry/registries/scopeMaps@2023-08-01-preview' = {
  parent: registries_resource
  name: '_repositories_admin'
  properties: {
    description: 'Can perform all read, write and delete operations on the registry'
    actions: [
      'repositories/*/metadata/read'
      'repositories/*/metadata/write'
      'repositories/*/content/read'
      'repositories/*/content/write'
      'repositories/*/content/delete'
    ]
  }
}

resource registries_repositories_pull 'Microsoft.ContainerRegistry/registries/scopeMaps@2023-08-01-preview' = {
  parent: registries_resource
  name: '_repositories_pull'
  properties: {
    description: 'Can pull any repository of the registry'
    actions: [
      'repositories/*/content/read'
    ]
  }
}

resource registries_pull_metadata_read 'Microsoft.ContainerRegistry/registries/scopeMaps@2023-08-01-preview' = {
  parent: registries_resource
  name: '_repositories_pull_metadata_read'
  properties: {
    description: 'Can perform all read operations on the registry'
    actions: [
      'repositories/*/content/read'
      'repositories/*/metadata/read'
    ]
  }
}

resource registries_repositories_push 'Microsoft.ContainerRegistry/registries/scopeMaps@2023-08-01-preview' = {
  parent: registries_resource
  name: '_repositories_push'
  properties: {
    description: 'Can push to any repository of the registry'
    actions: [
      'repositories/*/content/read'
      'repositories/*/content/write'
    ]
  }
}

resource registries_repositories_push_metadata_write 'Microsoft.ContainerRegistry/registries/scopeMaps@2023-08-01-preview' = {
  parent: registries_resource
  name: '_repositories_push_metadata_write'
  properties: {
    description: 'Can perform all read and write operations on the registry'
    actions: [
      'repositories/*/metadata/read'
      'repositories/*/metadata/write'
      'repositories/*/content/read'
      'repositories/*/content/write'
    ]
  }
}