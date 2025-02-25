using FluentAssertions;
using SpongeEngine.SpongeLLM.Core.Exceptions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Unit.Multiplayer
{
    public class MultiplayerStatus : UnitTestBase
    {
        public MultiplayerStatus(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task GetMultiplayerStatus_ShouldReturnCurrentStatus()
        {
            // Arrange
            var request = new KoboldSharpClient.MultiplayerStatusRequest
            {
                Sender = "TestUser",
                SenderBusy = false
            };

            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/multiplayer/status")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(@"{
                        ""turn_major"": 1,
                        ""turn_minor"": 2,
                        ""idle"": true,
                        ""data_format"": ""json""
                    }"));

            // Act
            var result = await Client.GetMultiplayerStatusAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.TurnMajor.Should().Be(1);
            result.TurnMinor.Should().Be(2);
            result.IsIdle.Should().BeTrue();
            result.DataFormat.Should().Be("json");
        }

        [Fact]
        public async Task GetMultiplayerStatus_WithBusySender_ShouldIndicateBusyState()
        {
            // Arrange
            var request = new KoboldSharpClient.MultiplayerStatusRequest
            {
                Sender = "TestUser",
                SenderBusy = true
            };

            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/multiplayer/status")
                    .WithBody(body => body.Contains("\"senderbusy\":true"))
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(@"{
                        ""turn_major"": 1,
                        ""turn_minor"": 0,
                        ""idle"": false,
                        ""data_format"": ""json""
                    }"));

            // Act
            var result = await Client.GetMultiplayerStatusAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsIdle.Should().BeFalse();
        }

        [Fact]
        public async Task GetMultiplayerStatus_WithInvalidSender_ShouldThrowException()
        {
            // Arrange
            var request = new KoboldSharpClient.MultiplayerStatusRequest
            {
                Sender = string.Empty,
                SenderBusy = false
            };

            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/multiplayer/status")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(400)
                    .WithBody("Invalid sender"));

            // Act & Assert
            await Client.Invoking(c => c.GetMultiplayerStatusAsync(request))
                .Should().ThrowAsync<SpongeLLMException>()
                .WithMessage("Failed to get multiplayer status");
        }

        [Fact]
        public async Task GetMultiplayerStatus_WhenServerError_ShouldThrowException()
        {
            // Arrange
            var request = new KoboldSharpClient.MultiplayerStatusRequest
            {
                Sender = "TestUser",
                SenderBusy = false
            };

            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/multiplayer/status")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(500)
                    .WithBody("Internal Server Error"));

            // Act & Assert
            await Client.Invoking(c => c.GetMultiplayerStatusAsync(request))
                .Should().ThrowAsync<SpongeLLMException>()
                .WithMessage("Failed to get multiplayer status");
        }

        [Fact]
        public async Task GetMultiplayerStatus_WithDifferentDataFormat_ShouldReturnFormat()
        {
            // Arrange
            var request = new KoboldSharpClient.MultiplayerStatusRequest
            {
                Sender = "TestUser",
                SenderBusy = false
            };

            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/multiplayer/status")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(@"{
                        ""turn_major"": 1,
                        ""turn_minor"": 0,
                        ""idle"": true,
                        ""data_format"": ""xml""
                    }"));

            // Act
            var result = await Client.GetMultiplayerStatusAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.DataFormat.Should().Be("xml");
        }
    }
}