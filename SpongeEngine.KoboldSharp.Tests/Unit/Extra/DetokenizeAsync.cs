using FluentAssertions;
using SpongeEngine.SpongeLLM.Core.Exceptions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Unit.Extra
{
    public class DetokenizeAsync : UnitTestBase
    {
        public DetokenizeAsync(ITestOutputHelper output) : base(output) {}
        
        [Fact]
        public async Task DetokenizeAsync_ShouldReturnDecodedText()
        {
            // Arrange
            var request = new KoboldSharpClient.DetokenizeRequest
            {
                TokenIds = new List<int> { 15043, 2787 } // Token IDs for "Hello world"
            };

            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/detokenize")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody("{\"result\": \"Hello world\", \"success\": true}"));
            
            // Act
            var result = await Client.DetokenizeAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Text.Should().Be("Hello world");
            result.Success.Should().BeTrue();
        }
        
        [Fact]
        public async Task DetokenizeAsync_WithEmptyTokens_ShouldReturnEmptyString()
        {
            // Arrange
            var request = new KoboldSharpClient.DetokenizeRequest
            {
                TokenIds = new List<int>()
            };

            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/detokenize")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody("{\"result\": \"\", \"success\": true}"));
            
            // Act
            var result = await Client.DetokenizeAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Text.Should().BeEmpty();
            result.Success.Should().BeTrue();
        }
        
        [Fact]
        public async Task DetokenizeAsync_WhenServerError_ShouldThrowException()
        {
            // Arrange
            var request = new KoboldSharpClient.DetokenizeRequest
            {
                TokenIds = new List<int> { 15043, 2787 }
            };

            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/detokenize")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(500)
                    .WithBody("Internal Server Error"));
            
            // Act & Assert
            await Client.Invoking(c => c.DetokenizeAsync(request))
                .Should().ThrowAsync<SpongeLLMException>()
                .WithMessage("Failed to detokenize");
        }
    }
}