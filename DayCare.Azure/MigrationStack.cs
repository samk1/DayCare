﻿using Constructs;
using DayCare.Azure;
using DayCare.Azure.Constructs;
using HashiCorp.Cdktf;
using HashiCorp.Cdktf.Providers.Azurerm.ContainerGroup;
using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermContainerGroup;
using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermMssqlServer;
using HashiCorp.Cdktf.Providers.Azurerm.MssqlServer;
using HashiCorp.Cdktf.Providers.Azurerm.UserAssignedIdentity;
using System;
using System.Collections.Generic;

namespace MyCompany.MyApp
{
    internal class MigrationStack : BaseAzureStack
    {
        public MigrationStack(
            Construct scope, 
            string containerAppName, 
            string containerImage
        ) : base(scope, "migrationsStack", "DayCare-Migrations")
        {
            var resourceGroup = new ResourceGroup(this);

            var containerRegistry = new ContainerRegistry(this);

            var mssqlServer = new DataAzurermMssqlServer(this, "mssql-server", new DataAzurermMssqlServerConfig
            {
                Name = Database.ServerName(containerAppName),
                ResourceGroupName = resourceGroup.Name,
            });

            var migrationsIdentity = new UserAssignedIdentity(this, $"{containerAppName}-migration-identity", new UserAssignedIdentityConfig
            {
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
                Name = $"{containerAppName}-migration-identity"
            });

            var databaseAccess = new DatabaseAccess(
                this, 
                $"{containerAppName}-migration-database-access", 
                resourceGroup,
                mssqlServer,
                migrationsIdentity.Name, 
                containerAppName
            );

            new ContainerGroup(this, $"{containerAppName}-migration-container-group", new ContainerGroupConfig
            {
                Name = $"{containerAppName}-database-migrations-container-group",
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
                RestartPolicy = "Never",
                OsType = "Linux",
                IpAddressType = "Public",
                DependsOn = new [] { databaseAccess },
                Identity = new ContainerGroupIdentity
                {
                    Type = "UserAssigned",
                    IdentityIds = new[] { migrationsIdentity.Id }
                },
                ImageRegistryCredential = new[]
                {
                    new ContainerGroupImageRegistryCredential
                    {
                        Server = containerRegistry.LoginServer,
                        Username = containerRegistry.AdminUsername,
                        Password = containerRegistry.AdminPassword
                    }
                },
                Container = new[]
                {
                    new ContainerGroupContainer()
                    {
                        Name = $"{containerAppName}-database-migrations-container",
                        Image = containerImage,
                        Cpu = 1,
                        Memory = 1,
                        Commands = new[] { "app/Migrations" },
                        EnvironmentVariables = new Dictionary<string, string>
                        {
                            {
                                "ConnectionStrings__DefaultConnection",
                                Database.ConnectionString(mssqlServer.FullyQualifiedDomainName, Database.DatabaseName(containerAppName))
                            }
                        }
                    }
                }
            });
        }
    }
}