namespace DayCare.Azure.Stacks.Data
{
    using Constructs;
    using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermUserAssignedIdentity;

    internal class SqlServerAdmin
    {
        private readonly DataAzurermUserAssignedIdentity dataAzurermUserAssignedIdentity;

        public SqlServerAdmin(Construct scope, string appName)
        {
            var resourceGroup = new ResourceGroup(scope);
            var name = Name(appName);

            this.dataAzurermUserAssignedIdentity = new DataAzurermUserAssignedIdentity(scope, "database-admin", new DataAzurermUserAssignedIdentityConfig
            {
                Name = name,
                ResourceGroupName = resourceGroup.Name,
            });
        }

        public string PrincipalId => this.dataAzurermUserAssignedIdentity.PrincipalId;

        public static string Name(string appName) => $"{appName}-sql-server-admin";
    }
}
