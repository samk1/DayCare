using System;
using DayCare.Azure;
using DayCare.Azure.Model;
using HashiCorp.Cdktf;
using HashiCorp.Cdktf.Providers.Azurerm.Provider;

namespace MyCompany.MyApp
{
    class Program
    {
        public static void Main(string[] args)
        {
            App app = new App();
            var context = new AzureContext(app);
            
            MainStack stack = new MainStack(
                app,
                Environment.GetEnvironmentVariable("CONTAINER_APP_IMAGE"),
                context
            );

            new CloudBackend(stack, new CloudBackendConfig { Hostname = "app.terraform.io", Organization = "DayCare", Workspaces = new NamedCloudWorkspace("DayCare") });

            new AzurermProvider(stack, "azurerm", new AzurermProviderConfig
            {
                Features = new AzurermProviderFeatures
            {
            },
                SkipProviderRegistration = true,
                SubscriptionId = (string)stack.Node.TryGetContext("subscriptionId"),
                TenantId = (string)stack.Node.TryGetContext("tenantId"),
            });

            app.Synth();
            Console.WriteLine("App synth complete");
        }
    }
}