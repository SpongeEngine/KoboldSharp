using FluentAssertions;
using SpongeEngine.SpongeLLM.Core.Exceptions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Unit.Multiplayer
{
    public class SetMultiplayerStoryTests : UnitTestBase
    {
        public SetMultiplayerStoryTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task SetMultiplayerStory_WithValidRequest_ShouldSucceed()
        {
            // Arrange
            var request = new KoboldSharpClient.MultiplayerStoryRequest
            {
                FullUpdate = true,
                Data = "New story content",
                Sender = "TestUser",
                DataFormat = "plain"
            };

            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/multiplayer/setstory")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(@"{
                        ""success"": true,
                        ""turn_major"": 1,
                        ""turn_minor"": 0,
                        ""idle"": true,
                        ""data_format"": ""plain"",
                        ""error"": null
                    }"));

            // Act
            var result = await Client.SetMultiplayerStoryAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.TurnMajor.Should().Be(1);
            result.TurnMinor.Should().Be(0);
            result.IsIdle.Should().BeTrue();
            result.DataFormat.Should().Be("plain");
            result.Error.Should().BeNull();
        }

        [Fact]
        public async Task SetMultiplayerStory_WithDifferentDataFormat_ShouldSucceed()
        {
            // Arrange
            var request = new KoboldSharpClient.MultiplayerStoryRequest
            {
                FullUpdate = true,
                Data = "{\"content\": \"Story in JSON format\"}",
                Sender = "TestUser",
                DataFormat = "json"
            };

            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/multiplayer/setstory")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(@"{
                        ""success"": true,
                        ""turn_major"": 1,
                        ""turn_minor"": 0,
                        ""idle"": true,
                        ""data_format"": ""json""
                    }"));

            // Act
            var result = await Client.SetMultiplayerStoryAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.DataFormat.Should().Be("json");
        }

        [Fact]
        public async Task SetMultiplayerStory_WhenServerError_ShouldThrowException()
        {
            // Arrange
            var request = new KoboldSharpClient.MultiplayerStoryRequest
            {
                FullUpdate = true,
                Data = "New content",
                Sender = "TestUser",
                DataFormat = "plain"
            };

            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/multiplayer/setstory")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(500)
                    .WithBody("Internal Server Error"));

            // Act & Assert
            await Client.Invoking(c => c.SetMultiplayerStoryAsync(request))
                .Should().ThrowAsync<SpongeLLMException>()
                .WithMessage("Failed to set multiplayer story");
        }

        [Fact]
        public async Task SetMultiplayerStory_WithLongContent_ShouldSucceed()
        {
            // Arrange
            var longContent = new string('A', 10000); // 10KB of content
            var request = new KoboldSharpClient.MultiplayerStoryRequest
            {
                FullUpdate = true,
                Data = longContent,
                Sender = "TestUser",
                DataFormat = "plain"
            };

            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/multiplayer/setstory")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(@"{
                        ""success"": true,
                        ""turn_major"": 1,
                        ""turn_minor"": 0,
                        ""idle"": true,
                        ""data_format"": ""plain""
                    }"));

            // Act
            var result = await Client.SetMultiplayerStoryAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task SetMultiplayerStory_WithFailureResponse_ShouldIncludeError()
        {
            // Arrange
            var request = new KoboldSharpClient.MultiplayerStoryRequest
            {
                FullUpdate = true,
                Data = "New content",
                Sender = "TestUser",
                DataFormat = "plain"
            };

            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/multiplayer/setstory")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(@"{
                        ""success"": false,
                        ""turn_major"": 1,
                        ""turn_minor"": 0,
                        ""idle"": true,
                        ""data_format"": ""plain"",
                        ""error"": ""Another user is currently editing""
                    }"));

            // Act
            var result = await Client.SetMultiplayerStoryAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
            result.Error.Should().Contain("Another user");
        }
    }
}