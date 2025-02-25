using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SpongeEngine.SpongeLLM.Core.Exceptions;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient
    {
        private class AbortResponse
        {
            [JsonPropertyName("success")]
            public string Success { get; set; } = string.Empty;

            [JsonPropertyName("done")]
            public string Done { get; set; } = string.Empty;
        }
        
        public async Task<bool> AbortGenerateAsync(string? genKey = null, CancellationToken cancellationToken = default)
        {
            try
            {
                using HttpRequestMessage httpRequest = new(HttpMethod.Post, "api/extra/abort");
                if (!string.IsNullOrEmpty(genKey))
                {
                    var serializedJson = JsonSerializer.Serialize(new { genkey = genKey }, Options.JsonSerializerOptions);
                    httpRequest.Content = new StringContent(serializedJson, Encoding.UTF8, "application/json");
                }
                
                using HttpResponseMessage? response = await Options.HttpClient.SendAsync(httpRequest, cancellationToken);
                string? content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new SpongeLLMException(
                        "Failed to abort generation",
                        (int)response.StatusCode,
                        content);
                }

                var result = JsonSerializer.Deserialize<AbortResponse>(content);
                return result?.Success == "true";
            }
            catch (Exception ex) when (ex is not SpongeLLMException)
            {
                throw new SpongeLLMException("Failed to abort generation", innerException: ex);
            }
        }
    }
}