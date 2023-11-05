@description('Specifies the location for resources.')
param location string = 'Australia East'

param container_app_name string = 'daycare20231105104246'
param container_app_image string = 'daycare20231105104604.azurecr.io/daycare.web:latest'

@secure()
param container_registry_password string

@secure()
param container_registry_username string

@secure()
param database_password string

resource sql_server_resource 'Microsoft.Sql/servers@2021-02-01-preview' = {
  name: container_app_name
  location: location
  properties: {
    administratorLogin: 'daycare20231105104246'
    administratorLoginPassword: database_password
    version: '12.0'
  }
}

resource sql_database_resource 'Microsoft.Sql/servers/databases@2021-02-01-preview' = {
  name: container_app_name
  parent: sql_server_resource
  location: location
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
  }
}

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
      activeRevisionsMode: 'Multiple'
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
          image: container_app_image
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