using HashiCorp.Cdktf.Providers.Azurerm.ContainerApp;
using System.Collections.Generic;

namespace DayCare.Azure.Model
{
    public class ContainerSpec
    {
        public string Image { get; }
        public Dictionary<string, string> EnvironmentVariables { get; }

        public ContainerSpec(string image, Dictionary<string, string> environmentVariables)
        {
            Image = image;
            EnvironmentVariables = environmentVariables;
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