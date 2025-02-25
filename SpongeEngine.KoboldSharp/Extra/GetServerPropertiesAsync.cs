using System.Text.Json;
using System.Text.Json.Serialization;
using SpongeEngine.SpongeLLM.Core.Exceptions;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient
    {
        public class ServerProperties
        {
            [JsonPropertyName("chat_template")]
            public string ChatTemplate { get; set; } = string.Empty;

            [JsonPropertyName("total_slots")]
            public int TotalSlots { get; set; }

            [JsonPropertyName("default_generation_settings")]
            public DefaultGenerationSettings Settings { get; set; } = new();
        }

        public class DefaultGenerationSettings
        {
            [JsonPropertyName("n_ctx")]
            public int ContextSize { get; set; }
        }
        
        /// <summary>
        /// Gets configuration properties for the server instance.
        /// </summary>
        public async Task<ServerProperties> GetServerPropertiesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                HttpResponseMessage? response = await Options.HttpClient.GetAsync("props", cancellationToken);
                string? content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new SpongeLLMException(
                        "Failed to get server properties",
                        (int)response.StatusCode,
                        content);
                }

                return JsonSerializer.Deserialize<ServerProperties>(content) ?? 
                       throw new SpongeLLMException("Failed to deserialize server properties");
            }
            catch (Exception ex) when (ex is not SpongeLLMException)
            {
                throw new SpongeLLMException("Failed to get server properties", innerException: ex);
            }
        }
    }
}