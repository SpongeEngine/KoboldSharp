using System.Text.Json;
using System.Text.Json.Serialization;
using SpongeEngine.SpongeLLM.Core.Exceptions;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient
    {
        public class StableDiffusionSampler
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;

            [JsonPropertyName("aliases")]
            public List<string> Aliases { get; set; } = new();

            [JsonPropertyName("options")]
            public Dictionary<string, object> Options { get; set; } = new();
        }
        
        /// <summary>
        /// Gets list of available Stable Diffusion samplers.
        /// </summary>
        public async Task<List<StableDiffusionSampler>> GetStableDiffusionSamplersAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                HttpResponseMessage response = await Options.HttpClient.GetAsync("sdapi/v1/samplers", cancellationToken);
                string content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new SpongeLLMException(
                        "Failed to get Stable Diffusion samplers",
                        (int)response.StatusCode,
                        content);
                }

                return JsonSerializer.Deserialize<List<StableDiffusionSampler>>(content) ?? new List<StableDiffusionSampler>();
            }
            catch (Exception ex) when (ex is not SpongeLLMException)
            {
                throw new SpongeLLMException("Failed to get Stable Diffusion samplers", innerException: ex);
            }
        }
    }
}