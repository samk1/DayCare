namespace DayCare.Azure.Stacks.Model
{
    using Constructs;

    internal class AzureContext
    {
        private readonly Node node;

        public AzureContext(Construct scope)
        {
            this.node = scope.Node;
        }

        public string SubscriptionId => this.Get("subscriptionId");

        public string TenantId => this.Get("tenantId");

        private string Get(string key)
        {
            return (string)this.node.TryGetContext(key);
        }
    }
}