﻿using System.Text.Json;
using System.Text.Json.Serialization;
using SpongeEngine.LLMSharp.Core.Exceptions;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient
    {
        public class KoboldVersionInfo
        {
            [JsonPropertyName("result")]
            public string Version { get; set; } = string.Empty;
        }
        
        /// <summary>
        /// Gets basic version information about KoboldCpp.
        /// </summary>
        public async Task<KoboldVersionInfo> GetVersionInfoAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                HttpResponseMessage? response = await Options.HttpClient.GetAsync("api/v1/info/version", cancellationToken);
                string? content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new LlmSharpException(
                        "Failed to get version info",
                        (int)response.StatusCode,
                        content);
                }

                return JsonSerializer.Deserialize<KoboldVersionInfo>(content) ?? throw new LlmSharpException("Failed to deserialize version info");
            }
            catch (Exception ex) when (ex is not LlmSharpException)
            {
                throw new LlmSharpException("Failed to get version info", innerException: ex);
            }
        }
    }
}