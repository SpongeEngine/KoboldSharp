using System.Text.Json;
using System.Text.Json.Serialization;
using SpongeEngine.SpongeLLM.Core.Exceptions;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient
    {
        public class KoboldSharpModelInfo
        {
            [JsonPropertyName("result")]
            public string ModelName { get; set; } = string.Empty;
        }
        
        public async Task<KoboldSharpModelInfo> GetModelInfoAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                HttpResponseMessage? response = await Options.HttpClient.GetAsync("api/v1/model", cancellationToken);
                string? content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new SpongeLLMException(
                        "Failed to get model info",
                        (int)response.StatusCode,
                        content);
                }

                return JsonSerializer.Deserialize<KoboldSharpModelInfo>(content) ?? throw new SpongeLLMException("Failed to deserialize model info");
            }
            catch (Exception ex) when (ex is not SpongeLLMException)
            {
                throw new SpongeLLMException("Failed to get model info");
            }
        }
    }
}