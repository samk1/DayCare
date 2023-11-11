using Constructs;
using DayCare.Azure.Model;
using HashiCorp.Cdktf.Providers.Azurerm.ContainerApp;
using HashiCorp.Cdktf.Providers.Azurerm.ContainerAppEnvironment;

namespace DayCare.Azure.Constructs
{
    internal class WebApp : Construct
    {
        private ContainerApp ContainerApp;
        public string ContainerAppName => ContainerApp.Name;

        public WebApp(
            Construct scope,
            ResourceGroup resourceGroup,
            ContainerSpec spec,
            string appName
        ) : base(scope, $"{appName}-webapp")
        {
            var containerAppEnvironment = new ContainerAppEnvironment(
                this, 
                $"{appName}-container-app-environment", 
                new ContainerAppEnvironmentConfig
                {
                    Name = $"{appName}-container-app-environment",
                    ResourceGroupName = resourceGroup.Name,
                    Location = resourceGroup.Location,
                }
            );

            ContainerApp = new ContainerApp(this, $"{appName}-container-app", new ContainerAppConfig
            {
                Name = $"{appName}-container-app",
                ResourceGroupName = resourceGroup.Name,
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
                        Value = spec.ContainerRegistryPassword,
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
                        Server = spec.ContainerRegistryServer,
                        Username = spec.ContainerRegistryUsername,
                        PasswordSecretName = "container-registry-password",
                    },
                },
                Template = new ContainerAppTemplate
                {

                    Container = new[]
                    {
                        new ContainerAppTemplateContainer
                        {
                            Name = $"{appName}-template",
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
