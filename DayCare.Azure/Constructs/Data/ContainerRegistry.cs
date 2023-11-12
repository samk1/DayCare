using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermContainerRegistry;

namespace DayCare.Azure.Constructs.Data;

internal class ContainerRegistry : Construct
{
    private DataAzurermContainerRegistry _dataAzurermContainerRegistry;

    public string AdminUsername => _dataAzurermContainerRegistry.AdminUsername;
    public string AdminPassword => _dataAzurermContainerRegistry.AdminPassword;
    public string LoginServer => _dataAzurermContainerRegistry.LoginServer;


    public ContainerRegistry(Construct scope) : base(scope, "container-registry")
    {
        var containerRegistryContext = (Dictionary<string, object>)scope.Node.TryGetContext("containerRegistry");

        _dataAzurermContainerRegistry = new DataAzurermContainerRegistry(this, "container-registry", new DataAzurermContainerRegistryConfig
        {
            Name = (string)containerRegistryContext["name"],
            ResourceGroupName = (string)containerRegistryContext["resourceGroupName"],
        });
    }
}