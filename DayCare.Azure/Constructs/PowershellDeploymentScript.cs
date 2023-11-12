using Constructs;
using DayCare.Azure.Constructs.Data;
using HashiCorp.Cdktf;
using HashiCorp.Cdktf.Providers.Azurerm.ResourceDeploymentScriptAzurePowerShell;
using System.Collections.Generic;

namespace DayCare.Azure.Constructs
{
    internal class PowershellDeploymentScript : Construct, ITerraformDependable
    {
        private ResourceDeploymentScriptAzurePowerShell resourceDeploymentScriptAzurePowerShell;

        public ITerraformDependable Dependable => resourceDeploymentScriptAzurePowerShell;

        public PowershellDeploymentScript(
            Construct scope, 
            string name, 
            string embeddedResourceName, 
            string principalId,
            Dictionary<string, string> parameters,
            ResourceGroup resourceGroup
        ) : base(scope, $"${name}-deployment-script")
        {
            var arguments = new List<string>();
            foreach (var parameter in parameters)
            {
                arguments.Add($"-{parameter.Key} '{parameter.Value}'");
            }

            resourceDeploymentScriptAzurePowerShell = new ResourceDeploymentScriptAzurePowerShell(
                this,
                $"{name}-deployment-script-resource",
                new ResourceDeploymentScriptAzurePowerShellConfig
                {
                    Location = resourceGroup.Location,
                    ResourceGroupName = resourceGroup.Name,
                    RetentionInterval = "P1D",
                    Version = "9.7",
                    CleanupPreference = "Always",
                    Name = $"{name}-deployment-script",
                    CommandLine = string.Join(" ", arguments),
                    ScriptContent = Resources.GetEmbeddedResource(embeddedResourceName),
                    Identity = new ResourceDeploymentScriptAzurePowerShellIdentity
                    {
                        Type = "UserAssigned",
                        IdentityIds = new string[] { principalId }
                    },
                    ForceUpdateTag = System.Guid.NewGuid().ToString(),
                }
            );
        }

        public string Fqn => resourceDeploymentScriptAzurePowerShell.Fqn;
    }
}
