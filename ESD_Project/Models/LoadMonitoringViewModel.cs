﻿namespace ESD_Project.Models
{
    public class LoadMonitoringViewModel
    {
        public List<CarriageLoadStat> CarriageStats { get; set; } = new();
        public List<TimeSeriesPoint> LoadSeries { get; set; } = new();
    }
}
