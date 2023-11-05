using './main.bicep'

var subscriptionId = '1ec0e96a-163c-4792-88f4-dc349c75bdbf'
var resourceGroup = 'daycare20231105104246ResourceGroup'
var keyVaultName = 'daycare20231105104604'

param location = 'Australia East'
param container_registry_username = getSecret(subscriptionId, resourceGroup, keyVaultName, 'container-registry-username')
param container_registry_password = getSecret(subscriptionId, resourceGroup, keyVaultName, 'container-registry-password')
param database_password = getSecret(subscriptionId, resourceGroup, keyVaultName, 'database-password')

