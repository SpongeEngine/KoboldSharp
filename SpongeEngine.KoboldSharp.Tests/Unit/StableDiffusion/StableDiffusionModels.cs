using FluentAssertions;
using SpongeEngine.SpongeLLM.Core.Exceptions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Unit.StableDiffusion
{
    public class StableDiffusionModels : UnitTestBase
    {
        public StableDiffusionModels(ITestOutputHelper output) : base(output) {}
        
        [Fact]
        public async Task GetStableDiffusionModels_ShouldReturnAvailableModels()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/sdapi/v1/sd-models")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(@"[
                        {
                            ""title"": ""Stable Diffusion v1.5"",
                            ""model_name"": ""sd_v1.5"",
                            ""hash"": ""hash123"",
                            ""filename"": ""sd_v1.5.safetensors""
                        },
                        {
                            ""title"": ""Stable Diffusion v2.1"",
                            ""model_name"": ""sd_v2.1"",
                            ""hash"": ""hash456"",
                            ""filename"": ""sd_v2.1.safetensors""
                        }
                    ]"));

            // Act
            var models = await Client.GetStableDiffusionModelsAsync();

            // Assert
            models.Should().NotBeNull();
            models.Should().HaveCount(2);
            
            var firstModel = models[0];
            firstModel.Title.Should().Be("Stable Diffusion v1.5");
            firstModel.ModelName.Should().Be("sd_v1.5");
            firstModel.Hash.Should().Be("hash123");
            firstModel.Filename.Should().Be("sd_v1.5.safetensors");
        }

        [Fact]
        public async Task GetStableDiffusionModels_WhenNoModels_ShouldReturnEmptyList()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/sdapi/v1/sd-models")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody("[]"));

            // Act
            var models = await Client.GetStableDiffusionModelsAsync();

            // Assert
            models.Should().NotBeNull();
            models.Should().BeEmpty();
        }

        [Fact]
        public async Task GetStableDiffusionModels_WhenServerError_ShouldThrowException()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/sdapi/v1/sd-models")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(500)
                    .WithBody("Internal Server Error"));

            // Act & Assert
            await Client.Invoking(c => c.GetStableDiffusionModelsAsync())
                .Should().ThrowAsync<SpongeLLMException>()
                .WithMessage("Failed to get Stable Diffusion models");
        }
    }
}