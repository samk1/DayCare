using System;
using System.Collections.Generic;
using Constructs;
using DayCare.Azure.Constructs;
using DayCare.Azure.Model;
using HashiCorp.Cdktf;

namespace DayCare.Azure
{
    class MainStack : TerraformStack
    {

        public MainStack(
            Construct scope, 
            string containerAppImage, 
            string containerAppName
        ) : base(scope, "daycareStack")
        {
            var keyVaultSecrets = new KeyVaultSecrets(
                scope: this
            );

            var resourceGroup = new ResourceGroup(
                scope: this
            );

            var database = new Database(
                scope: this, 
                keyVaultSecrets: keyVaultSecrets,
                appName: containerAppName,
                resourceGroup: resourceGroup,
                directoryReadersGroupId: (string)scope.Node.TryGetContext("directoryReadersGroupId")
            );

            var webapp = new WebApp(
                scope: this,
                appName: containerAppName,
                resourceGroup: resourceGroup,
                spec: new ContainerSpec(
                    image: containerAppImage,
                    containerRegistry: new ContainerRegistry(this),
                    environmentVariables: new Dictionary<string, string>
                    {
                        { "ConnectionStrings__DefaultConnection", database.ConnectionString },
                        { "ASPNETCORE_ENVIRONMENT", "Development" },
                    }
                )
            );

            database.GrantAccess(webapp.ContainerAppName, "container-app-database-access");
        }
    }
}