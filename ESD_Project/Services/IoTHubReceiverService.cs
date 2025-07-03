using System;
using System.Text.Json;
using Azure.Messaging.EventHubs.Consumer;
using ESD_Project.Data;
using ESD_Project.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ESD_Project.Services
{
    public class IoTHubReceiverService : BackgroundService
    {
        private readonly IConfiguration _cfg;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<IoTHubReceiverService> _log;

        public IoTHubReceiverService(
            IConfiguration cfg,
            IServiceScopeFactory scope,
            ILogger<IoTHubReceiverService> log)
        {
            _cfg = cfg;
            _scopeFactory = scope;
            _log = log;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connectionString = _cfg["IoT:EventHubConnectionString"];

            await using var consumer = new EventHubConsumerClient(
                EventHubConsumerClient.DefaultConsumerGroupName,
                connectionString);

            await foreach (var partitionEvent in consumer.ReadEventsAsync(stoppingToken))
            {
                var json = partitionEvent.Data.EventBody.ToString();
                JsonElement payload;

                try
                {
                    payload = JsonSerializer.Deserialize<JsonElement>(json);
                }
                catch (JsonException ex)
                {
                    _log.LogWarning("Skipping invalid JSON payload: {json} ({msg})", json, ex.Message);
                    continue;
                }

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var ts = DateTime.UtcNow;

                // GPS
                if (payload.TryGetProperty("latitude", out var lat) &&
                    payload.TryGetProperty("longitude", out var lon))
                {
                    db.TrainLocations.Add(new TrainLocation
                    {
                        Timestamp = ts,
                        Latitude = lat.GetDouble(),
                        Longitude = lon.GetDouble()
                    });
                }

                // Power Usage
                if (payload.TryGetProperty("powerUsage", out var pw))
                {
                    db.PowerUsages.Add(new PowerUsage
                    {
                        Timestamp = ts,
                        Source = "Train",
                        Watts = pw.GetDouble()
                    });
                }

                // Load Weight (with trainId)
                if (payload.TryGetProperty("trainId", out var tId) &&
                    payload.TryGetProperty("loadWeight", out var lw))
                {
                    db.LoadWeights.Add(new LoadWeight
                    {
                        Timestamp = ts,
                        TrainId = tId.GetString()!,
                        Kilograms = lw.GetDouble()
                    });
                }

                // Depot Slot Energy (with bayId)
                if (payload.TryGetProperty("bayId", out var b) &&
                    payload.TryGetProperty("bayPowerDraw", out var bp))
                {
                    db.DepotEnergySlots.Add(new DepotEnergySlot
                    {
                        Timestamp = ts,
                        BayId = b.GetString()!,
                        Watts = bp.GetDouble()
                    });
                }

                await db.SaveChangesAsync(stoppingToken);
            }
        }
    }
}
