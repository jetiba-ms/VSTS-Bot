using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Documents.Client;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using TestVSTSBot.Helper;
using TestVSTSBot.Model;

namespace TestVSTSBot.Controllers
{
    public class VSTSAuthController : Controller
    {
        [HttpGet]
        [Route("api/VSTSAuth")]
        public async Task<HttpResponseMessage> VSTSAuth([FromQuery] string code, [FromQuery] string state)
        {
            var queryParams = state;
            var cfbytes = WebEncoders.Base64UrlDecode(queryParams);
            var _cf = JsonConvert.DeserializeObject<ConversationReference>(System.Text.Encoding.UTF8.GetString(cfbytes));

            TokenModel token = new TokenModel();
            String error = null;

            if (!String.IsNullOrEmpty(code))
            {
                error = PerformTokenRequest(GenerateRequestPostData(code), out token);
                if (!String.IsNullOrEmpty(error))
                {
                    Debug.WriteLine(error);
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }
            }

            CosmosDbHelper cdh = new CosmosDbHelper("VSTSDb", "VSTSToken");

            BotAdapter ba = new BotFrameworkAdapter(Environment.GetEnvironmentVariable("MicrosoftAppId"), Environment.GetEnvironmentVariable("MicrosoftAppPassword"));

            await ba.ContinueConversation(Environment.GetEnvironmentVariable("MicrosoftAppId"), _cf, async (context) =>
            {
                // Write token to the database associated with a specific user
                var usertoken = new
                {
                    userId = context.Activity.From.Id,
                    token
                };

                try
                {
                    var document = await cdh.docClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri("VSTSDb", "VSTSToken"), usertoken);
                }
                catch (Exception ex)
                {
                    // TODO: More logic for what to do on a failed write can be added here
                    await context.SendActivity("exception in storing token");
                    throw ex;
                }

                await context.SendActivity("You are logged in VSTS!");
            });


            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        private String PerformTokenRequest(String postData, out TokenModel token)
        {
            var error = String.Empty;
            var strResponseData = String.Empty;

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(Environment.GetEnvironmentVariable("TokenUrl"));

            webRequest.Method = "POST";
            webRequest.ContentLength = postData.Length;
            webRequest.ContentType = "application/x-www-form-urlencoded";

            using (StreamWriter swRequestWriter = new StreamWriter(webRequest.GetRequestStream()))
            {
                swRequestWriter.Write(postData);
            }

            try
            {
                HttpWebResponse hwrWebResponse = (HttpWebResponse)webRequest.GetResponse();

                if (hwrWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader srResponseReader = new StreamReader(hwrWebResponse.GetResponseStream()))
                    {
                        strResponseData = srResponseReader.ReadToEnd();
                    }

                    token = JsonConvert.DeserializeObject<TokenModel>(strResponseData);
                    return null;
                }
            }
            catch (WebException wex)
            {
                error = "Request Issue: " + wex.Message;
            }
            catch (Exception ex)
            {
                error = "Issue: " + ex.Message;
            }

            token = new TokenModel();
            return error;
        }

        public string GenerateRequestPostData(string code)
        {
            return string.Format("client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion={0}&grant_type=urn:ietf:params:oauth:grant-type:jwt-bearer&assertion={1}&redirect_uri={2}",
                HttpUtility.UrlEncode(Environment.GetEnvironmentVariable("ClientSecret")),
                HttpUtility.UrlEncode(code), Environment.GetEnvironmentVariable("CallbackUrl"));
        }
    }
}