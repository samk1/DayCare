namespace DayCare.Azure.Stacks.Data
{
    using global::Constructs;
    using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermResourceGroup;

    internal class ResourceGroup
    {
        private readonly DataAzurermResourceGroup dataAzurermResourceGroup;

        private ResourceGroup(Construct scope)
        {
            var resourceGroupName = (string)scope.Node.TryGetContext("resourceGroupName");

            this.dataAzurermResourceGroup = new DataAzurermResourceGroup(scope, "resource-group", new DataAzurermResourceGroupConfig
            {
                Name = resourceGroupName,
            });
        }

        private ResourceGroup(DataAzurermResourceGroup dataAzurermResourceGroup)
        {
            this.dataAzurermResourceGroup = dataAzurermResourceGroup;
        }

        public string Name => this.dataAzurermResourceGroup.Name;

        public string Location => this.dataAzurermResourceGroup.Location;

        public static ResourceGroup FindOrCreate(Construct scope)
        {
            var construct = scope.Node.TryFindChild("resource-group");

            if (construct != null)
            {
                return new ResourceGroup((DataAzurermResourceGroup)construct);
            }

            return new ResourceGroup(scope);
        }
    }
}