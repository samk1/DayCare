using Constructs;
using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermResourceGroup;

namespace DayCare.Azure.Constructs.Data
{
    internal class ResourceGroup : Construct
    {
        private DataAzurermResourceGroup DataAzurermResourceGroup;

        public string Name => DataAzurermResourceGroup.Name;
        public string Location => DataAzurermResourceGroup.Location;

        public ResourceGroup(Construct scope) : base(scope, "resource-group")
        {
            var resourceGroupName = (string)scope.Node.TryGetContext("resourceGroupName");

            DataAzurermResourceGroup = new DataAzurermResourceGroup(this, "resource-group", new DataAzurermResourceGroupConfig
            {
                Name = resourceGroupName,
            });
        }
    }
}
