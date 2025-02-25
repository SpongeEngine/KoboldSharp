using FluentAssertions;
using SpongeEngine.SpongeLLM.Core.Exceptions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Unit.Extra
{
    public class CountTokensAsync : UnitTestBase
    {
        public CountTokensAsync(ITestOutputHelper output) : base(output) {}
        
        [Fact]
        public async Task CountTokensAsync_ShouldReturnTokenCount()
        {
            // Arrange
            var request = new KoboldSharpClient.CountTokensRequest
            {
                Prompt = "Hello world",
                IncludeSpecialTokens = true
            };

            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/tokencount")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody("{\"value\": 2, \"ids\": [15043, 2787]}"));
    
            // Act
            var result = await Client.CountTokensAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2);
            result.TokenIds.Should().BeEquivalentTo(new[] { 15043, 2787 });
        }
        
        [Fact]
        public async Task CountTokensAsync_WhenServerError_ShouldThrowException()
        {
            // Arrange
            var request = new KoboldSharpClient.CountTokensRequest
            {
                Prompt = "Hello world"
            };

            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/tokencount")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(500)
                    .WithBody("Internal Server Error"));
    
            // Act & Assert
            await Client.Invoking(c => c.CountTokensAsync(request))
                .Should().ThrowAsync<SpongeLLMException>()
                .WithMessage("Failed to count tokens");
        }
    }
}