-- Update Welcome Page Interval to 9999
DECLARE @RefreshIntervalSecondsGuid UNIQUEIDENTIFIER = 'C99D34BF-711B-4865-84B4-B0929C92D16C'

UPDATE [Attribute]
SET DefaultValue = '9999'
WHERE [Guid] = @RefreshIntervalSecondsGuid

-- Update all Welcome Page Interval to 9999
UPDATE AttributeValue
SET Value = '9999'
WHERE AttributeId IN (
        SELECT Id
        FROM [Attribute]
        WHERE Guid IN (@RefreshIntervalSecondsGuid)
        )
