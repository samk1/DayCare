namespace DayCare.Azure.Model
{
    internal class AzureContext
    {
        private Node _node;

        public string SubscriptionId => Get("subscriptionId");
        public string TenantId => Get("tenantId");

        public AzureContext(Construct scope)
        {
            _node = scope.Node;
        }

        private string Get(string key)
        {
            return (string)_node.TryGetContext(key);
        }
    }
}
