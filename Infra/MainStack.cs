using Microsoft.Extensions.Configuration;
using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Resources.Inputs;
using Pulumi.AzureNative.Sql;
using Pulumi.AzureNative.Sql.Inputs;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;
using Pulumi.AzureNative.Aad;
using Pulumi.AzureNative.Aad.Inputs;
using Pulumi.AzureNative.AzureActiveDirectory;
using Pulumi.AzureNative.AzureActiveDirectory.Inputs;
using System.Collections.Generic;
using System.IO;
using System;
using Pulumi.AzureAD;
using Pulumi.AzureAD.Inputs;
using Pulumi.AzureNative.CustomProviders;

public class MainStack : Stack
{

    [Output]
    public Output<string> ConnectionString { get; set; }

    [Output]
    public Output<string> ResourceGroupLink { get; set; }

    [Output]
    public Output<string> DemoUser { get; set; }

    public MainStack()
    {
        var c = new Pulumi.Config();
        var subscriptionId = c.Require("AZURE_SUBSCRIPTION_ID");
        //string? swaAccessToken = c.RequireSecret("SWA_ACCESS_TOKEN");
        
        // Create an Azure Resource Group
        var rg = new ResourceGroup("jj-swa-rg", new ResourceGroupArgs
        {
            Location = "EastUS2"
        });


        var server = new Server("jj-swa-db", new ServerArgs
        {
            ResourceGroupName = rg.Name,
            Location = rg.Location,
            AdministratorLogin = "myadmin",
            AdministratorLoginPassword = "P@ssword!",
            Version = "12.0"
        });

        var firewallRules = new List<FirewallRule>();
        var devIps = c.Get("DEV_IPS");
        if (!string.IsNullOrWhiteSpace(devIps))
        {
            var ipAddresses = devIps.Split(',');

            foreach (var ipAddress in ipAddresses)
            {
                firewallRules.Add(new FirewallRule($"dev-access-{ipAddress}", new FirewallRuleArgs
                {
                    ResourceGroupName = rg.Name,
                    ServerName = server.Name,
                    StartIpAddress = ipAddress,
                    EndIpAddress = ipAddress
                }));
            }
        }

        // this checks the "Allow Azure services and resources to access the server" Exception under Networking
        firewallRules.Add(new FirewallRule("allow-azure-services", new FirewallRuleArgs
        {
            ResourceGroupName = rg.Name,
            ServerName = server.Name,
            StartIpAddress = "0.0.0.0",
            EndIpAddress = "0.0.0.0"
        }));

        var RgSuffix = rg.Name.Apply(rgName =>
                {
                    return rgName["jj-swa-rg".Length..];
                });

        var HighlySecurePassword = Output.Create("P@ssword!");
        var aadDomains = GetDomains.Invoke();
        var demoUser = new User("demo.user", new UserArgs
        {
            DisplayName = RgSuffix.Apply(s => $"demo.user.{s}"),
            Password = HighlySecurePassword.Apply(p => p),
            UserPrincipalName = Output.Tuple(aadDomains, RgSuffix).Apply(t => 
            {
                var (domains, suffix) = t;
                return $"demo.user.{suffix}@{domains.Domains.First().DomainName}";
            }),
            AccountEnabled = true
        });

        var database = new Database("dev", new DatabaseArgs
        {
            ResourceGroupName = rg.Name,
            ServerName = server.Name,
            Location = rg.Location,
            Collation = "SQL_Latin1_General_CP1_CI_AS",
            CreateMode = CreateMode.Default
        });

        ConnectionString = Output.Tuple(server.Name, database.Name, server.AdministratorLogin).Apply(t =>
        {
            var (serverName, databaseName, login) = t;
            return $"Server=tcp:{serverName}.database.windows.net,1433;Initial Catalog={databaseName};Persist Security Info=False;User ID={login};Password=P@ssword!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        });

        ResourceGroupLink = rg.Name.Apply(rgName =>
        {
            return $"https://portal.azure.com/#resource/subscriptions/{subscriptionId}/resourceGroups/{rgName}";
        });

        DemoUser = demoUser.UserPrincipalName;
    }
}