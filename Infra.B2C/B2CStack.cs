using Pulumi;
using Pulumi.AzureAD;
using Pulumi.AzureAD.Inputs;
using Pulumi.AzureNative;

public class B2CStack : Stack
{

    [Output]
    public Output<string> AutoUserPasswords { get; set; }

    [Output]
    public Output<string> AutoEnabledUser { get; set; }

    [Output]
    public Output<string> AutoDisabledUser { get; set; }
    [Output]
    public Output<string> AutoRegClientId { get; set; }
    [Output]
    public Output<string> AutoRegClientSecret { get; set; }

    public B2CStack()
    {
        var c = new Pulumi.Config();
        var validDomain = c.Require("B2CNAME");

        var appReg = new Application("auto.registration", new ApplicationArgs
        {
            //PublicClient = true,
            //Oauth2AllowImplicitFlow = true,
            //AvailableToOtherTenants = true,
            DisplayName = "auto.registration",
            RequiredResourceAccesses =
            {
                new ApplicationRequiredResourceAccessArgs
                {
                    ResourceAppId = "00000003-0000-0000-c000-000000000000", // MS Graph
                    ResourceAccesses =
                    {
                        new ApplicationRequiredResourceAccessResourceAccessArgs
                        {
                            Id = "1bfefb4e-e0b5-418b-a88f-73c46d2cc8e9", // application.readwrite.all
                            Type = "Role",
                        },
                        new ApplicationRequiredResourceAccessResourceAccessArgs
                        {
                            Id = "9a5d68dd-52b0-4cc2-bd40-abcf44ac3a30", // application.read.all
                            Type = "Role",
                        },
                        new ApplicationRequiredResourceAccessResourceAccessArgs
                        {
                            Id = "741f803b-c850-494e-b5df-cde7c675a1ca", // user.readwrite.all
                            Type = "Role",
                        }
                    }
                }
            },
        }, new CustomResourceOptions
        {
            // force the re-creation of this registration because API Permissions were not updating properly by default
            ReplaceOnChanges = { "*" },
            DeleteBeforeReplace = true
        });

        var secret = new ApplicationPassword("auto.registration.secret", new ApplicationPasswordArgs
        {
            ApplicationObjectId = appReg.ObjectId,
            DisplayName = "password",
            EndDateRelative = "240h"
        });

        AutoUserPasswords = Output.Create(c.Get("autopass") ?? "P@ssword!");
        AutoEnabledUser = Output.Create($"auto.enabled@{validDomain}");
        AutoDisabledUser = Output.Create($"auto.disabled@{validDomain}");
        AutoRegClientId = appReg.ApplicationId;
        AutoRegClientSecret = secret.Value;

        var autoEnabled = new User("auto.enabled", new UserArgs
        {
            DisplayName = "auto.enabled",
            Password = AutoUserPasswords.Apply(p => p),
            UserPrincipalName = AutoEnabledUser.Apply(p => p),
            AccountEnabled = true
        });
        var autoDisabled = new User("auto.disabled", new UserArgs
        {
            DisplayName = "auto.disabled",
            Password = AutoUserPasswords.Apply(p => p),
            UserPrincipalName = AutoDisabledUser.Apply(p => p),
            AccountEnabled = false
        });
    }
}