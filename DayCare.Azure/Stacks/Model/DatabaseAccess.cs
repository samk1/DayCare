namespace DayCare.Azure.Stacks.Model
{
    using System.Collections.Generic;
    using Constructs;
    using DayCare.Azure.Stacks.Data;
    using HashiCorp.Cdktf.Providers.Azurerm.ResourceDeploymentScriptAzurePowerShell;

    internal class DatabaseAccess
    {
        public static ResourceDeploymentScriptAzurePowerShell CreateDeploymentScript(
            Construct scope,
            string containerAppName,
            string managedIdentityName,
            string[] roles)
        {
            var resourceGroup = ResourceGroup.FindOrCreate(scope);
            var sqlServerAdmin = new SqlServerAdmin(scope, containerAppName);
            var sqlServer = SqlServer.FindOrCreate(scope, containerAppName);

            var parameters = new Dictionary<string, string>
            {
                { "ManagedIdentityName", managedIdentityName },
                { "ServerName", sqlServer.FullyQualifiedDomainName },
                { "DatabaseName", sqlServer.ApplicationDatabaseName },
                { "Roles", string.Join(",", roles) },
            };

            var arguments = new List<string>();
            foreach (var parameter in parameters)
            {
                arguments.Add($"-{parameter.Key} '{parameter.Value}'");
            }

            return new ResourceDeploymentScriptAzurePowerShell(
                scope,
                $"{scope.Node.Id}-create-database-user-script",
                new ResourceDeploymentScriptAzurePowerShellConfig
                {
                    Location = resourceGroup.Location,
                    ResourceGroupName = resourceGroup.Name,
                    RetentionInterval = "P1D",
                    Version = "9.7",
                    CleanupPreference = "Always",
                    Name = $"grant-database-access-{managedIdentityName}",
                    CommandLine = string.Join(" ", arguments),
                    ScriptContent = Resources.GetEmbeddedResource("DayCare.Azure.Scripts.CreateDatabaseUser.ps1"),
                    Identity = new ResourceDeploymentScriptAzurePowerShellIdentity
                    {
                        Type = "UserAssigned",
                        IdentityIds = new string[] { sqlServerAdmin.Id },
                    },
                    ForceUpdateTag = System.Guid.NewGuid().ToString(),
                });
        }
    }
}