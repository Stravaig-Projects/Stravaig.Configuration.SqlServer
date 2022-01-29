-- Run this in to your database. You can change the schema to suit if you need to.

CREATE SCHEMA Stravaig;
GO
CREATE TABLE Stravaig.AppConfiguation
(
    ConfigKey NVARCHAR(1024) PRIMARY KEY CLUSTERED,
    ConfigValue NVARCHAR(MAX)
);
GO