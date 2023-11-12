namespace DayCare.Azure.Stacks.Data
{
    using global::Constructs;
    using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermMssqlServer;

    internal class SqlServer
    {
        private readonly DataAzurermMssqlServer mssqlServer;

        private readonly string databaseName;

        public SqlServer(Construct scope, string appName)
        {
            var resourceGroup = new ResourceGroup(scope);
            var serverName = ServerName(appName);
            this.databaseName = DatabaseName(appName);

            this.mssqlServer = new DataAzurermMssqlServer(scope, "mssql-server", new DataAzurermMssqlServerConfig
            {
                Name = serverName,
                ResourceGroupName = resourceGroup.Name,
            });
        }

        public string ApplicationDatabaseConnectionString => $"Server=tcp:{this.mssqlServer.FullyQualifiedDomainName},1433;Initial Catalog={this.databaseName};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=\"Active Directory Default\";";

        public string ApplicationDatabaseName => this.databaseName;

        public string FullyQualifiedDomainName => this.mssqlServer.FullyQualifiedDomainName;

        public static string DatabaseName(string appName) => $"{appName}-database";

        public static string ServerName(string appName) => $"{appName}-sql-server";
    }
}
