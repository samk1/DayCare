using System;
using DayCare.Azure;
using DayCare.Azure.Model;
using HashiCorp.Cdktf;
using HashiCorp.Cdktf.Providers.Azuread.Provider;
using HashiCorp.Cdktf.Providers.Azurerm.Provider;

namespace MyCompany.MyApp
{
    class Program
    {
        public static void Main(string[] args)
        {
            App app = new App();
            
            new InfrastuctureStack(
                scope: app,
                containerAppName: "daycare-web"
            );

            new MigrationStack(
                scope: app,
                containerImage: Environment.GetEnvironmentVariable("CONTAINER_APP_IMAGE"),
                containerAppName: "daycare-web"
            );
            
            new MainStack(
                scope: app,
                containerImage: Environment.GetEnvironmentVariable("CONTAINER_APP_IMAGE"),
                containerAppName: "daycare-web"
            );

            app.Synth();
            Console.WriteLine("App synth complete");
        }
    }
}