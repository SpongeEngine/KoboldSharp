using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SpongeEngine.SpongeLLM.Core.Exceptions;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient
    {
        public class MultiplayerStatusRequest
        {
            [JsonPropertyName("sender")]
            public string Sender { get; set; } = string.Empty;

            [JsonPropertyName("senderbusy")]
            public bool SenderBusy { get; set; }
        }

        public class MultiplayerStatusResponse
        {
            [JsonPropertyName("turn_major")]
            public int TurnMajor { get; set; }

            [JsonPropertyName("turn_minor")]
            public int TurnMinor { get; set; }

            [JsonPropertyName("idle")]
            public bool IsIdle { get; set; }

            [JsonPropertyName("data_format")]
            public string DataFormat { get; set; } = string.Empty;
        }
        
        /// <summary>
        /// Gets the current multiplayer status.
        /// </summary>
        public async Task<MultiplayerStatusResponse> GetMultiplayerStatusAsync(MultiplayerStatusRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                using HttpRequestMessage httpRequest = new(HttpMethod.Post, "api/extra/multiplayer/status");
                var serializedJson = JsonSerializer.Serialize(request, Options.JsonSerializerOptions);
                httpRequest.Content = new StringContent(serializedJson, Encoding.UTF8, "application/json");

                using HttpResponseMessage response = await Options.HttpClient.SendAsync(httpRequest, cancellationToken);
                string content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new SpongeLLMException(
                        "Failed to get multiplayer status",
                        (int)response.StatusCode,
                        content);
                }

                return JsonSerializer.Deserialize<MultiplayerStatusResponse>(content) ?? throw new SpongeLLMException("Failed to deserialize multiplayer status response");
            }
            catch (Exception ex) when (ex is not SpongeLLMException)
            {
                throw new SpongeLLMException("Failed to get multiplayer status", innerException: ex);
            }
        }
    }
}