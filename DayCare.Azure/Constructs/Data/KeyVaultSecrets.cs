using Constructs;
using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermKeyVault;
using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermKeyVaultSecret;
using System.Collections.Generic;

namespace DayCare.Azure.Constructs.Data
{
    internal class KeyVaultSecrets : Construct
    {
        private DataAzurermKeyVault _dataAzurermKeyVault;

        public KeyVaultSecrets(Construct scope) : base(scope, "keyvault-secrets")
        {
            var keyVaultContext = (Dictionary<string, object>)scope.Node.TryGetContext("keyVault");

            _dataAzurermKeyVault = new DataAzurermKeyVault(this, "keyvault", new DataAzurermKeyVaultConfig
            {
                Name = (string)keyVaultContext["name"],
                ResourceGroupName = (string)keyVaultContext["resourceGroupName"],
            });
        }

        public string GetSecret(string secretName)
        {
            var secret = new DataAzurermKeyVaultSecret(this, secretName, new DataAzurermKeyVaultSecretConfig
            {
                Name = secretName,
                KeyVaultId = _dataAzurermKeyVault.Id,
            });

            return secret.Value;
        }
    }
}
