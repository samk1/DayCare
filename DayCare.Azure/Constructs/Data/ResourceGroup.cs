using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermResourceGroup;

namespace DayCare.Azure.Constructs.Data
{
    internal class ResourceGroup : Construct
    {
        private DataAzurermResourceGroup _dataAzurermResourceGroup;

        public string Name => _dataAzurermResourceGroup.Name;
        public string Location => _dataAzurermResourceGroup.Location;

        public ResourceGroup(Construct scope) : base(scope, "resource-group")
        {
            var resourceGroupName = (string)scope.Node.TryGetContext("resourceGroupName");

            _dataAzurermResourceGroup = new DataAzurermResourceGroup(this, "resource-group", new DataAzurermResourceGroupConfig
            {
                Name = resourceGroupName,
            });
        }
    }
}
