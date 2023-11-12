namespace DayCare.Azure.Stacks
{
    using DayCare.Azure.Stacks.Data;
    using global::Constructs;
    using HashiCorp.Cdktf.Providers.Azuread.GroupMember;
    using HashiCorp.Cdktf.Providers.Azurerm.MssqlDatabase;
    using HashiCorp.Cdktf.Providers.Azurerm.MssqlFirewallRule;
    using HashiCorp.Cdktf.Providers.Azurerm.MssqlServer;
    using HashiCorp.Cdktf.Providers.Azurerm.UserAssignedIdentity;

    internal class InfrastuctureStack : BaseAzureStack
    {
        public InfrastuctureStack(
            Construct scope,
            string containerAppName)
            : base(scope, "infrastructureStack", "DayCare-Infrastructure")
        {
            var keyVaultSecrets = new KeyVaultSecrets(this);

            var resourceGroup = ResourceGroup.FindOrCreate(this);

            var directoryReadersGroupId = (string)scope.Node.TryGetContext("directoryReadersGroupId");

            var adminIdentity = new UserAssignedIdentity(this, $"sql-server-entra-admin", new UserAssignedIdentityConfig
            {
                Location = resourceGroup.Location,
                ResourceGroupName = resourceGroup.Name,
                Name = SqlServerAdmin.Name(containerAppName),
            });

            var mssqlServer = new MssqlServer(this, "sql-server", new MssqlServerConfig
            {
                Name = SqlServer.ServerName(containerAppName),
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
                Name = SqlServer.DatabaseName(containerAppName),
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
    }
}