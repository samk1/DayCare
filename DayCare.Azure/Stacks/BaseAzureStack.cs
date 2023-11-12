namespace DayCare.Azure.Stacks
{
    using DayCare.Azure.Stacks.Model;
    using global::Constructs;
    using HashiCorp.Cdktf;
    using HashiCorp.Cdktf.Providers.Azuread.Provider;
    using HashiCorp.Cdktf.Providers.Azurerm.Provider;

    internal class BaseAzureStack : TerraformStack
    {
        public BaseAzureStack(Construct scope, string id, string workspace)
            : base(scope, id)
        {
            var context = new AzureContext(scope);

            _ = new CloudBackend(this, new CloudBackendConfig
            {
                Hostname = "app.terraform.io",
                Organization = "DayCare",
                Workspaces = new NamedCloudWorkspace(workspace),
            });

            _ = new AzurermProvider(this, "azurerm", new AzurermProviderConfig
            {
                Features = new AzurermProviderFeatures
                {
                },
                SkipProviderRegistration = true,
                SubscriptionId = context.SubscriptionId,
                TenantId = context.TenantId,
            });

            _ = new AzureadProvider(this, "azuread", new AzureadProviderConfig
            {
                TenantId = context.TenantId,
            });
        }
    }
}