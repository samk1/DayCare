namespace DayCare.Azure.Stacks.Model
{
    using System.Collections.Generic;
    using HashiCorp.Cdktf.Providers.Azurerm.ContainerApp;

    internal class Container
    {
        public static ContainerAppTemplateContainerEnv[] BuildEnvironmentVariables(Dictionary<string, string> environmentVariables)
        {
            var env = new List<ContainerAppTemplateContainerEnv>();

            foreach (var environmentVariable in environmentVariables)
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