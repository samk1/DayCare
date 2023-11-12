namespace DayCare.Azure.Constructs
{
    using DayCare.Azure.Constructs.Data;
    using global::Constructs;
    using HashiCorp.Cdktf.Providers.Azuread.GroupMember;
    using HashiCorp.Cdktf.Providers.Azurerm.MssqlDatabase;
    using HashiCorp.Cdktf.Providers.Azurerm.MssqlFirewallRule;
    using HashiCorp.Cdktf.Providers.Azurerm.MssqlServer;
    using HashiCorp.Cdktf.Providers.Azurerm.UserAssignedIdentity;

    internal class Database : Construct
    {
        public Database(
            Construct scope,
            KeyVaultSecrets keyVaultSecrets,
            string appName,
            ResourceGroup resourceGroup,
            string directoryReadersGroupId)
            : base(scope, "Database")
        {
            var adminIdentity = new UserAssignedIdentity(this, $"sql-server-entra-admin", new UserAssignedIdentityConfig
            {
                Location = resourceGroup.Location,
                ResourceGroupName = resourceGroup.Name,
                Name = AdminIdentityName(appName),
            });

            var mssqlServer = new MssqlServer(this, "sql-server", new MssqlServerConfig
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
                    ObjectId = adminIdentity.PrincipalId,
                },
                Identity = new MssqlServerIdentity
                {
                    Type = "SystemAssigned",
                },
            });

            _ = new GroupMember(this, "sql-server-entra-admin-directory-readers-group-member", new GroupMemberConfig
            {
                GroupObjectId = directoryReadersGroupId,
                MemberObjectId = mssqlServer.Identity.PrincipalId,
            });

            _ = new MssqlDatabase(this, "sql-database", new MssqlDatabaseConfig
            {
                Name = DatabaseName(appName),
                ServerId = mssqlServer.Id,
                Collation = "SQL_Latin1_General_CP1_CI_AS",
            });

            _ = new MssqlFirewallRule(this, "sql-firewall-rule", new MssqlFirewallRuleConfig
            {
                Name = "AllowAzureServicesAndResourcesToAccessThisServer",
                ServerId = mssqlServer.Id,
                StartIpAddress = "0.0.0.0",
                EndIpAddress = "0.0.0.0",
            });
        }

        public static string ServerName(string appName) => $"{appName}-sql-server";

        public static string DatabaseName(string appName) => $"{appName}-database";

        public static string AdminIdentityName(string appName) => $"{appName}-sql-server-admin";

        public static string ConnectionString(string serverName, string databaseName)
        {
            return $"Server=tcp:{serverName},1433;Initial Catalog={databaseName};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=\"Active Directory Default\";";
        }
    }
}