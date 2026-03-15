namespace ParkingLot.Models
{
    public class ParkingRecord
    {
        public int Id { get; set; }
        public string TagNumber { get; set; } = string.Empty;
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public decimal? AmountCharged { get; set; }
        public bool IsActive { get; set; }
    }

    public class ParkingSettings
    {
        public int TotalSpots { get; set; }
        public decimal HourlyFee { get; set; }
    }

    public class CheckInResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public ParkingSnapshotViewModel? Snapshot { get; set; }
    }

    public class CheckOutResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public decimal? AmountCharged { get; set; }
        public string? TagNumber { get; set; }
        public ParkingSnapshotViewModel? Snapshot { get; set; }
    }

    public class ParkingSnapshotViewModel
    {
        public int TotalSpots { get; set; }
        public int AvailableSpots { get; set; }
        public int SpotsTaken { get; set; }
        public decimal HourlyFee { get; set; }
        public List<ParkedCarViewModel> ParkedCars { get; set; } = new();
    }

    public class ParkedCarViewModel
    {
        public string TagNumber { get; set; } = string.Empty;
        public string CheckInTime { get; set; } = string.Empty;
        public string ElapsedTime { get; set; } = string.Empty;
    }

    public class StatsViewModel
    {
        public int AvailableSpots { get; set; }
        public decimal TodaysRevenue { get; set; }
        public double AverageCarsPerDay { get; set; }
        public decimal AverageRevenuePerDay { get; set; }
    }
}
