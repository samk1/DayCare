using System;
using DayCare.Azure;

namespace MyCompany.MyApp
{
    class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();

            _ = new InfrastuctureStack(
                scope: app,
                containerAppName: "daycare-web"
            );

            _ = new MigrationStack(
                scope: app,
                containerImage: Environment.GetEnvironmentVariable("CONTAINER_APP_IMAGE"),
                containerAppName: "daycare-web"
            );

            _ = new ApplicationStack(
                scope: app,
                containerImage: Environment.GetEnvironmentVariable("CONTAINER_APP_IMAGE"),
                containerAppName: "daycare-web"
            );

            app.Synth();
            Console.WriteLine("App synth complete");
        }
    }
}