using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SpongeEngine.SpongeLLM.Core.Exceptions;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient
    {
        public class WhisperRequest
        {
            [JsonPropertyName("audio_data")]
            public string AudioData { get; set; } = string.Empty;

            [JsonPropertyName("prompt")]
            public string Prompt { get; set; } = string.Empty;

            [JsonPropertyName("suppress_non_speech")]
            public bool SuppressNonSpeech { get; set; } = false;
        }

        public class WhisperResponse
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = string.Empty;
        }
        
        /// <summary>
        /// Transcribes audio to text using Whisper.
        /// </summary>
        public async Task<WhisperResponse> TranscribeAudioAsync(WhisperRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                using HttpRequestMessage httpRequest = new(HttpMethod.Post, "api/extra/transcribe");
                var serializedJson = JsonSerializer.Serialize(request, Options.JsonSerializerOptions);
                httpRequest.Content = new StringContent(serializedJson, Encoding.UTF8, "application/json");

                using HttpResponseMessage response = await Options.HttpClient.SendAsync(httpRequest, cancellationToken);
                string content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new SpongeLLMException(
                        "Failed to transcribe audio",
                        (int)response.StatusCode,
                        content);
                }

                return JsonSerializer.Deserialize<WhisperResponse>(content) ??
                       throw new SpongeLLMException("Failed to deserialize transcribe response");
            }
            catch (Exception ex) when (ex is not SpongeLLMException)
            {
                throw new SpongeLLMException("Failed to transcribe audio", innerException: ex);
            }
        }
    }
}