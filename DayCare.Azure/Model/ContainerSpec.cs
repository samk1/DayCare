using DayCare.Azure.Constructs.Data;
using HashiCorp.Cdktf.Providers.Azurerm.ContainerApp;

namespace DayCare.Azure.Model
{
    public class ContainerSpec
    {
        private ContainerRegistry _containerRegistry;

        public string Image { get; }
        public Dictionary<string, string> EnvironmentVariables { get; }
        public string ContainerRegistryPassword => _containerRegistry.AdminPassword;
        public string ContainerRegistryServer => _containerRegistry.LoginServer;
        public string ContainerRegistryUsername => _containerRegistry.AdminUsername;

        internal ContainerSpec(
            string image, 
            Dictionary<string, string> environmentVariables,
            ContainerRegistry containerRegistry
        )
        {
            Image = image;
            EnvironmentVariables = environmentVariables;
            _containerRegistry = containerRegistry;
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