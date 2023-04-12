using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Cli
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<List<UserFlow>>(myJsonResponse);
    public record UserFlow(
        [property: JsonProperty("type")] string type,
        [property: JsonProperty("id")] string id,
        [property: JsonProperty("protocol")] string protocol,
        [property: JsonProperty("idpData")] IdpData idpData,
        [property: JsonProperty("booleanData")] BooleanData booleanData,
        [property: JsonProperty("tokenLifetimeData")] TokenLifetimeData tokenLifetimeData,
        [property: JsonProperty("tokenClaimsData")] TokenClaimsData tokenClaimsData,
        [property: JsonProperty("ssoSessionData")] SsoSessionData ssoSessionData,
        [property: JsonProperty("userAttributesData")] UserAttributesData userAttributesData,
        [property: JsonProperty("passwordComplexityData")] PasswordComplexityData passwordComplexityData,
        [property: JsonProperty("pageCustomizationData")] PageCustomizationData pageCustomizationData,
        [property: JsonProperty("supportedCulturesData")] SupportedCulturesData supportedCulturesData,
        [property: JsonProperty("ageGatingData")] AgeGatingData ageGatingData,
        [property: JsonProperty("templateData")] TemplateData templateData,
        [property: JsonProperty("restfulOptions")] RestfulOptions restfulOptions,
        [property: JsonProperty("accessControlConfiguration")] AccessControlConfiguration accessControlConfiguration
    );
    public record AccessControlConfiguration(

    );

    public record AgeGatingData(

    );

    public record BooleanData(
        [property: JsonProperty("mfa")] bool mfa,
        [property: JsonProperty("mfaDisable")] bool mfaDisable,
        [property: JsonProperty("allowPhoneFactor")] bool allowPhoneFactor,
        [property: JsonProperty("allowEmailFactor")] bool allowEmailFactor,
        [property: JsonProperty("allowTotp")] bool allowTotp
    );

    public record IdpData(

    );

    public record PageCustomizationData(

    );

    public record PasswordComplexityData(

    );

    public record PreSelfAssertedRestful(
        [property: JsonProperty("restfulProviderId")] string restfulProviderId
    );

    public record PreSendClaimsRestful(
        [property: JsonProperty("restfulProviderId")] string restfulProviderId
    );

    public record PreUserWriteRestful(
        [property: JsonProperty("restfulProviderId")] string restfulProviderId
    );

    public record RestApis(
        [property: JsonProperty("preSelfAssertedRestful")] PreSelfAssertedRestful preSelfAssertedRestful,
        [property: JsonProperty("preUserWriteRestful")] PreUserWriteRestful preUserWriteRestful,
        [property: JsonProperty("preSendClaimsRestful")] PreSendClaimsRestful preSendClaimsRestful,
        [property: JsonProperty("sendEmailForOTPVerificationRestful")] SendEmailForOTPVerificationRestful sendEmailForOTPVerificationRestful
    );

    public record RestfulOptions(
        [property: JsonProperty("restApis")] RestApis restApis
    );

    public record SendEmailForOTPVerificationRestful(
        [property: JsonProperty("restfulProviderId")] string restfulProviderId
    );

    public record SsoSessionData(

    );

    public record SupportedCulturesData(

    );

    public record TemplateData(

    );

    public record TokenClaimsData(

    );

    public record TokenLifetimeData(

    );

    public record UserAttributesData(

    );


}
