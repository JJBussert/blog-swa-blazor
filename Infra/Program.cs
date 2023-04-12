using Pulumi;
using Pulumi.AzureNative.AzureActiveDirectory;
using System.Threading.Tasks;

class Program
{

    static Task<int> Main()
    {
        return Deployment.RunAsync<MainStack>();
    }

    //static Task<int> Main()
    //{
    //    // Define the stack transformation
    //    var stackTransformation = new ResourceTransformation((args) =>
    //    {
    //        // Get the resources in the stack
    //        //var resources = Deployment.Instance.;

    //        // If the resource is a B2CTenant and it is being destroyed, perform the custom activity
    //        if (args is B2CTenantArgs b2cTenantArgs && resources.TryGetResource(args.Name, out var resource) && resource.Delete)
    //        {
    //            // Perform the custom activity here
    //            // ...
    //        }

    //        // Return the original arguments without modifying them
    //        return Task.FromResult(args);
    //    });

    //    // Register the stack transformation with the deployment
    //    var deployment = new Deployment();deployment.
    //    deployment.RegisterResourceTransformation(stackTransformation);

    //    // Run the Pulumi stack
    //    return deployment.RunAsync<MainStack>();
    //}
}

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