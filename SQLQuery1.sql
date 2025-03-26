USE renteasy;
GO

CREATE TABLE dbo.Users(
	UserId varchar(50) PRIMARY KEY NOT NULL,
	Username varchar(50), 
	Password varchar(50), 
	Role varchar(10) NULL
);
GO

CREATE TABLE dbo.ItemListing (
    ItemId VARCHAR(50) PRIMARY KEY NOT NULL,
    OwnerId VARCHAR(50) NOT NULL,
    Title VARCHAR(50) NOT NULL,
    Description VARCHAR(250),
    AvailableFrom DATE,
    AvailableTo DATE,
    PricePerDay DECIMAL(10,2),
    PricePerWeek DECIMAL(10,2),
    PricePerMonth DECIMAL(10,2),
    ItemImages NVARCHAR(MAX), 
    CONSTRAINT FK_Owner_Listing FOREIGN KEY (OwnerId) REFERENCES dbo.users(UserId)
);
GO

CREATE TABLE dbo.Bookings (
    BookingID VARCHAR(50) PRIMARY KEY NOT NULL,
    ItemId VARCHAR(50) NOT NULL,
    BookerId VARCHAR(50) NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    TotalAmount DECIMAL(10,2) NOT NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    CONSTRAINT FK_Item_Booking FOREIGN KEY (ItemId) REFERENCES dbo.itemlisting(ItemId),
    CONSTRAINT FK_Item_Renter FOREIGN KEY (BookerId) REFERENCES dbo.users(UserId),
    CONSTRAINT CHK_Status CHECK (status IN ('Pending', 'Confirmed', 'Completed', 'Canceled'))
);

GO