using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using SpongeEngine.SpongeLLM.Core;
using SpongeEngine.SpongeLLM.Core.Interfaces;
using SpongeEngine.SpongeLLM.Core.Models;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient : LLMClientBase, IIsAvailable, ITextCompletion, IStreamableTextCompletion, IChatClient, IStopInference
    {
        public override KoboldSharpClientOptions Options { get; }

        public KoboldSharpClient(KoboldSharpClientOptions options): base(options)
        {
            Options = options;
        }
        
        #region IIsAvailable
        public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var response = await Options.HttpClient.GetAsync(Options.HttpClient.BaseAddress, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Options.Logger?.LogWarning(ex, "Availability check failed");
                return false;
            }
        }
        #endregion

        #region ITextCompletion
        public async Task<TextCompletionResult> CompleteTextAsync(TextCompletionRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            var koboldRequest = new KoboldSharpRequest
            {
                Prompt = request.Prompt,
                MaxLength = request.MaxTokens ?? 80,
                Temperature = request.Temperature,
                TopP = request.TopP,
                StopSequences = request.StopSequences.ToList(),
                Stream = false
            };

            var startTime = DateTime.UtcNow;
            var response = await GenerateAsync(koboldRequest, cancellationToken);

            // Get token counts using KoboldCpp's token counting API
            // var promptTokens = await CountTokensAsync(new CountTokensRequest { Prompt = request.Prompt }, cancellationToken);
            // var responseTokens = await CountTokensAsync(new CountTokensRequest { Prompt = response.Results[0].Text }, cancellationToken);

            return new TextCompletionResult
            {
                Text = response.Results[0].Text,
                ModelId = request.ModelId,
                GenerationTime = DateTime.UtcNow - startTime,
                // TokenUsage = new TextCompletionTokenUsage
                // {
                //     PromptTokens = promptTokens.Count,
                //     CompletionTokens = responseTokens.Count,
                //     TotalTokens = promptTokens.Count + responseTokens.Count
                // }
            };
        }
        #endregion

        #region IStreamableTextCompletion
        public async IAsyncEnumerable<TextCompletionToken> CompleteTextStreamAsync(TextCompletionRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            var koboldRequest = new KoboldSharpRequest
            {
                Prompt = request.Prompt,
                MaxLength = request.MaxTokens ?? 80,
                Temperature = request.Temperature,
                TopP = request.TopP,
                StopSequences = request.StopSequences.ToList(),
                Stream = true
            };

            await foreach (var token in GenerateStreamAsync(koboldRequest, cancellationToken))
            {
                var tokenCount = await CountTokensAsync(new CountTokensRequest { Prompt = token }, cancellationToken);
        
                yield return new TextCompletionToken
                {
                    Text = token,
                    TokenCount = tokenCount.Count
                };
            }
        }
        #endregion

        #region IChatClient
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<ChatCompletion> CompleteAsync(IList<ChatMessage> chatMessages, ChatOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<StreamingChatCompletionUpdate> CompleteStreamingAsync(IList<ChatMessage> chatMessages, ChatOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public object? GetService(Type serviceType, object? serviceKey = null)
        {
            throw new NotImplementedException();
        }

        public ChatClientMetadata Metadata { get; }
        #endregion

        #region IStopInference
        public async Task<bool> StopInference()
        {
            return await AbortGenerateAsync();
        }
        #endregion
    }
}