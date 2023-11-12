namespace DayCare.Azure.Model
{
    internal class Database
    {
        public static string ServerName(string appName) => $"{appName}-sql-server";

        public static string DatabaseName(string appName) => $"{appName}-database";

        public static string AdminIdentityName(string appName) => $"{appName}-sql-server-admin";

        public static string ConnectionString(string serverName, string databaseName)
        {
            return $"Server=tcp:{serverName},1433;Initial Catalog={databaseName};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=\"Active Directory Default\";";
        }
    }
}