using System;
using Constructs;
using HashiCorp.Cdktf;

namespace MyCompany.MyApp
{
    class Program
    {
        public static void Main(string[] args)
        {
            App app = new App();
            MainStack stack = new MainStack(app, "DayCare.Azure");
            new CloudBackend(stack, new CloudBackendConfig { Hostname = "app.terraform.io", Organization = "DayCare", Workspaces = new NamedCloudWorkspace("DayCare.Azure") });
            app.Synth();
            Console.WriteLine("App synth complete");
        }
    }
}