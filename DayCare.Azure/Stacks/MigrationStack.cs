using DayCare.Azure.Constructs;
using DayCare.Azure.Stacks;
using HashiCorp.Cdktf.Providers.Azurerm.ContainerGroup;
using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermMssqlServer;
using HashiCorp.Cdktf.Providers.Azurerm.UserAssignedIdentity;
using DayCare.Azure.Constructs.Data;

namespace DayCare.Azure
{
    internal class MigrationStack : BaseAzureStack
    {
        public MigrationStack(
            Construct scope, 
            string containerAppName, 
            string containerImage
        ) : base(scope, "migrationsStack", "DayCare-Migrations")
        {
            var resourceGroup = new ResourceGroup(this);

            var containerRegistry = new ContainerRegistry(this);

            var mssqlServer = new DataAzurermMssqlServer(this, "mssql-server", new DataAzurermMssqlServerConfig
            {
                Name = Database.ServerName(containerAppName),
                ResourceGroupName = resourceGroup.Name,
            });

            var migrationsIdentity = new UserAssignedIdentity(this, $"{containerAppName}-migration-identity", new UserAssignedIdentityConfig
            {
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
                Name = $"{containerAppName}-migration-identity"
            });

            var databaseAccess = new DatabaseAccess(
                this, 
                $"{containerAppName}-migration-database-access", 
                resourceGroup,
                mssqlServer,
                migrationsIdentity.Name,
                containerAppName,
                roles: new[] { "db_owner" }
            );

            new ContainerGroup(this, $"{containerAppName}-migration-container-group", new ContainerGroupConfig
            {
                Name = $"{containerAppName}-database-migrations-container-group",
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
                RestartPolicy = "Never",
                OsType = "Linux",
                IpAddressType = "None",
                DependsOn = new [] { databaseAccess.Dependable },
                Identity = new ContainerGroupIdentity
                {
                    Type = "UserAssigned",
                    IdentityIds = new[] { migrationsIdentity.Id }
                },
                ImageRegistryCredential = new[]
                {
                    new ContainerGroupImageRegistryCredential
                    {
                        Server = containerRegistry.LoginServer,
                        Username = containerRegistry.AdminUsername,
                        Password = containerRegistry.AdminPassword
                    }
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
                                Database.ConnectionString(mssqlServer.FullyQualifiedDomainName, Database.DatabaseName(containerAppName))
                            }
                        }
                    }
                }
            });
        }
    }
}