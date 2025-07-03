namespace ESD_Project.Services
{
    using ESD_Project.Data;
    using ESD_Project.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using System.Net.Mail;

    public class AlertProcessingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEmailSender _email;
        private readonly ILogger<AlertProcessingService> _log;
        private const string AlertEmail = "darelleong29@gmail.com";

        public AlertProcessingService(
            IServiceScopeFactory scope,
            IEmailSender email,
            ILogger<AlertProcessingService> log)
        {
            _scopeFactory = scope;
            _email = email;
            _log = log;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _log.LogInformation("AlertProcessingService started.");

            while (!ct.IsCancellationRequested)
            {
                _log.LogInformation("Running alert check at {time}", DateTime.UtcNow);

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var cutoff = DateTime.UtcNow - TimeSpan.FromHours(1);

                // 1) Fetch per-bay averages & latest
                var bayStats = await db.DepotEnergySlots
                    .Where(x => x.Timestamp >= cutoff)
                    .GroupBy(x => x.BayId)
                    .Select(g => new {
                        BayId = g.Key,
                        Avg = g.Average(x => x.Watts),
                        Latest = g.OrderByDescending(x => x.Timestamp).First().Watts
                    })
                    .ToListAsync(ct);

                // 2) Fetch per-train averages & latest
                var loadStats = await db.LoadWeights
                    .Where(x => x.Timestamp >= cutoff)
                    .GroupBy(x => x.TrainId)
                    .Select(g => new {
                        TrainId = g.Key,
                        AvgLoad = g.Average(x => x.Kilograms),
                        Latest = g.OrderByDescending(x => x.Timestamp).First().Kilograms
                    })
                    .ToListAsync(ct);

                // 3) Load all alert rules
                var rules = await db.AlertDefinitions.ToListAsync(ct);

                foreach (var rule in rules)
                {
                    _log.LogInformation("Evaluating rule \"{ruleName}\" (Type={ruleType}, Threshold={th}%)",
                        rule.Name, rule.Type, rule.Threshold);

                    if (rule.Type == AlertType.BayPower)
                    {
                        // threshold% above each bay’s average
                        foreach (var b in bayStats)
                        {
                            // compute dynamic cutoff
                            var cutoffValue = b.Avg * (1 + rule.Threshold / 100.0);
                            _log.LogDebug("  Bay {bay}: Avg={avg:F0}, Latest={latest:F0}, Cutoff={cutoff:F0}",
                                b.BayId, b.Avg, b.Latest, cutoffValue);

                            if (b.Latest >= cutoffValue)
                            {
                                _log.LogWarning("    → Bay {bay} exceeded by {diff:F0}W",
                                    b.BayId, b.Latest - b.Avg);
                                await FireAlertAsync(db, rule, b.BayId, b.Latest - b.Avg);
                            }
                        }
                    }
                    else  // CapacityUtilization
                    {
                        // threshold% of capacity (assumed 3000kg)
                        const double capacity = 3000.0;
                        foreach (var t in loadStats)
                        {
                            var pct = t.Latest / capacity * 100;
                            _log.LogDebug("  Train {train}: {pct:F1}% of capacity", t.TrainId, pct);

                            if (pct >= rule.Threshold)
                            {
                                _log.LogWarning("    → Train {train} at {pct:F1}% capacity",
                                    t.TrainId, pct);
                                await FireAlertAsync(db, rule, t.TrainId, pct - rule.Threshold);
                            }
                        }
                    }
                }

                await db.SaveChangesAsync(ct);
                _log.LogInformation("Alert check complete; next run in 1min.");
                await Task.Delay(TimeSpan.FromMinutes(1), ct);
            }

            _log.LogInformation("AlertProcessingService is stopping.");
        }

        private async Task FireAlertAsync(
     AppDbContext db,
     AlertDefinition rule,
     string device,
     double overValue)
        {
            var now = DateTime.UtcNow;

            // 1) Rate‐limit skip
            if (rule.LastFiredAt.HasValue
             && (now - rule.LastFiredAt.Value) < TimeSpan.FromMinutes(30))
                return;

            // 2) Normalize Role (in case it's a leftover numeric string)
            string roleName = rule.Role;
            //if (int.TryParse(roleName, out var idx))
            //{
            //    string[] roles = rule.Type == AlertType.BayPower
            //        ? new[] { "Engineer1", "Engineer2", "Engineer3" }
            //        : new[] { "Staff1", "Staff2", "Staff3" };
            //    if (idx >= 0 && idx < roles.Length)
            //        roleName = roles[idx];
            //}

            // 3) Format subject & body
            var subject = $"[{roleName}] Alert: {rule.Name} @ {DateTime.UtcNow:HH:mm:ss}";
            var body = string.Format(
                rule.MessageTemplate ?? "{0} breached threshold with {1:F0}%",
                device, overValue);
            _log.LogInformation("About to email rule.Role = '{role}'", rule.Role);
            _log.LogInformation("Email subject will be: {subject}", subject);
            // 4) Send email
            try
            {
                await _email.SendEmailAsync(AlertEmail, subject, body);
            }
            catch (SmtpException) { /* log and swallow */ }

            // 5) Log history & update LastFiredAt
            db.AlertHistories.Add(new AlertHistory
            {
                DefinitionId = rule.Id,
                FiredAt = now,
                RecipientEmail = AlertEmail,
                ObservedValue = overValue,
                MessageSent = body
            });
            rule.LastFiredAt = now;
            db.AlertDefinitions.Update(rule);

            await db.SaveChangesAsync();
        }

    }
}
