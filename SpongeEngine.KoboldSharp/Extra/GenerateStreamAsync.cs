using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient
    { 
        /// <summary>
        /// Generates text given a prompt and generation settings, with SSE streaming. Unspecified values are set to defaults.
        /// SSE streaming establishes a persistent connection, returning ongoing process in the form of message events.
        /// event: message
        /// data: {data}
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="customJsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="LlmSharpException"></exception>
        public async IAsyncEnumerable<string> GenerateStreamAsync(KoboldSharpClient.KoboldSharpRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            request.Stream = true;
            
            using HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/extra/generate/stream");
            var serializedJson = JsonSerializer.Serialize(request, Options.JsonSerializerOptions);
            httpRequest.Content = new StringContent(serializedJson, Encoding.UTF8, "application/json");
            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

            using HttpResponseMessage httpResponse = await Options.HttpClient.SendAsync(
                httpRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            
            // Remove the ReadAsStringAsync call to avoid buffering the entire response.
            httpResponse.EnsureSuccessStatusCode();
            
            using Stream stream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken);
            using StreamReader reader = new StreamReader(stream, Encoding.UTF8);

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                string? line = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line))
                {
                    await Task.Delay(50, cancellationToken);
                    continue;
                }

                Options.Logger?.LogDebug("Received line: {Line}", line);

                if (!line.StartsWith("data: ")) continue;

                var data = line[6..];
                if (data == "[DONE]") break;

                string? token = null;
                try
                {
                    var streamResponse = JsonSerializer.Deserialize<StreamResponse>(data);
                    token = streamResponse?.Token;
                }
                catch (JsonException ex)
                {
                    Options.Logger?.LogWarning(ex, "Failed to parse SSE message: {Message}", data);
                    continue;
                }

                if (!string.IsNullOrEmpty(token))
                {
                    Options.Logger?.LogDebug("Yielding token: {Token}", token);
                    yield return token;
                }
            }
        }
        
        private class StreamResponse
        {
            [JsonPropertyName("token")]
            public string Token { get; set; } = string.Empty;

            [JsonPropertyName("finish_reason")] 
            public string? FinishReason { get; set; }
        }
    }
}