using Constructs;
using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermContainerRegistry;

namespace DayCare.Azure.Constructs
{
    internal class ContainerRegistry : Construct
    {
        private DataAzurermContainerRegistry DataAzurermContainerRegistry;

        public string AdminUsername => DataAzurermContainerRegistry.AdminUsername;
        public string AdminPassword => DataAzurermContainerRegistry.AdminPassword;
        public string LoginServer => DataAzurermContainerRegistry.LoginServer;


        public ContainerRegistry(Construct scope) : base(scope, "container-registry")
        {
            DataAzurermContainerRegistry = new DataAzurermContainerRegistry(this, "container-registry", new DataAzurermContainerRegistryConfig
            {
                Name = (string)scope.Node.TryGetContext("containerRegistry.name"),
                ResourceGroupName = (string)scope.Node.TryGetContext("containerRegistry.resourceGroupName"),
            });
        }
    }
}
