using HashiCorp.Cdktf.Providers.Azurerm.MssqlDatabase;
using HashiCorp.Cdktf.Providers.Azurerm.MssqlFirewallRule;
using HashiCorp.Cdktf.Providers.Azurerm.MssqlServer;
using HashiCorp.Cdktf.Providers.Azurerm.UserAssignedIdentity;
using HashiCorp.Cdktf.Providers.Azuread.GroupMember;
using DayCare.Azure.Constructs.Data;

namespace DayCare.Azure.Constructs
{
    internal class Database : Construct
    {
        public static string ServerName(string appName) => $"{appName}-sql-server";
        public static string DatabaseName(string appName) => $"{appName}-database";
        public static string AdminIdentityName(string appName) => $"{appName}-sql-server-admin";

        public static string ConnectionString(string serverName, string databaseName)
        {
            return $"Server=tcp:{serverName},1433;Initial Catalog={databaseName};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=\"Active Directory Default\";";
        }

        private MssqlServer _mssqlServer;
        private MssqlDatabase _mssqlDatabase;
        private UserAssignedIdentity _adminIdentity;
        private ResourceGroup _resourceGroup;

        public Database(
            Construct scope, 
            KeyVaultSecrets keyVaultSecrets,
            string appName,
            ResourceGroup resourceGroup,
            string directoryReadersGroupId
        ) : base(scope, "Database")
        {
            _resourceGroup = resourceGroup;

            _adminIdentity = new UserAssignedIdentity(this, $"sql-server-entra-admin", new UserAssignedIdentityConfig
            {
                Location = resourceGroup.Location,
                ResourceGroupName = resourceGroup.Name,
                Name = AdminIdentityName(appName),
            });

            _mssqlServer = new MssqlServer(this, "sql-server", new MssqlServerConfig
            {
                Name = ServerName(appName),
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
                Version = "12.0",
                AdministratorLogin = "sqladmin",
                AdministratorLoginPassword = keyVaultSecrets.GetSecret("database-password"),
                AzureadAdministrator = new MssqlServerAzureadAdministrator
                {
                    AzureadAuthenticationOnly = true,
                    LoginUsername = "azuread-sqladmin",
                    ObjectId = _adminIdentity.PrincipalId,
                },
                Identity = new MssqlServerIdentity
                {
                    Type = "SystemAssigned"
                }
            });

            _ = new GroupMember(this, "sql-server-entra-admin-directory-readers-group-member", new GroupMemberConfig
            {
                GroupObjectId = directoryReadersGroupId,
                MemberObjectId = _mssqlServer.Identity.PrincipalId,
            });

            _mssqlDatabase = new MssqlDatabase(this, "sql-database", new MssqlDatabaseConfig
            {
                Name = DatabaseName(appName),
                ServerId = _mssqlServer.Id,
                Collation = "SQL_Latin1_General_CP1_CI_AS",
            });

            _ = new MssqlFirewallRule(this, "sql-firewall-rule", new MssqlFirewallRuleConfig
            {
                Name = "AllowAzureServicesAndResourcesToAccessThisServer",
                ServerId = _mssqlServer.Id,
                StartIpAddress = "0.0.0.0",
                EndIpAddress = "0.0.0.0"
            });
        }
    }
}
