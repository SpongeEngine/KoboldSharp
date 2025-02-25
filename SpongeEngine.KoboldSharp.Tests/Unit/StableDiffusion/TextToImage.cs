using FluentAssertions;
using SpongeEngine.SpongeLLM.Core.Exceptions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Unit.StableDiffusion
{
    public class TextToImage : UnitTestBase
    {
        public TextToImage(ITestOutputHelper output) : base(output) {}
        
        [Fact]
        public async Task TextToImage_WithValidRequest_ShouldReturnImages()
        {
            // Arrange
            var request = new KoboldSharpClient.StableDiffusionGenerationRequest
            {
                Prompt = "A beautiful landscape",
                Width = 512,
                Height = 512,
                Steps = 20,
                CfgScale = 7.0f,
                SamplerName = "euler_a"
            };

            // Mock base64 image data
            var mockImageBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==";

            Server
                .Given(Request.Create()
                    .WithPath("/sdapi/v1/txt2img")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody($@"{{
                        ""images"": [""{mockImageBase64}""],
                        ""parameters"": {{
                            ""prompt"": ""A beautiful landscape"",
                            ""steps"": 20,
                            ""cfg_scale"": 7.0
                        }},
                        ""info"": ""Generation successful""
                    }}"));

            // Act
            var result = await Client.TextToImageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Images.Should().NotBeEmpty();
            result.Images.Should().HaveCount(1);
            result.Images[0].Should().Be(mockImageBase64);
            result.Parameters.Should().NotBeEmpty();
            result.Info.Should().Be("Generation successful");
        }

        [Fact]
        public async Task TextToImage_WithNegativePrompt_ShouldIncludeInRequest()
        {
            // Arrange
            var request = new KoboldSharpClient.StableDiffusionGenerationRequest
            {
                Prompt = "A beautiful landscape",
                NegativePrompt = "ugly, blurry, distorted",
                Width = 512,
                Height = 512
            };

            Server
                .Given(Request.Create()
                    .WithPath("/sdapi/v1/txt2img")
                    .WithBody(body => 
                        body.Contains("negative_prompt") && 
                        body.Contains("ugly, blurry, distorted"))
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(@"{
                        ""images"": [""base64data""],
                        ""parameters"": {},
                        ""info"": """"
                    }"));

            // Act
            var result = await Client.TextToImageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Images.Should().NotBeEmpty();
        }

        [Fact]
        public async Task TextToImage_WithInvalidDimensions_ShouldThrowException()
        {
            // Arrange
            var request = new KoboldSharpClient.StableDiffusionGenerationRequest
            {
                Prompt = "A beautiful landscape",
                Width = 0,  // Invalid width
                Height = 512
            };

            Server
                .Given(Request.Create()
                    .WithPath("/sdapi/v1/txt2img")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(400)
                    .WithBody("Invalid dimensions"));

            // Act & Assert
            await Client.Invoking(c => c.TextToImageAsync(request))
                .Should().ThrowAsync<SpongeLLMException>()
                .WithMessage("Failed to generate image from text");
        }

        [Fact]
        public async Task TextToImage_WithServerError_ShouldThrowException()
        {
            // Arrange
            var request = new KoboldSharpClient.StableDiffusionGenerationRequest
            {
                Prompt = "A beautiful landscape"
            };

            Server
                .Given(Request.Create()
                    .WithPath("/sdapi/v1/txt2img")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(500)
                    .WithBody("Internal Server Error"));

            // Act & Assert
            await Client.Invoking(c => c.TextToImageAsync(request))
                .Should().ThrowAsync<SpongeLLMException>()
                .WithMessage("Failed to generate image from text");
        }
    }
}