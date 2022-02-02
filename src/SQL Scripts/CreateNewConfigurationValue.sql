BEGIN TRANSACTION

INSERT INTO Stravaig.AppConfiguration(ConfigKey, ConfigValue)
VALUES('MyConfiguration:SomeString', 'A value from the database.');

INSERT INTO Stravaig.AppConfiguration(ConfigKey, ConfigValue)
VALUES('MyConfiguration:SomeNumber', '1234');

INSERT INTO Stravaig.AppConfiguration(ConfigKey, ConfigValue)
VALUES('FeatureManager:FeatureC', 'true');

COMMIT TRANSACTION