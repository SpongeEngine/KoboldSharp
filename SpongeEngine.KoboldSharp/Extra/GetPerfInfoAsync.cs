using System.Text.Json;
using System.Text.Json.Serialization;
using SpongeEngine.SpongeLLM.Core.Exceptions;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient
    {
        public class KoboldPerfInfo
        {
            [JsonPropertyName("last_process")]
            public float LastProcessTime { get; set; }

            [JsonPropertyName("last_eval")]
            public float LastEvalTime { get; set; }

            [JsonPropertyName("last_token_count")]
            public int LastTokenCount { get; set; }

            [JsonPropertyName("last_seed")]
            public int LastSeed { get; set; }

            [JsonPropertyName("total_gens")]
            public int TotalGenerations { get; set; }

            [JsonPropertyName("stop_reason")]
            public int StopReason { get; set; }

            [JsonPropertyName("total_img_gens")]
            public int TotalImageGenerations { get; set; }

            [JsonPropertyName("queue")]
            public int QueueSize { get; set; }

            [JsonPropertyName("idle")]
            public int IsIdle { get; set; }

            [JsonPropertyName("hordeexitcounter")]
            public int HordeExitCounter { get; set; }

            [JsonPropertyName("uptime")]
            public float Uptime { get; set; }

            [JsonPropertyName("idletime")]
            public float IdleTime { get; set; }

            [JsonPropertyName("quiet")]
            public bool IsQuiet { get; set; }
        }
        
        /// <summary>
        /// Gets performance information about the current KoboldCpp instance.
        /// </summary>
        public async Task<KoboldPerfInfo> GetPerfInfoAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                HttpResponseMessage? response = await Options.HttpClient.GetAsync("api/extra/perf", cancellationToken);
                string? content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new SpongeLLMException(
                        "Failed to get performance info",
                        (int)response.StatusCode,
                        content);
                }

                return JsonSerializer.Deserialize<KoboldPerfInfo>(content) ?? throw new SpongeLLMException("Failed to deserialize performance info");
            }
            catch (Exception ex) when (ex is not SpongeLLMException)
            {
                throw new SpongeLLMException("Failed to get performance info", innerException: ex);
            }
        }
    }
}