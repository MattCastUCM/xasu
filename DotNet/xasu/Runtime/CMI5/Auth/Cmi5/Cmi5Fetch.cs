using Newtonsoft.Json;

namespace Xasu.Cmi5.Auth.Cmi5
{
    internal class Cmi5Fetch
    {
        [JsonProperty("auth-token")]
        public string AuthToken { get; set; }
    }
}
