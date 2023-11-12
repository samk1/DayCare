namespace DayCare.Azure.Stacks.Data
{
    using System.Collections.Generic;
    using global::Constructs;
    using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermKeyVault;
    using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermKeyVaultSecret;

    internal class KeyVaultSecrets : Construct
    {
        private readonly DataAzurermKeyVault dataAzurermKeyVault;

        public KeyVaultSecrets(Construct scope)
            : base(scope, "keyvault-secrets")
        {
            var keyVaultContext = (Dictionary<string, object>)scope.Node.TryGetContext("keyVault");

            this.dataAzurermKeyVault = new DataAzurermKeyVault(this, "keyvault", new DataAzurermKeyVaultConfig
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
                KeyVaultId = this.dataAzurermKeyVault.Id,
            });

            return secret.Value;
        }
    }
}