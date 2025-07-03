// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// This application uses the Azure IoT Hub device SDK for .NET
// For samples see: https://github.com/Azure/azure-iot-sdk-csharp/tree/master/iothub/device/samples

using Microsoft.Azure.Devices.Client;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SimulatedDevice
{
    /// <summary>
    /// This sample illustrates the very basics of a device app sending telemetry. For a more comprehensive device app sample, please see
    /// <see href="https://github.com/Azure-Samples/azure-iot-samples-csharp/tree/master/iot-hub/Samples/device/DeviceReconnectionSample"/>.
    /// </summary>
    internal class Program
    {
        private static DeviceClient s_deviceClient;
        private static readonly TransportType s_transportType = TransportType.Mqtt;

        // The device connection string to authenticate the device with your IoT hub.
        // Using the Azure CLI:
        // az iot hub device-identity show-connection-string --hub-name {YourIoTHubName} --device-id MyDotnetDevice --output table
        //private static string s_connectionString = "{Your device connection string here}";
        private static string s_connectionString =
  "HostName=IT3681-00-IoTHub.azure-devices.net;" +
  "DeviceId=IT3681-GP02-DEV02;" +
  "SharedAccessKey=gUAhfJj5ORgRJqEz9jC3j3H4dqlX+0LEGCmSQflBpVc=";


        private static async Task Main(string[] args)
        {
            Console.WriteLine("IoT Hub Quickstarts #1 - Simulated device.");
            Console.WriteLine($"[DEBUG] s_connectionString = '{s_connectionString}'");


            // This sample accepts the device connection string as a parameter, if present
            ValidateConnectionString(args);

            // Connect to the IoT hub using the MQTT protocol
            s_deviceClient = DeviceClient.CreateFromConnectionString(s_connectionString, s_transportType);

            // Set up a condition to quit the sample
            Console.WriteLine("Press control-C to exit.");
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Exiting...");
            };

            // Run the telemetry loop
            await SendDeviceToCloudMessagesAsync(cts.Token);

            // SendDeviceToCloudMessagesAsync is designed to run until cancellation has been explicitly requested by Console.CancelKeyPress.
            // As a result, by the time the control reaches the call to close the device client, the cancellation token source would
            // have already had cancellation requested.
            // Hence, if you want to pass a cancellation token to any subsequent calls, a new token needs to be generated.
            // For device client APIs, you can also call them without a cancellation token, which will set a default
            // cancellation timeout of 4 minutes: https://github.com/Azure/azure-iot-sdk-csharp/blob/64f6e9f24371bc40ab3ec7a8b8accbfb537f0fe1/iothub/device/src/InternalClient.cs#L1922
            await s_deviceClient.CloseAsync();

            s_deviceClient.Dispose();
            Console.WriteLine("Device simulator finished.");
        }

        private static void ValidateConnectionString(string[] args)
        {
            if (args.Any())
            {
                try
                {
                    var cs = IotHubConnectionStringBuilder.Create(args[0]);
                    s_connectionString = cs.ToString();
                }
                catch (Exception)
                {
                    Console.WriteLine($"Error: Unrecognizable parameter '{args[0]}' as connection string.");
                    Environment.Exit(1);
                }
            }
            else
            {
                try
                {
                    _ = IotHubConnectionStringBuilder.Create(s_connectionString);
                }
                catch (Exception)
                {
                    Console.WriteLine("This sample needs a device connection string to run. Program.cs can be edited to specify it, or it can be included on the command-line as the only parameter.");
                    Environment.Exit(1);
                }
            }
        }

        // Async method to send simulated telemetry
        private static async Task SendDeviceToCloudMessagesAsync(CancellationToken ct)
        {
            // Initial telemetry values
            double minTemperature = 20;
            double minHumidity = 60;
            var rand = new Random();
            var trainIds = new[] { "Train1", "Train2", "Train3", "Train4", "Train5" };

            // Limit messages to avoid throttling
            int currCount = 0, maxCount = 10;

            // List of bay IDs to simulate
            var bayIds = new[] { "Bay-01", "Bay-02", "Bay-03" };

            while (!ct.IsCancellationRequested && currCount < maxCount)
            {
                currCount++;

                // Other telemetry
                double currentTemperature = minTemperature + rand.NextDouble() * 15;
                double currentHumidity = minHumidity + rand.NextDouble() * 20;
                double powerUsage = 1500 + rand.NextDouble() * 300;
               
                foreach (var trainId in trainIds)
                {
                    double loadWeight = 2500 + rand.NextDouble() * 500;

                    var trainTelemetry = new
                    {
                        // GPS & Live Location
                        latitude = 1.3521 + rand.NextDouble() * 0.01,
                        longitude = 103.8198 + rand.NextDouble() * 0.01,

                        // Environment
                        temperature = currentTemperature,
                        humidity = currentHumidity,

                        // Load Cell (Train Weight Sensor)
                        trainId,
                        loadWeight
                    };

                    string trainMsgBody = JsonSerializer.Serialize(trainTelemetry);
                    using var trainMsg = new Message(Encoding.UTF8.GetBytes(trainMsgBody))
                    {
                        ContentType = "application/json",
                        ContentEncoding = "utf-8",
                    };

                    await s_deviceClient.SendEventAsync(trainMsg);
                    Console.WriteLine($"{DateTime.Now:O} > Sending load for {trainId}: {loadWeight:F0}kg");
                }


                foreach (var bayId in bayIds)
                {
                    // Simulate bay power: idle 200–600W, 10% chance 800–1200W spike
                    double bayPower = rand.NextDouble() < 0.1
                        ? 800 + rand.NextDouble() * 400
                        : 200 + rand.NextDouble() * 400;

                    var telemetry = new
                    {
                        // GPS & Live Location
                        latitude = 1.3521 + rand.NextDouble() * 0.01,
                        longitude = 103.8198 + rand.NextDouble() * 0.01,

                        // Power Sensor (Energy Usage)
                        powerUsage,

                        // Load Cell (Train Weight Sensor)
                        

                        

                        // Depot Energy Usage Monitoring (Per Train Slot)
                        bayId,
                        bayPowerDraw = bayPower,

                        // Optional: carry over temperature/humidity
                        temperature = currentTemperature,
                        humidity = currentHumidity
                    };

                    string messageBody = JsonSerializer.Serialize(telemetry);
                    using var message = new Message(Encoding.UTF8.GetBytes(messageBody))
                    {
                        ContentType = "application/json",
                        ContentEncoding = "utf-8",
                    };

                    // Send the telemetry message
                    await s_deviceClient.SendEventAsync(message);
                    Console.WriteLine($"{DateTime.Now:O} > Sending for {bayId}: {messageBody}");
                }

                // 1 second between rounds
                await Task.Delay(1000, ct);
            }
        }

    }
}


