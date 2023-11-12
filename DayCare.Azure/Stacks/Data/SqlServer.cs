namespace DayCare.Azure.Stacks.Data
{
    using global::Constructs;
    using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermMssqlServer;

    internal class SqlServer
    {
        private readonly DataAzurermMssqlServer mssqlServer;

        private readonly string databaseName;

        private SqlServer(Construct scope, string appName)
        {
            var resourceGroup = ResourceGroup.FindOrCreate(scope);
            var serverName = ServerName(appName);
            this.databaseName = DatabaseName(appName);

            this.mssqlServer = new DataAzurermMssqlServer(scope, "mssql-server", new DataAzurermMssqlServerConfig
            {
                Name = serverName,
                ResourceGroupName = resourceGroup.Name,
            });
        }

        private SqlServer(DataAzurermMssqlServer mssqlServer, string databaseName)
        {
            this.mssqlServer = mssqlServer;
            this.databaseName = databaseName;
        }

        public string ApplicationDatabaseConnectionString => $"Server=tcp:{this.mssqlServer.FullyQualifiedDomainName},1433;Initial Catalog={this.databaseName};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=\"Active Directory Default\";";

        public string ApplicationDatabaseName => this.databaseName;

        public string FullyQualifiedDomainName => this.mssqlServer.FullyQualifiedDomainName;

        public static SqlServer FindOrCreate(Construct scope, string appName)
        {
            var construct = scope.Node.TryFindChild("mssql-server");

            if (construct != null)
            {
                return new SqlServer((DataAzurermMssqlServer)construct, DatabaseName(appName));
            }

            return new SqlServer(scope, appName);
        }

        public static string DatabaseName(string appName) => $"{appName}-database";

        public static string ServerName(string appName) => $"{appName}-sql-server";
    }
}
