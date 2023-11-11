using Constructs;
using DayCare.Azure.Model;
using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermKeyVault;
using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermKeyVaultSecret;

namespace DayCare.Azure.Constructs
{
  internal class KeyVaultSecrets : Construct
  {
    private DataAzurermKeyVault KeyVault;

    public KeyVaultSecrets(Construct scope, AzureContext azureContext) : base(scope, "keyvault-secrets")
    {
      KeyVault = new DataAzurermKeyVault(this, "keyvault", new DataAzurermKeyVaultConfig
      {
        Name = azureContext.KeyVaultName,
        ResourceGroupName = azureContext.ResourceGroupName,
      });
    }

    public string GetSecret(string secretName)
    {
      var secret = new DataAzurermKeyVaultSecret(this, secretName, new DataAzurermKeyVaultSecretConfig
      {
        Name = secretName,
        KeyVaultId = KeyVault.Id,
      });

      return secret.Value;
    }
  }
}
