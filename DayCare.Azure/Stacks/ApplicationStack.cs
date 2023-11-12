namespace DayCare.Azure.Stacks
{
    using System.Collections.Generic;
    using DayCare.Azure.Constructs;
    using DayCare.Azure.Constructs.Data;
    using DayCare.Azure.Model;
    using global::Constructs;
    using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermMssqlServer;

    internal class ApplicationStack : BaseAzureStack
    {
        public ApplicationStack(
            Construct scope,
            string containerImage,
            string containerAppName)
            : base(scope, "applicationStack", "DayCare-Application")
        {
            var resourceGroup = new ResourceGroup(
                scope: this);

            var mssqlServer = new DataAzurermMssqlServer(this, "mssql-server", new DataAzurermMssqlServerConfig
            {
                Name = Database.ServerName(containerAppName),
                ResourceGroupName = resourceGroup.Name,
            });

            var webapp = new WebApp(
                scope: this,
                appName: containerAppName,
                resourceGroup: resourceGroup,
                spec: new ContainerSpec(
                    image: containerImage,
                    containerRegistry: new ContainerRegistry(this),
                    environmentVariables: new Dictionary<string, string>
                    {
                        {
                            "ConnectionStrings__DefaultConnection",
                            Database.ConnectionString(mssqlServer.FullyQualifiedDomainName, Database.DatabaseName(containerAppName))
                        },
                        { "ASPNETCORE_ENVIRONMENT", "Development" },
                    }));

            _ = new DatabaseAccess(
                scope: this,
                mssqlServer: mssqlServer,
                managedIdentityName: webapp.ContainerAppName,
                appName: containerAppName,
                resourceGroup: resourceGroup,
                id: "container-app-database-access",
                roles: new[] { "db_datareader", "db_datawriter" });
        }
    }
}