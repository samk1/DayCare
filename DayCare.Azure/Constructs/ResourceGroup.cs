using Constructs;
using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermResourceGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayCare.Azure.Constructs
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
