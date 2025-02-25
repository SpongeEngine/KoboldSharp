using System.Text.Json.Serialization;
using SpongeEngine.SpongeLLM.Core.Exceptions;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient
    {
        public class MultiplayerStoryRequest
        {
            [JsonPropertyName("full_update")]
            public bool FullUpdate { get; set; }

            [JsonPropertyName("data_format")]
            public string DataFormat { get; set; } = string.Empty;

            [JsonPropertyName("sender")]
            public string Sender { get; set; } = string.Empty;

            [JsonPropertyName("data")]
            public string Data { get; set; } = string.Empty;
        }

        public class MultiplayerStoryResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("turn_major")]
            public int TurnMajor { get; set; }

            [JsonPropertyName("turn_minor")]
            public int TurnMinor { get; set; }

            [JsonPropertyName("idle")]
            public bool IsIdle { get; set; }

            [JsonPropertyName("data_format")]
            public string DataFormat { get; set; } = string.Empty;

            [JsonPropertyName("error")]
            public string? Error { get; set; }
        }
        
        /// <summary>
        /// Gets the current multiplayer story.
        /// </summary>
        public async Task<string> GetMultiplayerStoryAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                HttpResponseMessage response = await Options.HttpClient.GetAsync("api/extra/multiplayer/getstory", cancellationToken);
                string content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new SpongeLLMException(
                        "Failed to get multiplayer story",
                        (int)response.StatusCode,
                        content);
                }

                return content;
            }
            catch (Exception ex) when (ex is not SpongeLLMException)
            {
                throw new SpongeLLMException("Failed to get multiplayer story", innerException: ex);
            }
        }
    }
}