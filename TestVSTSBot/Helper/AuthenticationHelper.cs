using System;
using System.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using TestVSTSBot.Model;

namespace TestVSTSBot.Authentication
{
    public static class AuthenticationHelper 
    {
        public static String GenerateAuthorizeUrl(ConversationReference cf)
        {
            //incapsulate in the extraQueryParams the information needed for the proactive message from the controller after the auth
            var extraQueryParameters = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(cf)));

            UriBuilder uriBuilder = new UriBuilder(Environment.GetEnvironmentVariable("AuthURL"));
            var queryParams = HttpUtility.ParseQueryString(uriBuilder.Query ?? String.Empty);

            queryParams["client_id"] = Environment.GetEnvironmentVariable("AppId");
            queryParams["response_type"] = "Assertion";
            queryParams["state"] = extraQueryParameters;
            queryParams["scope"] = Environment.GetEnvironmentVariable("Scope");
            queryParams["redirect_uri"] = Environment.GetEnvironmentVariable("CallbackUrl");

            uriBuilder.Query = queryParams.ToString();

            return uriBuilder.ToString();
        }
    }
}
