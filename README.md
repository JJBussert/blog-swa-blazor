# blog-swa-blazor
Azure Static Web App Sample written in Blazor / C#

swa cli cannot handle dotnet watch when using /data-api/* calls


# Project Title

A brief description of what this project does and who it's for


# Installation
```
  winget install -e --id CoreyButler.NVMforWindows
  winget install -e --id Microsoft.DotNet.SDK.7
  winget install -e --id Microsoft.VisualStudioCode
  winget install -e --id Microsoft.WindowsTerminal
  winget install -e --id Pulumi.Pulumi
  dotnet tool install --global dotnet-ef
```

## Environment Variables

To run this project, you will need to add the following environment variables to your .env file

`DB_CONNECTION_STRING`
`AZURE_SUBSCRIPTION_ID`

https://learn.microsoft.com/en-us/azure/active-directory-b2c/configure-authentication-in-azure-static-app
https://learn.microsoft.com/en-us/azure/active-directory-b2c/enable-authentication-azure-static-app-options

pulumi new azure-csharp --dir Infra
pulumi login file://
$PULUMI_CONFIG_PASSPHRASE = "P@ssword!"

b2cTenant.BeforeDestroy += async () =>
    {
        // Delete the B2C tenant resource using the Azure .NET SDK
        var tenantId = await b2cTenant.TenantId.GetValueAsync();
        var credential = new AzureNativeCredential();

        var client = new AzureADTenantResourceProviderClient(credential);
        await client.Tenants.DeleteAsync(tenantId);
    };


az ad app permission admin-consent --id 93d04af4-0bab-4d8c-8ffd-df5c084400c3

    "infra-main-init": "cd Infra && pulumi login file:// && dotenv -e ../.env az login --subscription $AZURE_SUBSCRIPTION_ID && dotenv -e ../.env pulumi stack init local",
    "infra-main-up": "cd Infra && dotenv -e ../.env pulumi up",
    "infra-main-destroy": "cd Infra && dotenv -e ../.env pulumi destroy",