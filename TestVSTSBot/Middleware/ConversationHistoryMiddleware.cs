using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestVSTSBot.Helper;

namespace TestVSTSBot.Middleware
{
    public class ConversationHistoryMiddleware : IMiddleware
    {
        CosmosDbHelper cdh = new CosmosDbHelper("BotData", "BotCollection");

        public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            string botReply = "";
            if(context.Activity.Type == ActivityTypes.Message)
            {
                if(context.Activity.Text.ToLower() == "history")
                {
                    // Read last 5 responses from the database, and short circuit future execution
                    await context.SendActivity(await cdh.ReadFromDatabase(5, "BotData", "BotCollection"));
                    return;
                }

                // Create a send activity handler to grab all response activities 
                // from the activity list.
                context.OnSendActivities(async (activityContext, activityList, activityNext) =>
                {
                    foreach (Activity activity in activityList)
                    {
                        botReply += (activity.Text + " ");
                    }
                    return await activityNext();
                });
            }

            await next();

            // Save logs for each conversational exchange only
            if (context.Activity.Type == ActivityTypes.Message)
            {
                // Write log to the database
                var convHistory = new ConvInfo
                {
                    Time = DateTime.Now.ToString(),
                    Sender = context.Activity.From.Name,
                    Message = context.Activity.Text,
                    Reply = botReply
                };

                try
                {
                    var document = await cdh.docClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri("BotData", "BotCollection"), convHistory);
                }
                catch (Exception ex)
                {
                    //TODO: More logic for what to do on a failed write can be added here
                    throw ex;
                }
            }
        }
    }
}
