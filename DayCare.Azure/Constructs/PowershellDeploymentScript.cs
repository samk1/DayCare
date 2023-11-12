using DayCare.Azure.Constructs.Data;
using HashiCorp.Cdktf.Providers.Azurerm.ResourceDeploymentScriptAzurePowerShell;

namespace DayCare.Azure.Constructs;

internal class PowershellDeploymentScript : Construct, ITerraformDependable
{
    private ResourceDeploymentScriptAzurePowerShell _resourceDeploymentScriptAzurePowerShell;

    public ITerraformDependable Dependable => _resourceDeploymentScriptAzurePowerShell;

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

        _resourceDeploymentScriptAzurePowerShell = new ResourceDeploymentScriptAzurePowerShell(
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

    public string Fqn => _resourceDeploymentScriptAzurePowerShell.Fqn;
}