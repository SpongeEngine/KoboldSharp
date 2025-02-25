using FluentAssertions;
using SpongeEngine.SpongeLLM.Core.Exceptions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Unit.United
{
    public class GetVersionInfoAsync : UnitTestBase
    {
        public GetVersionInfoAsync(ITestOutputHelper output) : base(output) {}
        
        [Fact]
        public async Task GetVersionInfoAsync_ShouldReturnVersionInfo()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/api/v1/info/version")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody("{\"result\": \"1.2.3\"}"));
    
            // Act
            var versionInfo = await Client.GetVersionInfoAsync();

            // Assert
            versionInfo.Should().NotBeNull();
            versionInfo.Version.Should().Be("1.2.3");
        }
        
        [Fact]
        public async Task GetVersionInfoAsync_WhenServerError_ShouldThrowException()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/api/v1/info/version")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(500)
                    .WithBody("Internal Server Error"));
    
            // Act & Assert
            await Client.Invoking(c => c.GetVersionInfoAsync())
                .Should().ThrowAsync<SpongeLLMException>()
                .WithMessage("Failed to get version info");
        }
    }
}