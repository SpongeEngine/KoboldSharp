using FluentAssertions;
using Microsoft.Extensions.Logging;
using SpongeEngine.SpongeLLM.Core.Exceptions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Unit.StableDiffusion
{
    public class ImageToImage : UnitTestBase
    {
        public ImageToImage(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task ImageToImage_WithValidRequest_ShouldReturnModifiedImage()
        {
            // Arrange
            var request = new KoboldSharpClient.StableDiffusionImageToImageRequest
            {
                InitImages = new List<string> { SampleBase64Image },
                Prompt = "Convert to oil painting",
                DenoisingStrength = 0.75f,
                Steps = 20,
                CfgScale = 7.0f,
                SamplerName = "euler_a"
            };

            Server
                .Given(Request.Create()
                    .WithPath("/sdapi/v1/img2img")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody($@"{{
                        ""images"": [""{SampleBase64Image}""],
                        ""parameters"": {{
                            ""prompt"": ""Convert to oil painting"",
                            ""denoising_strength"": 0.75,
                            ""steps"": 20
                        }},
                        ""info"": ""Generation successful""
                    }}"));

            // Act
            var result = await Client.ImageToImageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Images.Should().NotBeEmpty();
            result.Images.Should().HaveCount(1);
            result.Images[0].Should().Be(SampleBase64Image);
            result.Parameters.Should().NotBeEmpty();
            result.Info.Should().Be("Generation successful");
        }

        [Fact]
        public async Task ImageToImage_WithDifferentDenoisingStrength_ShouldWork()
        {
            var mockBase64Image = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==";
            var strengths = new[] { 0.3f, 0.7f, 0.9f };
    
            // Setup a single response for all denoising strengths
            Server
                .Given(Request.Create()
                    .WithPath("/sdapi/v1/img2img")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody($@"{{
                ""images"": [""{mockBase64Image}""],
                ""parameters"": {{}},
                ""info"": """"
            }}"));

            foreach (var strength in strengths)
            {
                // Setup the request
                var request = new KoboldSharpClient.StableDiffusionImageToImageRequest
                {
                    InitImages = new List<string> { mockBase64Image },
                    Prompt = "Convert to sketch",
                    DenoisingStrength = strength,
                    Steps = 20
                };

                // Log what we're doing
                Client.Options.Logger.LogInformation($"Testing denoising strength: {strength}");

                // Act
                var result = await Client.ImageToImageAsync(request);

                // Assert
                result.Should().NotBeNull();
                result.Images.Should().NotBeEmpty();
                result.Images.Should().HaveCount(1);
                result.Images[0].Should().Be(mockBase64Image);
        
                Client.Options.Logger.LogInformation($"Successfully generated image with denoising strength {strength}");
            }
        }

        [Fact]
        public async Task ImageToImage_WithNoInitialImage_ShouldThrowException()
        {
            // Arrange
            var request = new KoboldSharpClient.StableDiffusionImageToImageRequest
            {
                InitImages = new List<string>(),
                Prompt = "Convert to oil painting"
            };

            Server
                .Given(Request.Create()
                    .WithPath("/sdapi/v1/img2img")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(400)
                    .WithBody("No input image provided"));

            // Act & Assert
            await Client.Invoking(c => c.ImageToImageAsync(request))
                .Should().ThrowAsync<SpongeLLMException>()
                .WithMessage("Failed to generate image from image");
        }

        [Fact]
        public async Task ImageToImage_WithInvalidImage_ShouldThrowException()
        {
            // Arrange
            var request = new KoboldSharpClient.StableDiffusionImageToImageRequest
            {
                InitImages = new List<string> { "invalid_base64_data" },
                Prompt = "Convert to oil painting"
            };

            Server
                .Given(Request.Create()
                    .WithPath("/sdapi/v1/img2img")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(400)
                    .WithBody("Invalid image data"));

            // Act & Assert
            await Client.Invoking(c => c.ImageToImageAsync(request))
                .Should().ThrowAsync<SpongeLLMException>()
                .WithMessage("Failed to generate image from image");
        }

        [Fact]
        public async Task ImageToImage_WithServerError_ShouldThrowException()
        {
            // Arrange
            var request = new KoboldSharpClient.StableDiffusionImageToImageRequest
            {
                InitImages = new List<string> { SampleBase64Image },
                Prompt = "Convert to oil painting"
            };

            Server
                .Given(Request.Create()
                    .WithPath("/sdapi/v1/img2img")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(500)
                    .WithBody("Internal Server Error"));

            // Act & Assert
            await Client.Invoking(c => c.ImageToImageAsync(request))
                .Should().ThrowAsync<SpongeLLMException>()
                .WithMessage("Failed to generate image from image");
        }
    }
}