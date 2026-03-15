
  CREATE TABLE ParkingRecords (
        Id              INT             NOT NULL IDENTITY(1,1) PRIMARY KEY,
        TagNumber       NVARCHAR(50)    NOT NULL,
        CheckInTime     DATETIME        NOT NULL,
        CheckOutTime    DATETIME        NULL,
        AmountCharged   DECIMAL(10, 2)  NULL,
        IsActive        BIT             NOT NULL DEFAULT 1
    );

   
    CREATE INDEX IX_ParkingRecords_TagNumber_IsActive
        ON ParkingRecords (TagNumber, IsActive);

    
    CREATE INDEX IX_ParkingRecords_CheckInTime
        ON ParkingRecords (CheckInTime);

    CREATE INDEX IX_ParkingRecords_CheckOutTime
        ON ParkingRecords (CheckOutTime);

