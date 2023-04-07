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

return await Pulumi.Deployment.RunAsync(() =>
{

    IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddEnvFile("../.env")
        .AddEnvironmentVariables();

    IConfiguration configuration = configurationBuilder.Build();

    string? swaAccessToken = configuration["SWA_ACCESS_TOKEN"];
    string? subscriptionId = configuration["AZURE_SUBSCRIPTION"];
    string? devIps = configuration["DEV_IPS"];
    // Create an Azure Resource Group
    var rg = new ResourceGroup("jj-swa-rg");

    //var swa = new StaticSite("jj-swa", new StaticSiteArgs {
    //    Branch="main",
    //    BuildProperties = new StaticSiteBuildPropertiesArgs {
    //        AppLocation = "/web",
    //        ApiLocation = "/api",
    //        AppArtifactLocation = "/wwwroot",
    //        AppBuildCommand = "npm run build",
    //        ApiBuildCommand = "npm run build"
    //    },
    //    //RepositoryToken = swaAccessToken,
    //    RepositoryUrl = "https://github.com/jjbussell/blog-swa-blazor",
    //    ResourceGroupName = resourceGroup.Name,
    //    Sku = new SkuDescriptionArgs {
    //        Name = "Standard",
    //        Tier = "Standard"
    //    },
    //});

    var server = new Server("jj-swa-db", new ServerArgs
    {
        ResourceGroupName = rg.Name,
        Location = rg.Location,
        AdministratorLogin = "myadmin",
        AdministratorLoginPassword = "P@ssword!",
        Version = "12.0"
    });

    var firewallRules = new List<FirewallRule>();
    if(!string.IsNullOrWhiteSpace(devIps))
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
                var suffix = rgName.Substring("jj-swa-rg".Length);
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

    // var example = new Application("example", new()
    // {
    //     DisplayName = "example",
    //     IdentifierUris = new[]
    //     {
    //         "api://example-app",
    //     },
    //     //LogoImage = ReadFileBase64("/path/to/logo.png"),
    //     Owners = new[]
    //     {
    //         b2cTenant.Id.Apply(id => id),
    //     },
    //     SignInAudience = "AzureADMultipleOrgs",
    //     Api = new ApplicationApiArgs
    //     {
    //         MappedClaimsEnabled = true,
    //         RequestedAccessTokenVersion = 2,
    //         KnownClientApplications = new[]
    //         {
    //             swa.Id.Apply(id => id),
    //         },
    //         Oauth2PermissionScopes = new[]
    //         {
    //             new ApplicationApiOauth2PermissionScopeArgs
    //             {
    //                 AdminConsentDescription = "Allow the application to access example on behalf of the signed-in user.",
    //                 AdminConsentDisplayName = "Access example",
    //                 Enabled = true,
    //                 Id = "96183846-204b-4b43-82e1-5d2222eb4b9b",
    //                 Type = "User",
    //                 UserConsentDescription = "Allow the application to access example on your behalf.",
    //                 UserConsentDisplayName = "Access example",
    //                 Value = "user_impersonation",
    //             },
    //             new ApplicationApiOauth2PermissionScopeArgs
    //             {
    //                 AdminConsentDescription = "Administer the example application",
    //                 AdminConsentDisplayName = "Administer",
    //                 Enabled = true,
    //                 Id = "be98fa3e-ab5b-4b11-83d9-04ba2b7946bc",
    //                 Type = "Admin",
    //                 Value = "administer",
    //             },
    //         },
    //     },
    //     AppRoles = new[]
    //     {
    //         new ApplicationAppRoleArgs
    //         {
    //             AllowedMemberTypes = new[]
    //             {
    //                 "User",
    //                 "Application",
    //             },
    //             Description = "Admins can manage roles and perform all task actions",
    //             DisplayName = "Admin",
    //             Enabled = true,
    //             Id = "1b19509b-32b1-4e9f-b71d-4992aa991967",
    //             Value = "admin",
    //         },
    //         new ApplicationAppRoleArgs
    //         {
    //             AllowedMemberTypes = new[]
    //             {
    //                 "User",
    //             },
    //             Description = "ReadOnly roles have limited query access",
    //             DisplayName = "ReadOnly",
    //             Enabled = true,
    //             Id = "497406e4-012a-4267-bf18-45a1cb148a01",
    //             Value = "User",
    //         },
    //     },
    //     FeatureTags = new[]
    //     {
    //         new ApplicationFeatureTagArgs
    //         {
    //             Enterprise = true,
    //             Gallery = true,
    //         },
    //     },
    //     OptionalClaims = new ApplicationOptionalClaimsArgs
    //     {
    //         AccessTokens = new[]
    //         {
    //             new ApplicationOptionalClaimsAccessTokenArgs
    //             {
    //                 Name = "myclaim",
    //             },
    //             new ApplicationOptionalClaimsAccessTokenArgs
    //             {
    //                 Name = "otherclaim",
    //             },
    //         },
    //         IdTokens = new[]
    //         {
    //             new ApplicationOptionalClaimsIdTokenArgs
    //             {
    //                 Name = "userclaim",
    //                 Source = "user",
    //                 Essential = true,
    //                 AdditionalProperties = new[]
    //                 {
    //                     "emit_as_roles",
    //                 },
    //             },
    //         },
    //         Saml2Tokens = new[]
    //         {
    //             new ApplicationOptionalClaimsSaml2TokenArgs
    //             {
    //                 Name = "samlexample",
    //             },
    //         },
    //     },
    //     RequiredResourceAccesses = new[]
    //     {
    //         new ApplicationRequiredResourceAccessArgs
    //         {
    //             ResourceAppId = "00000003-0000-0000-c000-000000000000",
    //             ResourceAccesses = new[]
    //             {
    //                 new ApplicationRequiredResourceAccessResourceAccessArgs
    //                 {
    //                     Id = "df021288-bdef-4463-88db-98f22de89214",
    //                     Type = "Role",
    //                 },
    //                 new ApplicationRequiredResourceAccessResourceAccessArgs
    //                 {
    //                     Id = "b4e74841-8e56-480b-be8b-910348b18b4c",
    //                     Type = "Scope",
    //                 },
    //             },
    //         },
    //         new ApplicationRequiredResourceAccessArgs
    //         {
    //             ResourceAppId = "c5393580-f805-4401-95e8-94b7a6ef2fc2",
    //             ResourceAccesses = new[]
    //             {
    //                 new ApplicationRequiredResourceAccessResourceAccessArgs
    //                 {
    //                     Id = "594c1fb6-4f81-4475-ae41-0c394909246c",
    //                     Type = "Role",
    //                 },
    //             },
    //         },
    //     },
    //     Web = new ApplicationWebArgs
    //     {
    //         HomepageUrl = "https://app.example.net",
    //         LogoutUrl = "https://app.example.net/logout",
    //         RedirectUris = new[]
    //         {
    //             "https://app.example.net/account",
    //         },
    //         ImplicitGrant = new ApplicationWebImplicitGrantArgs
    //         {
    //             AccessTokenIssuanceEnabled = true,
    //             IdTokenIssuanceEnabled = true,
    //         },
    //     },
    // });


    var database = new Database("dev", new DatabaseArgs
    {
        ResourceGroupName = rg.Name,
        ServerName = server.Name,
        Location = rg.Location,
        Collation = "SQL_Latin1_General_CP1_CI_AS",
        CreateMode = CreateMode.Default
    });

    var connectionString = Output.Tuple(server.Name, database.Name, server.AdministratorLogin).Apply(t =>
    {
        var (serverName, databaseName, login) = t;
        return $"Server=tcp:{serverName}.database.windows.net,1433;Initial Catalog={databaseName};Persist Security Info=False;User ID={login};Password=P@ssword!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
    });
    var resourceGroupLink = rg.Name.Apply(rg =>
        Output.Format($"https://portal.azure.com/#resource/subscriptions/{subscriptionId}/resourceGroups/{rg}")
    );
    var b2cLink = b2cTenant.Id.Apply(id =>
        Output.Format($"https://portal.azure.com/{id}#blade/Microsoft_AAD_B2CAdmin/TenantManagementMenuBlade/overview")
    );

    return new Dictionary<string, object?>
    {
        { "connectionString", connectionString },
        { "resourceGroupLink", resourceGroupLink },
        { "b2cLink", b2cLink }
    };
});