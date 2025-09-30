using Microsoft.Extensions.Logging;
using MyModularMonolith.Modules.AI.Agents;
using MyModularMonolith.Modules.AI.Services;
using MyModularMonolith.Modules.Gyms.Contracts;

namespace MyModularMonolith.ModulesAI.Tests;

public class ReservationAgentTests
{
    private readonly IAIService _mockAIService;
    private readonly ILogger<ReservationAgent> _mockLogger;
    private readonly ReservationAgent _agent;

    public ReservationAgentTests()
    {
        _mockAIService = Substitute.For<IAIService>();
        _mockLogger = Substitute.For<ILogger<ReservationAgent>>();
        _agent = new ReservationAgent(_mockAIService, _mockLogger);
    }

    [Fact]
    public async Task AnalyzeSlotReservationsAsync_WithValidData_ReturnsAnalysis()
    {
        // Arrange
        var slotData = CreateTestSlotData();
        _mockAIService.GenerateResponseAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                     .Returns("AI analysis suggests optimal capacity management with selective confirmation strategy");

        // Act
        var result = await _agent.AnalyzeSlotReservationsAsync(slotData);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.GymProductId.Should().Be(slotData.GymProductId);
        result.Value.SlotDateTime.Should().Be(slotData.SlotDateTime);
        result.Value.TotalReservations.Should().Be(slotData.TotalReservations);
        result.Value.ConfidenceScore.Should().BeGreaterThan(0);
        result.Value.UserAnalyses.Should().HaveCount(slotData.Reservations.Count);
        result.Value.Recommendations.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AnalyzeSlotReservationsAsync_WithOverbookedSlot_ReturnsHighOverbookingPercentage()
    {
        // Arrange
        var slotData = CreateOverbookedSlotData();
        _mockAIService.GenerateResponseAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                     .Returns("High demand detected. Recommend selective confirmation based on priority scoring.");

        // Act
        var result = await _agent.AnalyzeSlotReservationsAsync(slotData);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.IsOverbooked.Should().BeTrue();
        result.Value.OverbookingPercentage.Should().BeGreaterThan(0);
        result.Value.UserAnalyses.Should().HaveCount(slotData.Reservations.Count);
    }

    [Fact]
    public async Task GetReservationRecommendationsAsync_WithOverbookedSlot_ReturnsSelectiveRecommendations()
    {
        // Arrange
        var slotData = CreateOverbookedSlotData();
        _mockAIService.GenerateResponseAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                     .Returns("Recommend confirming early registrations with high engagement scores.");

        // Act
        var result = await _agent.GetReservationRecommendationsAsync(slotData);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(slotData.Reservations.Count);

        // Should have mix of recommendations
        var actions = result.Value.Select(r => r.RecommendedAction).Distinct().ToList();
        actions.Should().Contain("Confirm");

        // All recommendations should have reasons
        result.Value.Should().OnlyContain(r => r.Reasons.Count > 0);

        // All should have confidence scores
        result.Value.Should().OnlyContain(r => r.Confidence > 0);
    }

    [Fact]
    public async Task GetReservationRecommendationsAsync_WithNormalCapacity_RecommendsConfirmMost()
    {
        // Arrange
        var slotData = CreateTestSlotData(); // Normal capacity
        _mockAIService.GenerateResponseAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                     .Returns("Standard management approach. Confirm based on registration order and engagement.");

        // Act
        var result = await _agent.GetReservationRecommendationsAsync(slotData);

        // Assert
        result.IsError.Should().BeFalse();

        var confirmCount = result.Value.Count(r => r.RecommendedAction == "Confirm");
        var totalCount = result.Value.Count;

        // Most should be confirmed since capacity is not exceeded
        confirmCount.Should().BeGreaterThan(totalCount / 2);
    }

    [Fact]
    public async Task AnalyzeSlotOptimizationAsync_WithValidData_ReturnsOptimizationStrategy()
    {
        // Arrange
        var slotData = CreateTestSlotData();
        _mockAIService.GenerateResponseAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                     .Returns("Optimal strategy: maintain 85% capacity utilization with strategic overbooking.");

        // Act
        var result = await _agent.AnalyzeSlotOptimizationAsync(slotData);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.GymProductId.Should().Be(slotData.GymProductId);
        result.Value.OverallStrategy.Should().NotBeNullOrEmpty();
        result.Value.OptimalCapacityUtilization.Should().BeInRange(0, 1);
        result.Value.Insights.Should().NotBeEmpty();
        result.Value.Recommendations.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GenerateSlotSummaryAsync_WithValidData_ReturnsEnrichedSummary()
    {
        // Arrange
        var slotData = CreateTestSlotData();
        _mockAIService.GenerateResponseAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                     .Returns("Class shows healthy engagement with good advance booking patterns. Recommend standard management approach.");

        // Act
        var result = await _agent.GenerateSlotSummaryAsync(slotData);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNullOrEmpty();
        result.Value.Should().Contain(slotData.ProductName);
        result.Value.Should().Contain(slotData.GymName);
        result.Value.Should().Contain("📊"); // Should contain structured formatting
    }



    private SlotReservationsDto CreateTestSlotData()
    {
        var reservations = new List<SlotReservationDto>
        {
            new(Guid.NewGuid(), "user1", DateTime.UtcNow.AddDays(1), "Pending", "Excited for class!", DateTime.UtcNow.AddDays(-3)),
            new(Guid.NewGuid(), "user2", DateTime.UtcNow.AddDays(1), "Pending", null, DateTime.UtcNow.AddDays(-1)),
            new(Guid.NewGuid(), "user3", DateTime.UtcNow.AddDays(1), "Confirmed", "Regular attendee", DateTime.UtcNow.AddDays(-7))
        };

        return new SlotReservationsDto(
            Guid.NewGuid(),
            "Test Gym",
            "Test Class",
            DateTime.UtcNow.AddDays(1),
            "18:30-19:30",
            true,
            3,
            2,
            1,
            25,
            false,
            reservations
        );
    }

    private SlotReservationsDto CreateOverbookedSlotData()
    {
        var reservations = new List<SlotReservationDto>();

        // Create 30 reservations for a class with capacity of 25
        for (int i = 0; i < 30; i++)
        {
            reservations.Add(new SlotReservationDto(
                Guid.NewGuid(),
                $"user{i}",
                DateTime.UtcNow.AddDays(1),
                "Pending",
                i % 3 == 0 ? $"User {i} notes" : null,
                DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 10))
            ));
        }

        return new SlotReservationsDto(
            Guid.NewGuid(),
            "Test Gym",
            "Popular Class",
            DateTime.UtcNow.AddDays(1),
            "18:30-19:30",
            true,
            30,
            30,
            0,
            25,
            true, // Overbooked
            reservations
        );
    }
}
