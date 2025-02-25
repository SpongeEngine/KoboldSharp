using System.Text.Json;
using System.Text.Json.Serialization;
using SpongeEngine.SpongeLLM.Core.Exceptions;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient
    {
        public class StableDiffusionModelInfo
        {
            [JsonPropertyName("title")]
            public string Title { get; set; } = string.Empty;

            [JsonPropertyName("model_name")]
            public string ModelName { get; set; } = string.Empty;

            [JsonPropertyName("hash")]
            public string Hash { get; set; } = string.Empty;

            [JsonPropertyName("filename")]
            public string Filename { get; set; } = string.Empty;
        }
        
        /// <summary>
        /// Gets information about available Stable Diffusion models.
        /// </summary>
        public async Task<List<StableDiffusionModelInfo>> GetStableDiffusionModelsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                HttpResponseMessage? response = await Options.HttpClient.GetAsync("sdapi/v1/sd-models", cancellationToken);
                string? content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new SpongeLLMException(
                        "Failed to get Stable Diffusion models",
                        (int)response.StatusCode,
                        content);
                }

                return JsonSerializer.Deserialize<List<StableDiffusionModelInfo>>(content) ?? new List<StableDiffusionModelInfo>();
            }
            catch (Exception ex) when (ex is not SpongeLLMException)
            {
                throw new SpongeLLMException("Failed to get Stable Diffusion models", innerException: ex);
            }
        }
    }
}