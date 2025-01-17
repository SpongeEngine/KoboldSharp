﻿using System.Net.Http.Json;
using System.Text.Json;
using SpongeEngine.LLMSharp.Core.Exceptions;

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
                httpRequest.Content = JsonContent.Create(request);

                using HttpResponseMessage response = await Options.HttpClient.SendAsync(httpRequest, cancellationToken);
                string content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new LlmSharpException(
                        "Failed to set multiplayer story",
                        (int)response.StatusCode,
                        content);
                }

                return JsonSerializer.Deserialize<MultiplayerStoryResponse>(content) ?? throw new LlmSharpException("Failed to deserialize set multiplayer story response");
            }
            catch (Exception ex) when (ex is not LlmSharpException)
            {
                throw new LlmSharpException("Failed to set multiplayer story", innerException: ex);
            }
        }
    }
}