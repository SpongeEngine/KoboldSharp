using System.Text;
using System.Text.Json;
using SpongeEngine.SpongeLLM.Core.Exceptions;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient
    {
        /// <summary>
        /// Sets the current multiplayer story.
        /// </summary>
        public async Task<MultiplayerStoryResponse> SetMultiplayerStoryAsync(MultiplayerStoryRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                using HttpRequestMessage httpRequest = new(HttpMethod.Post, "api/extra/multiplayer/setstory");
                var serializedJson = JsonSerializer.Serialize(request, Options.JsonSerializerOptions);
                httpRequest.Content = new StringContent(serializedJson, Encoding.UTF8, "application/json");

                using HttpResponseMessage response = await Options.HttpClient.SendAsync(httpRequest, cancellationToken);
                string content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new SpongeLLMException(
                        "Failed to set multiplayer story",
                        (int)response.StatusCode,
                        content);
                }

                return JsonSerializer.Deserialize<MultiplayerStoryResponse>(content) ?? throw new SpongeLLMException("Failed to deserialize set multiplayer story response");
            }
            catch (Exception ex) when (ex is not SpongeLLMException)
            {
                throw new SpongeLLMException("Failed to set multiplayer story", innerException: ex);
            }
        }
    }
}