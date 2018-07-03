using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestVSTSBot.Model;

namespace TestVSTSBot.Helper
{
    public class CosmosDbHelper
    {
        private string Endpoint;
        private string Key;

        public DocumentClient docClient;

        //constructor
        public CosmosDbHelper(string db, string coll)
        {
            Endpoint = Environment.GetEnvironmentVariable("CosmosDbUri");
            Key = Environment.GetEnvironmentVariable("CosmosDbKey");
            docClient = new DocumentClient(new Uri(Endpoint), Key);
            CreateDatabaseAndCollection(db, coll).ConfigureAwait(false);
        }

        //destructor
        ~CosmosDbHelper()
        {
            docClient.Dispose();
        }

        private async Task CreateDatabaseAndCollection(string db, string coll)
        {
            try
            {
                await docClient.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(db));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await docClient.CreateDatabaseAsync(new Database { Id = db });
                }
                else
                {
                    throw;
                }
            }

            try
            {
                await docClient.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(db, coll));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await docClient.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(db),
                        new DocumentCollection { Id = coll });
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<string> ReadFromDatabase(int numberOfRecords, string db, string coll)
        {
            var documents = docClient.CreateDocumentQuery<ConvInfo>(UriFactory.CreateDocumentCollectionUri(db, coll)).AsDocumentQuery();
            List<ConvInfo> messages = new List<ConvInfo>();
            while (documents.HasMoreResults)
            {
                messages.AddRange(await documents.ExecuteNextAsync<ConvInfo>());
            }

            List<ConvInfo> messageSublist = new List<ConvInfo>();
            if (messages.Count >= numberOfRecords)
            {
                // Create a sublist of messages containing the number of requested records
                messageSublist = messages.GetRange(messages.Count - numberOfRecords, numberOfRecords);
            }
            else
            {
                messageSublist = messages;
            }

            string history = "";

            // Send the last 5 messages
            foreach (ConvInfo historyItem in messageSublist)
            {
                history += (historyItem.Sender + " sent: " + historyItem.Message + "at: " + historyItem.Time + ". Reply was: " + historyItem.Reply + "\n");
            }

            return history;
        }

        public async Task<UserToken> ReadTokenFromDB(string db, string coll, string userid)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            // Here we find the Andersen family via its LastName
            IQueryable<UserToken> tokenQuery = this.docClient.CreateDocumentQuery<UserToken>(
                    UriFactory.CreateDocumentCollectionUri(db, coll), queryOptions)
                    .Where(f => f.UserId == userid);

            var userToken = tokenQuery.AsEnumerable().FirstOrDefault();
            
            return userToken;
        }
    }
}
