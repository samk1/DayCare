using Constructs;
using HashiCorp.Cdktf;

namespace DayCare.Azure.Model
{
    internal class AzureContext
    {
        private Node Node;

        public string SubscriptionId => Get("subscriptionId");
        public string TenantId => Get("tenantId");

        public AzureContext(Construct scope)
        {
            Node = scope.Node;
        }

        private string Get(string key)
        {
            return (string)Node.TryGetContext(key);
        }
    }
}
