using Constructs;
using DayCare.Azure.Model;
using HashiCorp.Cdktf.Providers.Azurerm.ContainerApp;
using HashiCorp.Cdktf.Providers.Azurerm.ContainerAppEnvironment;

namespace DayCare.Azure.Constructs
{
    internal class WebApp : Construct
    {
        public WebApp(
            Construct scope, 
            AzureContext azureContext,
            KeyVaultSecrets keyVaultSecrets,
            ContainerSpec spec
        ) : base(scope, $"{azureContext.AppName}-webapp")
        {
            var containerAppEnvironment = new ContainerAppEnvironment(
                this, 
                $"{azureContext.AppName}-container-app-environment", 
                new ContainerAppEnvironmentConfig
                {
                    Name = $"{azureContext.AppName}-container-app-environment",
                    ResourceGroupName = azureContext.ResourceGroupName,
                    Location = azureContext.Location,
                }
            );

            new ContainerApp(this, $"{azureContext.AppName}-container-app", new ContainerAppConfig
            {
                Name = azureContext.AppName,
                ResourceGroupName = azureContext.ResourceGroupName,
                ContainerAppEnvironmentId = containerAppEnvironment.Id,
                RevisionMode = "Multiple",
                Identity = new ContainerAppIdentity
                {
                    Type = "SystemAssigned",
                },
                Secret = new[]
                {
                    new ContainerAppSecret
                    {
                        Name = "container-registry-password",
                        Value = keyVaultSecrets.GetSecret("container-registry-password"),
                    }
                },
                Ingress = new ContainerAppIngress
                {
                    TargetPort = 80,
                    ExternalEnabled = true,
                    Transport = "auto",
                    AllowInsecureConnections = false,
                    TrafficWeight = new[]
                    {
                        new ContainerAppIngressTrafficWeight
                        {
                          LatestRevision = true,
                          Percentage = 100,
                        }
                    }
                },
                Registry = new[]
                {
                    new ContainerAppRegistry
                    {
                        Server = azureContext.ContainerRegistryServer,
                        Username = keyVaultSecrets.GetSecret("container-registry-username"),
                        PasswordSecretName = "container-registry-password",
                    },
                },
                Template = new ContainerAppTemplate
                {

                    Container = new[]
                    {
                        new ContainerAppTemplateContainer
                        {
                            Name = $"${azureContext.AppName}-template",
                            Image = spec.Image,
                            Cpu = 0.5,
                            Memory = "1Gi",
                            Env = spec.BuildEnvironmentVariables(),
                        }
                    }
                }
            });
        }
    }
}
