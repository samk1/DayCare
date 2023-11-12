using Constructs;
using DayCare.Azure.Model;
using HashiCorp.Cdktf;
using HashiCorp.Cdktf.Providers.Azuread.Provider;
using HashiCorp.Cdktf.Providers.Azurerm.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayCare.Azure.Stacks
{
    internal class BaseAzureStack : TerraformStack
    {
        public BaseAzureStack(Construct scope, string id, string workspace) : base(scope, id)
        {
            var context = new AzureContext(scope);

            new CloudBackend(this, new CloudBackendConfig
            {
                Hostname = "app.terraform.io",
                Organization = "DayCare",
                Workspaces = new NamedCloudWorkspace(workspace)
            });

            new AzurermProvider(this, "azurerm", new AzurermProviderConfig
            {
                Features = new AzurermProviderFeatures
                {
                },
                SkipProviderRegistration = true,
                SubscriptionId = context.SubscriptionId,
                TenantId = context.TenantId,
            });

            new AzureadProvider(this, "azuread", new AzureadProviderConfig
            {
                TenantId = context.TenantId,
            });
        }
    }
}
