using Constructs;
using DayCare.Azure.Model;
using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermRoleDefinition;
using HashiCorp.Cdktf.Providers.Azurerm.MssqlDatabase;
using HashiCorp.Cdktf.Providers.Azurerm.MssqlFirewallRule;
using HashiCorp.Cdktf.Providers.Azurerm.MssqlServer;
using HashiCorp.Cdktf.Providers.Azurerm.RoleAssignment;
using HashiCorp.Cdktf.Providers.Azurerm.UserAssignedIdentity;
using System.Collections.Generic;

namespace DayCare.Azure.Constructs
{
    internal class Database : Construct
    {
        private MssqlServer MssqlServer;
        private MssqlDatabase MssqlDatabase;
        private UserAssignedIdentity AdminIdentity;

        public string ConnectionString
        {
            get
            {
                return $"Server=tcp:{MssqlServer.FullyQualifiedDomainName},1433;Initial Catalog={MssqlDatabase.Name};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=\"Active Directory Default\";";
            }
        }

        public Database(
            Construct scope, 
            KeyVaultSecrets keyVaultSecrets,
            string appName,
            ResourceGroup resourceGroup
        ) : base(scope, "Database")
        {
            AdminIdentity = new UserAssignedIdentity(this, $"sql-server-entra-admin", new UserAssignedIdentityConfig
            {
                Location = resourceGroup.Location,
                ResourceGroupName = resourceGroup.Name,
                Name = $"sql-server-entra-admin",
            });

            MssqlServer = new MssqlServer(this, "sql-server", new MssqlServerConfig
            {
                Name = $"{appName}-sql-server",
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
                Version = "12.0",
                AdministratorLogin = "sqladmin",
                AdministratorLoginPassword = keyVaultSecrets.GetSecret("database-password"),
                AzureadAdministrator = new MssqlServerAzureadAdministrator
                {
                    AzureadAuthenticationOnly = true,
                    LoginUsername = "azuread-sqladmin",
                    ObjectId = AdminIdentity.PrincipalId,
                },
                Identity = new MssqlServerIdentity
                {
                    Type = "SystemAssigned"
                }
            });

            MssqlDatabase = new MssqlDatabase(this, "sql-database", new MssqlDatabaseConfig
            {
                Name = $"{appName}-database",
                ServerId = MssqlServer.Id,
                Collation = "SQL_Latin1_General_CP1_CI_AS",
            });

            new MssqlFirewallRule(this, "sql-firewall-rule", new MssqlFirewallRuleConfig
            {
                Name = "AllowAzureServicesAndResourcesToAccessThisServer",
                ServerId = MssqlServer.Id,
                StartIpAddress = "0.0.0.0",
                EndIpAddress = "0.0.0.0"
            });
        }

        internal void GrantAccess(string managedIdentityName)
        {
            new PowershellDeploymentScript(
                this,
                name: $"{managedIdentityName}-create-database-user-script",
                embeddedResourceName: "DayCare.Azure.Scripts.CreateDatabaseUser.ps1",
                parameters: new Dictionary<string, string>
                {
                    { "ManagedIdentityName", managedIdentityName },
                    { "ServerName", MssqlServer.FullyQualifiedDomainName },
                    { "DatabaseName", MssqlDatabase.Name }
                },
                principalId: AdminIdentity.Id
            );
        }
    }
}
