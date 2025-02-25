using FluentAssertions;
using SpongeEngine.SpongeLLM.Core.Exceptions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Unit.Multiplayer
{
    public class GetMultiplayerStoryTests : UnitTestBase
    {
        public GetMultiplayerStoryTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task GetMultiplayerStory_ShouldReturnCurrentStory()
        {
            // Arrange
            const string expectedStory = "Once upon a time in a multiplayer game...";
            
            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/multiplayer/getstory")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(expectedStory));

            // Act
            var result = await Client.GetMultiplayerStoryAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedStory);
        }

        [Fact]
        public async Task GetMultiplayerStory_WhenEmpty_ShouldReturnEmptyString()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/multiplayer/getstory")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(string.Empty));

            // Act
            var result = await Client.GetMultiplayerStoryAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetMultiplayerStory_WhenServerError_ShouldThrowException()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/multiplayer/getstory")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(500)
                    .WithBody("Internal Server Error"));

            // Act & Assert
            await Client.Invoking(c => c.GetMultiplayerStoryAsync())
                .Should().ThrowAsync<SpongeLLMException>()
                .WithMessage("Failed to get multiplayer story");
        }

        [Fact]
        public async Task GetMultiplayerStory_WithLongStory_ShouldReturnFullContent()
        {
            // Arrange
            var longStory = new string('A', 10000); // 10KB story
            
            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/multiplayer/getstory")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(longStory));

            // Act
            var result = await Client.GetMultiplayerStoryAsync();

            // Assert
            result.Should().NotBeNull();
            result.Length.Should().Be(longStory.Length);
        }

        [Fact]
        public async Task GetMultiplayerStory_WithSpecialCharacters_ShouldPreserveContent()
        {
            // Arrange
            const string storyWithSpecialChars = "Story with\nnew lines,\ttabs, and 【special】 characters: 你好, مرحبا";
            
            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/multiplayer/getstory")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(storyWithSpecialChars));

            // Act
            var result = await Client.GetMultiplayerStoryAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(storyWithSpecialChars);
        }
    }
}