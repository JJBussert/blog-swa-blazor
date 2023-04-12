
using Pulumi.Automation;
using CommandLine;
using Pulumi;
using System.Net.Http.Headers;
using static Microsoft.Graph.CoreConstants;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Azure.Identity;
using System.Diagnostics;

namespace Infra.Cli
{
    public class CliOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option('e', "environment", Required = false, HelpText = "the pulumi environment, default is 'local'", Default = "local")]
        public string Environment { get; set; } = string.Empty;

    }

    [Verb("main-up", HelpText = "Set output to verbose messages.")]
    public class MainUpCliOptions : CliOptions
    {
        public MainUpCliOptions() { }
    }

    [Verb("main-destroy", HelpText = "Set output to verbose messages.")]
    public class MainDestroyCliOptions : CliOptions
    {
        public MainDestroyCliOptions() { }
    }

    [Verb("b2c-up", HelpText = "Set output to verbose messages.")]
    public class B2CUpCliOptions : CliOptions
    {
        public B2CUpCliOptions() { }
    }

    [Verb("b2c-destroy", HelpText = "Set output to verbose messages.")]
    public class B2CDestroyCliOptions : CliOptions
    {
        public B2CDestroyCliOptions() { }
    }

    class Program
    {
        static string envFile = @".env"; // Replace with the path to your .env file

        static async Task Main(string[] args)
        {
            //var config = new ConfigurationBuilder()
            //    .SetBasePath(Directory.GetCurrentDirectory())
            //    .AddEnvFile("../.env")
            //    .AddEnvironmentVariables()
            //    .Build();

            var parsedArgs = Parser.Default.ParseArguments<MainUpCliOptions, MainDestroyCliOptions, B2CUpCliOptions, B2CDestroyCliOptions>(args);

            parsedArgs.WithNotParsed((e) => throw new Exception("bad execution"));
            await parsedArgs.WithParsedAsync<MainUpCliOptions>(RunMainUp);
            await parsedArgs.WithParsedAsync<MainDestroyCliOptions>(RunMainDestroy);
            await parsedArgs.WithParsedAsync<B2CUpCliOptions>(RunB2CUp);
            await parsedArgs.WithParsedAsync<B2CDestroyCliOptions>(RunB2CDestroy);
        }
        static async Task RunMainUp(MainUpCliOptions opts)
        {
            var existingValues = await GetEnv();
            var mainStack = await GetStack<MainStack>(existingValues, opts.Environment, "main");

            var upOpts = new UpOptions
            {
                OnStandardOutput = Console.WriteLine,
                OnStandardError = Console.Error.WriteLine,
                LogVerbosity = opts.Verbose ? 3 : null
            };

            var upResult = await mainStack.UpAsync(upOpts);

            var newValues = upResult.Outputs.ToDictionary(kv => kv.Key.ToUpper(), kv => kv.Value.Value.ToString());
            var updatedValues = existingValues.ToDictionary(entry => entry.Key, entry => entry.Value);

            foreach (var entry in newValues.Where(v => v.Value != null))
            {
                if (updatedValues.ContainsKey(entry.Key))
                {
                    updatedValues[entry.Key] = entry.Value;
                }
                else
                {
                    updatedValues.Add(entry.Key, entry.Value);
                }
            }

            var envFileContents = string.Join("\n", updatedValues.Select(kv => $"{kv.Key}={kv.Value}"));
            await File.WriteAllTextAsync(envFile, envFileContents);
        }

        static async Task RunB2CUp(B2CUpCliOptions opts)
        {
            var existingValues = await GetEnv();

            await B2CTenantInit(existingValues["B2CNAME"]!, existingValues["AZ_CLI_TOKEN"]!);
            //var flows = await GetB2CUserFlows(existingValues["B2CNAME"]!, existingValues["AZ_CLI_TOKEN"]!);
            //if(!flows.Any(flow => flow.id == "B2C_1_susi"))
            //{
            //    await B2CUserFlow(existingValues["B2CNAME"]!, existingValues["AZ_CLI_TOKEN"]!);
            //}
            

            var b2cStack = await GetStack<B2CStack>(existingValues, opts.Environment, "b2c");
            var upOpts = new UpOptions
            {
                OnStandardOutput = Console.WriteLine,
                OnStandardError = Console.Error.WriteLine,
                LogVerbosity = opts.Verbose ? 3 : null
            };

            var upResult = await b2cStack.UpAsync(upOpts);

            var newValues = upResult.Outputs.ToDictionary(kv => kv.Key.ToUpper(), kv => kv.Value.Value.ToString());
            var updatedValues = existingValues.ToDictionary(entry => entry.Key, entry => entry.Value);

            foreach (var entry in newValues.Where(v => v.Value != null))
            {
                if (updatedValues.ContainsKey(entry.Key))
                {
                    updatedValues[entry.Key] = entry.Value;
                }
                else
                {
                    updatedValues.Add(entry.Key, entry.Value);
                }
            }

            GrantConsent(updatedValues["AUTOREGCLIENTID"]!);

            await GetAppRegistrations(updatedValues["AUTOREGCLIENTID"]!, updatedValues["AUTOREGCLIENTSECRET"]!, updatedValues["B2CID"]!);

            var envFileContents = string.Join("\n", updatedValues.Select(kv => $"{kv.Key}={kv.Value}"));
            await File.WriteAllTextAsync(envFile, envFileContents);
        }
        static async Task RunMainDestroy(MainDestroyCliOptions opts)
        {
            var existingValues = await GetEnv();
            var mainStack = await GetStack<MainStack>(existingValues, opts.Environment, "main");

            var destroyOpts = new DestroyOptions
            {
                OnStandardOutput = Console.WriteLine,
                OnStandardError = Console.Error.WriteLine,
                LogVerbosity = opts.Verbose ? 3 : null
            };

            var destroyResult = await mainStack.DestroyAsync(destroyOpts);
        }
        static async Task RunB2CDestroy(B2CDestroyCliOptions opts)
        {
            var existingValues = await GetEnv();

            var b2cStack = await GetStack<B2CStack>(existingValues, opts.Environment, "b2c");

            var destroyOpts = new DestroyOptions
            {
                OnStandardOutput = Console.WriteLine,
                OnStandardError = Console.Error.WriteLine,
                LogVerbosity = opts.Verbose ? 3 : null
            };

            var destroyResult = await b2cStack.DestroyAsync(destroyOpts);


            var flows = await GetB2CUserFlows(existingValues["B2CNAME"]!, existingValues["AZ_CLI_TOKEN"]!);
            foreach (var flow in flows)
            {
                await DeleteUserFlow(existingValues["B2CNAME"]!, existingValues["AZ_CLI_TOKEN"]!, flow.id);
            }
        }
        static async Task<WorkspaceStack> GetStack<T>(Dictionary<string, string?> existingValues, string envName, string stackLabel)
            where T : Stack, new()
        {
            //IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
            //    .SetBasePath(Directory.GetCurrentDirectory())
            //    .AddEnvFile("../.env")
            //    .AddEnvironmentVariables();

            //IConfiguration configuration = configurationBuilder.Build();

            var stackName = $"{stackLabel}-{envName}";

            var program = PulumiFn.Create<T>();
            var stackArgs = new InlineProgramArgs("blog-swa-blazor", stackName, program)
            {
                WorkDir = @"C:\_\JJBussert\blog-swa-blazor\Infra",
                EnvironmentVariables = existingValues,
                //BackendUrl = backendUrl
            };
            using var pulumiStack = await LocalWorkspace.CreateOrSelectStackAsync(stackArgs);
            await pulumiStack.Workspace.InstallPluginAsync("azure", "v4.0.0");
            await pulumiStack.Workspace.InstallPluginAsync("azure-native", "v1.0.0");
            await pulumiStack.SetConfigAsync("azure-native:location", new ConfigValue("EastUS2"));
            foreach (var existingValue in existingValues.Where(v => v.Value != null))
            {
                await pulumiStack.SetConfigAsync(existingValue.Key, new ConfigValue(existingValue.Value!, existingValue.Value!.ToUpper().Contains("PASSWORD=")));
            }

            return pulumiStack;
        }

        static async Task<Dictionary<string, string?>> GetEnv()
        {
            //var envFile = @"C:\_\JJBussert\blog-swa-blazor\.env"; // Replace with the path to your .env file

            var existingValues = new Dictionary<string, string?>();
            if (File.Exists(envFile))
            {
                var lines = await File.ReadAllLinesAsync(envFile);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;
                    // Split on the first = only, and trim surrounding whitespace
                    var parts = line.Split('=', 2);
                    existingValues[parts[0].ToUpper()] = parts[1].Trim('"');
                }
            }

            return existingValues;
        }

        public static async Task B2CTenantInit(string B2CTenantName, string accessToken)
        {
            var httpClient = new HttpClient();

            // Invoke tenant initialization which happens through the portal automatically.
            // Ref: https://stackoverflow.com/questions/67706798/creation-of-the-b2c-extensions-app-by-script
            Console.WriteLine("Invoking tenant initialization...");
            var url = $"https://main.b2cadmin.ext.azure.com/api/tenants/GetAndInitializeTenantPolicy?tenantId={B2CTenantName}&skipInitialization=false";
            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var res = await httpClient.SendAsync(req);

            if (!res.IsSuccessStatusCode)
            {
                throw new Exception(res.StatusCode.ToString());
            }
        }

        public static async Task<List<UserFlow>> GetB2CUserFlows(string B2CTenantName, string accessToken)
        {
            var httpClient = new HttpClient();

            Console.WriteLine("GetB2CUserFlows...");
            var url = $"https://main.b2cadmin.ext.azure.com/api/adminuserjourneys?tenantId={B2CTenantName}&skipInitialization=false";
            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var res = await httpClient.SendAsync(req);

            if (!res.IsSuccessStatusCode)
            {
                throw new Exception(res.StatusCode.ToString());
            }
            var jsonContent = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<UserFlow>>(jsonContent) ?? new List<UserFlow>();
        }

        public static async Task DeleteUserFlow(string B2CTenantName, string accessToken, string flowId)
        {
            var httpClient = new HttpClient();

            Console.WriteLine("DeleteUserFlow...");
            var url = $"https://main.b2cadmin.ext.azure.com/api/adminuserjourneys/{flowId}?tenantId={B2CTenantName}&skipInitialization=false";
            var req = new HttpRequestMessage(HttpMethod.Delete, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var res = await httpClient.SendAsync(req);

            if (!res.IsSuccessStatusCode)
            {
                throw new Exception(res.StatusCode.ToString());
            }
        }

        public static async Task B2CUserFlow(string B2CTenantName, string accessToken)
        {
            var httpClient = new HttpClient();

            Console.WriteLine("UserFlow...");
            var url = $"https://main.b2cadmin.ext.azure.com/api/adminuserjourneys?tenantId={B2CTenantName}";
            var req = new HttpRequestMessage(HttpMethod.Post, url);
            var jsonPayload = @"{""id"":""susi"",""type"":""B2CSignUpOrSignInWithPassword_V3"",""protocol"":""OpenIdConnect"",""booleanData"":{""mfa"":false,""mfaDisable"":true,""mfaConditional"":false,""mfaSignIn"":false,""mfaSignUp"":false,""allowEmailFactor"":true,""allowPhoneFactor"":false,""allowPhoneFactorSMS"":false,""allowPhoneFactorCall"":false,""allowTotp"":false},""idpData"":{""idpSelection"":[{""displayName"":""Email signup"",""label"":""Email signup"",""technicalProfileId"":""LocalAccountSignUpWithLogonEmail-Selection"",""providerDisplayName"":""Local Account"",""appId"":null,""appSecret"":""******"",""disableToggle"":false,""disablePhoneMfa"":false}]},""tokenLifetimeData"":{},""tokenClaimsData"":{},""ssoSessionData"":{},""userAttributesData"":{""attributes"":[{""id"":""displayName"",""displayName"":""Display Name"",""label"":""Display Name"",""dataType"":1,""userInputType"":1,""allowChangeUserInputType"":true,""adminHelpText"":""Display Name of the User."",""userHelpText"":""Your display name."",""userAttributeOptions"":[],""allowEdit"":false,""optional"":true,""mandatoryInUserJourney"":false,""requireVerification"":null,""attributeType"":0},{""id"":""displayName"",""displayName"":""Display Name"",""label"":""Display Name"",""dataType"":1,""userInputType"":1,""allowChangeUserInputType"":true,""adminHelpText"":""Display Name of the User."",""userHelpText"":""Your display name."",""userAttributeOptions"":[],""allowEdit"":false,""optional"":true,""mandatoryInUserJourney"":false,""requireVerification"":null,""attributeType"":0},{""id"":""email"",""displayName"":""Email Address"",""label"":""Email Address"",""dataType"":1,""userInputType"":10,""allowChangeUserInputType"":false,""adminHelpText"":""Email address of the user."",""userHelpText"":""Email address that can be used to contact you."",""userAttributeOptions"":[],""allowEdit"":false,""optional"":false,""mandatoryInUserJourney"":false,""requireVerification"":true,""attributeType"":0},{""id"":""email"",""displayName"":""Email Address"",""label"":""Email Address"",""dataType"":1,""userInputType"":10,""allowChangeUserInputType"":false,""adminHelpText"":""Email address of the user."",""userHelpText"":""Email address that can be used to contact you."",""userAttributeOptions"":[],""allowEdit"":false,""optional"":false,""mandatoryInUserJourney"":false,""requireVerification"":true,""attributeType"":0},{""id"":""email"",""displayName"":""Email Address"",""label"":""Email Address"",""dataType"":1,""userInputType"":10,""allowChangeUserInputType"":false,""adminHelpText"":""Email address of the user."",""userHelpText"":""Email address that can be used to contact you."",""userAttributeOptions"":[],""allowEdit"":false,""optional"":false,""mandatoryInUserJourney"":false,""requireVerification"":true,""attributeType"":0},{""id"":""email"",""displayName"":""Email Address"",""label"":""Email Address"",""dataType"":1,""userInputType"":10,""allowChangeUserInputType"":false,""adminHelpText"":""Email address of the user."",""userHelpText"":""Email address that can be used to contact you."",""userAttributeOptions"":[],""allowEdit"":false,""optional"":false,""mandatoryInUserJourney"":false,""requireVerification"":true,""attributeType"":0},{""id"":""email"",""displayName"":""Email Address"",""label"":""Email Address"",""dataType"":1,""userInputType"":10,""allowChangeUserInputType"":false,""adminHelpText"":""Email address of the user."",""userHelpText"":""Email address that can be used to contact you."",""userAttributeOptions"":[],""allowEdit"":false,""optional"":false,""mandatoryInUserJourney"":false,""requireVerification"":true,""attributeType"":0},{""id"":""displayName"",""displayName"":""Display Name"",""label"":""Display Name"",""dataType"":1,""userInputType"":1,""allowChangeUserInputType"":true,""adminHelpText"":""Display Name of the User."",""userHelpText"":""Your display name."",""userAttributeOptions"":[],""allowEdit"":false,""optional"":true,""mandatoryInUserJourney"":false,""requireVerification"":null,""attributeType"":0}],""claims"":[{""id"":""displayName"",""displayName"":""Display Name"",""label"":""Display Name"",""dataType"":1,""userInputType"":1,""allowChangeUserInputType"":true,""adminHelpText"":""Display Name of the User."",""userHelpText"":""Your display name."",""userAttributeOptions"":[],""allowEdit"":false,""optional"":true,""mandatoryInUserJourney"":false,""requireVerification"":null,""attributeType"":0},{""id"":""displayName"",""displayName"":""Display Name"",""label"":""Display Name"",""dataType"":1,""userInputType"":1,""allowChangeUserInputType"":true,""adminHelpText"":""Display Name of the User."",""userHelpText"":""Your display name."",""userAttributeOptions"":[],""allowEdit"":false,""optional"":true,""mandatoryInUserJourney"":false,""requireVerification"":null,""attributeType"":0},{""id"":""displayName"",""displayName"":""Display Name"",""label"":""Display Name"",""dataType"":1,""userInputType"":1,""allowChangeUserInputType"":true,""adminHelpText"":""Display Name of the User."",""userHelpText"":""Your display name."",""userAttributeOptions"":[],""allowEdit"":false,""optional"":true,""mandatoryInUserJourney"":false,""requireVerification"":null,""attributeType"":0},{""id"":""displayName"",""displayName"":""Display Name"",""label"":""Display Name"",""dataType"":1,""userInputType"":1,""allowChangeUserInputType"":true,""adminHelpText"":""Display Name of the User."",""userHelpText"":""Your display name."",""userAttributeOptions"":[],""allowEdit"":false,""optional"":true,""mandatoryInUserJourney"":false,""requireVerification"":null,""attributeType"":0},{""id"":""displayName"",""displayName"":""Display Name"",""label"":""Display Name"",""dataType"":1,""userInputType"":1,""allowChangeUserInputType"":true,""adminHelpText"":""Display Name of the User."",""userHelpText"":""Your display name."",""userAttributeOptions"":[],""allowEdit"":false,""optional"":true,""mandatoryInUserJourney"":false,""requireVerification"":null,""attributeType"":0},{""id"":""displayName"",""displayName"":""Display Name"",""label"":""Display Name"",""dataType"":1,""userInputType"":1,""allowChangeUserInputType"":true,""adminHelpText"":""Display Name of the User."",""userHelpText"":""Your display name."",""userAttributeOptions"":[],""allowEdit"":false,""optional"":true,""mandatoryInUserJourney"":false,""requireVerification"":null,""attributeType"":0},{""id"":""emails"",""displayName"":""Email Addresses"",""label"":""Email Addresses"",""dataType"":7,""userInputType"":1,""allowChangeUserInputType"":true,""adminHelpText"":""Email addresses of the user."",""userHelpText"":""Your email addresses."",""userAttributeOptions"":[],""allowEdit"":false,""optional"":true,""mandatoryInUserJourney"":false,""requireVerification"":null,""attributeType"":0},{""id"":""emails"",""displayName"":""Email Addresses"",""label"":""Email Addresses"",""dataType"":7,""userInputType"":1,""allowChangeUserInputType"":true,""adminHelpText"":""Email addresses of the user."",""userHelpText"":""Your email addresses."",""userAttributeOptions"":[],""allowEdit"":false,""optional"":true,""mandatoryInUserJourney"":false,""requireVerification"":null,""attributeType"":0},{""id"":""emails"",""displayName"":""Email Addresses"",""label"":""Email Addresses"",""dataType"":7,""userInputType"":1,""allowChangeUserInputType"":true,""adminHelpText"":""Email addresses of the user."",""userHelpText"":""Your email addresses."",""userAttributeOptions"":[],""allowEdit"":false,""optional"":true,""mandatoryInUserJourney"":false,""requireVerification"":null,""attributeType"":0},{""id"":""emails"",""displayName"":""Email Addresses"",""label"":""Email Addresses"",""dataType"":7,""userInputType"":1,""allowChangeUserInputType"":true,""adminHelpText"":""Email addresses of the user."",""userHelpText"":""Your email addresses."",""userAttributeOptions"":[],""allowEdit"":false,""optional"":true,""mandatoryInUserJourney"":false,""requireVerification"":null,""attributeType"":0},{""id"":""emails"",""displayName"":""Email Addresses"",""label"":""Email Addresses"",""dataType"":7,""userInputType"":1,""allowChangeUserInputType"":true,""adminHelpText"":""Email addresses of the user."",""userHelpText"":""Your email addresses."",""userAttributeOptions"":[],""allowEdit"":false,""optional"":true,""mandatoryInUserJourney"":false,""requireVerification"":null,""attributeType"":0},{""id"":""identityProvider"",""displayName"":""Identity Provider"",""label"":""Identity Provider"",""dataType"":1,""userInputType"":1,""allowChangeUserInputType"":true,""adminHelpText"":""The social identity provider used by the user to access to your application."",""userHelpText"":"""",""userAttributeOptions"":[],""allowEdit"":false,""optional"":true,""mandatoryInUserJourney"":false,""requireVerification"":null,""attributeType"":0},{""id"":""identityProvider"",""displayName"":""Identity Provider"",""label"":""Identity Provider"",""dataType"":1,""userInputType"":1,""allowChangeUserInputType"":true,""adminHelpText"":""The social identity provider used by the user to access to your application."",""userHelpText"":"""",""userAttributeOptions"":[],""allowEdit"":false,""optional"":true,""mandatoryInUserJourney"":false,""requireVerification"":null,""attributeType"":0},{""id"":""identityProvider"",""displayName"":""Identity Provider"",""label"":""Identity Provider"",""dataType"":1,""userInputType"":1,""allowChangeUserInputType"":true,""adminHelpText"":""The social identity provider used by the user to access to your application."",""userHelpText"":"""",""userAttributeOptions"":[],""allowEdit"":false,""optional"":true,""mandatoryInUserJourney"":false,""requireVerification"":null,""attributeType"":0},{""id"":""newUser"",""displayName"":""User is new"",""label"":""User is new"",""dataType"":2,""userInputType"":1,""allowChangeUserInputType"":true,""adminHelpText"":""True, if the user has just signed-up for your application."",""userHelpText"":"""",""userAttributeOptions"":[],""allowEdit"":false,""optional"":true,""mandatoryInUserJourney"":false,""requireVerification"":null,""attributeType"":0},{""id"":""newUser"",""displayName"":""User is new"",""label"":""User is new"",""dataType"":2,""userInputType"":1,""allowChangeUserInputType"":true,""adminHelpText"":""True, if the user has just signed-up for your application."",""userHelpText"":"""",""userAttributeOptions"":[],""allowEdit"":false,""optional"":true,""mandatoryInUserJourney"":false,""requireVerification"":null,""attributeType"":0},{""id"":""objectId"",""displayName"":""User's Object ID"",""label"":""User's Object ID"",""dataType"":1,""userInputType"":1,""allowChangeUserInputType"":true,""adminHelpText"":""Object identifier (ID) of the user object in Azure AD."",""userHelpText"":""Object identifier (ID) of the user object in Azure AD."",""userAttributeOptions"":[],""allowEdit"":false,""optional"":true,""mandatoryInUserJourney"":false,""requireVerification"":null,""attributeType"":0},{""id"":""displayName"",""displayName"":""Display Name"",""label"":""Display Name"",""dataType"":1,""userInputType"":1,""allowChangeUserInputType"":true,""adminHelpText"":""Display Name of the User."",""userHelpText"":""Your display name."",""userAttributeOptions"":[],""allowEdit"":false,""optional"":true,""mandatoryInUserJourney"":false,""requireVerification"":null,""attributeType"":0},{""id"":""emails"",""displayName"":""Email Addresses"",""label"":""Email Addresses"",""dataType"":7,""userInputType"":1,""allowChangeUserInputType"":true,""adminHelpText"":""Email addresses of the user."",""userHelpText"":""Your email addresses."",""userAttributeOptions"":[],""allowEdit"":false,""optional"":true,""mandatoryInUserJourney"":false,""requireVerification"":null,""attributeType"":0},{""id"":""identityProvider"",""displayName"":""Identity Provider"",""label"":""Identity Provider"",""dataType"":1,""userInputType"":1,""allowChangeUserInputType"":true,""adminHelpText"":""The social identity provider used by the user to access to your application."",""userHelpText"":"""",""userAttributeOptions"":[],""allowEdit"":false,""optional"":true,""mandatoryInUserJourney"":false,""requireVerification"":null,""attributeType"":0},{""id"":""newUser"",""displayName"":""User is new"",""label"":""User is new"",""dataType"":2,""userInputType"":1,""allowChangeUserInputType"":true,""adminHelpText"":""True, if the user has just signed-up for your application."",""userHelpText"":"""",""userAttributeOptions"":[],""allowEdit"":false,""optional"":true,""mandatoryInUserJourney"":false,""requireVerification"":null,""attributeType"":0},{""id"":""objectId"",""displayName"":""User's Object ID"",""label"":""User's Object ID"",""dataType"":1,""userInputType"":1,""allowChangeUserInputType"":true,""adminHelpText"":""Object identifier (ID) of the user object in Azure AD."",""userHelpText"":""Object identifier (ID) of the user object in Azure AD."",""userAttributeOptions"":[],""allowEdit"":false,""optional"":true,""mandatoryInUserJourney"":false,""requireVerification"":null,""attributeType"":0}]},""pageCustomizationData"":{},""passwordComplexityData"":{},""supportedCulturesData"":{},""ageGatingData"":{},""templateData"":{},""restfulOptions"":{},""accessControlConfiguration"":{""accessControl"":{""accessControlOption"":0,""customFilterIds"":[]}}}";
            var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
            req.Content = content;

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var res = await httpClient.SendAsync(req);

            if (!res.IsSuccessStatusCode)
            {
                throw new Exception(res.StatusCode.ToString());
            }
        }
        public static async Task GetAppRegistrations(string clientId, string clientSecret, string tenantId)
        {
            var scopes = new[] { "https://graph.microsoft.com/.default" };

            // using Azure.Identity;
            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            // https://learn.microsoft.com/dotnet/api/azure.identity.clientsecretcredential
            var clientSecretCredential = new ClientSecretCredential(
                tenantId, clientId, clientSecret, options);

            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

            var appRegistrations = await graphClient.Applications.GetAsync();

            foreach (var appRegistration in appRegistrations!.Value!)
            {
                Console.WriteLine($"App registration name: {appRegistration.DisplayName}");
            }
        }
        public static void GrantConsent(string clientId)
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "az",
                    Arguments = $"ad app permission admin-consent --id {clientId}",
                    RedirectStandardOutput = false,
                    UseShellExecute = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            //var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
        }
    }
}