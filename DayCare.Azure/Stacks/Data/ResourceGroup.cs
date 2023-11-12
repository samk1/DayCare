namespace DayCare.Azure.Stacks.Data
{
    using global::Constructs;
    using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermResourceGroup;

    internal class ResourceGroup : Construct
    {
        private readonly DataAzurermResourceGroup dataAzurermResourceGroup;

        public ResourceGroup(Construct scope)
            : base(scope, "resource-group")
        {
            var resourceGroupName = (string)scope.Node.TryGetContext("resourceGroupName");

            this.dataAzurermResourceGroup = new DataAzurermResourceGroup(this, "resource-group", new DataAzurermResourceGroupConfig
            {
                Name = resourceGroupName,
            });
        }

        public string Name => this.dataAzurermResourceGroup.Name;

        public string Location => this.dataAzurermResourceGroup.Location;
    }
}