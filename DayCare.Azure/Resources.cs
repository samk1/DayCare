using System;
using System.Linq;

namespace DayCare.Azure
{
    internal static class Resources
    {
        public static string GetEmbeddedResource(string resourceName)
        {
            var assembly = typeof(Resources).Assembly;
            var resourceNames = assembly.GetManifestResourceNames();
            var resource = resourceNames.FirstOrDefault(r => r.EndsWith(resourceName));
            if (resource == null)
            {
                throw new Exception($"Could not find embedded resource {resourceName}");
            }
            using var stream = assembly.GetManifestResourceStream(resource);
            using var reader = new System.IO.StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
