using Constructs;
using DayCare.Azure.Model;
using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermKeyVault;
using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermKeyVaultSecret;

namespace DayCare.Azure.Constructs
{
  internal class KeyVaultSecrets : Construct
  {
    private DataAzurermKeyVault DataAzurermKeyVault;

    public KeyVaultSecrets(Construct scope) : base(scope, "keyvault-secrets")
    {
      DataAzurermKeyVault = new DataAzurermKeyVault(this, "keyvault", new DataAzurermKeyVaultConfig
      {
        Name = (string)scope.Node.TryGetContext("keyVault.name"),
        ResourceGroupName = (string)scope.Node.TryGetContext("keyVaault.ResourceGroupName"),
      });
    }

    public string GetSecret(string secretName)
    {
      var secret = new DataAzurermKeyVaultSecret(this, secretName, new DataAzurermKeyVaultSecretConfig
      {
        Name = secretName,
        KeyVaultId = DataAzurermKeyVault.Id,
      });

      return secret.Value;
    }
  }
}
