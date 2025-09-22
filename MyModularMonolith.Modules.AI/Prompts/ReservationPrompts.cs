using MyModularMonolith.Modules.Gyms.Contracts;
using System.Text;

namespace MyModularMonolith.Modules.AI.Prompts;

public static class ReservationPrompts
{
    public static string CreateAnalysisPrompt(SlotReservationsDto slotData)
    {
        var sb = new StringBuilder();
        sb.AppendLine("🎯 RESERVATION ANALYSIS REQUEST");
        sb.AppendLine("===============================");
        sb.AppendLine($"Class: {slotData.ProductName} at {slotData.GymName}");
        sb.AppendLine($"Date/Time: {slotData.SlotDateTime:yyyy-MM-dd HH:mm} ({slotData.TimeSlotString})");
        sb.AppendLine($"Capacity: {slotData.MaxCapacity?.ToString() ?? "Unlimited"}");
        sb.AppendLine($"Current Status: {slotData.ConfirmedReservations} confirmed, {slotData.PendingReservations} pending");
        sb.AppendLine($"Overbooked: {slotData.IsOverbooked}");
        sb.AppendLine();

        sb.AppendLine("📋 RESERVATIONS DATA:");
        foreach (var reservation in slotData.Reservations.Take(15))
        {
            var daysInAdvance = (slotData.SlotDateTime - reservation.CreatedAt).TotalDays;
            sb.AppendLine($"• User {reservation.UserId.Substring(0, 8)}: {reservation.Status}, {daysInAdvance:F1} days advance" +
                         (string.IsNullOrEmpty(reservation.UserNotes) ? "" : $", Notes: \"{reservation.UserNotes.Substring(0, Math.Min(50, reservation.UserNotes.Length))}\""));
        }

        if (slotData.Reservations.Count > 15)
            sb.AppendLine($"... and {slotData.Reservations.Count - 15} more reservations");

        sb.AppendLine();
        sb.AppendLine("🔍 ANALYSIS REQUESTED:");
        sb.AppendLine("1. Assess overall demand pattern and user engagement level");
        sb.AppendLine("2. Identify optimal capacity utilization strategy");
        sb.AppendLine("3. Evaluate risk factors for no-shows or cancellations");
        sb.AppendLine("4. Recommend management approach for this specific class");
        sb.AppendLine("5. Suggest improvements for future similar classes");
        sb.AppendLine();
        sb.AppendLine("Please provide insights in a clear, actionable format.");

        return sb.ToString();
    }

    public static string CreateRecommendationPrompt(SlotReservationsDto slotData)
    {
        var sb = new StringBuilder();
        sb.AppendLine("🎯 RESERVATION RECOMMENDATIONS REQUEST");
        sb.AppendLine("====================================");
        sb.AppendLine($"Class: {slotData.ProductName} ({slotData.GymName})");
        sb.AppendLine($"Scheduled: {slotData.SlotDateTime:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"Capacity Constraint: {slotData.MaxCapacity?.ToString() ?? "No limit"}");
        sb.AppendLine($"Current Confirmed: {slotData.ConfirmedReservations}");
        sb.AppendLine($"Pending Decisions: {slotData.PendingReservations}");
        sb.AppendLine();

        if (slotData.PendingReservations > 0)
        {
            sb.AppendLine("📋 PENDING RESERVATIONS TO EVALUATE:");
            var pendingReservations = slotData.Reservations.Where(r => r.Status == "Pending").Take(10);
            foreach (var reservation in pendingReservations)
            {
                var daysInAdvance = (slotData.SlotDateTime - reservation.CreatedAt).TotalDays;
                sb.AppendLine($"• User {reservation.UserId.Substring(0, 8)}: {daysInAdvance:F1} days advance" +
                             (string.IsNullOrEmpty(reservation.UserNotes) ? "" : $", \"{reservation.UserNotes.Substring(0, Math.Min(40, reservation.UserNotes.Length))}\""));
            }
        }

        sb.AppendLine();
        sb.AppendLine("🎯 RECOMMENDATION CRITERIA:");
        sb.AppendLine("• CONFIRM: High attendance probability, fits capacity, good engagement");
        sb.AppendLine("• WAITING_LIST: Medium priority, over capacity but backup option");
        sb.AppendLine("• REJECT: Low engagement, very late registration, or clear capacity constraints");
        sb.AppendLine();
        sb.AppendLine("Consider factors:");
        sb.AppendLine("- Registration timing (early birds vs last-minute)");
        sb.AppendLine("- User engagement level (notes, enthusiasm)");
        sb.AppendLine("- Capacity optimization (prevent empty spots)");
        sb.AppendLine("- Business rules (fairness, customer satisfaction)");
        sb.AppendLine();
        sb.AppendLine("Provide strategic guidance for optimal class management.");

        return sb.ToString();
    }

    public static string CreateOptimizationPrompt(SlotReservationsDto slotData)
    {
        var sb = new StringBuilder();
        sb.AppendLine("🎯 SLOT OPTIMIZATION STRATEGY REQUEST");
        sb.AppendLine("===================================");
        sb.AppendLine($"Class: {slotData.ProductName}");
        sb.AppendLine($"Venue: {slotData.GymName}");
        sb.AppendLine($"Target Date: {slotData.SlotDateTime:yyyy-MM-dd HH:mm}");
        sb.AppendLine();

        var utilizationRate = slotData.MaxCapacity.HasValue ?
            (decimal)slotData.TotalReservations / slotData.MaxCapacity.Value : 1.0m;

        sb.AppendLine("📊 CURRENT METRICS:");
        sb.AppendLine($"• Demand Level: {slotData.TotalReservations} registrations");
        sb.AppendLine($"• Capacity Utilization: {utilizationRate:P1}");
        sb.AppendLine($"• Booking Status: {(slotData.IsOverbooked ? "OVERBOOKED" : "Within Capacity")}");
        sb.AppendLine($"• Decision Required: {slotData.PendingReservations} pending approvals");
        sb.AppendLine();

        sb.AppendLine("🎯 OPTIMIZATION GOALS:");
        sb.AppendLine("1. REVENUE MAXIMIZATION: Optimize class profitability");
        sb.AppendLine("2. CUSTOMER SATISFACTION: Balance fairness with business needs");
        sb.AppendLine("3. OPERATIONAL EFFICIENCY: Minimize no-shows and cancellations");
        sb.AppendLine("4. CAPACITY UTILIZATION: Achieve optimal attendance without overcrowding");
        sb.AppendLine();

        sb.AppendLine("🔍 STRATEGIC ANALYSIS REQUESTED:");
        sb.AppendLine("• Recommend optimal confirmation strategy");
        sb.AppendLine("• Identify risk mitigation approaches");
        sb.AppendLine("• Suggest capacity management improvements");
        sb.AppendLine("• Provide actionable next steps");
        sb.AppendLine();
        sb.AppendLine("Focus on practical, implementable strategies for immediate application.");

        return sb.ToString();
    }

    public static string CreateSummaryPrompt(SlotReservationsDto slotData)
    {
        var utilizationRate = slotData.MaxCapacity.HasValue ?
            (decimal)slotData.TotalReservations / slotData.MaxCapacity.Value * 100 : 100;

        return $"📊 EXECUTIVE SUMMARY REQUEST for {slotData.ProductName} at {slotData.GymName}\n" +
               $"Class Date: {slotData.SlotDateTime:yyyy-MM-dd HH:mm}\n" +
               $"Current Status: {slotData.ConfirmedReservations} confirmed + {slotData.PendingReservations} pending = {slotData.TotalReservations} total\n" +
               $"Capacity: {slotData.MaxCapacity?.ToString() ?? "Unlimited"} (Utilization: {utilizationRate:F1}%)\n" +
               $"Management Challenge: {(slotData.IsOverbooked ? "Overbooked - selective approval needed" : "Standard capacity management")}\n\n" +
               $"Please provide a concise executive summary covering:\n" +
               $"• Key insights about reservation patterns\n" +
               $"• Recommended management approach\n" +
               $"• Critical success factors\n" +
               $"• Next actions required\n\n" +
               $"Keep it brief but actionable for gym management decision-making.";
    }
}