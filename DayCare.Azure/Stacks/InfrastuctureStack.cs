using DayCare.Azure.Constructs;
using DayCare.Azure.Constructs.Data;
using DayCare.Azure.Stacks;

namespace DayCare.Azure
{
    internal class InfrastuctureStack : BaseAzureStack
    {
        public InfrastuctureStack(
            Construct scope, 
            string containerAppName
        ) : base(scope, "infrastructureStack", "DayCare-Infrastructure")
        {
            var keyVaultSecrets = new KeyVaultSecrets(
                scope: this
            );

            var resourceGroup = new ResourceGroup(
                scope: this
            );

            _ = new Database(
                scope: this,
                keyVaultSecrets: keyVaultSecrets,
                appName: containerAppName,
                resourceGroup: resourceGroup,
                directoryReadersGroupId: (string)scope.Node.TryGetContext("directoryReadersGroupId")
            );
        }
    }
}