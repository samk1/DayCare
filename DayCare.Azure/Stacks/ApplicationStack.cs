namespace DayCare.Azure.Stacks
{
    using System.Collections.Generic;
    using DayCare.Azure.Stacks.Data;
    using DayCare.Azure.Stacks.Model;
    using global::Constructs;
    using HashiCorp.Cdktf.Providers.Azurerm.ContainerApp;
    using HashiCorp.Cdktf.Providers.Azurerm.ContainerAppEnvironment;
    using HashiCorp.Cdktf.Providers.Azurerm.UserAssignedIdentity;

    internal class ApplicationStack : BaseAzureStack
    {
        public ApplicationStack(
            Construct scope,
            string containerImage,
            string containerAppName)
            : base(scope, "applicationStack", "DayCare-Application")
        {
            var resourceGroup = ResourceGroup.FindOrCreate(this);
            var sqlServer = SqlServer.FindOrCreate(this, containerAppName);

            var containerRegistry = new ContainerRegistry(this);

            var containerAppEnvironment = new ContainerAppEnvironment(
                this,
                $"{containerAppName}-container-app-environment",
                new ContainerAppEnvironmentConfig
                {
                    Name = $"{containerAppName}-container-app-environment",
                    ResourceGroupName = resourceGroup.Name,
                    Location = resourceGroup.Location,
                });

            var containerAppIndentity = new UserAssignedIdentity(this, $"{containerAppName}-application-identity", new UserAssignedIdentityConfig()
            {
                Name = $"{containerAppName}-application-identity",
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
            });

            var databaseAccess = DatabaseAccess.CreateDeploymentScript(
                this,
                containerAppName,
                containerAppIndentity.Name,
                roles: new[] { "db_datareader", "db_datawriter" });

            _ = new ContainerApp(this, $"{containerAppName}-container-app", new ContainerAppConfig
            {
                Name = $"{containerAppName}-container-app",
                ResourceGroupName = resourceGroup.Name,
                ContainerAppEnvironmentId = containerAppEnvironment.Id,
                RevisionMode = "Multiple",
                DependsOn = new[] { databaseAccess },
                Identity = new ContainerAppIdentity
                {
                    Type = "SystemAssigned",
                },
                Secret = new[]
                {
                    new ContainerAppSecret
                    {
                        Name = "container-registry-password",
                        Value = containerRegistry.AdminPassword,
                    },
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
                        },
                    },
                },
                Registry = new[]
                {
                    new ContainerAppRegistry
                    {
                        Server = containerRegistry.LoginServer,
                        Username = containerRegistry.AdminUsername,
                        PasswordSecretName = "container-registry-password",
                    },
                },
                Template = new ContainerAppTemplate
                {
                    Container = new[]
                    {
                        new ContainerAppTemplateContainer
                        {
                            Name = $"{containerAppName}-template",
                            Image = containerImage,
                            Cpu = 0.5,
                            Memory = "1Gi",
                            Env = Container.BuildEnvironmentVariables(
                                new Dictionary<string, string>
                                {
                                    {
                                        "ConnectionStrings__DefaultConnection",
                                        sqlServer.ApplicationDatabaseConnectionString
                                    },
                                    { "ASPNETCORE_ENVIRONMENT", "Development" },
                                }),
                        },
                    },
                },
            });
        }
    }
}