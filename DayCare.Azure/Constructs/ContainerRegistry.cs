using Constructs;
using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermContainerRegistry;
using System.Collections.Generic;

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
            var containerRegistryContext = (Dictionary<string, object>)scope.Node.TryGetContext("containerRegistry");

            DataAzurermContainerRegistry = new DataAzurermContainerRegistry(this, "container-registry", new DataAzurermContainerRegistryConfig
            {
                Name = (string)containerRegistryContext["name"],
                ResourceGroupName = (string)containerRegistryContext["resourceGroupName"],
            });
        }
    }
}
