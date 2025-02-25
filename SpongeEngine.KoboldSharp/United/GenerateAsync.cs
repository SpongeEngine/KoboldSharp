using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SpongeEngine.SpongeLLM.Core.Exceptions;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient
    {
        /// <summary>
        /// Generates text given a prompt and generation settings.
        /// Unspecified values are set to defaults.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="customJsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="LlmSharpException"></exception>
        public async Task<GenerateAsyncResponse> GenerateAsync(KoboldSharpRequest request, CancellationToken cancellationToken = default)
        {
            //KoboldCppUtils.ValidateRequest(request);
            
            try
            {
                using HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/v1/generate");
                var serializedJson = JsonSerializer.Serialize(request, Options.JsonSerializerOptions);
                httpRequest.Content = new StringContent(serializedJson, Encoding.UTF8, "application/json");
                
                using HttpResponseMessage? httpResponse = await Options.HttpClient.SendAsync(
                    httpRequest,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);
                string responseContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                
                Options.Logger?.LogDebug("Generation response status: {Status}", httpResponse.StatusCode);
                Options.Logger?.LogDebug("Generation raw response: {Response}", responseContent);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    Options.Logger?.LogError("Non-success status code: {Status}", httpResponse.StatusCode);
                    throw new SpongeLLMException(
                        "Generation request failed",
                        (int)httpResponse.StatusCode,
                        responseContent);
                }

                try {
                    GenerateAsyncResponse? result = JsonSerializer.Deserialize<GenerateAsyncResponse>(responseContent, Options.JsonSerializerOptions);
                    if (result == null)
                    {
                        Options.Logger?.LogError("Deserialized response is null");
                        throw new SpongeLLMException(
                            "Null response after deserialization",
                            null,
                            responseContent);
                    }
                    
                    if (result.Results == null || !result.Results.Any())
                    {
                        Options.Logger?.LogError("Response has no results");
                        throw new SpongeLLMException(
                            "No results in response",
                            null,
                            responseContent);
                    }

                    return result;
                }
                catch (JsonException ex)
                {
                    Options.Logger?.LogError(ex, "Failed to deserialize response: {Response}", responseContent);
                    throw new SpongeLLMException(
                        "Failed to deserialize response",
                        null,
                        $"Response: {responseContent}, Error: {ex.Message}");
                }
            }
            catch (Exception ex) when (ex is not SpongeLLMException)
            {
                Options.Logger?.LogError(ex, "Failed to generate response");
                throw new SpongeLLMException("Failed to generate response", null, ex.Message);
            }
        }
        
        public class GenerateAsyncResponse
        {
            [JsonPropertyName("results")]
            public List<GenerateAsyncResponseResult> Results { get; set; } = new();
        }

        public class GenerateAsyncResponseResult
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = string.Empty;
        }
    }
}