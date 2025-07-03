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
2. **Configure**
Copy appsettings.Development.json.template → appsettings.Development.json and fill in:
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=TrainIoT;Trusted_Connection=True;"
  },
  "IoT": {
    "EventHubConnectionString": "<YOUR_EVENT_HUB_CONN>"
  },
  "AlertEmail": {
    "From": "you@domain.com",
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "Username": "you@domain.com",
    "Password": "<your-smtp-pass>",
    "UseSsl": "true"
  }
}
3. Apply database migrations
  bash
  Copy
  Edit
  dotnet ef database update
4. Run the application
    bash
    Copy
    Edit
    dotnet run
    Navigate to https://localhost:5001.
   
## 🖼 Screenshots
<details>
  <summary>Live Dashboards</summary>

  ![Depot Energy Monitoring](docs/img/depot-energy-monitoring.png)
  ![Load Monitoring](docs/img/load-monitoring.png)

</details>

<details>
  <summary>Alert Management</summary>

  ![Manage Alerts – Bay Rules](docs/img/manage-alerts-bay.png)
  ![Manage Alerts – Capacity Rules](docs/img/manage-alerts-capacity.png)
  ![Alert History](docs/img/Alert-History.png)
  

</details>
