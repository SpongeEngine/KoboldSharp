using FluentAssertions;
using SpongeEngine.SpongeLLM.Core.Exceptions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Unit.StableDiffusion
{
    public class InterrogateImage : UnitTestBase
    {
        public InterrogateImage(ITestOutputHelper output) : base(output) {}

        [Fact]
        public async Task InterrogateImage_WithValidImage_ShouldReturnCaption()
        {
            // Arrange
            var request = new KoboldSharpClient.InterrogateRequest
            {
                Image = SampleBase64Image
            };

            Server
                .Given(Request.Create()
                    .WithPath("/sdapi/v1/interrogate")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(@"{
                        ""caption"": ""A digital artwork showing a landscape with mountains""
                    }"));

            // Act
            var result = await Client.InterrogateImageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Caption.Should().NotBeNullOrEmpty();
            result.Caption.Should().Be("A digital artwork showing a landscape with mountains");
        }

        [Fact]
        public async Task InterrogateImage_WithInvalidImage_ShouldThrowException()
        {
            // Arrange
            var request = new KoboldSharpClient.InterrogateRequest
            {
                Image = "invalid_base64_data"
            };

            Server
                .Given(Request.Create()
                    .WithPath("/sdapi/v1/interrogate")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(400)
                    .WithBody("Invalid image data"));

            // Act & Assert
            await Client.Invoking(c => c.InterrogateImageAsync(request))
                .Should().ThrowAsync<SpongeLLMException>()
                .WithMessage("Failed to interrogate image");
        }

        [Fact]
        public async Task InterrogateImage_WithEmptyImage_ShouldThrowException()
        {
            // Arrange
            var request = new KoboldSharpClient.InterrogateRequest
            {
                Image = string.Empty
            };

            Server
                .Given(Request.Create()
                    .WithPath("/sdapi/v1/interrogate")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(400)
                    .WithBody("No image provided"));

            // Act & Assert
            await Client.Invoking(c => c.InterrogateImageAsync(request))
                .Should().ThrowAsync<SpongeLLMException>()
                .WithMessage("Failed to interrogate image");
        }

        [Fact]
        public async Task InterrogateImage_WhenServerError_ShouldThrowException()
        {
            // Arrange
            var request = new KoboldSharpClient.InterrogateRequest
            {
                Image = SampleBase64Image
            };

            Server
                .Given(Request.Create()
                    .WithPath("/sdapi/v1/interrogate")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(500)
                    .WithBody("Internal Server Error"));

            // Act & Assert
            await Client.Invoking(c => c.InterrogateImageAsync(request))
                .Should().ThrowAsync<SpongeLLMException>()
                .WithMessage("Failed to interrogate image");
        }

        [Fact]
        public async Task InterrogateImage_WithNullResponse_ShouldThrowException()
        {
            // Arrange
            var request = new KoboldSharpClient.InterrogateRequest
            {
                Image = SampleBase64Image
            };

            Server
                .Given(Request.Create()
                    .WithPath("/sdapi/v1/interrogate")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(500)  // Changed to error status code
                    .WithBody("Internal Server Error"));

            // Act & Assert
            await Client.Invoking(c => c.InterrogateImageAsync(request))
                .Should().ThrowAsync<SpongeLLMException>()
                .WithMessage("Failed to interrogate image");
        }
    }
}