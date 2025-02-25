using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SpongeEngine.SpongeLLM.Core.Exceptions;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient
    {
        public class WebSearchRequest
        {
            [JsonPropertyName("q")]
            public string Query { get; set; } = string.Empty;
        }

        public class WebSearchResult
        {
            [JsonPropertyName("title")]
            public string Title { get; set; } = string.Empty;

            [JsonPropertyName("url")]
            public string Url { get; set; } = string.Empty;

            [JsonPropertyName("desc")]
            public string Description { get; set; } = string.Empty;

            [JsonPropertyName("content")]
            public string Content { get; set; } = string.Empty;
        }
        
        /// <summary>
        /// Performs a web search using the integrated search engine.
        /// </summary>
        public async Task<List<WebSearchResult>> WebSearchAsync(string query, CancellationToken cancellationToken = default)
        {
            try
            {
                using HttpRequestMessage httpRequest = new(HttpMethod.Post, "api/extra/websearch");
                var serializedJson = JsonSerializer.Serialize(new WebSearchRequest { Query = query }, Options.JsonSerializerOptions);
                httpRequest.Content = new StringContent(serializedJson, Encoding.UTF8, "application/json");
                
                using HttpResponseMessage? response = await Options.HttpClient.SendAsync(httpRequest, cancellationToken);
                string? content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new SpongeLLMException(
                        "Failed to perform web search",
                        (int)response.StatusCode,
                        content);
                }

                return JsonSerializer.Deserialize<List<WebSearchResult>>(content) ?? throw new SpongeLLMException("Failed to deserialize web search results");
            }
            catch (Exception ex) when (ex is not SpongeLLMException)
            {
                throw new SpongeLLMException("Failed to perform web search", innerException: ex);
            }
        }
    }
}