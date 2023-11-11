using DayCare.Azure.Constructs;
using HashiCorp.Cdktf.Providers.Azurerm.ContainerApp;
using System.Collections.Generic;

namespace DayCare.Azure.Model
{
    public class ContainerSpec
    {
        private ContainerRegistry ContainerRegistry;

        public string Image { get; }
        public Dictionary<string, string> EnvironmentVariables { get; }
        public string ContainerRegistryPassword => ContainerRegistry.AdminPassword;
        public string ContainerRegistryServer => ContainerRegistry.LoginServer;
        public string ContainerRegistryUsername => ContainerRegistry.AdminUsername;

        internal ContainerSpec(
            string image, 
            Dictionary<string, string> environmentVariables,
            ContainerRegistry containerRegistry
        )
        {
            Image = image;
            EnvironmentVariables = environmentVariables;
            ContainerRegistry = containerRegistry;
        }

        internal ContainerAppTemplateContainerEnv[] BuildEnvironmentVariables()
        {
            var env = new List<ContainerAppTemplateContainerEnv>();

            foreach (var environmentVariable in EnvironmentVariables)
            {
                env.Add(new ContainerAppTemplateContainerEnv
                {
                    Name = environmentVariable.Key,
                    Value = environmentVariable.Value,
                });
            }

            return env.ToArray();
        }
    }
}