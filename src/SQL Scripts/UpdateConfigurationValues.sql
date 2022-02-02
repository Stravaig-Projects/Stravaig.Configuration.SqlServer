BEGIN TRANSACTION

UPDATE Stravaig.AppConfiguration
SET ConfigValue = 'A different value from the database.'
WHERE ConfigKey = 'MyConfiguration:SomeString'


UPDATE Stravaig.AppConfiguration
SET ConfigValue = 'true'
WHERE ConfigKey = 'FeatureManager:FeatureC'

COMMIT TRANSACTION