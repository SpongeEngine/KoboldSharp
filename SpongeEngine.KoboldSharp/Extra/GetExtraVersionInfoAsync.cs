using System.Text.Json;
using System.Text.Json.Serialization;
using SpongeEngine.SpongeLLM.Core.Exceptions;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient
    {
        public class KoboldExtraVersionInfo 
        {
            [JsonPropertyName("result")]
            public string Result { get; set; } = string.Empty;
        
            [JsonPropertyName("version")]
            public string Version { get; set; } = string.Empty;
        
            [JsonPropertyName("protected")]
            public bool IsProtected { get; set; }

            [JsonPropertyName("txt2img")] 
            public bool HasImageGeneration { get; set; }

            [JsonPropertyName("vision")]
            public bool HasVisionCapabilities { get; set; }

            [JsonPropertyName("transcribe")]
            public bool HasTranscription { get; set; }

            [JsonPropertyName("multiplayer")]
            public bool HasMultiplayer { get; set; }

            [JsonPropertyName("websearch")]
            public bool HasWebSearch { get; set; }
        }
        
        /// <summary>
        /// Gets detailed version and capability information about KoboldCpp.
        /// </summary>
        public async Task<KoboldExtraVersionInfo> GetExtraVersionInfoAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                HttpResponseMessage? response = await Options.HttpClient.GetAsync("api/extra/version", cancellationToken);
                string? content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new SpongeLLMException(
                        "Failed to get extra version info",
                        (int)response.StatusCode,
                        content);
                }

                return JsonSerializer.Deserialize<KoboldExtraVersionInfo>(content) ?? throw new SpongeLLMException("Failed to deserialize extra version info");
            }
            catch (Exception ex) when (ex is not SpongeLLMException)
            {
                throw new SpongeLLMException("Failed to get extra version info", innerException: ex);
            }
        }
    }
}