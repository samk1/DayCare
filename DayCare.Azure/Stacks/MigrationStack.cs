namespace DayCare.Azure.Stacks
{
    using System.Collections.Generic;
    using DayCare.Azure.Stacks.Data;
    using DayCare.Azure.Stacks.Model;
    using global::Constructs;
    using HashiCorp.Cdktf.Providers.Azurerm.ContainerGroup;
    using HashiCorp.Cdktf.Providers.Azurerm.UserAssignedIdentity;

    internal class MigrationStack : BaseAzureStack
    {
        public MigrationStack(
            Construct scope,
            string containerAppName,
            string containerImage)
            : base(scope, "migrationsStack", "DayCare-Migrations")
        {
            var resourceGroup = new ResourceGroup(this);

            var containerRegistry = new ContainerRegistry(this);

            var migrationsIdentity = new UserAssignedIdentity(this, $"{containerAppName}-migration-identity", new UserAssignedIdentityConfig
            {
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
                Name = $"{containerAppName}-migration-identity",
            });

            var sqlServer = new SqlServer(this, containerAppName);

            var databaseAccess = DatabaseAccess.CreateDeploymentScript(
                this,
                containerAppName,
                migrationsIdentity.Name,
                roles: new[] { "db_owner" });

            _ = new ContainerGroup(this, $"{containerAppName}-migration-container-group", new ContainerGroupConfig
            {
                Name = $"{containerAppName}-database-migrations-container-group",
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
                RestartPolicy = "Never",
                OsType = "Linux",
                IpAddressType = "None",
                DependsOn = new[] { databaseAccess },
                Identity = new ContainerGroupIdentity
                {
                    Type = "UserAssigned",
                    IdentityIds = new[] { migrationsIdentity.Id },
                },
                ImageRegistryCredential = new[]
                {
                    new ContainerGroupImageRegistryCredential
                    {
                        Server = containerRegistry.LoginServer,
                        Username = containerRegistry.AdminUsername,
                        Password = containerRegistry.AdminPassword,
                    },
                },
                Container = new[]
                {
                    new ContainerGroupContainer()
                    {
                        Name = $"{containerAppName}-database-migrations-container",
                        Image = containerImage,
                        Cpu = 1,
                        Memory = 1,
                        Commands = new[] { "/app/Migrations" },
                        EnvironmentVariables = new Dictionary<string, string>
                        {
                            {
                                "ConnectionStrings__DefaultConnection",
                                sqlServer.ApplicationDatabaseConnectionString
                            },
                        },
                    },
                },
            });
        }
    }
}