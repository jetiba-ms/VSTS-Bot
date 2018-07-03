using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TestVSTSBot.Model;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;

namespace TestVSTSBot.Helper
{
    public static class TeamsChannelInfoHelper
    {
        //At the moment this class is not used

        //This could be done by SDK
        //var connector = new ConnectorClient(new Uri(context.Activity.ServiceUrl));
        //IEnumerable<TeamsChannelAccount> members = (IEnumerable<TeamsChannelAccount>)await connector.Conversations.GetConversationMembersAsync(context.Activity.Conversation.Id);
        //foreach(var member in members)

        //At the moment the SDK method doesn't retrieve the id for the user in the conversation with the bot
        //so this is a different implementation fot retrieving it

        public static async Task<string> GetUPNfromTeamsAsync(string userid, string convid)
        {
            //1. retrieve token from AppId and AppPsw
            var bearerToken = new BearerToken();
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

                var keyValues = new List<KeyValuePair<string, string>>();
                keyValues.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
                keyValues.Add(new KeyValuePair<string, string>("client_id", Environment.GetEnvironmentVariable("MicrosoftAppId")));
                keyValues.Add(new KeyValuePair<string, string>("client_secret", Environment.GetEnvironmentVariable("MicrosoftAppPassword")));
                keyValues.Add(new KeyValuePair<string, string>("scope", "https://api.botframework.com/.default"));

                var content = new FormUrlEncodedContent(keyValues);

                HttpResponseMessage resp = await client.PostAsync("https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token", content);
                bearerToken = JsonConvert.DeserializeObject<BearerToken>(resp.Content.ReadAsStringAsync().Result);
            }

            //2. retrieve user information -> class TeamsUserInfo required
            var userinfo = new List<TeamsUserInfo>();
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken.AccessToken);

                HttpResponseMessage resp = await client.GetAsync(
                    "https://smba.trafficmanager.net/amer-client-ss.msg/v3/conversations/" + convid + "/members");
                userinfo = JsonConvert.DeserializeObject<List<TeamsUserInfo>>(resp.Content.ReadAsStringAsync().Result);
            }

            //3. check if TeamsUserInfo == userid
            var upn = "";
            foreach(var user in userinfo)
            {
                if(user.Id == userid)
                {
                    //4. retrieve associated upn
                    upn = user.UserPrincipalName;
                }
            }
            return upn;           
        }
    }
}
