using Constructs;
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
    internal class DatabaseAccess : Construct, ITerraformDependable
    {
        private PowershellDeploymentScript powershellDeploymentScript;
        public string Fqn => powershellDeploymentScript.Fqn;

        public DatabaseAccess(
            Construct scope, 
            string id, 
            ResourceGroup resourceGroup,
            DataAzurermMssqlServer mssqlServer,
            string managedIdentityName, 
            string appName) : base(scope, id)
        {
            var databaseAdmin = new DataAzurermUserAssignedIdentity(this, "database-admin", new DataAzurermUserAssignedIdentityConfig
            {
                Name = Database.AdminIdentityName(appName),
                ResourceGroupName = resourceGroup.Name,
            });


            powershellDeploymentScript = new PowershellDeploymentScript(
                this,
                name: $"{id}-create-database-user-script",
                embeddedResourceName: "DayCare.Azure.Scripts.CreateDatabaseUser.ps1",
                parameters: new Dictionary<string, string>
                {
                    { "ManagedIdentityName", managedIdentityName },
                    { "ServerName", mssqlServer.FullyQualifiedDomainName },
                    { "DatabaseName", Database.DatabaseName(appName) }
                },
                principalId: databaseAdmin.Id,
                resourceGroup: resourceGroup
            );
        }

    }
}
