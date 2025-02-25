using FluentAssertions;
using SpongeEngine.SpongeLLM.Core.Exceptions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Unit.Extra
{
    public class AbortGenerate : UnitTestBase
    {
        public AbortGenerate(ITestOutputHelper output) : base(output) {}
        
        [Fact]
        public async Task AbortGenerate_WithNoGenKey_ShouldAbortGeneration()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/abort")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody("{\"success\":\"true\",\"done\":\"true\"}"));

            // Act
            var result = await Client.AbortGenerateAsync();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task AbortGenerate_WithGenKey_ShouldAbortSpecificGeneration()
        {
            // Arrange
            const string genKey = "test-key";
            
            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/abort")
                    .WithBody(body => body.Contains(genKey))
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody("{\"success\":\"true\",\"done\":\"true\"}"));

            // Act
            var result = await Client.AbortGenerateAsync(genKey);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task AbortGenerate_WhenServerError_ShouldThrowException()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/abort")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(500)
                    .WithBody("Internal Server Error"));

            // Act & Assert
            await Client.Invoking(c => c.AbortGenerateAsync())
                .Should().ThrowAsync<SpongeLLMException>()
                .WithMessage("Failed to abort generation");
        }
    }
}