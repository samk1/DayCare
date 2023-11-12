namespace DayCare.Azure.Stacks.Data
{
    using System.Collections.Generic;
    using global::Constructs;
    using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermContainerRegistry;

    internal class ContainerRegistry : Construct
    {
        private readonly DataAzurermContainerRegistry dataAzurermContainerRegistry;

        public ContainerRegistry(Construct scope)
            : base(scope, "container-registry")
        {
            var containerRegistryContext = (Dictionary<string, object>)scope.Node.TryGetContext("containerRegistry");

            this.dataAzurermContainerRegistry = new DataAzurermContainerRegistry(this, "container-registry", new DataAzurermContainerRegistryConfig
            {
                Name = (string)containerRegistryContext["name"],
                ResourceGroupName = (string)containerRegistryContext["resourceGroupName"],
            });
        }

        public string AdminUsername => this.dataAzurermContainerRegistry.AdminUsername;

        public string AdminPassword => this.dataAzurermContainerRegistry.AdminPassword;

        public string LoginServer => this.dataAzurermContainerRegistry.LoginServer;
    }
}