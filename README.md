# VSTS-Bot

This is a sample of a bot, developed using the [Bot Framework C# SDK v4](https://github.com/Microsoft/botbuilder-dotnet), that provides the VSTS access to users who are conversating on MS Teams.
The authentication is managed as in the official [OAuth Web Sample](https://github.com/Microsoft/vsts-auth-samples/tree/master/OAuthWebSample) sample and an Azure Cosmos DB is used to keep the relation between a specific user and his VSTS authorization token.

VSTS query calls are not yet implemented and they are leaved to other customizations.

## Requirements
- Deploy the sample solution on an [Azure Web App](https://docs.microsoft.com/it-it/azure/app-service/app-service-web-get-started-dotnet#publish-to-azure), you need the web app endpoint for the next step
- Create an [Azure Bot Channels Registration](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-3.0) to register the bot and retrive **App Id** and **App Password** and to enable the conversation on Teams
- [Register an app in your VSTS account](https://docs.microsoft.com/en-us/vsts/integrate/get-started/authentication/oauth?view=vsts#authorize-your-app) in order to retrieve
  * **ClientSecret**
  * **AppId**
  
  in the *AuthenticationHeper.cs* you need also other VSTS settings that you can find in the *appsetting.json* file:
  * **AuthURL**: "https://app.vssps.visualstudio.com/oauth2/authorize"
  * **CallbackUrl**: "https://testvstsbotv1.azurewebsites.net/api/VSTSAuth"
  * **TokenUrl**: "https://app.vssps.visualstudio.com/oauth2/token"
  * **Scope**: "vso.graph vso.work_full" - you can add other scopes as you need
  
- Create an [Azure Cosmos DB account](https://docs.microsoft.com/en-us/azure/cosmos-db/create-sql-api-dotnet#create-a-database-account) with SQL API, you need to add to the appsettings **endpoint** and **key** - in the code 2 DBs are used:
  * **VSTSDb** for user tokens with a collection **VSTSToken**
  * **BotData** for the conversation history with a collection called **BotCollection**
  
  you can easily change this names, if you want/need
  
- Add a Teams App in Teams that you can configure creating an [App Package](https://docs.microsoft.com/it-it/microsoftteams/platform/concepts/apps/apps-package) and [upload it in Teams](https://docs.microsoft.com/it-it/microsoftteams/platform/concepts/apps/apps-upload) - see [App Studio](https://docs.microsoft.com/it-it/microsoftteams/platform/get-started/get-started-app-studio) to create and export the Manifest directly in Teams


## Next Steps
- Configure LUIS for Natural Language Processing, in particular to execute VSTS query on workitems
- Configure other features (if not "vsts")
- ...
  
