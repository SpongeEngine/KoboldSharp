using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SpongeEngine.SpongeLLM.Core.Exceptions;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient
    {
        public class StableDiffusionGenerationRequest 
        {
            [JsonPropertyName("prompt")]
            public string Prompt { get; set; } = string.Empty;

            [JsonPropertyName("negative_prompt")]
            public string NegativePrompt { get; set; } = string.Empty;

            [JsonPropertyName("cfg_scale")]
            public float CfgScale { get; set; } = 7.0f;

            [JsonPropertyName("steps")]
            public int Steps { get; set; } = 20;

            [JsonPropertyName("width")]
            public int Width { get; set; } = 512;

            [JsonPropertyName("height")]
            public int Height { get; set; } = 512;

            [JsonPropertyName("seed")]
            public int Seed { get; set; } = -1;

            [JsonPropertyName("sampler_name")]
            public string SamplerName { get; set; } = "euler_a";
        }
        
        public class StableDiffusionGenerationResponse
        {
            [JsonPropertyName("images")]
            public List<string> Images { get; set; } = new();

            [JsonPropertyName("parameters")]
            public Dictionary<string, object> Parameters { get; set; } = new();

            [JsonPropertyName("info")]
            public string Info { get; set; } = string.Empty;
        }
        
        /// <summary>
        /// Generates an image from text using Stable Diffusion.
        /// </summary>
        public async Task<StableDiffusionGenerationResponse> TextToImageAsync(StableDiffusionGenerationRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                using HttpRequestMessage httpRequest = new(HttpMethod.Post, "sdapi/v1/txt2img");
                var serializedJson = JsonSerializer.Serialize(request, Options.JsonSerializerOptions);
                httpRequest.Content = new StringContent(serializedJson, Encoding.UTF8, "application/json");

                using HttpResponseMessage response = await Options.HttpClient.SendAsync(httpRequest, cancellationToken);
                string content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new SpongeLLMException(
                        "Failed to generate image from text",
                        (int)response.StatusCode,
                        content);
                }

                return JsonSerializer.Deserialize<StableDiffusionGenerationResponse>(content) ?? throw new SpongeLLMException("Failed to deserialize image generation response");
            }
            catch (Exception ex) when (ex is not SpongeLLMException)
            {
                throw new SpongeLLMException("Failed to generate image from text", innerException: ex);
            }
        }
    }
}