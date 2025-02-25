using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SpongeEngine.SpongeLLM.Core.Exceptions;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient
    {
        public class CountTokensRequest
        {
            [JsonPropertyName("prompt")]
            public string Prompt { get; set; } = string.Empty;

            [JsonPropertyName("special")]
            public bool IncludeSpecialTokens { get; set; } = true;
        }
        
        public class CountTokensResponse
        {
            [JsonPropertyName("value")]
            public int Count { get; set; }

            [JsonPropertyName("ids")]
            public List<int> TokenIds { get; set; } = new();
        }
        
        /// <summary>
        /// Counts tokens in a given prompt string.
        /// </summary>
        public async Task<CountTokensResponse> CountTokensAsync(CountTokensRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                using HttpRequestMessage httpRequest = new(HttpMethod.Post, "api/extra/tokencount");
                var serializedJson = JsonSerializer.Serialize(request, Options.JsonSerializerOptions);
                httpRequest.Content = new StringContent(serializedJson, Encoding.UTF8, "application/json");
                
                using HttpResponseMessage? response = await Options.HttpClient.SendAsync(httpRequest, cancellationToken);
                string? content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new SpongeLLMException(
                        "Failed to count tokens",
                        (int)response.StatusCode,
                        content);
                }

                return JsonSerializer.Deserialize<CountTokensResponse>(content) ?? throw new SpongeLLMException("Failed to deserialize token count response");
            }
            catch (Exception ex) when (ex is not SpongeLLMException)
            {
                throw new SpongeLLMException("Failed to count tokens", innerException: ex);
            }
        }
    }
}