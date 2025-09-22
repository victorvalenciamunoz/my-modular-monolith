using ErrorOr;
using Microsoft.Extensions.Logging;
using MyModularMonolith.Modules.AI.Models;
using MyModularMonolith.Modules.AI.Prompts;
using MyModularMonolith.Modules.AI.Services;
using MyModularMonolith.Modules.Gyms.Contracts;

namespace MyModularMonolith.Modules.AI.Agents;

public class ReservationAgent : IReservationAgent
{
    private readonly IAIService _aiService;
    private readonly ILogger<ReservationAgent> _logger;

    public ReservationAgent(IAIService aiService, ILogger<ReservationAgent> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<ErrorOr<ReservationAnalysis>> AnalyzeSlotReservationsAsync(
        SlotReservationsDto slotData,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Analyzing slot reservations for {GymProductId} at {SlotDateTime}",
                slotData.GymProductId, slotData.SlotDateTime);

            var prompt = ReservationPrompts.CreateAnalysisPrompt(slotData);
            var response = await _aiService.GenerateResponseAsync(prompt, cancellationToken);

            if (response.IsError)
            {
                return response.Errors;
            }

            // Crear análisis basado en datos + respuesta de IA
            var analysis = CreateReservationAnalysis(slotData, response.Value);

            _logger.LogInformation("Analysis completed for {GymProductId} with confidence {Confidence}",
                slotData.GymProductId, analysis.ConfidenceScore);

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing slot reservations for {GymProductId} at {SlotDateTime}",
                slotData.GymProductId, slotData.SlotDateTime);
            return Error.Failure("AI.AnalysisError", "Failed to analyze slot reservations");
        }
    }

    public async Task<ErrorOr<List<ReservationRecommendation>>> GetReservationRecommendationsAsync(
        SlotReservationsDto slotData,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating recommendations for {GymProductId} at {SlotDateTime}",
                slotData.GymProductId, slotData.SlotDateTime);

            var prompt = ReservationPrompts.CreateRecommendationPrompt(slotData);
            var response = await _aiService.GenerateResponseAsync(prompt, cancellationToken);

            if (response.IsError)
            {
                return response.Errors;
            }

            // Generar recomendaciones inteligentes
            var recommendations = GenerateRecommendations(slotData, response.Value);

            _logger.LogInformation("Generated {Count} recommendations for {GymProductId}",
                recommendations.Count, slotData.GymProductId);

            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recommendations for {GymProductId} at {SlotDateTime}",
                slotData.GymProductId, slotData.SlotDateTime);
            return Error.Failure("AI.RecommendationError", "Failed to generate recommendations");
        }
    }

    public async Task<ErrorOr<SlotAnalysis>> AnalyzeSlotOptimizationAsync(
        SlotReservationsDto slotData,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Analyzing slot optimization for {GymProductId} at {SlotDateTime}",
                slotData.GymProductId, slotData.SlotDateTime);

            var prompt = ReservationPrompts.CreateOptimizationPrompt(slotData);
            var response = await _aiService.GenerateResponseAsync(prompt, cancellationToken);

            if (response.IsError)
            {
                return response.Errors;
            }

            // Primero generar recomendaciones
            var recommendationsResult = await GetReservationRecommendationsAsync(slotData, cancellationToken);
            var recommendations = recommendationsResult.IsError ?
                new List<ReservationRecommendation>() : recommendationsResult.Value;

            var analysis = CreateSlotAnalysis(slotData, response.Value, recommendations);

            _logger.LogInformation("Optimization analysis completed for {GymProductId} with strategy {Strategy}",
                slotData.GymProductId, analysis.OverallStrategy);

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing slot optimization for {GymProductId} at {SlotDateTime}",
                slotData.GymProductId, slotData.SlotDateTime);
            return Error.Failure("AI.OptimizationError", "Failed to analyze slot optimization");
        }
    }

    public async Task<ErrorOr<string>> GenerateSlotSummaryAsync(
        SlotReservationsDto slotData,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating summary for {GymProductId} at {SlotDateTime}",
                slotData.GymProductId, slotData.SlotDateTime);

            var prompt = ReservationPrompts.CreateSummaryPrompt(slotData);
            var response = await _aiService.GenerateResponseAsync(prompt, cancellationToken);

            if (response.IsError)
            {
                return response.Errors;
            }

            // Enriquecer la respuesta de IA con datos estructurados
            var enrichedSummary = EnrichSummaryWithData(slotData, response.Value);

            return enrichedSummary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating summary for {GymProductId} at {SlotDateTime}",
                slotData.GymProductId, slotData.SlotDateTime);
            return Error.Failure("AI.SummaryError", "Failed to generate slot summary");
        }
    }

    private ReservationAnalysis CreateReservationAnalysis(SlotReservationsDto slotData, string aiResponse)
    {
        // Calcular métricas básicas
        var overbookingPercentage = slotData.MaxCapacity.HasValue && slotData.MaxCapacity > 0 ?
            Math.Max(0, (decimal)(slotData.TotalReservations - slotData.MaxCapacity.Value) / slotData.MaxCapacity.Value * 100) : 0;

        // Analizar cada reserva individual
        var userAnalyses = slotData.Reservations.Select((reservation, index) =>
            AnalyzeUserReservation(reservation, index, slotData)).ToList();

        // Generar recomendaciones basadas en IA + lógica de negocio
        var recommendations = GenerateAnalysisRecommendations(slotData, aiResponse);

        // Calcular score de confianza basado en varios factores
        var confidenceScore = CalculateConfidenceScore(slotData, userAnalyses);

        return new ReservationAnalysis(
            slotData.GymProductId,
            slotData.SlotDateTime,
            slotData.TotalReservations,
            slotData.PendingReservations,
            slotData.ConfirmedReservations,
            slotData.MaxCapacity,
            slotData.IsOverbooked,
            overbookingPercentage,
            userAnalyses,
            recommendations,
            confidenceScore
        );
    }

    private UserReservationAnalysis AnalyzeUserReservation(SlotReservationDto reservation, int index, SlotReservationsDto slotData)
    {
        // Calcular probabilidad de asistencia basada en varios factores
        var attendanceProbability = CalculateAttendanceProbability(reservation, slotData);

        // Calcular score de prioridad
        var priorityScore = CalculatePriorityScore(reservation, index, slotData);

        // Determinar acción recomendada
        var recommendedAction = DetermineRecommendedAction(reservation, priorityScore, slotData);

        // Generar razones para la recomendación
        var reasons = GenerateRecommendationReasons(reservation, attendanceProbability, priorityScore, slotData);

        return new UserReservationAnalysis(
            reservation.UserId,
            reservation.Id,
            reservation.Status,
            reservation.CreatedAt,
            attendanceProbability,
            priorityScore,
            recommendedAction,
            reasons
        );
    }

    private decimal CalculateAttendanceProbability(SlotReservationDto reservation, SlotReservationsDto slotData)
    {
        decimal probability = 0.5m; // Base probability

        // Factor 1: Tiempo de anticipación (más anticipación = mayor probabilidad)
        var daysInAdvance = (slotData.SlotDateTime - reservation.CreatedAt).TotalDays;
        if (daysInAdvance > 7) probability += 0.2m;
        else if (daysInAdvance > 3) probability += 0.1m;
        else if (daysInAdvance < 1) probability -= 0.1m;

        // Factor 2: Tiene notas del usuario (muestra interés)
        if (!string.IsNullOrWhiteSpace(reservation.UserNotes)) probability += 0.15m;

        // Factor 3: Estado actual
        if (reservation.Status == "Confirmed") probability += 0.1m;

        // Factor 4: Día de la semana y hora
        var dayOfWeek = slotData.SlotDateTime.DayOfWeek;
        var hour = slotData.SlotDateTime.Hour;

        // Clases en horas prime time tienen mayor asistencia
        if ((hour >= 18 && hour <= 20) || (hour >= 8 && hour <= 10)) probability += 0.1m;

        // Weekends pueden tener diferente patrón
        if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday) probability += 0.05m;

        return Math.Max(0.1m, Math.Min(0.95m, probability));
    }

    private int CalculatePriorityScore(SlotReservationDto reservation, int index, SlotReservationsDto slotData)
    {
        int score = 50; // Base score

        // Factor 1: Orden de llegada (early bird bonus)
        var totalReservations = slotData.Reservations.Count;
        var positionBonus = Math.Max(0, 30 - (index * 30 / totalReservations));
        score += positionBonus;

        // Factor 2: Tiempo de anticipación
        var daysInAdvance = (slotData.SlotDateTime - reservation.CreatedAt).TotalDays;
        if (daysInAdvance > 7) score += 20;
        else if (daysInAdvance > 3) score += 10;

        // Factor 3: Engagement (tiene notas)
        if (!string.IsNullOrWhiteSpace(reservation.UserNotes)) score += 15;

        // Factor 4: Estado confirmado
        if (reservation.Status == "Confirmed") score += 10;

        return Math.Max(1, Math.Min(100, score));
    }

    private string DetermineRecommendedAction(SlotReservationDto reservation, int priorityScore, SlotReservationsDto slotData)
    {
        // Si no hay límite de capacidad, confirmar todos
        if (!slotData.MaxCapacity.HasValue)
            return "Confirm";

        // Si ya está confirmado, mantener
        if (reservation.Status == "Confirmed")
            return "Maintain";

        // Calcular cuántos pueden ser confirmados
        var availableSpots = slotData.MaxCapacity.Value - slotData.ConfirmedReservations;
        var pendingByPriority = slotData.Reservations
            .Where(r => r.Status == "Pending")
            .OrderByDescending(r => CalculatePriorityScore(r, slotData.Reservations.ToList().IndexOf(r), slotData))
            .ToList();

        var positionInQueue = pendingByPriority.IndexOf(reservation) + 1;

        if (positionInQueue <= availableSpots)
        {
            if (priorityScore >= 70) return "Confirm";
            if (priorityScore >= 50) return "Confirm";
            return "WaitingList";
        }
        else
        {
            if (priorityScore >= 80) return "WaitingList";
            return "Reject";
        }
    }

    private List<string> GenerateRecommendationReasons(SlotReservationDto reservation, decimal attendanceProbability, int priorityScore, SlotReservationsDto slotData)
    {
        var reasons = new List<string>();

        if (priorityScore >= 70)
            reasons.Add("High priority score due to early registration and engagement");

        if (attendanceProbability >= 0.7m)
            reasons.Add($"High attendance probability ({attendanceProbability:P0})");

        if (!string.IsNullOrWhiteSpace(reservation.UserNotes))
            reasons.Add("User showed engagement with personal notes");

        var daysInAdvance = (slotData.SlotDateTime - reservation.CreatedAt).TotalDays;
        if (daysInAdvance > 7)
            reasons.Add("Early registration shows commitment");

        if (slotData.IsOverbooked)
            reasons.Add("Class is overbooked, selective approval needed");

        if (reasons.Count == 0)
            reasons.Add("Standard evaluation based on capacity and timing");

        return reasons;
    }

    private List<ReservationRecommendation> GenerateRecommendations(SlotReservationsDto slotData, string aiResponse)
    {
        return slotData.Reservations.Select((reservation, index) =>
        {
            var priorityScore = CalculatePriorityScore(reservation, index, slotData);
            var attendanceProbability = CalculateAttendanceProbability(reservation, slotData);
            var recommendedAction = DetermineRecommendedAction(reservation, priorityScore, slotData);
            var reasons = GenerateRecommendationReasons(reservation, attendanceProbability, priorityScore, slotData);

            var confidence = CalculateRecommendationConfidence(priorityScore, attendanceProbability);

            return new ReservationRecommendation(
                reservation.Id,
                reservation.UserId,
                recommendedAction,
                confidence,
                reasons,
                new Dictionary<string, object>
                {
                    { "PriorityScore", priorityScore },
                    { "AttendanceProbability", attendanceProbability },
                    { "RegistrationOrder", index + 1 },
                    { "AIInsight", aiResponse.Substring(0, Math.Min(100, aiResponse.Length)) }
                }
            );
        }).ToList();
    }

    private SlotAnalysis CreateSlotAnalysis(SlotReservationsDto slotData, string aiResponse, List<ReservationRecommendation> recommendations)
    {
        var strategy = DetermineOverallStrategy(slotData, recommendations);
        var optimalUtilization = CalculateOptimalUtilization(slotData, recommendations);
        var insights = GenerateInsights(slotData, aiResponse, recommendations);

        return new SlotAnalysis(
            slotData.GymProductId,
            slotData.SlotDateTime,
            slotData.GymName,
            slotData.ProductName,
            slotData.TotalReservations,
            slotData.MaxCapacity,
            recommendations,
            strategy,
            optimalUtilization,
            insights
        );
    }

    private string DetermineOverallStrategy(SlotReservationsDto slotData, List<ReservationRecommendation> recommendations)
    {
        if (!slotData.MaxCapacity.HasValue)
            return "ConfirmAll";

        var confirmCount = recommendations.Count(r => r.RecommendedAction == "Confirm");
        var utilizationRate = (decimal)confirmCount / slotData.MaxCapacity.Value;

        if (utilizationRate >= 0.9m)
            return "OptimalCapacity";
        else if (slotData.IsOverbooked)
            return "SelectivePlacement";
        else if (utilizationRate < 0.5m)
            return "PromoteClass";
        else
            return "StandardManagement";
    }

    private decimal CalculateOptimalUtilization(SlotReservationsDto slotData, List<ReservationRecommendation> recommendations)
    {
        if (!slotData.MaxCapacity.HasValue)
            return 1.0m;

        var recommendedConfirms = recommendations.Count(r => r.RecommendedAction == "Confirm");
        return Math.Min(1.0m, (decimal)recommendedConfirms / slotData.MaxCapacity.Value);
    }

    private List<string> GenerateInsights(SlotReservationsDto slotData, string aiResponse, List<ReservationRecommendation> recommendations)
    {
        var insights = new List<string>();

        // Análisis de demanda
        if (slotData.IsOverbooked)
            insights.Add($"High demand: {slotData.TotalReservations} reservations for {slotData.MaxCapacity} capacity");

        // Análisis de confirmaciones recomendadas
        var confirmRecommendations = recommendations.Count(r => r.RecommendedAction == "Confirm");
        insights.Add($"Recommend confirming {confirmRecommendations} out of {slotData.PendingReservations} pending reservations");

        // Análisis temporal
        var avgDaysInAdvance = slotData.Reservations.Average(r => (slotData.SlotDateTime - r.CreatedAt).TotalDays);
        insights.Add($"Average registration lead time: {avgDaysInAdvance:F1} days");

        // Insight de IA
        if (!string.IsNullOrWhiteSpace(aiResponse))
            insights.Add($"AI Analysis: {aiResponse.Substring(0, Math.Min(150, aiResponse.Length))}");

        return insights;
    }

    private List<string> GenerateAnalysisRecommendations(SlotReservationsDto slotData, string aiResponse)
    {
        var recommendations = new List<string>();

        if (slotData.IsOverbooked)
        {
            recommendations.Add("Implement selective confirmation strategy to optimize capacity");
            recommendations.Add("Consider setting up waiting list for declined reservations");
        }

        if (slotData.PendingReservations > 0)
        {
            recommendations.Add($"Review {slotData.PendingReservations} pending reservations for optimal selection");
        }

        // Agregar insight de IA si está disponible
        if (!string.IsNullOrWhiteSpace(aiResponse))
        {
            recommendations.Add($"AI recommends: {aiResponse.Substring(0, Math.Min(100, aiResponse.Length))}");
        }

        return recommendations;
    }

    private decimal CalculateConfidenceScore(SlotReservationsDto slotData, List<UserReservationAnalysis> userAnalyses)
    {
        decimal confidence = 0.7m; // Base confidence

        // Más datos = más confianza
        if (slotData.TotalReservations >= 10) confidence += 0.1m;
        if (slotData.TotalReservations >= 20) confidence += 0.1m;

        // Reservas con notas = más información para análisis
        var reservationsWithNotes = slotData.Reservations.Count(r => !string.IsNullOrWhiteSpace(r.UserNotes));
        if (reservationsWithNotes > slotData.TotalReservations * 0.3m) confidence += 0.1m;

        return Math.Max(0.5m, Math.Min(0.95m, confidence));
    }

    private decimal CalculateRecommendationConfidence(int priorityScore, decimal attendanceProbability)
    {
        // Combinar priority score y attendance probability para confidence
        var normalizedPriority = priorityScore / 100m;
        var combinedScore = (normalizedPriority + attendanceProbability) / 2m;

        return Math.Max(0.3m, Math.Min(0.95m, combinedScore));
    }

    private string EnrichSummaryWithData(SlotReservationsDto slotData, string aiSummary)
    {
        var summary = $"📊 **{slotData.ProductName} at {slotData.GymName}**\n";
        summary += $"🗓️ {slotData.SlotDateTime:yyyy-MM-dd HH:mm} | {slotData.TimeSlotString}\n\n";

        summary += $"📈 **Current Status:**\n";
        summary += $"• Total Reservations: {slotData.TotalReservations}\n";
        summary += $"• Confirmed: {slotData.ConfirmedReservations}\n";
        summary += $"• Pending: {slotData.PendingReservations}\n";
        summary += $"• Capacity: {slotData.MaxCapacity?.ToString() ?? "Unlimited"}\n";
        summary += $"• Overbooked: {(slotData.IsOverbooked ? "Yes" : "No")}\n\n";

        summary += $"🤖 **AI Analysis:**\n{aiSummary}";

        return summary;
    }
}