using System.Text;
using System.Text.Json;
using SpongeEngine.SpongeLLM.Core.Exceptions;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient
    {
        public async Task<string> GetPendingOutputAsync(string? genKey = null, CancellationToken cancellationToken = default)
        {
            try
            {
                using HttpRequestMessage httpRequest = new(HttpMethod.Post, "api/extra/generate/check");
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
                        "Failed to get pending output",
                        (int)response.StatusCode,
                        content);
                }

                var result = JsonSerializer.Deserialize<GenerateAsyncResponse>(content);
                return result?.Results.FirstOrDefault()?.Text ?? string.Empty;
            }
            catch (Exception ex) when (ex is not SpongeLLMException)
            {
                throw new SpongeLLMException("Failed to get pending output", innerException: ex); 
            }
        }
    }
}