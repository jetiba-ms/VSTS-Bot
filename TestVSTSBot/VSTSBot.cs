using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using RestSharp;
using TestVSTSBot.Authentication;
using TestVSTSBot.Controllers;
using TestVSTSBot.Helper;
using TestVSTSBot.Model;

namespace TestVSTSBot
{
    internal class VSTSBot : IBot
    {
        public async Task OnTurn(ITurnContext context)
        {
            if (context.Activity.Type is ActivityTypes.Message)
            {
                var message = context.Activity;

                //check mentions in the message -> MSTEAMS
                if (context.Activity.ChannelId == "msteams")
                {
                    Mention[] m = context.Activity.GetMentions();

                    for (int i = 0; i < m.Length; i++)
                    {
                        if (m[i].Mentioned.Id == context.Activity.Recipient.Id)
                        {
                            //Bot is in the @mention list.
                            //The below example will strip the bot name out of the message, so you can parse it as if it wasn't included. Note that the Text object will contain the full bot name, if applicable.
                            if (m[i].Text != null)
                                message.Text = message.Text.Replace(m[i].Text, "").Trim();
                        }
                    }
                }

                //when user ask about info in VSTS, he/she should be authenticated
                if (message.AsMessageActivity().Text == "vsts")
                {
                    //check in the DB if there is a valid token for the specific user
                    CosmosDbHelper cdh = new CosmosDbHelper("VSTSDb", "VSTSToken");
                    var tk = await cdh.ReadTokenFromDB("VSTSDb", "VSTSToken", context.Activity.From.Id);

                    if (tk != null)
                    {
                        //TODO: query VSTS to retrieve information
                        await context.SendActivity("Here there will be the result of a custom query...");
                    }
                    else
                    {
                        //if there isn't a token for the user get ConversationReference
                        var cf = TurnContext.GetConversationReference(context.Activity);

                        //and generate the URL for user authentication
                        var url = AuthenticationHelper.GenerateAuthorizeUrl(cf);
                        Activity replyToConversation = message.CreateReply();
                        replyToConversation.Attachments = new List<Attachment>();

                        List<CardAction> cardButtons = new List<CardAction>();

                        CardAction plButton = new CardAction()
                        {
                            Value = url,
                            Type = "openUrl",
                            Title = "Connect"
                        };

                        cardButtons.Add(plButton);

                        SigninCard plCard = new SigninCard(text: "You need to authorize me", buttons: cardButtons);

                        Attachment plAttachment = plCard.ToAttachment();
                        replyToConversation.Attachments.Add(plAttachment);

                        var reply = await context.SendActivity(replyToConversation);
                        await context.SendActivity(url);
                    }
                }
                else
                {
                    // send the information about the Activity, at the moment
                    //TODO: manage other features for the bot
                    await context.SendActivity(
                        $"ConversationId " + context.Activity.Conversation.Id + "\n" +
                        "BotId " + context.Activity.Recipient.Id + "\n" +
                        "BotName " + context.Activity.Name + "\n" +
                        "tenantId " + context.Activity.ChannelData);
                }
            }
        }
    }
}