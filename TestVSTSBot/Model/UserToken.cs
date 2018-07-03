using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestVSTSBot.Model
{
    public class UserToken
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("token")]
        public TokenModel Token { get; set; }
    }
}
