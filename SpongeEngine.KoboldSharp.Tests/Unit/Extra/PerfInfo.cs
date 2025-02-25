using FluentAssertions;
using SpongeEngine.SpongeLLM.Core.Exceptions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Unit.Extra
{
    public class PerfInfo : UnitTestBase
    {
        public PerfInfo(ITestOutputHelper output) : base(output) {}
        
        [Fact]
        public async Task GetPerfInfo_ShouldReturnPerformanceData()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/perf")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(@"{
                        ""last_process"": 0.5,
                        ""last_eval"": 0.3,
                        ""last_token_count"": 100,
                        ""last_seed"": 12345,
                        ""total_gens"": 50,
                        ""stop_reason"": 0,
                        ""total_img_gens"": 10,
                        ""queue"": 0,
                        ""idle"": 1,
                        ""hordeexitcounter"": 0,
                        ""uptime"": 3600.0,
                        ""idletime"": 1800.0,
                        ""quiet"": false
                    }"));

            // Act
            var result = await Client.GetPerfInfoAsync();

            // Assert
            result.Should().NotBeNull();
            result.LastProcessTime.Should().Be(0.5f);
            result.LastEvalTime.Should().Be(0.3f);
            result.LastTokenCount.Should().Be(100);
            result.LastSeed.Should().Be(12345);
            result.TotalGenerations.Should().Be(50);
            result.TotalImageGenerations.Should().Be(10);
            result.QueueSize.Should().Be(0);
            result.IsIdle.Should().Be(1);
            result.Uptime.Should().Be(3600.0f);
            result.IdleTime.Should().Be(1800.0f);
            result.IsQuiet.Should().BeFalse();
        }

        [Fact]
        public async Task GetPerfInfo_WithMinimalData_ShouldHandleDefaults()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/perf")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(@"{
                        ""last_process"": 0.0,
                        ""last_eval"": 0.0,
                        ""last_token_count"": 0,
                        ""last_seed"": 0,
                        ""total_gens"": 0,
                        ""stop_reason"": 0
                    }"));

            // Act
            var result = await Client.GetPerfInfoAsync();

            // Assert
            result.Should().NotBeNull();
            result.LastProcessTime.Should().Be(0);
            result.LastEvalTime.Should().Be(0);
            result.LastTokenCount.Should().Be(0);
            result.TotalGenerations.Should().Be(0);
        }

        [Fact]
        public async Task GetPerfInfo_WhenServerError_ShouldThrowException()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/perf")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(500)
                    .WithBody("Internal Server Error"));

            // Act & Assert
            await Client.Invoking(c => c.GetPerfInfoAsync())
                .Should().ThrowAsync<SpongeLLMException>()
                .WithMessage("Failed to get performance info");
        }
    }
}