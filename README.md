# Train IoT Monitoring

A sample ASP.NET Core 9.0 web app that demonstrates end-to-end IoT data ingestion, real-time dashboards, and alerting. It uses Azure Event Hubs to receive simulated device telemetry, EF Core for storage, and TailwindCSS for a clean, responsive UI.

---

## 🚀 Features

- **Background Service**  
  - `IoTHubReceiverService` reads events from an Azure Event Hub consumer group  
  - Persists GPS, power, load, and depot‐bay power readings into SQL via EF Core  

- **Live Dashboards**  
  - **Depot Energy Monitoring**: select a bay or “All” to view peak/average power draw  
  - **Train Load Monitoring**: show Avg/Max load, capacity utilization bars, and load‐over‐time chart  
  - Built with Razor pages, TailwindCSS, and Chart.js for dynamic visualization  

- **Alerts & Rules**  
  - CRUD UI for “Bay Power” and “Capacity Utilization” alert definitions  
  - Role-based dropdowns (Engineer1–3 for bay, Staff1–3 for train)  
  - Searches/filter by name or target  
  - Background `AlertProcessingService` that checks rules every minute and emails via SMTP, rate-limited to 30 min  

- **Alert History**  
  - Read-only log of all fired alerts with timestamp, recipient, role, and message  

---

## 📦 Tech Stack

- **Framework**: .NET 9.0 (ASP.NET Core MVC)  
- **Data**: Entity Framework Core (SQL Server / InMemory)  
- **IoT**: Azure.Messaging.EventHubs  
- **Background**: `IHostedService` / `BackgroundService`  
- **UI**: Razor + TailwindCSS  
- **Charts**: Chart.js via Blazor interop  
- **Email**: System.Net.Mail SMTP client  

---

## 🔧 Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)  
- SQL Server (or use the built-in InMemory provider for testing)  
- Azure Event Hub (or point to a simulated device)  
- SMTP credentials (Gmail, SendGrid, etc.)

---

## ⚙️ Setup

1. **Clone & restore**  
   ```bash
   git clone https://github.com/your-username/Train-IoT-Monitoring.git
   cd Train-IoT-Monitoring
   dotnet restore
