using FluentAssertions;
using SpongeEngine.SpongeLLM.Core.Exceptions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Unit.Extra
{
    public class PendingOutput : UnitTestBase
    {
        public PendingOutput(ITestOutputHelper output) : base(output) {}
        
        [Fact]
        public async Task GetPendingOutput_WithNoGenKey_ShouldReturnCurrentOutput()
        {
            // Arrange
            const string expectedOutput = "Generated text in progress";
            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/generate/check")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody($"{{\"results\": [{{\"text\": \"{expectedOutput}\"}}]}}"));

            // Act
            var result = await Client.GetPendingOutputAsync();

            // Assert
            result.Should().Be(expectedOutput);
        }

        [Fact]
        public async Task GetPendingOutput_WithGenKey_ShouldReturnSpecificOutput()
        {
            // Arrange
            const string genKey = "test-generation";
            const string expectedOutput = "Specific generation output";
            
            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/generate/check")
                    .WithBody(body => body.Contains(genKey))
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody($"{{\"results\": [{{\"text\": \"{expectedOutput}\"}}]}}"));

            // Act
            var result = await Client.GetPendingOutputAsync(genKey);

            // Assert
            result.Should().Be(expectedOutput);
        }

        [Fact]
        public async Task GetPendingOutput_WithNoActiveGeneration_ShouldReturnEmpty()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/generate/check")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody("{\"results\": []}"));

            // Act
            var result = await Client.GetPendingOutputAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPendingOutput_WhenServerError_ShouldThrowException()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/generate/check")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(500)
                    .WithBody("Internal Server Error"));

            // Act & Assert
            await Client.Invoking(c => c.GetPendingOutputAsync())
                .Should().ThrowAsync<SpongeLLMException>()
                .WithMessage("Failed to get pending output");
        }
    }
}