using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestVSTSBot.Model
{
    public class BearerToken
    {
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }

        [JsonProperty("ext_expires_in")]
        public long ExtExpiresIn { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }

    public class TeamsUserInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("objectId")]
        public string ObjectId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("givenName")]
        public string GivenName { get; set; }

        [JsonProperty("surname")]
        public string Surname { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("userPrincipalName")]
        public string UserPrincipalName { get; set; }
    }
}
