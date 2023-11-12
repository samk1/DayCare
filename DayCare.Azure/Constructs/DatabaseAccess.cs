namespace DayCare.Azure.Constructs
{
    using System.Collections.Generic;
    using DayCare.Azure.Constructs.Data;
    using global::Constructs;
    using HashiCorp.Cdktf;
    using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermMssqlServer;
    using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermUserAssignedIdentity;

    internal class DatabaseAccess : Construct
    {
        private readonly PowershellDeploymentScript powershellDeploymentScript;

        public DatabaseAccess(
            Construct scope,
            string id,
            ResourceGroup resourceGroup,
            DataAzurermMssqlServer mssqlServer,
            string managedIdentityName,
            string appName,
            string[] roles)
            : base(scope, id)
        {
            var databaseAdmin = new DataAzurermUserAssignedIdentity(this, "database-admin", new DataAzurermUserAssignedIdentityConfig
            {
                Name = Database.AdminIdentityName(appName),
                ResourceGroupName = resourceGroup.Name,
            });

            this.powershellDeploymentScript = new PowershellDeploymentScript(
                this,
                name: $"{id}-create-database-user-script",
                embeddedResourceName: "DayCare.Azure.Scripts.CreateDatabaseUser.ps1",
                parameters: new Dictionary<string, string>
                {
                    { "ManagedIdentityName", managedIdentityName },
                    { "ServerName", mssqlServer.FullyQualifiedDomainName },
                    { "DatabaseName", Database.DatabaseName(appName) },
                    { "Roles", string.Join(",", roles) },
                },
                principalId: databaseAdmin.Id,
                resourceGroup: resourceGroup);
        }

        public ITerraformDependable Dependable => this.powershellDeploymentScript.Dependable;
    }
}