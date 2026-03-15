using Dapper;
using ParkingLot.Data;
using ParkingLot.Models;

namespace ParkingLot.Services
{
    public class ParkingService
    {
        private readonly DbConnectionFactory _dbFactory;
        private readonly ParkingSettings _settings;

        public ParkingService(DbConnectionFactory dbFactory, IConfiguration configuration)
        {
            _dbFactory = dbFactory;
            _settings = configuration.GetSection("ParkingLot").Get<ParkingSettings>()
                ?? throw new InvalidOperationException("ParkingLot settings not found.");
        }

        public async Task<CheckInResult> CheckInAsync(string tagNumber)
        {
            tagNumber = tagNumber.Trim().ToUpper();

            using var conn = _dbFactory.CreateConnection();

            
            var existing = await conn.QueryFirstOrDefaultAsync<int>(
                "SELECT COUNT(1) FROM ParkingRecords WHERE TagNumber = @Tag AND IsActive = 1",
                new { Tag = tagNumber });

            if (existing > 0)
                return new CheckInResult { Success = false, ErrorMessage = $"Car '{tagNumber}' is already in the parking lot." };

            
            var occupied = await conn.QueryFirstOrDefaultAsync<int>(
                "SELECT COUNT(1) FROM ParkingRecords WHERE IsActive = 1");

            if (occupied >= _settings.TotalSpots)
                return new CheckInResult { Success = false, ErrorMessage = "No spots available. The parking lot is full." };

            
            await conn.ExecuteAsync(
                "INSERT INTO ParkingRecords (TagNumber, CheckInTime, IsActive) VALUES (@Tag, @Now, 1)",
                new { Tag = tagNumber, Now = DateTime.Now });

            return new CheckInResult
            {
                Success = true,
                Snapshot = await GetSnapshotAsync()
            };
        }

        public async Task<CheckOutResult> CheckOutAsync(string tagNumber)
        {
            tagNumber = tagNumber.Trim().ToUpper();

            using var conn = _dbFactory.CreateConnection();

            var record = await conn.QueryFirstOrDefaultAsync<ParkingRecord>(
                "SELECT * FROM ParkingRecords WHERE TagNumber = @Tag AND IsActive = 1",
                new { Tag = tagNumber });

            if (record == null)
                return new CheckOutResult { Success = false, ErrorMessage = $"Car '{tagNumber}' is not registered in the parking lot." };

            var checkOut = DateTime.Now;
            var hoursParked = (checkOut - record.CheckInTime).TotalMinutes;
            
            var billableHours = (int)Math.Ceiling(hoursParked / 60.0);
            if (billableHours < 1) billableHours = 1;
            var amount = billableHours * _settings.HourlyFee;

            await conn.ExecuteAsync(
                "UPDATE ParkingRecords SET CheckOutTime = @Out, AmountCharged = @Amount, IsActive = 0 WHERE Id = @Id",
                new { Out = checkOut, Amount = amount, record.Id });

            return new CheckOutResult
            {
                Success = true,
                AmountCharged = amount,
                TagNumber = tagNumber,
                Snapshot = await GetSnapshotAsync()
            };
        }

        public async Task<ParkingSnapshotViewModel> GetSnapshotAsync()
        {
            using var conn = _dbFactory.CreateConnection();

            var parkedCars = (await conn.QueryAsync<ParkingRecord>(
                "SELECT * FROM ParkingRecords WHERE IsActive = 1 ORDER BY CheckInTime")).ToList();

            var now = DateTime.Now;

            var carViews = parkedCars.Select(c =>
            {
                var elapsed = now - c.CheckInTime;
                string elapsedStr;
                if (elapsed.TotalMinutes < 60)
                    elapsedStr = $"{(int)elapsed.TotalMinutes} min";
                else
                    elapsedStr = elapsed.TotalHours < 2
                        ? $"{(int)elapsed.TotalHours} hour"
                        : $"{(int)elapsed.TotalHours} hours";

                return new ParkedCarViewModel
                {
                    TagNumber = c.TagNumber,
                    CheckInTime = c.CheckInTime.ToString("hh:mmtt"),
                    ElapsedTime = elapsedStr
                };
            }).ToList();

            int spotsTaken = parkedCars.Count;

            return new ParkingSnapshotViewModel
            {
                TotalSpots = _settings.TotalSpots,
                AvailableSpots = _settings.TotalSpots - spotsTaken,
                SpotsTaken = spotsTaken,
                HourlyFee = _settings.HourlyFee,
                ParkedCars = carViews
            };
        }

        public async Task<StatsViewModel> GetStatsAsync()
        {
            using var conn = _dbFactory.CreateConnection();

            var occupied = await conn.QueryFirstOrDefaultAsync<int>(
                "SELECT COUNT(1) FROM ParkingRecords WHERE IsActive = 1");

            var todayStart = DateTime.Today;
            var todaysRevenue = await conn.QueryFirstOrDefaultAsync<decimal>(
                "SELECT ISNULL(SUM(AmountCharged), 0) FROM ParkingRecords WHERE CheckOutTime >= @Today",
                new { Today = todayStart });

            
            var thirtyDaysAgo = DateTime.Today.AddDays(-30);
            var avgCarsResult = await conn.QueryFirstOrDefaultAsync<double>(
                @"SELECT ISNULL(AVG(CAST(DailyCars AS FLOAT)), 0)
                  FROM (
                      SELECT CAST(CheckInTime AS DATE) AS Day, COUNT(*) AS DailyCars
                      FROM ParkingRecords
                      WHERE CheckInTime >= @Start
                      GROUP BY CAST(CheckInTime AS DATE)
                  ) AS DailyStats",
                new { Start = thirtyDaysAgo });

            var avgRevenueResult = await conn.QueryFirstOrDefaultAsync<decimal>(
                @"SELECT ISNULL(AVG(DailyRevenue), 0)
                  FROM (
                      SELECT CAST(CheckOutTime AS DATE) AS Day, SUM(AmountCharged) AS DailyRevenue
                      FROM ParkingRecords
                      WHERE CheckOutTime >= @Start AND AmountCharged IS NOT NULL
                      GROUP BY CAST(CheckOutTime AS DATE)
                  ) AS DailyStats",
                new { Start = thirtyDaysAgo });

            return new StatsViewModel
            {
                AvailableSpots = _settings.TotalSpots - occupied,
                TodaysRevenue = todaysRevenue,
                AverageCarsPerDay = Math.Round(avgCarsResult, 1),
                AverageRevenuePerDay = avgRevenueResult
            };
        }
    }
}
