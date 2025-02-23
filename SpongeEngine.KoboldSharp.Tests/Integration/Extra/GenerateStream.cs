using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Integration.Extra
{
    [Trait("Category", "Integration")]
    [Trait("API", "Native")]
    public class GenerateStream : IntegrationTestBase
    {
        public GenerateStream(ITestOutputHelper output) : base(output) {}
        
        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task GenerateStream_ShouldStreamTokens()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            KoboldSharpClient.KoboldSharpRequest request = new KoboldSharpClient.KoboldSharpRequest
            {
                Prompt = "Write a short story about",
                MaxLength = 100,
                Temperature = 0.7f,
                TopP = 0.9f,
                TopK = 40,
                RepetitionPenalty = 1.1f,
                RepetitionPenaltyRange = 64,
                TrimStop = true
            };

            List<string> tokens = new List<string>();
            using CancellationTokenSource? cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            try
            {
                await foreach (String token in Client.GenerateStreamAsync(request, cts.Token))
                {
                    tokens.Add(token);
                    Output.WriteLine($"Received token: {token}");

                    if (tokens.Count >= request.MaxLength)
                    {
                        Output.WriteLine("Reached max length, breaking");
                        break;
                    }
                }
            }
            catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
            {
                Output.WriteLine($"Stream timed out after receiving {tokens.Count} tokens");
            }
            catch (Exception ex)
            {
                Output.WriteLine($"Error during streaming: {ex}");
                throw;
            }

            tokens.Should().NotBeEmpty("No tokens were received from the stream");
            string.Concat(tokens).Should().NotBeNullOrEmpty("Combined token text should not be empty");
        }
    }
}