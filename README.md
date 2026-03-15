# Parking Lot Manager

A full-stack ASP.NET Core MVC web application for managing a parking lot.
## Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 9 MVC (Server-Side Rendered) |
| Database | SQL Server + Dapper (no Entity Framework) |
| Frontend | Plain JavaScript (ES6+), no frameworks |
| CSS | Bootstrap 5.3 |



## Features

- Check In a car by tag number with validation (spots available, car not already parked)
- Check Out a car with automatic fee calculation (ceiling by hour)
- Live snapshot (Area B) updates via AJAX — no full page reload
- Stats modal showing available spots, today's revenue, and 30-day averages
- Configurable total spots and hourly fee via `appsettings.json`



## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server (local or remote — SQL Server Express works fine)
- A terminal / command prompt



## Setup & Run

### 1. Clone the repository

```bash
git clone https://github.com/YOUR_USERNAME/ParkingLotManager.git
cd ParkingLotManager
```

### 2. Set up the database

Open SQL Server Management Studio (or Azure Data Studio) and run the script:

```
database.sql
```

This creates the `ParkingLotDb` database and the `ParkingRecords` table.

### 3. Update the connection string

Open `appsettings.json` and update the `DefaultConnection` string to match your SQL Server instance:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=ParkingLotDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "ParkingLot": {
    "TotalSpots": 15,
    "HourlyFee": 15.00
  }
}
```



### 4. Run the application

```bash
dotnet run
```

Open your browser at `https://localhost:5001` (or the URL shown in the terminal).



## Configuration

All parking lot settings live in `appsettings.json`:

```json
"ParkingLot": {
    "TotalSpots": 15,
    "HourlyFee": 15.00
}
```

Change these values and restart the app — no code changes needed.


## Fee Calculation Logic

Fees are billed in **full-hour increments** (ceiling):

| Time parked | Fee (at $15/hr) |
|---|---|
| 2 minutes | $15 |
| 60 minutes | $15 |
| 61 minutes | $30 |
| 3 hours | $45 |

