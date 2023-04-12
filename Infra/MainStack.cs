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
public class MainStack : Stack
{

    [Output]
    public Output<string> ConnectionString { get; set; }

    [Output]
    public Output<string> ResourceGroupLink { get; set; }

    [Output]
    public Output<string> B2CLink { get; set; }

    [Output]
    public Output<string> B2CName { get; set; }

    [Output]
    public Output<string?> B2CId { get; set; }

    public MainStack()
    {
        var c = new Pulumi.Config();
        //string? swaAccessToken = c.RequireSecret("SWA_ACCESS_TOKEN");
        
        // Create an Azure Resource Group
        var rg = new ResourceGroup("jj-swa-rg");


        //var server = new Server("jj-swa-db", new ServerArgs
        //{
        //    ResourceGroupName = rg.Name,
        //    Location = rg.Location,
        //    AdministratorLogin = "myadmin",
        //    AdministratorLoginPassword = "P@ssword!",
        //    Version = "12.0"
        //});

        //var firewallRules = new List<FirewallRule>();
        //var devIps = c.Get("DEV_IPS");
        //if(!string.IsNullOrWhiteSpace(devIps))
        //{
        //    var ipAddresses = devIps.Split(',');

        //    foreach (var ipAddress in ipAddresses)
        //    {
        //        firewallRules.Add(new FirewallRule($"dev-access-{ipAddress}", new FirewallRuleArgs
        //        {
        //            ResourceGroupName = rg.Name,
        //            ServerName = server.Name,
        //            StartIpAddress = ipAddress,
        //            EndIpAddress = ipAddress
        //        }));
        //    }
        //}

        //// this checks the "Allow Azure services and resources to access the server" Exception under Networking
        //firewallRules.Add(new FirewallRule("allow-azure-services", new FirewallRuleArgs
        //{
        //    ResourceGroupName = rg.Name,
        //    ServerName = server.Name,
        //    StartIpAddress = "0.0.0.0",
        //    EndIpAddress = "0.0.0.0"
        //}));
        
        var b2cTenant = new B2CTenant("jjtestb2c", new B2CTenantArgs
        {
            Location = "United States",
            Properties = new CreateTenantRequestBodyPropertiesArgs
            {
                CountryCode = "US",
                DisplayName = "Contoso",
            },
            ResourceGroupName = rg.Name,
            ResourceName = rg.Name.Apply(rgName =>
                {
                    var suffix = rgName["jj-swa-rg".Length..];
                    return $"jjtestb2c{suffix}.onmicrosoft.com";
                }),
            Sku = new B2CResourceSKUArgs
            {
                Name = B2CResourceSKUName.PremiumP1,
                Tier = B2CResourceSKUTier.A0,
            },
        }, new CustomResourceOptions
        {
            CustomTimeouts = new CustomTimeouts
            {
                Create = TimeSpan.FromMinutes(10), // 10-minute create timeout
                Update = TimeSpan.FromMinutes(5),  // 5-minute update timeout
                Delete = TimeSpan.FromMinutes(5),  // 5-minute delete timeout
            }
        });

        //var AutoUserPasswords = Output.Create("P@ssword!");
        //var AutoEnabledUser = Output.Create(rg.Name.Apply(rgName =>
        //{
        //    var suffix = rgName["jj-swa-rg".Length..];
        //    return $"auto.enabled@jjtestb2c{suffix}.onmicrosoft.com";
        //}));
        //var autoEnabled = new User("auto.enabled", new UserArgs
        //{
        //    DisplayName = "auto.enabled",
        //    Password = AutoUserPasswords.Apply(p => p),
        //    UserPrincipalName = AutoEnabledUser.Apply(p => p),
        //    AccountEnabled = true
        //}, new CustomResourceOptions
        //{
        //    Provider = new Provider("b2c", new ProviderArgs
        //    {
        //        TenantId = b2cTenant.Id.Apply(id => id),
        //        MetadataHost = rg.Name.Apply(rgName =>
        //        {
        //            var suffix = rgName["jj-swa-rg".Length..];
        //            return $"jjtestb2c{suffix}.b2clogin.com/jjtestb2c{suffix}.onmicrosoft.com/v2.0/.well-known/openid-configuration";
        //        }), // Set the metadata_host argument
        //    }),
        //});

        //var database = new Database("dev", new DatabaseArgs
        //{
        //    ResourceGroupName = rg.Name,
        //    ServerName = server.Name,
        //    Location = rg.Location,
        //    Collation = "SQL_Latin1_General_CP1_CI_AS",
        //    CreateMode = CreateMode.Default
        //});

        //ConnectionString = Output.Tuple(server.Name, database.Name, server.AdministratorLogin).Apply(t =>
        //{
        //    var (serverName, databaseName, login) = t;
        //    return $"Server=tcp:{serverName}.database.windows.net,1433;Initial Catalog={databaseName};Persist Security Info=False;User ID={login};Password=P@ssword!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        //});

        ResourceGroupLink = Output.Tuple(rg.Name, c.RequireSecret("AZURE_SUBSCRIPTION_ID")).Apply(t =>
        {
            var (rgName, subscriptionId) = t;
            return $"https://portal.azure.com/#resource/subscriptions/{subscriptionId}/resourceGroups/{rgName}";
        });
        B2CLink = b2cTenant.Id.Apply(id =>
            Output.Format($"https://portal.azure.com/{id}#blade/Microsoft_AAD_B2CAdmin/TenantManagementMenuBlade/overview")
        );
        B2CId = b2cTenant.TenantId.Apply(id => id);
        B2CName = rg.Name.Apply(rgName =>
        {
            var suffix = rgName["jj-swa-rg".Length..];
            return $"jjtestb2c{suffix}.onmicrosoft.com";
        });

    }
}