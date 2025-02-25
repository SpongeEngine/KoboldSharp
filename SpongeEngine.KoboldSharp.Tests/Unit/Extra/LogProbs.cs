using FluentAssertions;
using SpongeEngine.SpongeLLM.Core.Exceptions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Unit.Extra
{
    public class LogProbs : UnitTestBase
    {
        public LogProbs(ITestOutputHelper output) : base(output) {}
        
        [Fact]
        public async Task GetLastLogProbs_ShouldReturnProbabilityData()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/last_logprobs")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(@"{
                        ""logprobs"": {
                            ""content"": [
                                {
                                    ""token"": ""test"",
                                    ""logprob"": -0.5,
                                    ""bytes"": [116, 101, 115, 116],
                                    ""top_logprobs"": [
                                        {
                                            ""token"": ""test"",
                                            ""logprob"": -0.5,
                                            ""bytes"": [116, 101, 115, 116]
                                        }
                                    ]
                                }
                            ],
                            ""tokens"": [""test""],
                            ""token_logprobs"": [-0.5],
                            ""top_logprobs"": [{""test"": -0.5}],
                            ""text_offset"": [0]
                        }
                    }"));

            // Act
            var result = await Client.GetLastLogProbsAsync();

            // Assert
            result.Should().NotBeNull();
            result.LogProbabilities.Should().NotBeNull();
            result.LogProbabilities.Content.Should().HaveCount(1);
            result.LogProbabilities.Tokens.Should().HaveCount(1);
            result.LogProbabilities.TokenLogProbs.Should().HaveCount(1);
            
            var firstContent = result.LogProbabilities.Content[0];
            firstContent.Token.Should().Be("test");
            firstContent.LogProbability.Should().Be(-0.5f);
            firstContent.Bytes.Should().BeEquivalentTo(new[] { 116, 101, 115, 116 });
        }

        [Fact]
        public async Task GetLastLogProbs_WithNoGeneration_ShouldReturnEmptyData()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/last_logprobs")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(@"{
                        ""logprobs"": {
                            ""content"": [],
                            ""tokens"": [],
                            ""token_logprobs"": [],
                            ""top_logprobs"": [],
                            ""text_offset"": []
                        }
                    }"));

            // Act
            var result = await Client.GetLastLogProbsAsync();

            // Assert
            result.Should().NotBeNull();
            result.LogProbabilities.Should().NotBeNull();
            result.LogProbabilities.Content.Should().BeEmpty();
            result.LogProbabilities.Tokens.Should().BeEmpty();
            result.LogProbabilities.TokenLogProbs.Should().BeEmpty();
        }

        [Fact]
        public async Task GetLastLogProbs_WhenServerError_ShouldThrowException()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/last_logprobs")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(500)
                    .WithBody("Internal Server Error"));

            // Act & Assert
            await Client.Invoking(c => c.GetLastLogProbsAsync())
                .Should().ThrowAsync<SpongeLLMException>()
                .WithMessage("Failed to get last log probabilities");
        }
    }
}