using System.Text.Json;
using System.Text.Json.Serialization;
using SpongeEngine.SpongeLLM.Core.Exceptions;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient
    {
        public class LastLogProbsResponse 
        {
            [JsonPropertyName("logprobs")]
            public LastLogProbsData? LogProbabilities { get; set; }
        }

        public class LastLogProbsData
        {
            [JsonPropertyName("content")]
            public List<LogProbItem> Content { get; set; } = new();

            [JsonPropertyName("tokens")]
            public List<string> Tokens { get; set; } = new();

            [JsonPropertyName("token_logprobs")]
            public List<float> TokenLogProbs { get; set; } = new();

            [JsonPropertyName("top_logprobs")]
            public List<Dictionary<string, float>> TopLogProbs { get; set; } = new();

            [JsonPropertyName("text_offset")]
            public List<int> TextOffsets { get; set; } = new();
        }
        
        public class LogProbItem
        {
            [JsonPropertyName("token")]
            public string Token { get; set; } = string.Empty;

            [JsonPropertyName("logprob")]
            public float LogProbability { get; set; }

            [JsonPropertyName("bytes")] 
            public List<int> Bytes { get; set; } = new();

            [JsonPropertyName("top_logprobs")]
            public List<TopLogProbItem> TopLogProbabilities { get; set; } = new();
        }
        
        public class TopLogProbItem
        {
            [JsonPropertyName("token")]
            public string Token { get; set; } = string.Empty;

            [JsonPropertyName("logprob")]
            public float LogProbability { get; set; }

            [JsonPropertyName("bytes")]
            public List<int> Bytes { get; set; } = new();
        }
        
        /// <summary>
        /// Gets log probabilities from the last generation.
        /// </summary>
        public async Task<LastLogProbsResponse> GetLastLogProbsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                HttpResponseMessage? response = await Options.HttpClient.GetAsync("api/extra/last_logprobs", cancellationToken);
                string? content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new SpongeLLMException(
                        "Failed to get last log probabilities",
                        (int)response.StatusCode,
                        content);
                }

                return JsonSerializer.Deserialize<LastLogProbsResponse>(content) ?? throw new SpongeLLMException("Failed to deserialize last log probabilities");
            }
            catch (Exception ex) when (ex is not SpongeLLMException)
            {
                throw new SpongeLLMException("Failed to get last log probabilities", innerException: ex);
            }
        }
    }
}