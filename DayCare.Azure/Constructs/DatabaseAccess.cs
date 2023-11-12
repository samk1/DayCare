using Constructs;
using DayCare.Azure.Constructs.Data;
using HashiCorp.Cdktf;
using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermMssqlServer;
using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermUserAssignedIdentity;
using HashiCorp.Cdktf.Providers.Azurerm.MssqlDatabase;
using HashiCorp.Cdktf.Providers.Azurerm.MssqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayCare.Azure.Constructs
{
    internal class DatabaseAccess : Construct
    {
        private PowershellDeploymentScript _powershellDeploymentScript;
        public ITerraformDependable Dependable => _powershellDeploymentScript.Dependable;

        public DatabaseAccess(
            Construct scope, 
            string id, 
            ResourceGroup resourceGroup,
            DataAzurermMssqlServer mssqlServer,
            string managedIdentityName, 
            string appName,
            string[] roles
        ) : base(scope, id)
        {
            var databaseAdmin = new DataAzurermUserAssignedIdentity(this, "database-admin", new DataAzurermUserAssignedIdentityConfig
            {
                Name = Database.AdminIdentityName(appName),
                ResourceGroupName = resourceGroup.Name,
            });


            _powershellDeploymentScript = new PowershellDeploymentScript(
                this,
                name: $"{id}-create-database-user-script",
                embeddedResourceName: "DayCare.Azure.Scripts.CreateDatabaseUser.ps1",
                parameters: new Dictionary<string, string>
                {
                    { "ManagedIdentityName", managedIdentityName },
                    { "ServerName", mssqlServer.FullyQualifiedDomainName },
                    { "DatabaseName", Database.DatabaseName(appName) },
                    { "Roles", string.Join(",", roles) }
                },
                principalId: databaseAdmin.Id,
                resourceGroup: resourceGroup
            );
        }

    }
}
