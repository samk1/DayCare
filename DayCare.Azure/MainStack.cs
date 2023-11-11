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

        public MainStack(Construct scope, string containerAppImage, AzureContext context) : base(scope, "daycareStack")
        {
            var keyVaultSecrets = new KeyVaultSecrets(this);

            var database = new Database(
                scope: this, 
                keyVaultSecrets: keyVaultSecrets, 
                azureContext: context
            );

            new WebApp(
                scope: this, 
                azureContext: context, 
                keyVaultSecrets: keyVaultSecrets, 
                spec: new ContainerSpec(
                    image: containerAppImage,
                    environmentVariables: new Dictionary<string, string>
                    {
                        { "ConnectionStrings__DefaultConnection", database.ConnectionString },
                        { "ASPNETCORE_ENVIRONMENT", "Development" },
                    }
                )
            );

            database.GrantAccess(context.AppName);
        }
    }
}