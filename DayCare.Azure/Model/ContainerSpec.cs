namespace DayCare.Azure.Model
{
    using System.Collections.Generic;
    using DayCare.Azure.Constructs.Data;
    using HashiCorp.Cdktf.Providers.Azurerm.ContainerApp;

    internal class ContainerSpec
    {
        private readonly ContainerRegistry containerRegistry;

        internal ContainerSpec(
            string image,
            Dictionary<string, string> environmentVariables,
            ContainerRegistry containerRegistry)
        {
            this.Image = image;
            this.EnvironmentVariables = environmentVariables;
            this.containerRegistry = containerRegistry;
        }

        public string Image { get; }

        public Dictionary<string, string> EnvironmentVariables { get; }

        public string ContainerRegistryPassword => this.containerRegistry.AdminPassword;

        public string ContainerRegistryServer => this.containerRegistry.LoginServer;

        public string ContainerRegistryUsername => this.containerRegistry.AdminUsername;

        internal ContainerAppTemplateContainerEnv[] BuildEnvironmentVariables()
        {
            var env = new List<ContainerAppTemplateContainerEnv>();

            foreach (var environmentVariable in this.EnvironmentVariables)
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