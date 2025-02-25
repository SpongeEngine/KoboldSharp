using FluentAssertions;
using SpongeEngine.SpongeLLM.Core.Exceptions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Unit.StableDiffusion
{
    public class StableDiffusionSamplers : UnitTestBase
    {
        public StableDiffusionSamplers(ITestOutputHelper output) : base(output) {}
        
        [Fact]
        public async Task GetStableDiffusionSamplers_ShouldReturnAvailableSamplers()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/sdapi/v1/samplers")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(@"[
                        {
                            ""name"": ""Euler a"",
                            ""aliases"": [""euler_ancestral"", ""euler a""],
                            ""options"": {
                                ""k"": ""auto"",
                                ""steps"": 20
                            }
                        },
                        {
                            ""name"": ""DDIM"",
                            ""aliases"": [""ddim""],
                            ""options"": {
                                ""steps"": 20
                            }
                        }
                    ]"));

            // Act
            var samplers = await Client.GetStableDiffusionSamplersAsync();

            // Assert
            samplers.Should().NotBeNull();
            samplers.Should().HaveCount(2);
            
            var firstSampler = samplers[0];
            firstSampler.Name.Should().Be("Euler a");
            firstSampler.Aliases.Should().BeEquivalentTo(new[] { "euler_ancestral", "euler a" });
            firstSampler.Options.Should().ContainKey("k");
            firstSampler.Options.Should().ContainKey("steps");
        }

        [Fact]
        public async Task GetStableDiffusionSamplers_WhenNoSamplers_ShouldReturnEmptyList()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/sdapi/v1/samplers")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody("[]"));

            // Act
            var samplers = await Client.GetStableDiffusionSamplersAsync();

            // Assert
            samplers.Should().NotBeNull();
            samplers.Should().BeEmpty();
        }

        [Fact]
        public async Task GetStableDiffusionSamplers_WhenServerError_ShouldThrowException()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/sdapi/v1/samplers")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(500)
                    .WithBody("Internal Server Error"));

            // Act & Assert
            await Client.Invoking(c => c.GetStableDiffusionSamplersAsync())
                .Should().ThrowAsync<SpongeLLMException>()
                .WithMessage("Failed to get Stable Diffusion samplers");
        }

        [Fact]
        public async Task GetStableDiffusionSamplers_WithInvalidResponse_ShouldHandleGracefully()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/sdapi/v1/samplers")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(@"[{""name"": ""Invalid Sampler"",""aliases"": [],""options"": {}}]"));

            // Act
            var samplers = await Client.GetStableDiffusionSamplersAsync();

            // Assert
            samplers.Should().NotBeNull();
            samplers.Should().ContainSingle();
            var sampler = samplers[0];
            sampler.Name.Should().Be("Invalid Sampler");
            sampler.Aliases.Should().NotBeNull();
            sampler.Aliases.Should().BeEmpty();
            sampler.Options.Should().BeEmpty();
        }
    }
}